using ElmanhagPlatform.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElmanhagPlatform.ViewModels.CourseDetails
{
    public class CourseVM
    {
        public string teachId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateOnly CreationDate { get; set; }
        public DateOnly UpdateDate { get; set; }
        public int Price { get; set; }
        public string Year { get; set; }
        public string ImageOfCourse { get; set; }

        // إضافة معلومات إضافية 
        public int VideosCount { get; set; }
        public int FilesCount { get; set; }
        public int ExamsCount { get; set; }
        //public int AssignmentsCount { get; set; }

        public List<LectureVM> LecturesVM { get; set; }
    }
}
