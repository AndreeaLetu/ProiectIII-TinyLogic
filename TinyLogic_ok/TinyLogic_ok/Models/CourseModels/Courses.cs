/* Exista mai multe cursuri cu mai multe lectii 
 * -> legatura intre cursuri si lectii */


using System.ComponentModel.DataAnnotations;
using TinyLogic_ok.Models.LessonModels;  // <--- OBLIGATORIU
using TinyLogic_ok.Models.TestModels;

namespace TinyLogic_ok.Models.CourseModels
{
    public class Courses
    {
        [Key] public int CourseId { get; set; }
        public string CourseName { get; set; }

        public string Description { get; set; }
        public string Difficulty { get; set; }

        // Relații
        public ICollection<Lessons> Lessons { get; set; } = new List<Lessons>();

        public List<Tests> Tests { get; set; }

    }
}
