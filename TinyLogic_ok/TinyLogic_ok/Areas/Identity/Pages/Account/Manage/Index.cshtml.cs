// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TinyLogic_ok.Models;
using TinyLogic_ok.Models.LessonModels;

namespace TinyLogic_ok.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        // ✅ NOU: DbContext pentru cursuri/lecții/diplome
        private readonly TinyLogicDbContext _context;

        public IndexModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            TinyLogicDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        // ============================
        //   POZA PENTRU VIEW
        // ============================
        public string PhotoBase64 { get; set; }

        // ============================
        // ✅ NOI: Stats cursuri/diplome/badge-uri
        // ============================
        public List<CourseProgressVM> CourseProgress { get; set; } = new();
        public List<CertificateVM> MyCertificates { get; set; } = new();
        public List<BadgeVM> Badges { get; set; } = new();

        public int TotalLessonsCompleted { get; set; }
        public int TotalCoursesCompleted { get; set; }
        public int TotalCertificates { get; set; }

        public class CourseProgressVM
        {
            public int CourseId { get; set; }
            public string CourseName { get; set; } = "";
            public string Language { get; set; }
            public int TotalLessons { get; set; }
            public int CompletedLessons { get; set; }
            public bool IsCompleted => TotalLessons > 0 && CompletedLessons >= TotalLessons;
            public int Percent => TotalLessons == 0 ? 0 : (int)Math.Round((double)CompletedLessons * 100 / TotalLessons);
        }

        public class CertificateVM
        {
            public int CourseId { get; set; }
            public string CourseName { get; set; } = "";
            public DateTime DateGenerated { get; set; }
            public string CertificateCode { get; set; } = "";
            public string PdfPath { get; set; } = "";
        }

        public class BadgeVM
        {
            public string Title { get; set; } = "";
            public string Subtitle { get; set; } = "";
            public string Icon { get; set; } = "🏅";
            public string StyleClass { get; set; } = "bg-indigo-50 text-indigo-700 border-indigo-200";
        }

        public class InputModel
        {
            [Phone]
            [Display(Name = "Număr de telefon")]
            public string PhoneNumber { get; set; }

            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Prenume")]
            public string FirstName { get; set; }

            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Nume")]
            public string LastName { get; set; }

            [Display(Name = "Rol")]
            public string Role { get; set; }

            [DataType(DataType.Date)]
            [Display(Name = "Data nașterii")]
            public DateTime? BirthDate { get; set; }

            // ============================
            //   UPLOAD POZA PROFIL
            // ============================
            [DataType(DataType.Upload)]
            public IFormFile PhotoFile { get; set; }
        }

        private async Task LoadAsync(User user)
        {
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            // ============================
            //   ÎNCĂRCĂM POZA ÎN BASE64
            // ============================
            if (user.Photo != null)
                PhotoBase64 = $"data:image/png;base64,{Convert.ToBase64String(user.Photo)}";

            Username = user.Email;

            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                BirthDate = user.BirthDate
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            await LoadAsync(user);

            // ✅ NOU: încărcăm progres + diplome + badge-uri
            await LoadLearningStatsAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                await LoadLearningStatsAsync(); // ca să nu dispară stats din pagină
                return Page();
            }

            // Actualizare numar telefon
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            // ============================
            //   ACTUALIZARE POZĂ PROFIL
            // ============================
            if (Input.PhotoFile != null)
            {
                using var ms = new MemoryStream();
                await Input.PhotoFile.CopyToAsync(ms);
                user.Photo = ms.ToArray();
            }

            // ============================
            //   ACTUALIZARE DATE USER
            // ============================
            user.FirstName = Input.FirstName;
            user.LastName = Input.LastName;
            user.Role = Input.Role;

            if (Input.BirthDate.HasValue)
            {
                user.BirthDate = DateTime.SpecifyKind(Input.BirthDate.Value, DateTimeKind.Utc);
            }

            await _userManager.UpdateAsync(user);

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Profilul a fost actualizat!";
            return RedirectToPage();
        }

        // ============================
        // ✅ NOU: încărcare progres/diplome/badge-uri
        // ============================
        private async Task LoadLearningStatsAsync()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // 1) cursuri + lecții
            var courses = await _context.Courses
                .Include(c => c.Lessons)
                .OrderBy(c => c.CourseId)
                .ToListAsync();

            // 2) lecții completate (ID-uri)
            var completedLessonIds = await _context.UserLessons
                .Where(ul => ul.UserId == userId && ul.IsCompleted)
                .Select(ul => ul.LessonId)
                .ToListAsync();

            var completedSet = completedLessonIds.ToHashSet();
            TotalLessonsCompleted = completedLessonIds.Count;

            // 3) progres per curs
            CourseProgress = courses.Select(c =>
            {
                var total = c.Lessons?.Count ?? 0;
                var done = (c.Lessons ?? new List<Lessons>())
                    .Count(l => completedSet.Contains(l.IdLesson));

                return new CourseProgressVM
                {
                    CourseId = c.CourseId,
                    CourseName = c.CourseName,
                    Language = c.Language,
                    TotalLessons = total,
                    CompletedLessons = done
                };
            }).ToList();

            TotalCoursesCompleted = CourseProgress.Count(x => x.IsCompleted);

            // 4) diplome
            var certs = await _context.Certificates
                .Include(c => c.Course)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.DateGenerated)
                .ToListAsync();

            MyCertificates = certs.Select(c => new CertificateVM
            {
                CourseId = c.CourseId,
                CourseName = c.Course.CourseName,
                DateGenerated = c.DateGenerated,
                CertificateCode = c.CertificateCode,
                PdfPath = c.PdfPath
            }).ToList();

            TotalCertificates = MyCertificates.Count;

            // 5) badge-uri
            Badges = new List<BadgeVM>();

            foreach (var cp in CourseProgress.Where(x => x.IsCompleted))
            {
                var icon = (cp.Language ?? "").ToLower() switch
                {
                    "python" => "🐍",
                    "c" => "💻",
                    "blocks" => "🧩",
                    _ => "🏁"
                };

                Badges.Add(new BadgeVM
                {
                    Title = $"Absolvent: {cp.CourseName}",
                    Subtitle = "Curs finalizat 100%",
                    Icon = icon,
                    StyleClass = "bg-green-50 text-green-700 border-green-200"
                });
            }

            if (TotalCoursesCompleted >= 1)
                Badges.Add(new BadgeVM { Title = "Primul curs terminat", Subtitle = "Bravo!", Icon = "🎉" });

            if (TotalCoursesCompleted >= 3)
                Badges.Add(new BadgeVM
                {
                    Title = "Maraton de cursuri",
                    Subtitle = "3 cursuri finalizate",
                    Icon = "🔥",
                    StyleClass = "bg-orange-50 text-orange-700 border-orange-200"
                });

            if (TotalLessonsCompleted >= 20)
                Badges.Add(new BadgeVM
                {
                    Title = "Consecvent",
                    Subtitle = "20+ lecții completate",
                    Icon = "📚",
                    StyleClass = "bg-indigo-50 text-indigo-700 border-indigo-200"
                });
        }
    }
}
