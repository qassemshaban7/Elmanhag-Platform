using System.ComponentModel.DataAnnotations;

namespace ElmanhagPlatform.ViewModels
{
    public class EditCourseVM
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public int Price { get; set; }
        [Required]
        public string Year { get; set; }
        public IFormFile? ImageOfCourse { get; set; }
    }
}
