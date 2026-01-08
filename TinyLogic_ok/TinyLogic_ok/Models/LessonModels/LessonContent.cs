namespace TinyLogic_ok.Models.LessonModels
{
    public class LessonContent
    {
        public string Title { get; set; }

        public List<LessonSection> Sections { get; set; } = new();

        public LessonExercise Exercise { get; set; }
    }

    public class LessonSection
    {
        public string Heading { get; set; }
        public string Text { get; set; }
    }

    public class LessonExercise
    {
        public string Description { get; set; }

        public string ExpectedOutput { get; set; }

     
        public List<string> ExpectedBlocks { get; set; } = new();
    }
}
