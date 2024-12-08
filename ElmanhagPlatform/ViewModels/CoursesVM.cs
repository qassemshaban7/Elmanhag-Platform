using ElmanhagPlatform.Models;

namespace ElmanhagPlatform.ViewModels
{
    public class CoursesVM
    {
        public int Id { get; set; }
        public ApplicationUser Teacher { get; set; }
        public List<Course> Courses { get; set; }
        public string Year { get; set; }
    }
}
