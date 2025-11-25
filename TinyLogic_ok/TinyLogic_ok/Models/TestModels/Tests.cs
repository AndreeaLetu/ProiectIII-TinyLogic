using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TinyLogic_ok.Models.CourseModels;

namespace TinyLogic_ok.Models.TestModels
{
    public class Tests
    {
        [Key]
        public int IdTest { get; set; }

        public string TestName { get; set; }

        public string Description { get; set; }

        public string TestJson { get; set; }

        public int PassingScore { get; set; } = 70;

        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        public Courses Course { get; set; }

        public ICollection<TestProgress> TestProgresses { get; set; } = new List<TestProgress>();
    }
}
