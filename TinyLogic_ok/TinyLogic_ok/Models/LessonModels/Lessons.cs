using System.ComponentModel.DataAnnotations;
using TinyLogic_ok.Models.CourseModels;
namespace TinyLogic_ok.Models.LessonModels
{
    public class Lessons
    {
        [Key] public int IdLesson { get; set; }

        public string LessonName { get; set; }
        public string Description { get; set; }
        public string? PdfPath { get; set; }
        public int OrderIndex { get; set; }
        public string ContentJson { get; set; }

        public int CourseId { get; set; }
        public Courses Course { get; set; }
      
        public ICollection<UserLessons> UserProgress { get; set; } = new List<UserLessons>();


    }
}
