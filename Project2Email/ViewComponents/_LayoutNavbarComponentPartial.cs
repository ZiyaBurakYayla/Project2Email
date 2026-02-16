using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Project2Email.Entities;
using System.Drawing.Printing;

namespace Project2Email.ViewComponents
{
    public class _LayoutNavbarComponentPartial : ViewComponent
    {
        private readonly UserManager<AppUser> _userManager;

        public _LayoutNavbarComponentPartial(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            AppUser appUser = new AppUser();
            if (User.Identity.IsAuthenticated && User.Identity.Name != null)
            {
                appUser = await _userManager.FindByNameAsync(User.Identity.Name);
            }
            return View(appUser);
        }
    }
}
