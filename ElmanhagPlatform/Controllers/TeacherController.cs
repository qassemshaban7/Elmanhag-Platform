using ElmanhagPlatform.Data;
using ElmanhagPlatform.Models;
using ElmanhagPlatform.Services;
using ElmanhagPlatform.Utility;
using ElmanhagPlatform.ViewModels.StudentVM;
using ElmanhagPlatform.ViewModels.TeacherVM;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElmanhagPlatform.Controllers
{
    public class TeacherController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IEmailProvider _emailProvider;

        public TeacherController(AppDbContext context, IWebHostEnvironment hostingEnvironment
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
                                   where role.Name == StaticDetails.Teacher
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
        public async Task<IActionResult> Create(CreateTeacherVM model, IFormFile Image, string? returnUrl = null)
        {
            if (User.IsInRole("Admin"))
            {
                ViewData["ReturnUrl"] = returnUrl;
                if (ModelState.IsValid)
                {
                    ApplicationUser user = new()
                    {
                        FullName = model.FullName,
                        UserName = model.UserName,
                        EmailConfirmed = true,
                        PhoneNumber = model.PhoneNumber,
                        ConfirmAccount = 3,
                        Email = model.Email,
                        MaterialName = model.MaterialName,
                        descreption = model.descreption,
                        IsPreparatory = model.Preparatory,
                        IsSecondary = model.Secondry
                    };

                    if (model.Image != null)
                    {
                        string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "images");

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
                        user.ImageOfCard = uniqueFileName;
                    }

                    var result = await _userManager.CreateAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, StaticDetails.Teacher);
                        await _context.SaveChangesAsync();

                        HttpContext.Session.SetString("created", "true");
                        return RedirectToAction("Index", "Teacher");
                    }
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
                return View(model);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (User.IsInRole("Admin"))
            {
                if (id == null)
                {
                    return RedirectToAction("Index", "Teacher");
                }

                var user = await _context.ApplicationUsers
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (user == null)
                {
                    return RedirectToAction("Index", "Teacher");
                }

                if (user.ConfirmAccount == 3)
                {
                    var editUserVM = new EditTeacherVM
                    {
                        Id = id,
                        FullName = user.FullName,
                        UserName = user.UserName,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        MaterialName = user.MaterialName,
                        descreption = user.descreption,
                        Preparatory = user.IsPreparatory ?? false,
                        Secondry = user.IsSecondary ?? false,    
                    };
                    return View(editUserVM);
                }
                else
                {
                    return RedirectToAction("Index", "Teacher");
                }
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, EditTeacherVM editUserVM)
        {
            if (User.IsInRole("Admin"))
            {
                try
                {
                    var user = await _context.ApplicationUsers
                       .FirstOrDefaultAsync(d => d.Id == id);
                    if (user == null)
                    {
                        return RedirectToAction("Index", "Teacher");
                    }

                    if (user.ConfirmAccount == 3)
                    {
                        user.FullName = editUserVM.FullName;
                        user.UserName = editUserVM.UserName;
                        user.Email = editUserVM.Email;
                        user.PhoneNumber = editUserVM.PhoneNumber;
                        user.MaterialName = editUserVM.MaterialName;
                        user.descreption = editUserVM.descreption;
                        user.IsPreparatory = editUserVM.Preparatory;
                        user.IsSecondary = editUserVM.Secondry;

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
                        HttpContext.Session.SetString("updated", "true");
                        return RedirectToAction("Index", "Teacher");
                    }
                }
                catch
                {
                    return RedirectToAction("Index", "Teacher");
                }
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
            return View(editUserVM);
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (User.IsInRole("Admin"))
            {
                if (id == null)
                {
                    return NotFound();
                }

                var dept = await _context.ApplicationUsers.FindAsync(id);

                string oldImageFileName = dept.ImageOfCard;
                string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "images");
                if (!string.IsNullOrEmpty(oldImageFileName))
                {
                    string oldFilePath = Path.Combine(uploadsFolder, oldImageFileName);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                _context.ApplicationUsers.Remove(dept);
                await _context.SaveChangesAsync();
                HttpContext.Session.SetString("deleted", "true");
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return RedirectToAction("Index", "Teacher");
            }
        }

    }
}
