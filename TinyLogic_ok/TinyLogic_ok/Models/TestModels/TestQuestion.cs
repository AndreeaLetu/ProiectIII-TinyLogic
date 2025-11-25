namespace TinyLogic_ok.Models.TestModels
{
    public class TestQuestion
    {
        public int QuestionNumber { get; set; }

        public string QuestionText { get; set; }

        public string QuestionType { get; set; } 

        public List<string> Options { get; set; }

        public string CorrectAnswer { get; set; }

        public int Points { get; set; } = 10;
    }
}
