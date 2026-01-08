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
            Console.WriteLine(" START SEED ");

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

            var courses = new List<Courses>
    {
        new Courses
        {
            CourseName = "Python pentru copii",
            Description = "Curs Python interactiv pentru începători",
            Difficulty = "Ușor",
            Language = "python"
        },
        new Courses
        {
            CourseName = "C pentru începători",
            Description = "Bazele limbajului C, pas cu pas",
            Difficulty = "Mediu",
            Language = "c"
        },

        new Courses
        {
            CourseName = "Programare Vizuală",
            Description = "Învață programarea prin blocuri vizuale",
            Difficulty = "Ușor",
            Language = "blocks"
        }
    };

            _context.Courses.AddRange(courses);
            await _context.SaveChangesAsync();

            foreach (var c in courses)
                Console.WriteLine($"Creat curs: {c.CourseName} (ID={c.CourseId})");
        }


        private async Task SeedLessonsAsync()
        {
            if (await _context.Lessons.AnyAsync())
            {
                Console.WriteLine("Lecțiile există deja – skip.");
                return;
            }

            var courses = await _context.Courses.ToListAsync();

            foreach (var course in courses)
            {
                string fileName = course.Language switch
                {
                    "python" => "python_lessons.json",
                    "c" => "c_lessons.json",
                    "blocks" => "blocks_lessons.json",
                    _ => null
                };

                if (fileName == null)
                {
                    Console.WriteLine($"Skip curs necunoscut: {course.Language}");
                    continue;
                }


                string jsonPath = Path.Combine(_env.ContentRootPath, "Data", fileName);

                if (!File.Exists(jsonPath))
                {
                    Console.WriteLine($"Nu exista {fileName}");
                    continue;
                }

                string json = await File.ReadAllTextAsync(jsonPath);
                var lessons = JsonSerializer.Deserialize<List<LessonJsonModel>>(json);

                foreach (var item in lessons)
                {
                    _context.Lessons.Add(new Lessons
                    {
                        LessonName = item.LessonName,
                        OrderIndex = item.OrderIndex,
                        Description = item.ContentJson.Title,
                        CourseId = course.CourseId,
                        ContentJson = JsonSerializer.Serialize(item.ContentJson)
                    });
                }

                Console.WriteLine($"Lectii adaugate pentru {course.CourseName}");
            }

            await _context.SaveChangesAsync();
        }

        private async Task SeedTestsAsync()
        {
            if (await _context.Tests.AnyAsync())
            {
                Console.WriteLine("Testele exista deja – skip.");
                return;
            }

            var testFiles = new[]
            {
        "python_tests.json",
        "c_tests.json",
        "blockly_test.json"
    };

            foreach (var file in testFiles)
            {
                string jsonPath = Path.Combine(_env.ContentRootPath, "Data", file);
                Console.WriteLine($"Caut json teste: {jsonPath}");

                if (!File.Exists(jsonPath))
                {
                    Console.WriteLine($"{file} nu exista.");
                    continue;
                }

                string json = await File.ReadAllTextAsync(jsonPath);

                List<TestJsonModel>? tests;
                try
                {
                    tests = JsonSerializer.Deserialize<List<TestJsonModel>>(json);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Eroare parsare {file}: {ex.Message}");
                    continue;
                }

                if (tests == null) continue;

                foreach (var item in tests)
                {
                    var course = await _context.Courses
                        .FirstOrDefaultAsync(c => c.CourseName == item.CourseName);

                    if (course == null)
                    {
                        Console.WriteLine($" NU exista cursul {item.CourseName} – skip test.");
                        continue;
                    }

                    _context.Tests.Add(new Tests
                    {
                        TestName = item.TestName,
                        Description = item.Description,
                        PassingScore = item.PassingScore,
                        CourseId = course.CourseId,
                        TestJson = JsonSerializer.Serialize(item.TestJson)
                    });

                    Console.WriteLine($"Adaug test: {item.TestName}");
                }
            }

            await _context.SaveChangesAsync();
        }


    }
}
