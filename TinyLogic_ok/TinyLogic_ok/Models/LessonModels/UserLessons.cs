using System.ComponentModel.DataAnnotations;

namespace TinyLogic_ok.Models.LessonModels
{
    public class UserLessons
    {
        [Key] public int IdUserLesson { get; set; }

        
        public int UserId { get; set; }
        public User User { get; set; }

       
        public int LessonId { get; set; }
        public Lessons Lesson { get; set; }

        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }



    }
}
