using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TinyLogic_ok.Models.CourseModels;
using TinyLogic_ok.Models.CourseModels;

namespace TinyLogic_ok.Models
{
    public class Certificate
    {
        [Key]
        public int CertificateId { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }

        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        public Courses Course { get; set; }

        
        public DateTime DateGenerated { get; set; } = DateTime.Now;
        public string PdfPath { get; set; }
        public string CertificateCode { get; set; }
    }
}
