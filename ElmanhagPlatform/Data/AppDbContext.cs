using ElmanhagPlatform.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Reflection.Emit;

namespace ElmanhagPlatform.Data
{
    public class AppDbContext : IdentityDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Lecture> Lectures { get; set; }
        public DbSet<Video> Videos { get; set; }
        public DbSet<PFile> PFiles { get; set; }
        public DbSet<Exam> Exams { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<VideoWatchLog> VideoWatchLogs { get; set; }
        public DbSet<ExamAnswer> ExamAnswers { get; set; }
        public DbSet<Result> Results { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<StudentCourse> StudentCourses { get; set; }
        public DbSet<Card> Cards { get; set; }
        public DbSet<Config> Configs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Config>().HasData(new Config
            {
                Id = 1,
                Url = "Not Add"
            });

            base.OnModelCreating(builder);

            var dateOnlyConverter = new ValueConverter<DateOnly, DateTime>(
               d => d.ToDateTime(TimeOnly.MinValue),
               d => DateOnly.FromDateTime(d)
           );

            builder.Entity<Course>()
                .Property(e => e.CreationDate)
                .HasConversion(dateOnlyConverter);

            builder.Entity<Course>()
                .Property(e => e.UpdateDate)
                .HasConversion(dateOnlyConverter);

            builder.Entity<Card>()
                .Property(e => e.ExpireAt)
                .HasConversion(dateOnlyConverter);
        }
    }
}