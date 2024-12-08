using ElmanhagPlatform.Data;
using ElmanhagPlatform.Models;
using ElmanhagPlatform.Services;
using ElmanhagPlatform.Utility;
using ElmanhagPlatform.ViewModels;
using ElmanhagPlatform.ViewModels.TeacherVM;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ElmanhagPlatform.Controllers
{
    public class CourseController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IEmailProvider _emailProvider;

        public CourseController(AppDbContext context, IWebHostEnvironment hostingEnvironment
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
                if (teacherId == null)
                {
                    return RedirectToAction("Index", "Home");
                }
                var courses = await _context.Courses.Where(u => u.TeacherId == teacherId).ToListAsync();

                return View(courses);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Create(string? returnUrl = null)
        {
            if (User.IsInRole("Teacher"))
            {
                var teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (teacherId == null)
                {
                    return RedirectToAction("Index", "Home");
                }
                var Teacher = await _context.ApplicationUsers.FindAsync(teacherId);

                ViewBag.teacher = Teacher;
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
        public async Task<IActionResult> Create( CreateCourseVM model, string? returnUrl = null)
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

                    Course course = new()
                    {
                        Name = model.Name,
                        Description = model.Description,
                        Year = model.Year,
                        CreationDate = DateOnly.FromDateTime(DateTime.Now),
                        UpdateDate = DateOnly.FromDateTime(DateTime.Now),
                        TeacherId = teacherId,
                        Price = model.Price,
                        ApplicationUser = Teacher,
                    };

                    if (model.ImageOfCourse != null)
                    {
                        string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "ImageOfCourse");

                        string[] allowedExtensions = { ".png", ".jpg", ".jpeg" };
                        if (!allowedExtensions.Contains(Path.GetExtension( model.ImageOfCourse.FileName).ToLower()))
                        {
                            TempData["ErrorMessage"] = "مسموح بالامتدادات التالية فقط .png و .jpg و .jpeg";
                            return RedirectToAction("Create");
                        }

                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ImageOfCourse.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        await using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.ImageOfCourse.CopyToAsync(fileStream);
                        }
                        course.ImageOfCourse = uniqueFileName;
                    }

                    _context.Courses.Add(course);
                    await _context.SaveChangesAsync();

                    HttpContext.Session.SetString("created", "true");
                    return RedirectToAction("Index", "Course");

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
                if (id == null)
                {
                    return RedirectToAction("Index", "Course");
                }
                var teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (teacherId == null)
                {
                    return RedirectToAction("Index", "Home");
                }
                var Teacher = await _context.ApplicationUsers.FindAsync(teacherId);

                ViewBag.teacher = Teacher;

                var course = await _context.Courses.FindAsync(id);

                var coursemodel = new EditCourseVM
                {
                    Id = id,
                    Name = course.Name,
                    Description = course.Description,
                    Year = course.Year,
                    Price = course.Price,
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
        public async Task<IActionResult> Edit(int id, EditCourseVM courseForm)
        {
            if (User.IsInRole("Teacher"))
            {
                try
                {
                    var course = await _context.Courses.FindAsync(id);
                    if (course == null)
                    {
                        return RedirectToAction("Index", "Course");
                    }

                    course.Name = courseForm.Name;
                    course.Description = courseForm.Description;
                    course.Year = courseForm.Year;
                    course.Price = courseForm.Price;
                    course.UpdateDate = DateOnly.FromDateTime(DateTime.Now);

                    string oldImageFileName = course.ImageOfCourse;
                    string Oldd = course.ImageOfCourse;


                    if (courseForm.ImageOfCourse != null)
                    {
                        string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "ImageOfCourse");

                        string[] allowedExtensions = { ".png", ".jpg", ".jpeg" };
                        if (!allowedExtensions.Contains(Path.GetExtension(courseForm.ImageOfCourse.FileName).ToLower()))
                        {
                            TempData["ErrorMessage"] = "Only .png and .jpg and .jpeg images are allowed!";
                            return RedirectToAction("Index");
                        }

                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + courseForm.ImageOfCourse.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        await using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await courseForm.ImageOfCourse.CopyToAsync(fileStream);
                        }

                        if (!string.IsNullOrEmpty(oldImageFileName))
                        {
                            string oldFilePath = Path.Combine(uploadsFolder, oldImageFileName);
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }
                        course.ImageOfCourse = uniqueFileName;
                    }
                    else
                    {
                        course.ImageOfCourse = Oldd;
                    }

                    _context.Update(course);
                    await _context.SaveChangesAsync();
                    HttpContext.Session.SetString("updated", "true");
                    return RedirectToAction("Index", "Course");
                }
                catch
                {
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

                var course = await _context.Courses.FindAsync(id);

                var teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (teacherId == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                var Teacher = await _context.ApplicationUsers.FindAsync(teacherId);
                if (course.TeacherId != teacherId)
                {
                    return RedirectToAction("Index", "Home");
                }

                string oldImageFileName = course.ImageOfCourse;
                string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "ImageOfCourse");
                if (!string.IsNullOrEmpty(oldImageFileName))
                {
                    string oldFilePath = Path.Combine(uploadsFolder, oldImageFileName);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                var studentCourses = await _context.StudentCourses.Where(x => x.CourseId == course.Id).ToListAsync();

                _context.StudentCourses.RemoveRange(studentCourses);
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
                HttpContext.Session.SetString("deleted", "true");
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

    }
}
