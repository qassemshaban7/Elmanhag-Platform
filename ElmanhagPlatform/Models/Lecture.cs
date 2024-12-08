using System.ComponentModel.DataAnnotations.Schema;

namespace ElmanhagPlatform.Models
{
    public class Lecture
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }

        [ForeignKey("CourseId")]
        public int CourseId { get; set; }
        public Course Course { get; set; }

        public ICollection<Video> Videos { get; set; }
        public ICollection<PFile> PFiles { get; set; }
        public ICollection<Exam> Exams{ get; set; } 

        public ICollection<Card> Cards{ get; set; }

    }
}
