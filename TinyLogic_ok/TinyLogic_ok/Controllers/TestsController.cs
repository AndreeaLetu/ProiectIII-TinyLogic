using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;
using TinyLogic_ok.Models;
using TinyLogic_ok.Models.TestModels;

namespace TinyLogic_ok.Controllers
{
    [Authorize]
    public class TestsController : Controller
    {
        private readonly TinyLogicDbContext _context;
        private readonly UserManager<User> _userManager;

        public TestsController(TinyLogicDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var tests = await _context.Tests
                .Include(t => t.Course)
                    .ThenInclude(c => c.Lessons)
                .ToListAsync();

            var allLessonIds = tests
                .SelectMany(t => t.Course.Lessons.Select(l => l.IdLesson))
                .Distinct()
                .ToList();

            var allLessonProgress = await _context.UserLessons
                .Where(ul => ul.UserId == userId &&
                             ul.IsCompleted &&
                             allLessonIds.Contains(ul.LessonId))
                .ToListAsync();

            var allTestProgress = await _context.TestProgresses
                .Where(tp => tp.UserId == userId)
                .OrderByDescending(tp => tp.CompletedAt)
                .ToListAsync();

            var testVMs = new List<TestVM>();

            foreach (var test in tests)
            {
                var currentTestLessonIds = test.Course.Lessons.Select(l => l.IdLesson).ToList();
                var totalLessons = currentTestLessonIds.Count;

                var completedLessons = allLessonProgress
                    .Count(lp => currentTestLessonIds.Contains(lp.LessonId));

                bool isLocked = completedLessons < totalLessons;

                var testProgress = allTestProgress
                    .FirstOrDefault(tp => tp.TestId == test.IdTest);

                testVMs.Add(new TestVM
                {
                    Test = test,
                    ParsedContent = null,
                    IsLocked = isLocked,
                    IsCompleted = testProgress != null && testProgress.IsPassed,
                    LastScore = testProgress?.Score,
                    RequiredCourse = test.Course,
                    CompletedLessons = completedLessons,
                    TotalLessons = totalLessons
                });
            }

            return View(testVMs);
        }

        
        public async Task<IActionResult> TakeTest(int testId)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var test = await _context.Tests
                .Include(t => t.Course)
                    .ThenInclude(c => c.Lessons)
                .FirstOrDefaultAsync(t => t.IdTest == testId);

            if (test == null)
                return NotFound();

            var courseLessonIds = test.Course.Lessons.Select(l => l.IdLesson).ToList();
            var totalLessons = courseLessonIds.Count;

            var completedLessons = await _context.UserLessons
                .Where(ul => ul.UserId == userId &&
                             ul.IsCompleted &&
                             courseLessonIds.Contains(ul.LessonId))
                .CountAsync();

            if (completedLessons < totalLessons)
            {
                TempData["Error"] = "Trebuie să finalizezi toate lecțiile pentru a da acest test!";
                return RedirectToAction("Index");
            }

            var parsedContent = JsonConvert.DeserializeObject<TestContent>(test.TestJson);

            var testProgress = await _context.TestProgresses
                .Where(tp => tp.UserId == userId && tp.TestId == test.IdTest)
                .OrderByDescending(tp => tp.CompletedAt)
                .FirstOrDefaultAsync();

            var vm = new TestVM
            {
                Test = test,
                ParsedContent = parsedContent,
                IsLocked = false,
                IsCompleted = testProgress != null && testProgress.IsPassed,
                LastScore = testProgress?.Score,
                RequiredCourse = test.Course,
                CompletedLessons = completedLessons,
                TotalLessons = totalLessons
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitTest([FromBody] SubmitTestRequest request)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var test = await _context.Tests.FindAsync(request.TestId);
            if (test == null)
                return Json(new { success = false, message = "Testul nu există!" });

            var parsedContent = JsonConvert.DeserializeObject<TestContent>(test.TestJson);

            int totalScore = 0;
            int maxScore = parsedContent.Questions.Sum(q => q.Points);

            foreach (var answer in request.Answers)
            {
                var question = parsedContent.Questions
                    .FirstOrDefault(q => q.QuestionNumber == answer.QuestionNumber);

                if (question != null)
                {
                    string userAnswer = Normalize(answer.Answer);
                    string correctAnswer = Normalize(question.CorrectAnswer);

                    if (userAnswer == correctAnswer)
                        totalScore += question.Points;
                }
            }

            int percentage = (int)((double)totalScore / maxScore * 100);
            bool isPassed = percentage >= test.PassingScore;

            var testProgress = new TestProgress
            {
                UserId = userId,
                TestId = request.TestId,
                Score = percentage,
                IsPassed = isPassed,
                CompletedAt = DateTime.UtcNow,
                AnswersJson = JsonConvert.SerializeObject(request.Answers)
            };

            _context.TestProgresses.Add(testProgress);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                score = percentage,
                passed = isPassed,
                message = isPassed
                    ? $"Felicitări! Ai trecut testul cu {percentage}%!"
                    : $"Ai obținut {percentage}%. Nota minimă este {test.PassingScore}%."
            });
        }

        private string Normalize(string text)
        {
            return text.ToLower()
                .Replace("ă", "a").Replace("â", "a").Replace("î", "i")
                .Replace("ș", "s").Replace("ş", "s")
                .Replace("ț", "t").Replace("ţ", "t")
                .Trim();
        }

        public class SubmitTestRequest
        {
            public int TestId { get; set; }
            public List<TestAnswer> Answers { get; set; }
        }

        public class TestAnswer
        {
            public int QuestionNumber { get; set; }
            public string Answer { get; set; }
        }
    }
}
