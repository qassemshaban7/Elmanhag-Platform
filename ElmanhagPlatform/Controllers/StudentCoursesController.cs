using ElmanhagPlatform.Data;
using ElmanhagPlatform.Services;
using ElmanhagPlatform.Utility;
using ElmanhagPlatform.ViewModels.CourseDetails;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ElmanhagPlatform.Controllers
{
    public class StudentCoursesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public StudentCoursesController(AppDbContext context, IWebHostEnvironment hostingEnvironment
            , UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> Student(int courseId)
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
                    return RedirectToAction("Index", "Home");

                var course = await _context.Courses.FindAsync(courseId);

                if(course == null || course.TeacherId != teacherId)
                    return RedirectToAction("Index", "Home");

                ViewBag.Course = course;
                var users = await (from x in _context.ApplicationUsers
                                   join userRole in _context.UserRoles
                                   on x.Id equals userRole.UserId
                                   join role in _context.Roles
                                   on userRole.RoleId equals role.Id
                                   where role.Name == StaticDetails.Student
                                   && x.ConfirmAccount == 3
                                   && x.StudentCourses.Any(sc => sc.CourseId == courseId)
                                   select x)
                                .ToListAsync();

                return View(users);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        
        [HttpGet]
        public async Task<IActionResult> GetStudentCourseDetails(int id, string userId, int? videoId = null)
        {
            var sudtCourse = await _context.StudentCourses.Where(x => x.StudentId == userId && x.CourseId == id).FirstOrDefaultAsync();
            if (sudtCourse == null)
                return RedirectToAction("Index", "Home");

            var course = await _context.Courses
                                       .Include(c => c.Lectures)
                                       .ThenInclude(l => l.Videos)
                                       .Include(c => c.Lectures)
                                       .ThenInclude(l => l.PFiles)
                                       .Include(c => c.Lectures)
                                       .ThenInclude(l => l.Exams)
                                       .ThenInclude(d => d.Questions)
                                       .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                return NotFound();
            }

            var user = await _context.ApplicationUsers.FindAsync(userId);

            var watchLog = await _context.VideoWatchLogs
                   .Include(log => log.Video)
                   .ThenInclude(video => video.Lecture)
                   .Where(log => log.Video.Lecture.CourseId == id && log.UserId == userId)
                   .ToListAsync();
            ViewBag.Watchinglog = watchLog;

            var examSol = await _context.ExamAnswers
                           .Include(ans => ans.Exam)
                           .ThenInclude(ex => ex.Lecture)
                           .Include(ans => ans.Results)
                           .ThenInclude(res => res.Question)
                           .Where(ans => ans.Exam.Lecture.CourseId == id && ans.UserId == userId)
                           .ToListAsync();
            ViewBag.ExamesSol = examSol;

            var courseDetails = new CourseVM
            {
                teachId = course.TeacherId,
                Name = course.Name,
                Description = course.Description,
                CreationDate = course.CreationDate,
                UpdateDate = course.UpdateDate,
                Price = course.Price,
                Year = course.Year,
                ImageOfCourse = course.ImageOfCourse,
                VideosCount = course.Lectures.Sum(lec => lec.Videos.Count),
                FilesCount = course.Lectures.Sum(lec => lec.PFiles.Count),
                ExamsCount = course.Lectures.Sum(lec => lec.Exams.Count),
                LecturesVM = course.Lectures.Select(lec => new LectureVM
                {
                    Id = lec.Id,
                    Name = lec.Name,
                    Content = lec.Content,
                    VideosVM = lec.Videos.Select(v => new VideoVM
                    {
                        Id = v.Id,
                        Name = v.Name,
                        Description = v.Description,
                    }).ToList(),
                    PFilesVM = lec.PFiles.Select(f => new PFileVM
                    {
                        Id = f.Id,
                        Name = f.Name,
                        Description = f.Description,
                        FilePath = f.FilePath
                    }).ToList(),
                    ExamsVM = lec.Exams.Select(e => new ExamVM
                    {
                        Id = e.Id,
                        Name = e.Name,
                        Description = e.Description,
                        Duration = e.Duration,
                        Questions = e.Questions.ToList()
                    }).ToList()
                }).ToList()
            };

            return View(courseDetails);
        }


    }
}
