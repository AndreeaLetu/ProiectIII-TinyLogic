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

        string finalPrompt = rag.Similarity > 0.5
    ? $"Întrebare utilizator: {message}\nContext RAG: {rag.BestMatchAnswer}\nRăspunde clar și pe scurt, în română, folosind DOAR text simplu (fără Markdown: fără #, *, liste sau ```)."
    : $"Întrebare utilizator: {message}\nNu există context RAG.\nRăspunde clar și pe scurt, în română, folosind DOAR text simplu (fără Markdown: fără #, *, liste sau ```).";


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
