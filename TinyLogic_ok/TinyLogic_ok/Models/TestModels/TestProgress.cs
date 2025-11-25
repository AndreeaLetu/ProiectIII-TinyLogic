using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TinyLogic_ok.Models.TestModels
{
    public class TestProgress
    {
        [Key]
        public int IdTestProgress { get; set; }

        public int UserId { get; set; }

        public User User { get; set; }

        [Required]
        public int TestId { get; set; }

        [ForeignKey("TestId")]
        public Tests Test { get; set; }

        public int Score { get; set; }

        public bool IsPassed { get; set; }

        public DateTime CompletedAt { get; set; }

        public string AnswersJson { get; set; }
    }
}
