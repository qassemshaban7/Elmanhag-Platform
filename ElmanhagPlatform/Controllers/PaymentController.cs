using ElmanhagPlatform.Data;
using ElmanhagPlatform.Models;
using ElmanhagPlatform.Services;
using ElmanhagPlatform.Utility;
using ElmanhagPlatform.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ElmanhagPlatform.Controllers
{
    public class PaymentController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly UserManager<IdentityUser> _userManager;

        public PaymentController(AppDbContext context, IWebHostEnvironment hostingEnvironment
           , UserManager<IdentityUser> userManager)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Student"))
            {
                if (HttpContext.Session.GetString("created") != null)
                {
                    ViewBag.created = true;
                    HttpContext.Session.Remove("created");
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var payment = await _context.Payments.Where(x => x.StudentId == userId).ToListAsync();

                return View(payment);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Add(string? returnUrl = null)
        {
            if (User.IsInRole("Student"))
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
        public async Task<IActionResult> Add(CreatePaymentVM model, string? returnUrl = null)
        {
            if (User.IsInRole("Student"))
            {
                ViewData["ReturnUrl"] = returnUrl;
                if (ModelState.IsValid)
                {
                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    var user = await _context.ApplicationUsers.FindAsync(userId);

                    Payment pay = new()
                    {
                        Value = model.Value,
                        Status = 1,
                        PhoneNumber = model.PhoneNumber,
                        StudentId = userId,
                        ApplicationUser = user
                    };

                    if (model.Image != null)
                    {
                        string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "PaymentImage");

                        string[] allowedExtensions = { ".png", ".jpg", ".jpeg" };
                        if (!allowedExtensions.Contains(Path.GetExtension(model.Image.FileName).ToLower()))
                        {
                            TempData["ErrorMessage"] = "مسموح بالامتدادات التالية فقط .png و .jpg و .jpeg";
                            return RedirectToAction("Create");
                        }

                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Image.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        await using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.Image.CopyToAsync(fileStream);
                        }
                        pay.Image = uniqueFileName;
                    }

                    _context.Payments.Add(pay);
                    await _context.SaveChangesAsync();

                    HttpContext.Session.SetString("created", "true");
                    return RedirectToAction("Index", "Payment");


                }
                return View(model);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> PindingInvoice()
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

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var payment = await _context.Payments.Include(c => c.ApplicationUser).Where(x => x.Status == 1).ToListAsync();

                return View(payment);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int? Id)
        {
            if (User.IsInRole("Admin"))
            {
                try
                {
                    if (Id == null)
                        return RedirectToAction(nameof(PindingInvoice));

                    var payment = await _context.Payments.FirstOrDefaultAsync(m => m.Id == Id);

                    if (payment == null)
                        return RedirectToAction(nameof(PindingInvoice));

                    payment.Status = 3;
                    
                    var user = _context.ApplicationUsers.Find(payment.StudentId);
                    user.Money += payment.Value;

                    await _context.SaveChangesAsync();
                    HttpContext.Session.SetString("created", "true");
                    return RedirectToAction(nameof(PindingInvoice));
                }
                catch (Exception ex)
                {
                    return RedirectToAction(nameof(PindingInvoice));
                }
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int? Id)
        {
            if (User.IsInRole("Admin"))
            {
                try
                {
                    if (Id == null)
                        return RedirectToAction(nameof(PindingInvoice));

                    var payment = await _context.Payments.FirstOrDefaultAsync(m => m.Id == Id);

                    if (payment == null)
                        return RedirectToAction(nameof(PindingInvoice));

                    payment.Status = 2;

                    await _context.SaveChangesAsync();
                    HttpContext.Session.SetString("updated", "true");
                    return RedirectToAction(nameof(PindingInvoice));
                }
                catch (Exception ex)
                {
                    return RedirectToAction(nameof(PindingInvoice));
                }
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> AcceptedInvoice()
        {
            if (User.IsInRole("Admin"))
            {
                var payment = await _context.Payments.Include(c => c.ApplicationUser).Where(x => x.Status == 3).ToListAsync();

                return View(payment);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> RejectedInvoice()
        {
            if (User.IsInRole("Admin"))
            {
                var payment = await _context.Payments.Include(c => c.ApplicationUser).Where(x => x.Status == 2).ToListAsync();

                return View(payment);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(int id, string? year)
        {
            if (User.IsInRole("Student"))
            {
                ViewBag.year = year;

                if (id == null)
                    return RedirectToAction("Index", "Home");

                var course = await _context.Courses.FirstOrDefaultAsync(m => m.Id == id);
                if (course == null)
                    return RedirectToAction("Index", "Home");

                var teacherId = course.TeacherId;
                try
                {
                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    var user = await _context.ApplicationUsers.FindAsync(userId);

                    if(course.Price > user.Money)
                    {
                        return RedirectToAction("Courses", "Home", new {id = teacherId, year = year});
                    }

                    user.Money -= course.Price;
                    await _context.SaveChangesAsync();

                    StudentCourse pay = new()
                    {
                        CourseId = id,
                        StudentId = userId,
                        ApplicationUser = user
                    };

                    _context.StudentCourses.Add(pay);
                    await _context.SaveChangesAsync();

                    HttpContext.Session.SetString("created", "true");
                    return RedirectToAction("Courses", "Home", new { id = teacherId, year = year });
                }
                catch (Exception ex)
                {
                    return RedirectToAction("Courses", "Home", new { id = teacherId, year = year });
                }
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }


    }
}
