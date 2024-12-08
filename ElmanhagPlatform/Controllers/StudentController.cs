using ElmanhagPlatform.Data;
using ElmanhagPlatform.Models;
using ElmanhagPlatform.Utility;
using ElmanhagPlatform.ViewModels.StudentVM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ElmanhagPlatform.Controllers
{
    public class StudentController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public StudentController(AppDbContext context, IWebHostEnvironment hostingEnvironment
            , UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> Create(string? returnUrl = null)
        {
            if (!_signInManager.IsSignedIn(User))
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
        public async Task<IActionResult> Create(CreateStudentVM model, IFormFile Image, string? returnUrl = null)
        {
            if (!_signInManager.IsSignedIn(User))
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
                        CardNumber = model.CardNumber,
                        ConfirmAccount = 1,
                        Email = model.Email,
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
                        await _userManager.AddToRoleAsync(user, StaticDetails.Student);
                        await _context.SaveChangesAsync();
                        await _signInManager.SignOutAsync();

                        HttpContext.Session.SetString("created", "true");
                        return RedirectToAction("Index", "Home");
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
            ViewBag.user = user;

            if (user.ConfirmAccount == 2)
            {
                var editUserVM = new EditStudentVM
                {
                    Id = id,
                    FullName = user.FullName,
                    UserName = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    CardNumber = user.CardNumber,
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
        public async Task<IActionResult> Edit(string id, EditStudentVM editUserVM)
        {
            try
            {
                var user = await _context.ApplicationUsers
                   .FirstOrDefaultAsync(d => d.Id == id);
                if (user == null)
                {
                    await _signInManager.SignOutAsync();
                    return RedirectToAction("Index", "Home");
                }

                if (user.ConfirmAccount == 2)
                {
                    user.FullName = editUserVM.FullName;
                    user.UserName = editUserVM.UserName;
                    user.Email = editUserVM.Email;
                    user.PhoneNumber = editUserVM.PhoneNumber;
                    user.ConfirmAccount = 1;
                    user.CardNumber = editUserVM.CardNumber;

                    string oldImageFileName = user.ImageOfCard;
                    string Oldd = user.ImageOfCard;


                    if (editUserVM.Image != null)
                    {
                        string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "images");

                        string[] allowedExtensions = { ".png", ".jpg", ".jpeg" };
                        if (!allowedExtensions.Contains(Path.GetExtension(editUserVM.Image.FileName).ToLower()))
                        {
                            TempData["ErrorMessage"] = "Only .png and .jpg and .jpeg images are allowed!";
                            return RedirectToAction("Edit");
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
                    await _signInManager.SignOutAsync();
                    return RedirectToAction("Index", "Home");
                }
            }
            catch
            {
                await _signInManager.SignOutAsync();
                return RedirectToAction("Index", "Home");
            }
            return View(editUserVM);
        }
    
    }
}
