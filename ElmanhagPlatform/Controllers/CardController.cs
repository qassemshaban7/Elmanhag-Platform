using ElmanhagPlatform.Data;
using ElmanhagPlatform.Models;
using ElmanhagPlatform.Services;
using ElmanhagPlatform.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ElmanhagPlatform.Controllers
{
    public class CardController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IEmailProvider _emailProvider;

        public CardController(AppDbContext context, IWebHostEnvironment hostingEnvironment
            , UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager,
            IEmailProvider emailProvider)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailProvider = emailProvider;
        }

        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin"))
            {
                if (HttpContext.Session.GetString("created") != null)
                {
                    ViewBag.created = true;
                    HttpContext.Session.Remove("created");
                }
                if (HttpContext.Session.GetString("updated") != null)
                {
                    ViewBag.updated = true;
                    HttpContext.Session.Remove("updated");
                }
                if (HttpContext.Session.GetString("deleted") != null)
                {
                    ViewBag.deleted = true;
                    HttpContext.Session.Remove("deleted");
                }

                var cards = await _context.Cards.Where(u => u.Used == false).ToListAsync();

                return View(cards);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Create(string? returnUrl = null)
        {
            if (User.IsInRole("Admin"))
            {
                ViewData["ReturnUrl"] = returnUrl;
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Card model, string? returnUrl = null)
        {
            if (User.IsInRole("Admin"))
            {
                ViewData["ReturnUrl"] = returnUrl;
                try
                {
                    string randomContent;

                    do
                    {
                        randomContent = GenerateRandomContent(16);
                    }
                    while (await _context.Cards.AnyAsync(c => c.Content == randomContent));

                    Card card = new()
                    {
                        Content = randomContent,
                        ExpireAt = model.ExpireAt,
                        Used = false,
                    };

                    _context.Cards.Add(card);
                    await _context.SaveChangesAsync();

                    HttpContext.Session.SetString("created", "true");
                    return RedirectToAction("Index", "Card");
                }
                catch
                {
                    return View(model);
                }
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        private string GenerateRandomContent(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%&";
            Random random = new();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (User.IsInRole("Admin"))
            {
                if (id == null)
                    return RedirectToAction("Index", "Card");

                var card = await _context.Cards.FindAsync(id);
                if (card == null)
                    return RedirectToAction("Index", "Card");

                var coursemodel = new EditCardVM
                {
                    Id = id,
                    ExpireAt = card.ExpireAt,
                    ExpireAtString = card.ExpireAt.ToString("yyyy-MM-dd")
                };

                return View(coursemodel);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditCardVM courseForm)
        {
            if (User.IsInRole("Admin"))
            {
                try
                {
                    if (DateOnly.TryParse(courseForm.ExpireAtString, out DateOnly expireDate))
                    {
                        var card = await _context.Cards.FindAsync(courseForm.Id);
                        if (card == null)
                            return RedirectToAction("Index", "Card");

                        card.ExpireAt = expireDate;

                        _context.Update(card);
                        await _context.SaveChangesAsync();
                        HttpContext.Session.SetString("updated", "true");
                        return RedirectToAction("Index", "Card");
                    }
                    else
                    {
                        ModelState.AddModelError("ExpireAtString", "تاريخ غير صالح.");
                    }
                    return RedirectToAction("Index", "Home");
                }
                catch
                {
                    return RedirectToAction("Index", "Card");
                }
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (User.IsInRole("Admin"))
            {
                if (id == null)
                {
                    return NotFound();
                }

                var card = await _context.Cards.FindAsync(id);

                _context.Cards.Remove(card);
                await _context.SaveChangesAsync();
                HttpContext.Session.SetString("deleted", "true");
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
        
        
        public async Task<IActionResult> AddCard(int id, string content)
        {
            if (User.IsInRole("Student"))
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = await _context.ApplicationUsers.FindAsync(userId);

                var lec = await _context.Lectures.FindAsync(id);

                if (lec == null || user == null)
                    return RedirectToAction("Index", "Home");

                var card = await _context.Cards.Where(m => m.Content == content).FirstOrDefaultAsync();
                if (card == null || card.Used == true)
                {
                    if(user.RechargedCard == null)
                    {
                        user.RechargedCard = 1;
                    }
                    else
                    {
                        user.RechargedCard++;
                    }

                    _context.Update(user);
                    await _context.SaveChangesAsync();

                    TempData["AlertMessage"] = "الكود غير صحيح. يرجى التأكد من صحة الكود قبل الإرسال. عدد المحاولات محدود، وقد يؤدي تكرار المحاولات الخاطئة إلى إيقاف حسابك.";
                    return RedirectToAction("GetCourseDetails", "Home", new { id = lec.CourseId });
                }

                if (card.ExpireAt < DateOnly.FromDateTime(DateTime.Now))
                {
                    TempData["AlertMessage"] = "تاريخ انتهاء البطاقة قد انقضى.";
                    return RedirectToAction("GetCourseDetails", "Home", new { id = lec.CourseId });
                }
                else
                {
                    card.StudentId = user.Id;
                    card.ApplicationUser = user;
                    card.Used = true;
                    card.LectureId = id;

                    _context.Update(card);
                    await _context.SaveChangesAsync();

                    HttpContext.Session.SetString("updated", "true");
                    return RedirectToAction("GetCourseDetails", "Home", new { id = lec.CourseId });
                }
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }


        public async Task<IActionResult> UsedCards()
        {
            if (User.IsInRole("Admin"))
            {
                var cards = await _context.Cards
                    .Where(u => u.Used == true)
                    .Include(e => e.ApplicationUser)
                    .Include(l => l.Lecture)
                    .ThenInclude(c => c.Course)
                    .ThenInclude(e => e.ApplicationUser)
                    .ToListAsync();

                return View(cards);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

    }
}