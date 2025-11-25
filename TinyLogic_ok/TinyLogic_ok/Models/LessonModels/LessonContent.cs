namespace TinyLogic_ok.Models.LessonModels
{
    public class LessonContent
    {
        public string Title { get; set; }
        public List<LessonSection> Sections { get; set; }
        public LessonExercise Exercise { get; set; }
    }

}
