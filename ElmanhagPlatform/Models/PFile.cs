using System.ComponentModel.DataAnnotations.Schema;

namespace ElmanhagPlatform.Models
{
    public class PFile
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string FilePath { get; set; }

        [ForeignKey("LectureId")]
        public int LectureId { get; set; }
        public Lecture Lecture { get; set; }
    }
}
