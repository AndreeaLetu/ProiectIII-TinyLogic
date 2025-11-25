using System.Text.Json;
using TinyLogic_ok.Models;
using Microsoft.EntityFrameworkCore;

using TinyLogic_ok.Models.CourseModels;
using TinyLogic_ok.Models.LessonModels;
using TinyLogic_ok.Models.TestModels;


namespace TinyLogic_ok.Services
{
    public class DataSeeder
    {
        private readonly TinyLogicDbContext _context;
        private readonly IWebHostEnvironment _env;

        public DataSeeder(TinyLogicDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task SeedAsync()
        {
            Console.WriteLine("=== START SEED ===");

            await SeedCoursesAsync();
            await SeedLessonsAsync();
            await SeedTestsAsync();


            Console.WriteLine("=== END SEED ===");
        }

        private async Task SeedCoursesAsync()
        {
            if (await _context.Courses.AnyAsync())
            {
                Console.WriteLine("Cursurile există deja – skip.");
                return;
            }

            var pythonCourse = new Courses
            {
                CourseName = "Python pentru copii",
                Description = "Curs Python interactiv pentru începători",
                Difficulty = "Ușor"
            };

            _context.Courses.Add(pythonCourse);
            await _context.SaveChangesAsync();

            Console.WriteLine($"Creat cursul cu ID = {pythonCourse.CourseId}");
        }

        private async Task SeedLessonsAsync()
        {
            if (await _context.Lessons.AnyAsync())
            {
                Console.WriteLine("Lecțiile există deja – skip.");
                return;
            }

            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseName == "Python pentru copii");

            if (course == null)
            {
                Console.WriteLine("NU există cursul Python – nu pot importa lecțiile.");
                return;
            }

            string jsonPath = Path.Combine(_env.ContentRootPath, "Data/python_lessons.json");
            Console.WriteLine("Caut json la: " + jsonPath);

            if (!File.Exists(jsonPath))
            {
                Console.WriteLine("!!! JSON-ul pentru lecții NU există.");
                return;
            }

            string json = await File.ReadAllTextAsync(jsonPath);

            List<LessonJsonModel>? lessons;

            try
            {
                lessons = JsonSerializer.Deserialize<List<LessonJsonModel>>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Eroare la parsarea JSON: " + ex.Message);
                return;
            }

            if (lessons == null)
            {
                Console.WriteLine("JSON-ul este gol.");
                return;
            }

            foreach (var item in lessons)
            {
                var lessonEntity = new Lessons
                {
                    LessonName = item.LessonName,
                    OrderIndex = item.OrderIndex,
                    Description = item.ContentJson.Title,
                    CourseId = course.CourseId,
                    ContentJson = JsonSerializer.Serialize(item.ContentJson)
                };

                _context.Lessons.Add(lessonEntity);
                Console.WriteLine($"Adaug lecția: {item.LessonName}");
            }

            await _context.SaveChangesAsync();
        }
        private async Task SeedTestsAsync()
        {
            if (await _context.Tests.AnyAsync())
            {
                Console.WriteLine("Testele există deja – skip.");
                return;
            }

            string jsonPath = Path.Combine(_env.ContentRootPath, "Data/python_tests.json");
            Console.WriteLine("Caut json de teste la: " + jsonPath);

            if (!File.Exists(jsonPath))
            {
                Console.WriteLine("!!! JSON-ul pentru teste NU există.");
                return;
            }

            string json = await File.ReadAllTextAsync(jsonPath);

            List<TestJsonModel>? tests;

            try
            {
                tests = JsonSerializer.Deserialize<List<TestJsonModel>>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Eroare la parsare JSON TESTE: " + ex.Message);
                return;
            }

            if (tests == null)
            {
                Console.WriteLine("JSON-ul pentru teste este gol.");
                return;
            }

            foreach (var item in tests)
            {
                var course = await _context.Courses
                    .FirstOrDefaultAsync(c => c.CourseName == item.CourseName);

                if (course == null)
                {
                    Console.WriteLine($"!!! NU există cursul {item.CourseName} – skip test.");
                    continue;
                }

                var testEntity = new Tests
                {
                    TestName = item.TestName,
                    Description = item.Description,
                    PassingScore = item.PassingScore,
                    CourseId = course.CourseId,
                    TestJson = JsonSerializer.Serialize(item.TestJson)
                };

                _context.Tests.Add(testEntity);
                Console.WriteLine($"Adaug testul: {item.TestName}");
            }

            await _context.SaveChangesAsync();
        }

    }
}
