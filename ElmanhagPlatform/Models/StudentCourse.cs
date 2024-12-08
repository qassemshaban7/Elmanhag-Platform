using System.ComponentModel.DataAnnotations.Schema;

namespace ElmanhagPlatform.Models
{
    public class StudentCourse
    {
        public int Id { get; set; }

        [ForeignKey("StudentId")]
        public string StudentId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        [ForeignKey("CourseId")]
        public int CourseId { get; set; }
        public Course Course { get; set; }
    }
}
