using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project2Email.Context;
using Project2Email.Dtos;
using Project2Email.Entities;
using Project2Email.Services;

namespace Project2Email.Controllers
{
    public class MessageController : Controller
    {
        private readonly GeminiService _geminiService;
        private readonly EmailContext _context;
        private readonly UserManager<AppUser> _userManager;

        public MessageController(GeminiService geminiService, EmailContext context, UserManager<AppUser> userManager)
        {
            _geminiService = geminiService;
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult SendMessage()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(Project2Email.Entities.Message p)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            if (user == null)
            {
                return Json(new { success = false, message = "Oturum süreniz dolmuş." });
            }

            p.SenderEmail = user.Email;
            p.SendDate = DateTime.Now;

            string rawContent = p.MessageDetail ?? "";
            string cleanContent = System.Text.RegularExpressions.Regex.Replace(rawContent, "<.*?>", String.Empty);

            try
            {
                var analysisResult = await _geminiService.AnalyzeEmailAsync(p.Subject, cleanContent);

                p.Summary = analysisResult.Summary;

                p.Category = analysisResult.CategoryName;

                switch (analysisResult.CategoryName)
                {
                    case "İş": p.CategoryId = 1; break;
                    case "Sosyal": p.CategoryId = 2; break;
                    case "Finans": p.CategoryId = 3; break;
                    case "Tanıtım": p.CategoryId = 4; break;
                    case "Önemli": p.CategoryId = 5; break;
                    default:
                        p.CategoryId = 1;
                        p.Category = "İş";
                        break;
                }
                _context.Messages.Add(p);
                await _context.SaveChangesAsync();

                var senderState = new UserMessageState
                {
                    MessageId = p.MessageId,
                    AppUserId = user.Id.ToString(),
                    IsRead = true,
                    IsTrash = false,
                    IsStarred = false,
                    Folder = "Sent"
                };
                _context.UserMessageStates.Add(senderState);

                if (p.ReceiverEmail != user.Email)
                {
                    var receiverUser = await _userManager.FindByEmailAsync(p.ReceiverEmail);
                    if (receiverUser != null)
                    {
                        var receiverState = new UserMessageState
                        {
                            MessageId = p.MessageId,
                            AppUserId = receiverUser.Id.ToString(),
                            IsRead = false,
                            IsTrash = false,
                            IsStarred = false,
                            Folder = "Inbox"
                        };
                        _context.UserMessageStates.Add(receiverState);
                    }
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, aiSummary = $"Kategori: {p.Category} | Özet: {p.Summary}" });
            }
            catch (Exception ex)
            {
                string err = "Hata: " + ex.Message;
                if (ex.InnerException != null) err += " | " + ex.InnerException.Message;

                return Json(new { success = false, message = err });
            }
        }
        public async Task<IActionResult> Inbox()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var messages = await _context.Messages
                .Join(_context.UserMessageStates,
                    message => message.MessageId,
                    state => state.MessageId,
                    (message, state) => new { message, state })
                .Where(x => x.state.AppUserId == user.Id.ToString() && x.state.Folder == "Inbox" && !x.state.IsTrash)
                .OrderByDescending(x => x.message.SendDate)
                .Select(x => new InboxMessageDto
                {
                    MessageId = x.message.MessageId,
                    SenderEmail = x.message.SenderEmail,
                    Subject = x.message.Subject,
                    Summary = x.message.Summary,
                    SendDate = x.message.SendDate,
                    IsRead = x.state.IsRead,
                    IsStarred = x.state.IsStarred
                })
                .ToListAsync();
            ViewBag.ısReadCount = messages.Count(m => !m.IsRead);
            return View(messages);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStar(int id)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var messageState = await _context.UserMessageStates
                .FirstOrDefaultAsync(x => x.MessageId == id && x.AppUserId == user.Id.ToString());

            if (messageState != null)
            {
                messageState.IsStarred = !messageState.IsStarred;

                await _context.SaveChangesAsync();

                return Json(new { success = true, isStarred = messageState.IsStarred });
            }

