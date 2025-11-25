// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TinyLogic_ok.Models;

namespace TinyLogic_ok.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public IndexModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
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
    }
}
