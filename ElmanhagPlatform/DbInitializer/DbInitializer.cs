using ElmanhagPlatform.Data;
using ElmanhagPlatform.Models;
using ElmanhagPlatform.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElmanhagPlatform.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _db;

        public DbInitializer(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            AppDbContext db)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _db = db;
        }


        public async Task InitializeAsync()
        {
            if (_db.Database.GetPendingMigrations().Any())
            {
                await _db.Database.MigrateAsync();
            }

            if (!await _roleManager.RoleExistsAsync(StaticDetails.Student))
            {
                await _roleManager.CreateAsync(new IdentityRole(StaticDetails.Admin));
                await _roleManager.CreateAsync(new IdentityRole(StaticDetails.Teacher));
                await _roleManager.CreateAsync(new IdentityRole(StaticDetails.Student));

                var adminUser = new ApplicationUser
                {
                    Id = "ecc07b18-f55e-4f6b-95bd-0e84f556135f",
                    EmailConfirmed = true,
                    FullName = "الادمن",
                    UserName = "01150799451",
                    NormalizedUserName = "01150799451",
                    Email = "Admin@gmail.com",
                    ConfirmAccount = 3,
                    PhoneNumber = "01050799451",
                };

                await _userManager.CreateAsync(adminUser, "123456");
                await _userManager.AddToRoleAsync(adminUser, StaticDetails.Admin);

            }
        }

    }
}
