using System.ComponentModel.DataAnnotations.Schema;

namespace ElmanhagPlatform.Models
{
    public class Card
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateOnly ExpireAt { get; set; }
        public bool Used { get; set; }


        [ForeignKey("StudentId")]
        public string? StudentId { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }

        [ForeignKey("LectureId")]
        public int? LectureId { get; set; }
        public Lecture? Lecture { get; set; }
    }
}
