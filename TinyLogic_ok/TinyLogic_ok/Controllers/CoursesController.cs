using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using TinyLogic_ok.Models;
using TinyLogic_ok.Models.LessonModels;
using TinyLogic_ok.Models.CourseModels;

namespace TinyLogic_ok.Controllers
{
    public class CoursesController : Controller
    {
        private readonly TinyLogicDbContext _context;
        private readonly UserManager<User> _userManager;

        public CoursesController(TinyLogicDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        
        public async Task<IActionResult> PythonCourse(int courseId, int? lessonId)
        {
            var course = await _context.Courses
                .Include(c => c.Lessons)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);

            if (course == null)
                return NotFound();

            var lessons = course.Lessons
                .OrderBy(l => l.OrderIndex)
                .ToList();

            var selectedLesson = lessonId.HasValue
                ? lessons.FirstOrDefault(l => l.IdLesson == lessonId.Value)
                : lessons.FirstOrDefault();

            if (selectedLesson == null)
            {
                return View(new PythonCourseVM
                {
                    Course = course,
                    Lessons = lessons,
                    SelectedLesson = null,
                    ParsedContent = null,
                    CompletedLessonIds = new List<int>(),
                    IsCourseCompleted = false,
                    IsSelectedLessonCompleted = false,
                    HighestCompletedOrder = 0
                });
            }

            var parsed = JsonConvert.DeserializeObject<LessonContent>(selectedLesson.ContentJson);

         
            int intUserId = int.Parse(_userManager.GetUserId(User));


            var completedLessonIds = await _context.UserLessons
                 .Where(lp => lp.UserId == intUserId && lp.IsCompleted)
                 .Select(lp => lp.LessonId)
                 .ToListAsync();


            bool isLessonCompleted = completedLessonIds.Contains(selectedLesson.IdLesson);

            int highestCompletedOrder = lessons
                .Where(l => completedLessonIds.Contains(l.IdLesson))
                .Select(l => l.OrderIndex)
                .DefaultIfEmpty(0)
                .Max();

            bool isCourseCompleted = completedLessonIds.Count == lessons.Count;

            var vm = new PythonCourseVM
            {
                Course = course,
                Lessons = lessons,
                SelectedLesson = selectedLesson,
                ParsedContent = parsed,
                CompletedLessonIds = completedLessonIds,
                IsSelectedLessonCompleted = isLessonCompleted,
                IsCourseCompleted = isCourseCompleted,
                HighestCompletedOrder = highestCompletedOrder
            };

            return View(vm);
        }


        public async Task<IActionResult> Index()
        {
            return View(await _context.Courses.ToListAsync());
        }
    }
}
