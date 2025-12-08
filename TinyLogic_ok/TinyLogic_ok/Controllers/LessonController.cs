using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using QuestPDF.Fluent;
using System.Security.Claims;
using TinyLogic_ok.Models;
using TinyLogic_ok.Models.LessonModels;
using TinyLogic_ok.Services;

public class LessonsController : Controller
{
    private readonly TinyLogicDbContext _context;
    private readonly IPythonRunner _pythonRunner;
    private readonly ILessonProgressService _lessonProgressService;

    public LessonsController(
        TinyLogicDbContext context,
        IPythonRunner pythonRunner,
        ILessonProgressService lessonProgressService)
    {
        _context = context;
        _pythonRunner = pythonRunner;
        _lessonProgressService = lessonProgressService;
    }

    [HttpPost]
    public async Task<IActionResult> CheckPython([FromBody] CodeRequest request)
    {
        if (request == null)
            return Json(new { success = false, message = "Request lipsă!" });

        if (request.LessonId <= 0)
            return Json(new { success = false, message = "LessonId lipsă!" });


        string output = "";
        try
        {
            output = _pythonRunner.Run(request.Code)?.Trim() ?? "";
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Eroare la rularea codului Python!", details = ex.Message });
        }

        var lesson = await _context.Lessons
            .Include(l => l.Course)
            .FirstOrDefaultAsync(l => l.IdLesson == request.LessonId);

        if (lesson == null)
            return Json(new { success = false, message = "Lecția nu există în baza de date!" });

        LessonContent content;
        try
        {
            content = JsonConvert.DeserializeObject<LessonContent>(lesson.ContentJson);
        }
        catch
        {
            return Json(new { success = false, message = "Eroare la citirea JSON-ului!" });
        }

        if (content?.Exercise == null)
            return Json(new { success = false, message = "Exercițiul nu este definit în JSON!" });

        string expected = content.Exercise.ExpectedOutput?.Trim() ?? "";
        output = string.Join("\n", output.Split('\n').Select(line => line.Trim()));

        string Normalize(string s) =>
            (s ?? "")
            .ToLower()
            .Replace("ă", "a").Replace("â", "a").Replace("î", "i")
            .Replace("ș", "s").Replace("ş", "s")
            .Replace("ț", "t").Replace("ţ", "t")
            .Trim();

        if (Normalize(output) != Normalize(expected))
        {
            return Json(new
            {
                success = false,
                message = $"Expected: '{expected}', dar ai produs: '{output}'."
            });
        }


        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdString, out int userId))
            return Json(new { success = false, message = "User ID invalid!" });

        await _lessonProgressService.MarkLessonCompletedAsync(userId, request.LessonId);


        int courseId = lesson.CourseId;

        int totalLessons = await _context.Lessons
            .CountAsync(l => l.CourseId == courseId);

        int doneLessons = await _context.UserLessons
            .CountAsync(lp =>
                lp.UserId == userId &&
                lp.IsCompleted &&
                lp.Lesson.CourseId == courseId);

        if (doneLessons == totalLessons)
        {

            await GenerateCertificateIfNotExists(userId, courseId);
        }

        return Json(new { success = true });
    }
    private async Task GenerateCertificateIfNotExists(int userId, int courseId)
    {
        var exists = await _context.Certificates
            .AnyAsync(c => c.UserId == userId && c.CourseId == courseId);

        if (exists) return; 

        var user = await _context.Users.FindAsync(userId);
        var course = await _context.Courses.FindAsync(courseId);

        var doc = new DiplomaDocument(user, course);
        byte[] pdf = doc.GeneratePdf();

        string folder = Path.Combine("wwwroot", "certificates");
        Directory.CreateDirectory(folder);

        string fileName = $"Diploma_{course.CourseName}_{user.FirstName}_{user.LastName}.pdf";
        string full = Path.Combine(folder, fileName);
        System.IO.File.WriteAllBytes(full, pdf);

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
    }

    public class CodeRequest
    {
        public string Code { get; set; }
        public int LessonId { get; set; }
    }

    [HttpGet]
    public IActionResult DownloadPdf(int courseId)
    {
        var course = _context.Courses.FirstOrDefault(c => c.CourseId == courseId);
        if (course == null) return NotFound();

        var lessons = _context.Lessons
            .Where(l => l.CourseId == courseId)
            .OrderBy(l => l.OrderIndex)
            .ToList();

        var parsed = lessons
            .Select(l => (l, LessonJsonParser.Parse(l.ContentJson)))
            .ToList();

        var document = new LessonPdfDocument(course.CourseName, parsed);
        byte[] pdf = document.GeneratePdf();

        return File(pdf, "application/pdf", $"{course.CourseName}_Complet.pdf");
    }

}
