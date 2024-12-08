using ElmanhagPlatform.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElmanhagPlatform.ViewModels.CourseDetails
{
    public class ExamVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Duration { get; set; }

        public ICollection<Question> Questions { get; set; }
    }
}
