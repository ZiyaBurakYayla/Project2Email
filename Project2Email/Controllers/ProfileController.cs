using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Project2Email.Dtos;
using Project2Email.Entities;

namespace Project2Email.Controllers
{
    public class ProfileController : Controller
    {
        private readonly UserManager<AppUser> _userManager;

        public ProfileController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var values =await _userManager.FindByNameAsync(User.Identity.Name);
            UserEditDto userEditDto = new UserEditDto();
            userEditDto.Name = values.Name;
            userEditDto.Surname = values.Surname;
            userEditDto.Email = values.Email;
            userEditDto.ImageUrl = values.ImageUrl;
            userEditDto.About = values.About;
            return View(userEditDto);
        }

        [HttpPost]
        public async Task<IActionResult> Index(UserEditDto userEditDto, bool deleteImage)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            user.Name = userEditDto.Name;
            user.Surname = userEditDto.Surname;
            user.Email = userEditDto.Email;
            user.About = userEditDto.About;

            if (!string.IsNullOrEmpty(userEditDto.Password))
            {
                if (userEditDto.Password == userEditDto.ConfirmPassword)
                {
                    user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, userEditDto.Password);
                }
                else
                {
                    ModelState.AddModelError("ConfirmPassword", "Şifreler birbiriyle uyuşmuyor.");
                    return View(userEditDto);
                }
            }
            if (userEditDto.Image != null)
            {
                var resource = Directory.GetCurrentDirectory();
                var extension = Path.GetExtension(userEditDto.Image.FileName);
                var imagename = Guid.NewGuid() + extension;
                var saveLocation = resource + "/wwwroot/images/" + imagename;

                using (var stream = new FileStream(saveLocation, FileMode.Create))
                {
                    await userEditDto.Image.CopyToAsync(stream);
                }
                user.ImageUrl = imagename;
            }
            else if (deleteImage)
            {
                user.ImageUrl = null; 
            }

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return RedirectToAction("Inbox", "Message");
            }

            return View(userEditDto);
        }
    }
}
