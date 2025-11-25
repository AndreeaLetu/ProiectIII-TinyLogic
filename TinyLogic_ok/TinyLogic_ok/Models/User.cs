using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using TinyLogic_ok.Models.LessonModels;
using TinyLogic_ok.Models.TestModels;

namespace TinyLogic_ok.Models
{
    public class User : IdentityUser<int>
    {
        public string? LastName { get; set; }
        public string? FirstName { get; set; }
        public byte[]? Photo { get; set; }

        public string? Role { get; set; }

        public DateTime? BirthDate { get; set; }

        public ICollection<UserLessons>? LessonsProgress { get; set; } = new List<UserLessons>();
        public ICollection<TestProgress> TestsProgress { get; set; } = new List<TestProgress>();



    }
}
