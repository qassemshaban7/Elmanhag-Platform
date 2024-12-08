using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElmanhagPlatform.ViewModels
{
    public class CreateCourseVM
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public int Price { get; set; }
        [Required]
        public string Year { get; set; }
        [Required]
        public IFormFile ImageOfCourse { get; set; }
    }
}
