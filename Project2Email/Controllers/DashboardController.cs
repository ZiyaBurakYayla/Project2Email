using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project2Email.Context;
using Project2Email.Context;
using Project2Email.Dtos;
using Project2Email.Entities;

namespace Project2Email.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly EmailContext _context;
        private readonly UserManager<AppUser> _userManager;

        public DashboardController(EmailContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            var userId = user.Id.ToString();

            ViewBag.User = user;

            ViewBag.InboxCount = await _context.Messages
                .Join(_context.UserMessageStates, m => m.MessageId, s => s.MessageId, (m, s) => new { m, s })
                .Where(x => x.s.AppUserId == userId && x.m.ReceiverEmail == user.Email )
                .CountAsync();

            ViewBag.SentCount = await _context.Messages
                .Join(_context.UserMessageStates, m => m.MessageId, s => s.MessageId, (m, s) => new { m, s })
                .Where(x => x.m.SenderEmail == user.Email && x.s.AppUserId == userId && !x.s.IsTrash)
                .CountAsync();

            ViewBag.UnreadCount = await _context.Messages
                .Join(_context.UserMessageStates, m => m.MessageId, s => s.MessageId, (m, s) => new { m, s })
                .Where(x => x.s.AppUserId == userId && x.m.ReceiverEmail == user.Email && !x.s.IsTrash && !x.s.IsRead)
                .CountAsync();

            ViewBag.TrashCount = await _context.Messages
                .Join(_context.UserMessageStates, m => m.MessageId, s => s.MessageId, (m, s) => new { m, s })
                .Where(x => x.s.AppUserId == userId && x.s.IsTrash)
                .CountAsync();

            ViewBag.StarredCount = await _context.UserMessageStates
                .Where(x => x.AppUserId == userId && x.IsStarred && !x.IsTrash)
                .CountAsync();

            var topSender = await _context.Messages
                .Where(x => x.ReceiverEmail == user.Email)
                .GroupBy(x => x.SenderEmail)
                .Select(g => new { Email = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .FirstOrDefaultAsync();

            ViewBag.TopSenderEmail = topSender?.Email ?? "Yok";
            ViewBag.TopSenderCount = topSender?.Count ?? 0;

            var recentMessages = await _context.Messages
                .Join(_context.UserMessageStates, m => m.MessageId, s => s.MessageId, (m, s) => new { m, s })
                .Where(x => x.s.AppUserId == userId && x.m.ReceiverEmail == user.Email && !x.s.IsTrash)
                .OrderByDescending(x => x.m.SendDate)
                .Take(6)
                .Select(x => new InboxMessageDto
                {
                    MessageId = x.m.MessageId,
                    SenderEmail = x.m.SenderEmail,
                    Subject = x.m.Subject,
                    Summary = x.m.Summary,
                    SendDate = x.m.SendDate,
                    IsRead = x.s.IsRead,
                })
                .ToListAsync();

            var last7Days = Enumerable.Range(0, 7).Select(i => DateTime.Now.Date.AddDays(-i)).Reverse().ToList();
            var chartData = new List<int>();

            foreach (var date in last7Days)
            {
                var count = await _context.Messages
                    .Where(x => x.SendDate.Date == date && (x.SenderEmail == user.Email || x.ReceiverEmail == user.Email))
                    .CountAsync();
                chartData.Add(count);
            }

            ViewBag.ChartDates = last7Days.Select(x => x.ToString("dd MMM")).ToList();
            ViewBag.ChartValues = chartData;

            var categoryNames = new List<string> { "İş", "Sosyal", "Finans", "Tanıtım", "Önemli" };
            var categoryCounts = new List<int>();

            foreach (var catName in categoryNames)
            {
                var count = await _context.Messages
                    .Join(_context.UserMessageStates, m => m.MessageId, s => s.MessageId, (m, s) => new { m, s })
                    .Where(x => x.s.AppUserId == userId && !x.s.IsTrash && x.m.Category == catName)
                    .CountAsync();
                categoryCounts.Add(count);
            }

            ViewBag.CategoryCounts = categoryCounts;
            ViewBag.CategoryNames = categoryNames;

            return View(recentMessages);
        }
    }
}