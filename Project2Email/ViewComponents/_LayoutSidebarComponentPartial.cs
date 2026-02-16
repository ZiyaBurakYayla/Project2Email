using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project2Email.Context;
using Project2Email.Context;
using Project2Email.Entities;

namespace Project2Email.ViewComponents
{
    public class _LayoutSidebarComponentPartial : ViewComponent
    {
        private readonly EmailContext _context;
        private readonly UserManager<AppUser> _userManager;

        public _LayoutSidebarComponentPartial(UserManager<AppUser> userManager, EmailContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            if (user == null) return View();

            var userId = user.Id.ToString();

            ViewBag.InboxCount = await _context.UserMessageStates
                .Where(x => x.AppUserId == userId && x.Folder == "Inbox" )
                .CountAsync();

            ViewBag.TrashCount = await _context.UserMessageStates
                .Where(x => x.AppUserId == userId && x.IsTrash)
                .CountAsync();

            ViewBag.StarredCount = await _context.UserMessageStates
                .Where(x => x.AppUserId == userId && x.IsStarred && !x.IsTrash)
                .CountAsync();

            ViewBag.SentCount = await _context.UserMessageStates
                .Where(x => x.AppUserId == userId && x.Folder == "Sent" && !x.IsTrash)
                .CountAsync();

            var catCounts = await _context.Messages
     .Join(_context.UserMessageStates, m => m.MessageId, s => s.MessageId, (m, s) => new { m, s })
     .Where(x => x.s.AppUserId == userId &&
                 x.s.Folder == "Inbox" && 
                 !x.s.IsTrash &&
                 x.m.CategoryId != null)
     .GroupBy(x => x.m.CategoryId)
     .Select(g => new { Id = g.Key, Count = g.Count() })
     .ToListAsync();

            ViewBag.Cat1Count = catCounts.FirstOrDefault(x => x.Id == 1)?.Count ?? 0;
            ViewBag.Cat2Count = catCounts.FirstOrDefault(x => x.Id == 2)?.Count ?? 0;
            ViewBag.Cat3Count = catCounts.FirstOrDefault(x => x.Id == 3)?.Count ?? 0;
            ViewBag.Cat4Count = catCounts.FirstOrDefault(x => x.Id == 4)?.Count ?? 0;
            ViewBag.Cat5Count = catCounts.FirstOrDefault(x => x.Id == 5)?.Count ?? 0;

            return View();
        }
    }
}