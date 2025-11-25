using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using TinyLogic_ok.Models;
using TinyLogic_ok.Models.CourseModels;
using TinyLogic_ok.Models.LessonModels;

[Authorize]
public class CertificatesController : Controller
{
    private readonly TinyLogicDbContext _context;
    private readonly UserManager<User> _userManager;

    public CertificatesController(
        TinyLogicDbContext context,
        UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;

        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<IActionResult> Generate(int courseId)
    {
        int userId = int.Parse(_userManager.GetUserId(User));

        var existingCert = await _context.Certificates
            .FirstOrDefaultAsync(c => c.UserId == userId && c.CourseId == courseId);

        if (existingCert != null)
        {
           
            var pdfBytes = System.IO.File.ReadAllBytes("wwwroot" + existingCert.PdfPath);
            return File(pdfBytes, "application/pdf", Path.GetFileName(existingCert.PdfPath));
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());
        var course = await _context.Courses.FindAsync(courseId);

  
        int totalLessons = await _context.Lessons.CountAsync(l => l.CourseId == courseId);
        int done = await _context.UserLessons.CountAsync(lp =>
            lp.UserId == userId && lp.IsCompleted && lp.Lesson.CourseId == courseId);

        if (done != totalLessons)
            return BadRequest("Cursul nu este finalizat.");

        var doc = new DiplomaDocument(user, course);
        byte[] pdf = doc.GeneratePdf();

        string fileName = $"Diploma_{course.CourseName}_{user.FirstName}_{user.LastName}.pdf";
        string folderPath = Path.Combine("wwwroot", "certificates");
        Directory.CreateDirectory(folderPath);

        string fullPath = Path.Combine(folderPath, fileName);
        System.IO.File.WriteAllBytes(fullPath, pdf);

        var cert = new Certificate
        {
            UserId = userId,
            CourseId = courseId,
            DateGenerated = DateTime.UtcNow,
            CertificateCode = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper(),
            PdfPath = "/certificates/" + fileName
        };

        _context.Certificates.Add(cert);
        await _context.SaveChangesAsync();

        return File(pdf, "application/pdf", fileName);
    }
    public async Task<IActionResult> Index()
    {
        int userId = int.Parse(_userManager.GetUserId(User));

        var certs = await _context.Certificates
            .Include(c => c.Course)
            .Where(c => c.UserId == userId)
            .ToListAsync();

        return View(certs);
    }

}
