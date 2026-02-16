using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using Project2Email.Dtos;
using Project2Email.Entities;

namespace Project2Email.Controllers
{
    public class LoginController : Controller
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;

        public LoginController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult UserLogin()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UserLogin(UserLoginDto userLoginDto)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(userLoginDto.Email);

                if (user != null)
                {
                    if (!user.EmailConfirmed)
                    {
                        ViewBag.ShowConfirmPopup = true;
                        ViewBag.UnconfirmedEmail = userLoginDto.Email;
                        return View(userLoginDto);
                    }

                    var result = await _signInManager.PasswordSignInAsync(user.UserName, userLoginDto.Password, true, false);

                    if (result.Succeeded)
                    {
                        return RedirectToAction("Inbox", "Message");
                    }
                }

                ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı.");
            }
            return View(userLoginDto);
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                ModelState.AddModelError("", "Bu email adresiyle kayıtlı kullanıcı bulunamadı.");
                return View();
            }

            Random random = new Random();
            int code = random.Next(100000, 1000000);

            HttpContext.Session.SetInt32("ResetCode", code);
            HttpContext.Session.SetString("ResetEmail", email);

            MimeMessage mimeMessage = new MimeMessage();
            MailboxAddress mailboxAddressFrom = new MailboxAddress("Admin", "ziyayayla75@gmail.com");
            mimeMessage.From.Add(mailboxAddressFrom);
            MailboxAddress mailboxAddressTo = new MailboxAddress("User", email);
            mimeMessage.To.Add(mailboxAddressTo);

            mimeMessage.Subject = "Şifre Sıfırlama Kodu";
            var bodyBuilder = new BodyBuilder();
            bodyBuilder.TextBody = "Şifrenizi sıfırlamak için doğrulama kodunuz: " + code;
            mimeMessage.Body = bodyBuilder.ToMessageBody();

            using (SmtpClient smtpClient = new SmtpClient())
            {
                smtpClient.Connect("smtp.gmail.com", 587, false);
                smtpClient.Authenticate("ziyayayla75@gmail.com", "ybzf zrxb zhdn tqub");
                smtpClient.Send(mimeMessage);
                smtpClient.Disconnect(true);
            }

            return RedirectToAction("VerifyResetCode");
        }

        [HttpGet]
        public IActionResult VerifyResetCode()
        {
            return View();
        }

        [HttpPost]
        public IActionResult VerifyResetCode(int code)
        {
            var sessionCode = HttpContext.Session.GetInt32("ResetCode");
            if (sessionCode == code)
            {
                return RedirectToAction("ResetPassword");
            }

            ViewBag.Error = "Girdiğiniz kod hatalı.";
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            var email = HttpContext.Session.GetString("ResetEmail");
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("ForgotPassword");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto model)
        {
            var email = HttpContext.Session.GetString("ResetEmail");
            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("", "Şifreler uyuşmuyor.");
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(email);
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, model.Password);

            if (result.Succeeded)
            {
                HttpContext.Session.Remove("ResetCode");
                HttpContext.Session.Remove("ResetEmail");
                return RedirectToAction("UserLogin");
            }
            else
            {
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError("", item.Description);
                }
            }
            return View(model);
        }
    }
}