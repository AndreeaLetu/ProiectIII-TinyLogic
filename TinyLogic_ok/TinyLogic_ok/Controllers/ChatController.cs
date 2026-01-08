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
        var rag = await _rag.SearchAsync(message);

        string finalPrompt;

        if (rag != null && rag.Similarity >= 0.5)
        {
            finalPrompt =
                $"Întrebare utilizator: {message}\n" +
                $"Context din curs: {rag.BestMatchAnswer}\n" +
                "Răspunde clar și pe scurt, în română, folosind DOAR text simplu (fără Markdown ). " +
                "Dacă utilizatorul cere un exercițiu dintr-o lecție, oferă exact enunțul exercițiului și apoi rezolvarea  sub formă de pseudocod structurat (tip algoritmic), folosind doar instrucțiuni precum START, DECLARĂ, CITEȘTE, CALCULEAZĂ, DACĂ, REPETĂ, AFIȘEAZĂ, STOP. NU explica pseudocodul în text. Specifică clar că soluția este oferită intenționat în pseudocod pentru ca utilizatorul să poată implementa singur codul în limbajul cerut.";
        }
        else
        {
            finalPrompt =
                $"Întrebare utilizator: {message}\n" +
                "Răspunde clar și pe scurt, în română, folosind DOAR text simplu. " +
                "Dacă este o întrebare despre Python de bază, explică pe înțelesul unui începător.";
        }

        var ai = await _ai.GenerateAsync(finalPrompt);

        return Json(new { message = message, reply = ai });
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
