using ElmanhagPlatform.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElmanhagPlatform.ViewModels.CourseDetails
{
    public class PFileVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string FilePath { get; set; }
    }
}
