using ElmanhagPlatform.Data;
using ElmanhagPlatform.Models;
using ElmanhagPlatform.Utility;
using ElmanhagPlatform.ViewModels;
using ElmanhagPlatform.ViewModels.AnswerVM;
using ElmanhagPlatform.ViewModels.CourseDetails;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Stripe;
using Stripe.Entitlements;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Security.Claims;

namespace ElmanhagPlatform.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
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

            var SubscribeVideo = await _context.Configs.FindAsync(1);
            ViewBag.Url = SubscribeVideo.Url;
            
            var users = await (from x in _context.ApplicationUsers
                               join userRole in _context.UserRoles
                               on x.Id equals userRole.UserId
                               join role in _context.Roles
                               on userRole.RoleId equals role.Id
                               where role.Name == StaticDetails.Teacher
                               select x)
                           .Where(u => u.ConfirmAccount == 3)
                           .ToListAsync();

            return View(users);
        }
        
        public async Task<IActionResult> View(string id, string? year)
        {
            var teacher = await _context.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == id);

            if (teacher == null)
            {
                return NotFound();
            }

            return View(teacher);
        }

        public async Task<IActionResult> Courses(string id, string? year)
        {
            if (HttpContext.Session.GetString("created") != null)
            {
                ViewBag.created = true;
                HttpContext.Session.Remove("created");
            }

            var teacher = await _context.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == id);
            if (teacher == null)
            {
                return NotFound();
            }

            var courses = await _context.Courses
                .Where(c => c.TeacherId == id && c.Year == year)
                .OrderBy(x => x.CreationDate)
                .ToListAsync();

            ViewBag.year = year;
            return View(courses);
        }

        public async Task<IActionResult> Subscription()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            //var user = await _context.ApplicationUsers.FindAsync(userId);

            var courses = await _context.Courses
                .Where(c => c.StudentCourses.Any(sc => sc.StudentId == userId) 
                 || c.Lectures.Any(l => l.Cards.Any(sc => sc.StudentId == userId)))
                .OrderBy(x => x.CreationDate)
                .ToListAsync();

            return View(courses);
        }


        [HttpGet]
        public async Task<IActionResult> GetCourseDetails(int id, int? videoId = null)
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

            //var sudtCourse = await _context.StudentCourses.Where(x => x.StudentId == userId && x.CourseId == id).FirstOrDefaultAsync();
            //if(sudtCourse == null)
            //{
            //    return RedirectToAction("Index", "Home");
            //}

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
                    CourseId = lec.CourseId,
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

        [HttpGet]
        public async Task<IActionResult> WatchVideo(int videoId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _context.ApplicationUsers.FindAsync(userId);
            if(user == null)
                return RedirectToAction("Index", "Home");

            var watchLog = await _context.VideoWatchLogs
                   .Include(log => log.Video)
                   .ThenInclude(video => video.Lecture)
                   .FirstOrDefaultAsync(log => log.VideoId == videoId && log.UserId == userId);

            var video = await _context.Videos.Include(x => x.Lecture).FirstOrDefaultAsync( y => y.Id == videoId);

            ViewBag.StudentName = user.FullName;
            ViewBag.UserName = user.UserName;
            ViewBag.courseId = video.Lecture.CourseId;
            var sudtCourse = await _context.StudentCourses.Where(x => x.StudentId == userId && x.CourseId == video.Lecture.CourseId).FirstOrDefaultAsync();
            if (sudtCourse == null)
                return RedirectToAction("Index", "Home");

            const int allowedWatchCount = 3;

            if (watchLog == null)
            {
                watchLog = new VideoWatchLog
                {
                    VideoId = videoId,
                    UserId = userId,
                    ApplicationUser = user,
                    WatchCount = 0,
                    TotalMinutesWatched = 0,
                    LastWatchDate = DateTime.Now,
                };
                _context.VideoWatchLogs.Add(watchLog);
            }
            else
            {
                if (watchLog.WatchCount >= allowedWatchCount)
                {
                    TempData["AlertMessage"] = "لقد تجاوزت الحد الأقصى لعدد مرات المشاهدة.";
                    return RedirectToAction("GetCourseDetails", "Home", new { id = watchLog.Video.Lecture.CourseId });
                }

                //watchLog.WatchCount++;
                watchLog.LastWatchDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            if (video == null)
                return NotFound();

            var videoVM = new VideoVM
            {
                Id = video.Id,
                Name = video.Name,
                Description = video.Description,
                VideoPath = video.VideoPath,
            };

            return View(videoVM);
        }

        [HttpPost]
        public async Task<IActionResult> StartWatching([FromBody] int videoId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var watchLog = await _context.VideoWatchLogs.FirstOrDefaultAsync(log => log.VideoId == videoId && log.UserId == userId);

            if (watchLog != null)
            {
                watchLog.LastWatchDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> EndWatching( [FromBody] EndWatchingRequest request)
        {
            int videoId = request.VideoId;
            var video = await _context.Videos.FindAsync(videoId);
            int minutesWatched = request.MinutesWatched;

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var watchLog = await _context.VideoWatchLogs.FirstOrDefaultAsync(log => log.VideoId == videoId && log.UserId == userId);

            if (watchLog != null)
            {
                watchLog.TotalMinutesWatched += minutesWatched;
                if(watchLog.TotalMinutesWatched >= (video.Duration/2))
                {
                    watchLog.WatchCount++;
                }
                watchLog.LastWatchDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true });
        }


        [HttpGet]
        public async Task<IActionResult> StartExam(int id)
        {
            var exam = await _context.Exams.Include(x => x.Lecture).FirstOrDefaultAsync(x => x.Id == id);
            if (exam == null)
                return RedirectToAction(nameof(Index));

            ViewBag.Exam = exam;
            ViewBag.id = id;
            ViewBag.CourseId = exam.Lecture.CourseId;

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var sudtCourse = await _context.StudentCourses.Where(x => x.StudentId == userId && x.CourseId == exam.Lecture.CourseId).FirstOrDefaultAsync();
            if (sudtCourse == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var examQuestions = await _context.Questions.Where(x => x.ExamId == id).ToListAsync();
            return View( examQuestions);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendAnswer(List<ResultVM> resultsVM, int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _context.ApplicationUsers.FindAsync(userId);
                var exam = await _context.Exams.Include(x => x.Lecture).FirstOrDefaultAsync(x => x.Id == id);

                var sudtCourse = await _context.StudentCourses.Where(x => x.StudentId == userId && x.CourseId == exam.Lecture.CourseId).FirstOrDefaultAsync();
                if (sudtCourse == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                var examAnswer = new ExamAnswer
                {
                    ExamId = id,
                    UserId = userId,
                    ApplicationUser = user,
                };
                await _context.ExamAnswers.AddAsync(examAnswer);
                await _context.SaveChangesAsync();
                HttpContext.Session.SetString("created", "true");

                foreach (var ans in resultsVM)
                {
                    var result = new Result
                    {
                        QuestionId = ans.QuestionId,
                        Answer = ans.SelectedAnswer,
                        ExamAnswerId = examAnswer.Id,
                    };
                    await _context.Results.AddAsync(result);
                }

                await _context.SaveChangesAsync();
                
                return RedirectToAction("GetCourseDetails", "Home", new { id = exam.Lecture.CourseId });
            }
            catch
            {
                var exam = await _context.Exams.Include(x => x.Lecture).FirstOrDefaultAsync(x => x.Id == id);

                return RedirectToAction("GetCourseDetails", "Home", new { id = exam.Lecture.CourseId });
            }
        }

        public IActionResult SomeAction()
        {
            TempData["AlertMessage"] = "هل أنت متأكد أنك تريد مغادرة الصفحة؟ سيتم فقدان جميع الإجابات غير المحفوظة.";
            return View();
        }

        public async Task<IActionResult> ShowResult(int examAnswerId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var results = _context.Results
                .Include(r => r.Question)
                .Include(e => e.ExamAnswer)
                .Where(r => r.ExamAnswer.UserId == userId && r.ExamAnswerId == examAnswerId)
                .Select(r => new ShowResultVM
                {
                    QuestionId = r.QuestionId,
                    QuestionText = r.Question.Content,
                    SelectedAnswer = r.Answer,
                    CorrectAnswer = r.Question.CorrectAnswer,
                    FirstAnswer = r.Question.FirstAnswer,
                    SecondAnswer = r.Question.SecondAnswer,
                    ThirdAnswer = r.Question.ThirdAnswer,
                    ForthAnswer = r.Question.ForthAnswer,
                })
                .ToList();

            var examAnswer = await _context.ExamAnswers.FindAsync(examAnswerId);
            var exam = await _context.Exams.Include(c => c.Lecture).FirstOrDefaultAsync(x => x.Id == examAnswer.ExamId);
            ViewBag.ExamName = exam?.Name;
            ViewBag.CourseId = exam.Lecture.CourseId;

            var sudtCourse = await _context.StudentCourses.Where(x => x.StudentId == userId && x.CourseId == exam.Lecture.CourseId).FirstOrDefaultAsync();
            if (sudtCourse == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(results);
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}