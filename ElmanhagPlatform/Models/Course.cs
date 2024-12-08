using System.ComponentModel.DataAnnotations.Schema;

namespace ElmanhagPlatform.Models
{
    public class Course
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateOnly CreationDate { get; set; }
        public DateOnly UpdateDate { get; set; }
        public int Price { get; set; }
        public string Year { get; set; }
        public string ImageOfCourse { get; set; }


        [ForeignKey("TeacherId")]
        public string TeacherId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public ICollection<Lecture> Lectures { get; set; }
        public ICollection<StudentCourse> StudentCourses { get; set; }
    }
}
