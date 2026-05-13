using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using Project2Email.Dtos;
using Project2Email.Entities;

namespace Project2Email.Controllers
{
    public class RegisterController : Controller
    {
        private readonly UserManager<AppUser> _userManager;

        public RegisterController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public ActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(UserRegisterDto userRegisterDto)
        {
            if (!userRegisterDto.IReadAgreeStatus)
            {
                ModelState.AddModelError("", "Kullanım şartlarını kabul etmeniz gerekiyor");
                return View(userRegisterDto);
            }

            Random random = new Random();
            int code = random.Next(100000, 1000000);

            AppUser appUser = new AppUser()
            {
                Name = userRegisterDto.Name,
                Surname = userRegisterDto.Surname,
                UserName = userRegisterDto.Username,
                Email = userRegisterDto.Email,
                ConfirmCode = code
            };

            var result = await _userManager.CreateAsync(appUser, userRegisterDto.Password);
            if (result.Succeeded)
            {
                SendVerificationEmail(appUser.Email, code);
                return RedirectToAction("ConfirmEmail", new { email = appUser.Email });
            }
            else
            {
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError("", item.Description);
                }
            }
            return View(userRegisterDto);
        }

        [HttpGet]
        public IActionResult ConfirmEmail(string email)
        {
            var model = new ConfirmEmailDto
            {
                Email = email
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmEmail(ConfirmEmailDto confirmEmailDto)
        {
            var user = await _userManager.FindByEmailAsync(confirmEmailDto.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "Kullanıcı bulunamadı.");
                return View(confirmEmailDto);
            }

            if (user.ConfirmCode == confirmEmailDto.ConfirmCode)
            {
                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);
                return RedirectToAction("UserLogin", "Login");
            }
            else
            {
                ModelState.AddModelError("", "Girdiğiniz kod hatalı.");
                return View(confirmEmailDto);
            }
        }

        private void SendVerificationEmail(string email, int code)
        {
            MimeMessage mimeMessage = new MimeMessage();
            MailboxAddress mailboxAddressFrom = new MailboxAddress("IdentityAdmin", "xxx@gmail.com");
            mimeMessage.From.Add(mailboxAddressFrom);

            MailboxAddress mailboxAddressTo = new MailboxAddress("User", email);
            mimeMessage.To.Add(mailboxAddressTo);

            mimeMessage.Subject = "Email Doğrulama Kodu";

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.TextBody = "Kayıt işlemini tamamlamak için doğrulama kodunuz: " + code;
            mimeMessage.Body = bodyBuilder.ToMessageBody();

            using (SmtpClient smtpClient = new SmtpClient())
            {
                smtpClient.Connect("smtp.gmail.com", 587, false);
                smtpClient.Authenticate("xxx@gmail.com", "key");
                smtpClient.Send(mimeMessage);
                smtpClient.Disconnect(true);
            }
        }
    }
}
