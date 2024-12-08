using ElmanhagPlatform.Data;
using ElmanhagPlatform.Models;
using ElmanhagPlatform.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ElmanhagPlatform.Controllers
{
    public class LectureController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IEmailProvider _emailProvider;

        public LectureController(AppDbContext context, IWebHostEnvironment hostingEnvironment
            , UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager,
            IEmailProvider emailProvider)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailProvider = emailProvider;
        }
        public async Task<IActionResult> Index(int id)
        {
            if (User.IsInRole("Teacher"))
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

                var teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (teacherId == null )
                {
                    return RedirectToAction("Index", "Home");
                }

                ViewBag.course = await _context.Courses.FindAsync(id);

                var Lec = await _context.Lectures.Where(u => u.CourseId == id && u.Course.TeacherId == teacherId).ToListAsync();
                
                ViewBag.CoursId = id;
                return View(Lec);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Create(int id, string? returnUrl = null)
        {
            if (User.IsInRole("Teacher"))
            {
                ViewData["ReturnUrl"] = returnUrl;

                ViewBag.course = await _context.Courses.FindAsync(id);
                
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( int id, Lecture model, string? returnUrl = null)
        {
            if (User.IsInRole("Teacher"))
            {
                ViewData["ReturnUrl"] = returnUrl;
                try
                {
                    var teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (teacherId == null)
                    {
                        return RedirectToAction("Index", "Home");
                    }

                    var Teacher = await _context.ApplicationUsers.FindAsync(teacherId);

                    var course = await _context.Courses.FindAsync(id);
                    if (course == null)
                    {
                        return RedirectToAction("Index", "Course");
                    }

                    if(course.TeacherId != teacherId)
                    {
                        return RedirectToAction("Index", "Home");
                    }

                    Lecture lec = new()
                    {
                        Name = model.Name,
                        Content = model.Content,
                        CourseId = id,
                    };

                    course.UpdateDate = DateOnly.FromDateTime(DateTime.Now);

                    _context.Update(course);
                    _context.Lectures.Add(lec);
                    await _context.SaveChangesAsync();

                    HttpContext.Session.SetString("created", "true");
                    return RedirectToAction("Index", new { id = course.Id });
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

        public async Task<IActionResult> Edit(int id)
        {
            if (User.IsInRole("Teacher"))
            {
                var lec = await _context.Lectures.FindAsync(id);

                var course = await _context.Courses.FindAsync(lec.CourseId);
                ViewBag.course = course;
                lec = new Lecture

                {
                    Id = id,
                    Name = lec.Name,
                    Content = lec.Content,
                    CourseId = course.Id,
                };
                return View(lec);
            }

            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Lecture courseForm)
        {
            if (User.IsInRole("Teacher"))
            {
                try
                {
                    var lec = await _context.Lectures.FindAsync(id);
                    if (lec == null)
                    {
                        return RedirectToAction("Index", "Course");
                    }

                    var course = await _context.Courses.FindAsync(lec.CourseId);
                    if (course == null)
                    {
                        return RedirectToAction("Index", "Course");
                    }

                    var teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (teacherId == null)
                    {
                        return RedirectToAction("Index", "Home");
                    }

                    if (course.TeacherId != teacherId)
                    {
                        return RedirectToAction("Index", "Home");
                    }

                    lec.Name = courseForm.Name;
                    lec.Content = courseForm.Content;

                    course.UpdateDate = DateOnly.FromDateTime(DateTime.Now);

                    _context.Update(course);
                    _context.Update(lec);
                    await _context.SaveChangesAsync();
                    HttpContext.Session.SetString("updated", "true");
                    return RedirectToAction("Index", new { id = course.Id });
                }
                catch
                {
                    var lec = await _context.Lectures.FindAsync(id);
                    if (lec == null)
                    {
                        return RedirectToAction("Index", "Course");
                    }

                    var course = await _context.Courses.FindAsync(lec.CourseId);
                    if (course == null)
                    {
                        return RedirectToAction("Index", "Course");
                    }

                    return RedirectToAction("Index", "Course");
                }
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (User.IsInRole("Teacher"))
            {
                if (id == null)
                {
                    return NotFound();
                }

                var lec = await _context.Lectures.FindAsync(id);

                var teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (teacherId == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                var Teacher = await _context.ApplicationUsers.FindAsync(teacherId);
                var course = await _context.Courses.FindAsync(lec.CourseId);
                if (course.TeacherId != teacherId)
                {
                    return RedirectToAction("Index", "Home");
                }

                var cards = await _context.Cards.Where(x => x.LectureId == lec.Id).ToListAsync();

                _context.Cards.RemoveRange(cards);

                _context.Lectures.Remove(lec);
                await _context.SaveChangesAsync();
                HttpContext.Session.SetString("deleted", "true");
                return RedirectToAction("Index", new { id = lec.CourseId });
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

    }
}
