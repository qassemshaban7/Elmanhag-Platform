using ElmanhagPlatform.Data;
using ElmanhagPlatform.Models;
using ElmanhagPlatform.Services;
using ElmanhagPlatform.ViewModels.Video;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using FFMpegCore;
using FFMpegCore.Pipes;
using System.IO;
using System.Threading.Tasks;


namespace ElmanhagPlatform.Controllers
{
    public class VideoController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IEmailProvider _emailProvider;

        public VideoController(AppDbContext context, IWebHostEnvironment hostingEnvironment
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
                if (teacherId == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                ViewBag.course = await _context.Lectures.FindAsync(id);

                var videos = await _context.Videos.Where(u => u.LectureId == id && u.Lecture.Course.TeacherId == teacherId).ToListAsync();

                var lec = await _context.Lectures.FindAsync(id);
                ViewBag.CourseId = lec.CourseId; 
                ViewBag.LectureId = id;
                return View(videos);
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

                ViewBag.Lectures = await _context.Lectures.FindAsync(id);
                var lecture = await _context.Lectures.FindAsync(id);
                ViewBag.lecture = lecture;
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int id, AddVideoVM model, string? returnUrl = null)
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
                    var lecture = await _context.Lectures.FindAsync(id);
                    if (lecture == null)
                    {
                        return RedirectToAction("Index", "Lecture");
                    }

                    ViewBag.lecture = lecture;
                    var course = await _context.Courses.FindAsync(lecture.CourseId);

                    if (course.TeacherId != teacherId)
                    {
                        return RedirectToAction("Index", "Home");
                    }

                    string[] allowedExtensions = { ".mp4", ".avi", ".mkv" };
                    string uploadsFolder = "videos";

                    if (!allowedExtensions.Contains(Path.GetExtension(model.VideoPath.FileName).ToLower()))
                    {
                        return BadRequest("Only .mp4, .avi, and .mkv videos are allowed!");
                    }

                    var uniqueFileName = await SaveFile(model.VideoPath, uploadsFolder);

                    string videoFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", uploadsFolder, uniqueFileName);

                    Video video = new()
                    {
                        Name = model.Name,
                        Description = model.Description,
                        LectureId = id,
                        VideoPath = uniqueFileName,
                        Duration = model.Duration
                    };

                    course.UpdateDate = DateOnly.FromDateTime(DateTime.Now);

                    _context.Update(course);
                    _context.Videos.Add(video);
                    await _context.SaveChangesAsync();

                    HttpContext.Session.SetString("created", "true");
                    return RedirectToAction("Index", new { id = lecture.Id });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"خطأ أثناء حفظ الفيديو: {ex.Message}");
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
                var video = await _context.Videos.FindAsync(id);

                var lecture = await _context.Lectures.FindAsync(video.LectureId);
                ViewBag.lecture = lecture;
                var videoVM = new EditVideoVM
                {
                    Id = id,
                    Name = video.Name,
                    Duration = video.Duration,
                    Description = video.Description,
                    LectureId = lecture.Id,
                };
                return View(videoVM);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit( int id, EditVideoVM courseForm)
        {
            if (User.IsInRole("Teacher"))
            {
                try
                {
                    var video = await _context.Videos.FindAsync(id);
                    if (video == null)
                    {
                        return RedirectToAction("Index", "Lecture");
                    }

                    var lec = await _context.Lectures.FindAsync(video.LectureId);
                    if (lec == null)
                    {
                        return RedirectToAction("Index", "Lecture");
                    }

                    var teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (teacherId == null)
                    {
                        return RedirectToAction("Index", "Home");
                    }

                    var course = await _context.Courses.FindAsync(lec.CourseId);
                    if (course.TeacherId != teacherId)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    string oldImageFileName = video.VideoPath;
                    string Oldd = video.VideoPath; 

                    if ( courseForm.VideoPath != null)
                    {
                        string[] allowedExtensions = { ".mp4", ".avi", ".mkv" };
                        string uploadsFolder = "videos";

                        if (!allowedExtensions.Contains(Path.GetExtension( courseForm.VideoPath.FileName).ToLower()))
                        {
                            return BadRequest("Only .mp4, .avi, and .mkv videos are allowed!");
                        }

                        var uniqueFileName = await SaveFile(courseForm.VideoPath, uploadsFolder);

                        if (!string.IsNullOrEmpty(oldImageFileName))
                        {
                            var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "videos", video.VideoPath);
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }
                        video.VideoPath = uniqueFileName;
                    }
                    else
                    {
                        video.VideoPath = Oldd;
                    }

                    video.Name = courseForm.Name;
                    video.Description = courseForm.Description;
                    video.Name = courseForm.Name;

                    course.UpdateDate = DateOnly.FromDateTime(DateTime.Now);

                    _context.Update(course);
                    _context.Update(video);
                    await _context.SaveChangesAsync();
                    HttpContext.Session.SetString("updated", "true");
                    return RedirectToAction("Index", new { id = lec.Id });
                }
                catch
                {
                    var video = await _context.Videos.FindAsync(id);
                    if (video == null)
                    {
                        return RedirectToAction("Index", "Lecture", new { id = video.LectureId });
                    }

                    var lec = await _context.Lectures.FindAsync(video.LectureId);
                    if (lec == null)
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
            if (User.IsInRole("Teacher") && id != null)
            {
                var video = await _context.Videos.FindAsync(id);
                if (video == null)
                {
                    return NotFound();
                }

                var teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (teacherId == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                var lec = _context.Lectures.Find(video.LectureId);
                var Teacher = await _context.ApplicationUsers.FindAsync(teacherId);
                var course = await _context.Courses.FindAsync(lec.CourseId);
                if (course.TeacherId != teacherId)
                {
                    return RedirectToAction("Index", "Home");
                }

                var videoPath = Path.Combine("wwwroot", "videos", video.VideoPath);

                if (System.IO.File.Exists(videoPath))
                {
                    System.IO.File.Delete(videoPath);
                }

                _context.Videos.Remove(video);
                await _context.SaveChangesAsync();
                HttpContext.Session.SetString("deleted", "true");
                return RedirectToAction("Index", new { id = video.LectureId });
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }


        private async Task<string> SaveFile(IFormFile file, string folderPath)
        {
            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folderPath, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return uniqueFileName;
        }
    }
}
