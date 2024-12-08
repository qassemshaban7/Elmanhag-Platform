// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using ElmanhagPlatform.Models;
using ElmanhagPlatform.Data;
using Microsoft.EntityFrameworkCore;
using Azure;
using Microsoft.AspNetCore.Routing;
using System.Runtime.InteropServices.JavaScript;

namespace ElmanhagPlatform.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly AppDbContext _context;

        public LoginModel(SignInManager<IdentityUser> signInManager,
                          ILogger<LoginModel> logger,
                          UserManager<IdentityUser> userManager,
                          AppDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _context = context;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            [Required(ErrorMessage = "رقم الهاتف  مطلوب")]
            public string PhoneNumber { get; set; }

            [Required(ErrorMessage = "الرقم السري مطلوب")]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            public string DeviceFingerprint { get; set; }

            [Display(Name = "تذكرني")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            try
            {
                var user = await _context.ApplicationUsers.FirstOrDefaultAsync(x => x.UserName == Input.PhoneNumber);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "خطأ في رقم الهاتف أو الرقم السري");
                    return Page();
                }

                var currentSessionId = HttpContext.Session.Id;
                if (string.IsNullOrEmpty(user.SessionId) || user.SessionId != currentSessionId)
                {
                    if (!string.IsNullOrEmpty(user.SessionId) && user.SessionId != currentSessionId)
                    {
                        await _userManager.UpdateSecurityStampAsync(user);
                    }

                    user.SessionId = currentSessionId;
                    await _context.SaveChangesAsync();
                }

                var result = await _signInManager.PasswordSignInAsync(Input.PhoneNumber, Input.Password, Input.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded && user.ConfirmAccount == 2) 
                {
                    _logger.LogInformation("User logged in.");
                    return RedirectToAction("Edit", "Student", new { id = user.Id });
                }
                if (result.Succeeded && user.ConfirmAccount == 1)
                {
                    TempData["AlertMessage"] = "لم يتم تأكيد حسابك بعد.";
                    await _signInManager.SignOutAsync();
                    return RedirectToPage("/Account/Login", new { area = "Identity" });
                }
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    return RedirectToAction(nameof(Controllers.HomeController.Index),
                                            nameof(Controllers.HomeController).Replace("Controller", ""));
                }

                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "خطأ في رقم الهاتف أو الرقم السري");
                    return Page();
                }
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "حدث خطأ أثناء محاولة تسجيل الدخول");
                return Page();
            }
        }
    
    }
}