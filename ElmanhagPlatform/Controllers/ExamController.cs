using ElmanhagPlatform.Data;
using ElmanhagPlatform.Models;
using ElmanhagPlatform.Services;
using ElmanhagPlatform.ViewModels.ExamVM;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ElmanhagPlatform.Controllers
{
    public class ExamController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IEmailProvider _emailProvider;

        public ExamController(AppDbContext context, IWebHostEnvironment hostingEnvironment
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

                var exames = await _context.Exams.Where(u => u.LectureId == id && u.Lecture.Course.TeacherId == teacherId).ToListAsync();

                ViewBag.LectureId = id;

                var lec = await _context.Lectures.FindAsync(id);
                ViewBag.CourseId = lec.CourseId;
                ViewBag.LectureId = id;

                return View(exames);
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
                var model = new AddExamVM
                {
                    Questions = new List<QuestionVM>()
                };
                return View(model);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int id, AddExamVM model, string? returnUrl = null)
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

                    var course = await _context.Courses.FindAsync(lecture.CourseId);
                    if (course.TeacherId != teacherId)
                    {
                        return RedirectToAction("Index", "Home");
                    }

                    var exam = new Exam
                    {
                        Name = model.Name,
                        Duration = model.Duration,
                        Description = model.Description,
                        LectureId = id,
                    };

                    _context.Exams.Add(exam);
                    await _context.SaveChangesAsync();

                    var questions = model.Questions.Select(q => new Question
                    {
                        Content = q.Content,
                        FirstAnswer = q.FirstAnswer,
                        SecondAnswer = q.SecondAnswer,
                        ThirdAnswer = q.ThirdAnswer,
                        ForthAnswer = q.ForthAnswer,
                        CorrectAnswer = q.CorrectAnswer,
                        ExamId = exam.Id
                    }).ToList();

                    _context.Questions.AddRange(questions);
                    await _context.SaveChangesAsync();

                    course.UpdateDate = DateOnly.FromDateTime(DateTime.Now);
                    _context.Update(course);

                    await _context.SaveChangesAsync();

                    HttpContext.Session.SetString("created", "true");
                    return RedirectToAction("Index", new { id = lecture.Id });
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

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (User.IsInRole("Teacher"))
            {
                var exam = await _context.Exams.Include(e => e.Questions).FirstOrDefaultAsync(e => e.Id == id);

                if (exam == null)
                {
                    return RedirectToAction("Index", "Lecture");
                }

                var lec = await _context.Lectures.FindAsync(exam.LectureId);
                var course = await _context.Courses.FindAsync(lec.CourseId);
                var teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (course.TeacherId != teacherId)
                {
                    return RedirectToAction("Index", "Home");
                }

                var model = new AddExamVM
                {
                    Name = exam.Name,
                    Duration = exam.Duration,
                    Description = exam.Description,
                    Questions = exam.Questions.Select(q => new QuestionVM
                    {
                        Content = q.Content,
                        FirstAnswer = q.FirstAnswer,
                        SecondAnswer = q.SecondAnswer,
                        ThirdAnswer = q.ThirdAnswer,
                        ForthAnswer = q.ForthAnswer,
                        CorrectAnswer = q.CorrectAnswer
                    }).ToList()
                };

                ViewBag.Lectures = lec;
                return View(model);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AddExamVM model)
        {
            if (User.IsInRole("Teacher"))
            {
                try
                {
                    var teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (teacherId == null)
                    {
                        return RedirectToAction("Index", "Home");
                    }

                    var exam = await _context.Exams.Include(e => e.Questions).FirstOrDefaultAsync(e => e.Id == id);
                    if (exam == null)
                    {
                        return RedirectToAction("Index", "Lecture");
                    }

                    var lecture = await _context.Lectures.FindAsync(exam.LectureId);

                    ViewBag.Lectures = lecture;
                    var course = await _context.Courses.FindAsync(lecture.CourseId);
                    if (course.TeacherId != teacherId)
                    {
                        return RedirectToAction("Index", "Home");
                    }

                    exam.Name = model.Name;
                    exam.Duration = model.Duration;
                    exam.Description = model.Description;

                    var anss = await _context.ExamAnswers.Where(e => e.ExamId == exam.Id).ToListAsync();
                    //var res = await _context.Results
                    //    .Include(r => r.ExamAnswer)
                    //    .Where(r => r.ExamAnswer.ExamId == exam.Id)
                    //    .ToListAsync();
                    _context.ExamAnswers.RemoveRange(anss);

                    var existingQuestions = exam.Questions.ToList();
                    _context.Questions.RemoveRange(existingQuestions);

                    var questions = model.Questions.Select(q => new Question
                    {
                        Content = q.Content,
                        FirstAnswer = q.FirstAnswer,
                        SecondAnswer = q.SecondAnswer,
                        ThirdAnswer = q.ThirdAnswer,
                        ForthAnswer = q.ForthAnswer,
                        CorrectAnswer = q.CorrectAnswer,
                        ExamId = exam.Id
                    }).ToList();

                    _context.Questions.AddRange(questions);
                    await _context.SaveChangesAsync();

                    course.UpdateDate = DateOnly.FromDateTime(DateTime.Now);
                    _context.Update(course);
                    await _context.SaveChangesAsync();

                    HttpContext.Session.SetString("updated", "true");
                    return RedirectToAction("Index", new { id = lecture.Id });
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

        public async Task<IActionResult> Delete(int id)
        {
            if (User.IsInRole("Teacher") && id != null)
            {
                var exam = await _context.Exams.FindAsync(id);
                if (exam == null)
                {
                    return NotFound();
                }

                var teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (teacherId == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                var lec = _context.Lectures.Find(id);
                var Teacher = await _context.ApplicationUsers.FindAsync(teacherId);
                var course = await _context.Courses.FindAsync(lec.CourseId);
                if (course.TeacherId != teacherId)
                {
                    return RedirectToAction("Index", "Home");
                }

                _context.Exams.Remove(exam);
                await _context.SaveChangesAsync();
                HttpContext.Session.SetString("deleted", "true");
                return RedirectToAction("Index", new { id = exam.LectureId });
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Questions(int id)
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

                var exam = await _context.Exams.FindAsync(id);
                var lec = await _context.Lectures.FindAsync(exam.LectureId) ;
                ViewBag.lectures = lec;

                var quest = await _context.Questions.Where(u => u.ExamId == id && u.Exam.Lecture.Course.TeacherId == teacherId).ToListAsync();

                ViewBag.exam = exam;
                return View(quest);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
    
    }
}
