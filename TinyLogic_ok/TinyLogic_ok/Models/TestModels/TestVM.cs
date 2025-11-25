using TinyLogic_ok.Models.CourseModels;

namespace TinyLogic_ok.Models.TestModels
{
    public class TestVM
    {
        public Tests Test { get; set; }
        public TestContent ParsedContent { get; set; }

        public bool IsLocked { get; set; }
        public bool IsCompleted { get; set; }
        public int? LastScore { get; set; }

        public Courses RequiredCourse { get; set; }
        public int CompletedLessons { get; set; }
        public int TotalLessons { get; set; }
    }
}
