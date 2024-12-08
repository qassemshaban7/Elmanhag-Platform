using ElmanhagPlatform.Data;
using ElmanhagPlatform.Services;
using ElmanhagPlatform.Utility;
using ElmanhagPlatform.ViewModels;
using ElmanhagPlatform.ViewModels.StudentVM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace ElmanhagPlatform.Controllers
{
    public class UserController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IEmailProvider _emailProvider;

        public UserController(AppDbContext context, IWebHostEnvironment hostingEnvironment
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

                var users = await (from x in _context.ApplicationUsers
                                   join userRole in _context.UserRoles
                                   on x.Id equals userRole.UserId
                                   join role in _context.Roles
                                   on userRole.RoleId equals role.Id
                                   where role.Name == StaticDetails.Student
                                   select x)
                                   .Where(u => u.ConfirmAccount == 1)
                                   .ToListAsync();

                return View(users);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Confirm(string? Id)
        {
            if (User.IsInRole("Admin"))
            {
                try
                {
                    if (Id == null)
                        return RedirectToAction(nameof(Index));

                    var service = await _context.ApplicationUsers.FirstOrDefaultAsync(m => m.Id == Id);

                    if (service == null)
                        return RedirectToAction(nameof(Index));

                    service.ConfirmAccount = 3;
                    await _context.SaveChangesAsync();
                    HttpContext.Session.SetString("updated", "true");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> SendEmail(string? Id, string Descreption)
        {
            if (User.IsInRole("Admin"))
            {
                try
                {
                    if (Id == null)
                        return NotFound();
                    var service = await _context.ApplicationUsers.FirstOrDefaultAsync(m => m.Id == Id);
                    if (service == null)
                        return NotFound();
                    service.descreption = Descreption;
                    service.ConfirmAccount = 2;
                    await _context.SaveChangesAsync();

                    //await _emailProvider.SendMail(Id, Descreption);
                    HttpContext.Session.SetString("deleted", "true");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> EditMyAccount(string id)
        {
            if (User.IsInRole("Admin"))
            {
                if (id == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                var user = await _context.ApplicationUsers
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (user == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                var config = await _context.Configs.FindAsync(1);

                var editUserVM = new EditAdminVM
                {
                    Id = id,
                    FullName = user.FullName,
                    UserName = user.UserName,
                    Email = user.Email,
                    AppsPassword = user.AppsPassword,
                    Url = config.Url,
                };

                return View(editUserVM);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMyAccount(string id, EditAdminVM editUserVM)
        {
            if (User.IsInRole("Admin"))
            {
                try
                {
                    var user = await _context.ApplicationUsers
                       .FirstOrDefaultAsync(d => d.Id == id);
                    var config = await _context.Configs.FindAsync(1);
                    if (user == null || config == null)
                    {
                        return RedirectToAction("Index", "Home");
                    }

                    user.FullName = editUserVM.FullName;
                    user.UserName = editUserVM.UserName;
                    user.Email = editUserVM.Email;
                    
                    var url = editUserVM.Url;
                    var videoId = url.Split('/').Last().Split('?').First();
                    config.Url = videoId;

                    if (user.Id == "ecc07b18-f55e-4f6b-95bd-0e84f556135f")
                    {
                        user.AppsPassword = editUserVM.AppsPassword;
                    }

                    if (editUserVM.Password != null)
                    {
                        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                        var result2 = await _userManager.ResetPasswordAsync(user, token, editUserVM.Password);
                        await _userManager.UpdateAsync(user);
                    }

                    _context.Update(user);
                    _context.Update(config);
                    await _context.SaveChangesAsync();
                    HttpContext.Session.SetString("updated", "true");
                    return RedirectToAction("Index", "Home");
                }
                catch
                {
                    return View(editUserVM);
                }
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }


        public async Task<IActionResult> Student()
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

                var users = await (from x in _context.ApplicationUsers
                                   join userRole in _context.UserRoles
                                   on x.Id equals userRole.UserId
                                   join role in _context.Roles
                                   on userRole.RoleId equals role.Id
                                   where role.Name == StaticDetails.Student
                                   select x)
                                   .Where(u => u.ConfirmAccount == 3)
                                   .ToListAsync();

                return View(users);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
        
        public async Task<IActionResult> EditStudent(string id)
        {
            if (User.IsInRole("Admin"))
            {
                if (id == null)
                {
                    return RedirectToAction("Student", "User");
                }

                var user = await _context.ApplicationUsers
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (user == null)
                {
                    return RedirectToAction("Student", "User");
                }

                if (user.ConfirmAccount == 3)
                {
                    var editUserVM = new EditStudentFromAdminVM
                    {
                        Id = id,
                        FullName = user.FullName,
                        UserName = user.UserName,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        CardNumber = user.CardNumber,
                        ReachargedCardNum = user.RechargedCard,
                    };
                    return View(editUserVM);
                }
                else
                {
                    return RedirectToAction("Student", "User");
                }
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStudent(string id, EditStudentFromAdminVM editUserVM)
        {
            if (User.IsInRole("Admin"))
            {
                try
                {
                    var user = await _context.ApplicationUsers
                       .FirstOrDefaultAsync(d => d.Id == id);
                    if (user == null)
                    {
                        return RedirectToAction("Student", "User");
                    }

                    if (user.ConfirmAccount == 3)
                    {
                        user.FullName = editUserVM.FullName;
                        user.UserName = editUserVM.UserName;
                        user.Email = editUserVM.Email;
                        user.PhoneNumber = editUserVM.PhoneNumber;
                        user.CardNumber = editUserVM.CardNumber;
                        user.RechargedCard = editUserVM.ReachargedCardNum;

                        string oldImageFileName = user.ImageOfCard;
                        string Oldd = user.ImageOfCard;


                        if (editUserVM.Image != null)
                        {
                            string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "images");

                            string[] allowedExtensions = { ".png", ".jpg", ".jpeg" };
                            if (!allowedExtensions.Contains(Path.GetExtension(editUserVM.Image.FileName).ToLower()))
                            {
                                TempData["ErrorMessage"] = "Only .png and .jpg and .jpeg images are allowed!";
                                return RedirectToAction("Index");
                            }

                            string uniqueFileName = Guid.NewGuid().ToString() + "_" + editUserVM.Image.FileName;
                            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                            await using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await editUserVM.Image.CopyToAsync(fileStream);
                            }

                            if (!string.IsNullOrEmpty(oldImageFileName))
                            {
                                string oldFilePath = Path.Combine(uploadsFolder, oldImageFileName);
                                if (System.IO.File.Exists(oldFilePath))
                                {
                                    System.IO.File.Delete(oldFilePath);
                                }
                            }
                            user.ImageOfCard = uniqueFileName;
                        }
                        else
                        {
                            user.ImageOfCard = Oldd;
                        }

                        if (editUserVM.Password != null)
                        {
                            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                            var result2 = await _userManager.ResetPasswordAsync(user, token, editUserVM.Password);
                            await _userManager.UpdateAsync(user);
                        }

                        _context.Update(user);
                        await _context.SaveChangesAsync();
                        //HttpContext.Session.SetString("updated", "true");
                        return RedirectToAction("Student", "User");
                    }
                }
                catch
                {
                    return RedirectToAction("Student", "User");
                }
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (User.IsInRole("Admin"))
            {
                if (id == null)
                {
                    return NotFound();
                }

                var stu = await _context.ApplicationUsers.FindAsync(id);

                string oldImageFileName = stu.ImageOfCard;
                string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "images");
                if (!string.IsNullOrEmpty(oldImageFileName))
                {
                    string oldFilePath = Path.Combine(uploadsFolder, oldImageFileName);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                var answers = await _context.ExamAnswers.Where(x => x.UserId == stu.Id).ToListAsync();

                _context.ExamAnswers.RemoveRange(answers);

                _context.ApplicationUsers.Remove(stu);

                await _context.SaveChangesAsync();
                HttpContext.Session.SetString("deleted", "true");
                return RedirectToAction("Student", "User");

            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

    }
}