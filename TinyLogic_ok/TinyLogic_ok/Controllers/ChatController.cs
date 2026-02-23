using Microsoft.AspNetCore.Mvc;
using TinyLogic_ok.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using TinyLogic_ok.Models;

public class ChatController : Controller
{
    private readonly IRagService _rag;
    private readonly IAiService _ai;
    private readonly UserManager<User> _userManager;
    private readonly IWebHostEnvironment _env;

    public ChatController(
        IRagService rag,
        IAiService ai,
        UserManager<User> userManager,
        IWebHostEnvironment env)
    {
        _rag = rag;
        _ai = ai;
        _userManager = userManager;
        _env = env;
    }

    [HttpGet]
    [Authorize]
    public IActionResult Index()
    {
        ViewBag.UserAvatarUrl = Url.Action("Avatar", "Chat");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Send(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return Json(new { message = "", reply = "Te rog să îmi pui o întrebare!" });
        }

        // Search in knowledge base
        var rag = await _rag.SearchAsync(message);

        string finalPrompt;

        // Use RAG context if similarity is good enough
        if (rag != null && rag.Similarity >= 0.3)
        {
            Console.WriteLine($"📚 Using RAG context (similarity: {rag.Similarity:F2})");
            Console.WriteLine($"   Matched: '{rag.MatchedQuestion}'");

            finalPrompt = $@"Întrebare utilizator: {message}

Context din baza de cunoștințe (întrebare similară: ""{rag.MatchedQuestion}""):
{rag.BestMatchAnswer}

Instrucțiuni:
- Răspunde clar și pe scurt, în română
- Folosește DOAR text simplu, fără formatare Markdown
- Bazează-te pe contextul furnizat mai sus
- Dacă utilizatorul cere un exercițiu sau o problemă, oferă:
  1. Enunțul clar
  2. Soluția în pseudocod structurat (START, DECLARĂ, CITEȘTE, CALCULEAZĂ, DACĂ, REPETĂ, AFIȘEAZĂ, STOP)
  3. Menționează clar că soluția este în pseudocod pentru ca utilizatorul să o implementeze singur în limbajul dorit
- Dacă întrebarea utilizatorului diferă ușor de contextul furnizat, adaptează răspunsul la întrebarea exactă";
        }
        else
        {
            Console.WriteLine($"❌ RAG similarity too low ({rag?.Similarity:F2}), using general AI");

            finalPrompt = $@"Întrebare utilizator: {message}

Instrucțiuni:
- Răspunde clar și pe scurt, în română
- Folosește DOAR text simplu, fără formatare Markdown
- Ești un asistent care ajută la învățarea programării (Python, C, programare vizuală)
- Dacă este o întrebare despre concepte de bază în programare, explică pe înțelesul unui începător
- Dacă nu știi răspunsul exact, spune-i utilizatorului să consulte materialele cursului";
        }

        // Generate AI response
        var aiResponse = await _ai.GenerateAsync(finalPrompt);

        return Json(new
        {
            message = message,
            reply = aiResponse,
            usedRag = rag?.Similarity >= 0.3,
            similarity = rag?.Similarity ?? 0
        });
    }

    [Authorize]
    public async Task<IActionResult> Avatar()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user?.Photo == null || user.Photo.Length == 0)
        {
            var defaultPath = Path.Combine(_env.WebRootPath, "images", "chat", "user.png");
            if (System.IO.File.Exists(defaultPath))
            {
                var bytes = await System.IO.File.ReadAllBytesAsync(defaultPath);
                return File(bytes, "image/png");
            }
            return NoContent();
        }

        return File(user.Photo, "image/jpeg");
    }
}