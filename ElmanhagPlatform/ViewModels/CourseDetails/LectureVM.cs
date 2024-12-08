using ElmanhagPlatform.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElmanhagPlatform.ViewModels.CourseDetails
{
    public class LectureVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public int CourseId { get; set; }
        
        public List<VideoVM> VideosVM { get; set; }
        public List<PFileVM> PFilesVM { get; set; }
        public List<ExamVM> ExamsVM { get; set; }
    }
}
