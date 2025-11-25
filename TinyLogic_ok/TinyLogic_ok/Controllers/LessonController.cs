using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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

        
        var lesson = await _context.Lessons.FindAsync(request.LessonId);
        if (lesson == null)
            return Json(new { success = false, message = "Lecția nu există în baza de date!" });

        LessonContent content = null;
        try
        {
            content = JsonConvert.DeserializeObject<LessonContent>(lesson.ContentJson);
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Eroare la citirea JSON-ului!", details = ex.Message });
        }

        if (content == null)
            return Json(new { success = false, message = "Conținutul JSON este gol sau invalid!" });

        if (content.Exercise == null)
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

        if (Normalize(output) == Normalize(expected))
        {
            

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdString, out int userId))
            {
                await _lessonProgressService.MarkLessonCompletedAsync(userId, request.LessonId);
            }
            else
            {
                return Json(new { success = false, message = "Eroare: ID-ul utilizatorului nu este valid!" });
            }

            return Json(new { success = true });
        }

       
        return Json(new
        {
            success = false,
            message = $"Expected: '{expected}', dar ai produs: '{output}'."
        });
    }
    public class CodeRequest
    {
        public string Code { get; set; }
        public int LessonId { get; set; }
    }
}
