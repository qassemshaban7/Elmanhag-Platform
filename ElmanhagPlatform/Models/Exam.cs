using System.ComponentModel.DataAnnotations.Schema;

namespace ElmanhagPlatform.Models
{
    public class Exam
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Duration { get; set; }

        [ForeignKey("LectureId")]
        public int LectureId { get; set; }
        public Lecture Lecture { get; set; }
        public ICollection<Question> Questions { get; set; }
    }
}
