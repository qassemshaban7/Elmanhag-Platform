using Microsoft.AspNetCore.Identity;

namespace ElmanhagPlatform.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public string? descreption { get; set; }
        public string? AppsPassword { get; set; }
        public long? CardNumber { get; set; }
        public string? ImageOfCard { get; set; }
        public string? MaterialName { get; set; }
        public string? Telegram { get; set; }
        public string? Facbook { get; set; }
        public string? Youtube { get; set; }
        public int ConfirmAccount { get; set; } = 0;
        public int Money { get; set; } = 0;
        public string SessionId { get; set; }
        public bool? IsPreparatory { get; set; }
        public bool? IsSecondary { get; set; }

        public int RechargedCard { get; set; } = 0;

        public ICollection<StudentCourse> StudentCourses { get; set; }

    }
}