            return Json(new { success = false });
        }

        public async Task<IActionResult> IsStarred()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var values = await _context.Messages
                .Join(_context.UserMessageStates,
                    message => message.MessageId,
                    state => state.MessageId,
                    (message, state) => new { message, state })
                .Where(x => x.state.AppUserId == user.Id.ToString() && x.state.IsStarred && !x.state.IsTrash)
                .OrderByDescending(x => x.message.SendDate)
                .Select(x => new InboxMessageDto
                {
                    MessageId = x.message.MessageId,
                    SenderEmail = x.message.SenderEmail,
                    Subject = x.message.Subject,
                    Summary = x.message.Summary,
                    SendDate = x.message.SendDate,
                    IsRead = x.state.IsRead,
                    IsStarred = true
                })
                .ToListAsync();

            return View(values);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsTrash(int[] ids)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var messageStates = await _context.UserMessageStates
                .Where(x => ids.Contains(x.MessageId) && x.AppUserId == user.Id.ToString())
                .ToListAsync();

            foreach (var state in messageStates)
            {
                state.IsTrash = true; 
                state.Folder = "Trash"; 
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleReadStatus(int[] ids)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var messageStates = await _context.UserMessageStates
                .Where(x => ids.Contains(x.MessageId) && x.AppUserId == user.Id.ToString())
                .ToListAsync();

            foreach (var state in messageStates)
            {
                state.IsRead = !state.IsRead;
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
        [HttpGet]
        public async Task<IActionResult> Trash()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var values = await _context.Messages
                .Join(_context.UserMessageStates,
                    message => message.MessageId,
                    state => state.MessageId,
                    (message, state) => new { message, state })
                .Where(x => x.state.AppUserId == user.Id.ToString() && x.state.IsTrash)
                .OrderByDescending(x => x.message.SendDate)
                .Select(x => new InboxMessageDto
                {
                    MessageId = x.message.MessageId,
                    SenderEmail = x.message.SenderEmail,
                    Subject = x.message.Subject,
                    Summary = x.message.Summary,
                    SendDate = x.message.SendDate,
                    IsRead = x.state.IsRead,
                    IsStarred = x.state.IsStarred
                })
                .ToListAsync();

            return View(values);
        }

        [HttpPost]
        public async Task<IActionResult> RestoreFromTrash(int[] ids)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var messageStates = await _context.UserMessageStates
                .Where(x => ids.Contains(x.MessageId) && x.AppUserId == user.Id.ToString())
                .ToListAsync();

            foreach (var state in messageStates)
            {
                state.IsTrash = false;
                state.Folder = "Inbox";
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteForever(int[] ids)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var messageStates = await _context.UserMessageStates
                .Where(x => ids.Contains(x.MessageId) && x.AppUserId == user.Id.ToString())
                .ToListAsync();

            _context.UserMessageStates.RemoveRange(messageStates);

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var messageState = await _context.UserMessageStates
                .Where(x => x.MessageId == id && x.AppUserId == user.Id.ToString())
                .FirstOrDefaultAsync();

            if (messageState != null)
            {
                if (!messageState.IsRead)
                {
                    messageState.IsRead = true;
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                return RedirectToAction("Inbox");
            }

            var messageValue = await _context.Messages
                .Where(x => x.MessageId == id)
                .Select(x => new InboxMessageDto
                {
                    MessageId = x.MessageId,
                    SenderEmail = x.SenderEmail,
                    ReceiverEmail = x.ReceiverEmail,
                    Subject = x.Subject,
                    MessageDetail = x.MessageDetail,
                    SendDate = x.SendDate
                })
                .FirstOrDefaultAsync();

            return View(messageValue);
        }

        [HttpGet]
        public async Task<IActionResult> Reply(int id)
        {
            var values = await _context.Messages.FindAsync(id);

            if (values == null) return RedirectToAction("Inbox");

            var model = new MailRequestDto
            {
                ReceiverEmail = values.SenderEmail,
                Subject = "Re: " + values.Subject,
                MessageDetail = $"<br><br><br><blockquote><hr><strong>Kimden:</strong> {values.SenderEmail}<br><strong>Tarih:</strong> {values.SendDate}<br><strong>Konu:</strong> {values.Subject}<br><br>{values.MessageDetail}</blockquote>"
            };

            return View("SendMessage", model);
        }

        [HttpGet]
        public async Task<IActionResult> SearchInbox(string p)
        {
            if (string.IsNullOrEmpty(p))
            {
                return Json(new List<object>());
            }

            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var messages = await _context.Messages
                .Join(_context.UserMessageStates,
                    message => message.MessageId,
                    state => state.MessageId,
                    (message, state) => new { message, state })
                .Where(x => x.state.AppUserId == user.Id.ToString()
                            && !x.state.IsTrash
                            && x.message.Subject.Contains(p))
                .Select(x => new
                {
                    x.message.MessageId,
                    x.message.Subject,
                    x.message.SenderEmail
                })
                .Take(5) 
                .ToListAsync();

            return Json(messages);
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? id)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var query = from m in _context.Messages
                        join s in _context.UserMessageStates on m.MessageId equals s.MessageId
                        join c in _context.Categories on m.CategoryId equals c.CategoryId into catJoin
                        from cat in catJoin.DefaultIfEmpty()
                        where s.AppUserId == user.Id.ToString()
                              && m.ReceiverEmail == user.Email
                              && !s.IsTrash
                        select new
                        {
                            m,
                            s,
                            CategoryName = cat.CategoryName 
                        };
            if (id.HasValue)
            {
                query = query.Where(x => x.m.CategoryId == id.Value);
            }

            var values = await query
                .OrderByDescending(x => x.m.SendDate)
                .Select(x => new InboxMessageDto
                {
                    MessageId = x.m.MessageId,
                    SenderEmail = x.m.SenderEmail,
                    Subject = x.m.Subject,
                    Summary = x.m.Summary,
                    SendDate = x.m.SendDate,
                    IsRead = x.s.IsRead,
                    CategoryId = x.m.CategoryId ?? 0,

                    CategoryName = x.CategoryName
                })
                .ToListAsync();

            return View(values);
        }

        public async Task<IActionResult> SendBox()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            var userId = user.Id.ToString();

            var values = await _context.Messages
                .Join(_context.UserMessageStates,
                    m => m.MessageId,
                    s => s.MessageId,
                    (m, s) => new { m, s })
                .Where(x =>
                    x.m.SenderEmail == user.Email &&  
                    x.s.AppUserId == userId &&        
                    x.s.Folder == "Sent" &&          
                    !x.s.IsTrash                      
                )
                .OrderByDescending(x => x.m.SendDate)
                .Select(x => new InboxMessageDto
                {
                    MessageId = x.m.MessageId,
                    SenderEmail = x.m.SenderEmail,
                    ReceiverEmail = x.m.ReceiverEmail,
                    Subject = x.m.Subject,
                    Summary = x.m.Summary,
                    SendDate = x.m.SendDate,
                    IsRead = x.s.IsRead,
                    IsStarred = x.s.IsStarred,
                    CategoryName = x.m.Category, 
                    CategoryId = x.m.CategoryId ?? 1
                })
                .ToListAsync();

            return View(values);
        }
    }
}