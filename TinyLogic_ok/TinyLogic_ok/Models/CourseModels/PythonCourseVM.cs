using TinyLogic_ok.Models.LessonModels;

namespace TinyLogic_ok.Models.CourseModels
{
    public class PythonCourseVM
    {
        public Courses? Course { get; set; }
        public List<Lessons>? Lessons { get; set; }
        public Lessons? SelectedLesson { get; set; }
        public LessonContent? ParsedContent { get; set; }
    
        public List<int>? CompletedLessonIds { get; set; }

        public bool IsCourseCompleted { get; set; }
        public bool IsSelectedLessonCompleted { get; set; }

        public int HighestCompletedOrder { get; set; }



    }

}
