using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SkillBridge.Data;
using SkillBridge.Models;

namespace SkillBridge.Pages.Admin
{
    public class ProfileModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ProfileModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public User? Admin { get; set; }

        [BindProperty]
        public string FullName { get; set; } = string.Empty;

        [BindProperty]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string? Phone { get; set; }

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            var role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(email) ||
                !string.Equals(role, "Admin",
                    StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToPage("/Admin/Login");
            }

            Admin = _context.Users.FirstOrDefault(u =>
                u.Email.ToLower() == email.ToLower());

            if (Admin == null)
            {
                return RedirectToPage("/Admin/Login");
            }

            LoadProfileData();

            return Page();
        }

        public IActionResult OnPostSave()
        {
            var sessionEmail = HttpContext.Session.GetString("UserEmail");
            var role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(sessionEmail) ||
                !string.Equals(role, "Admin",
                    StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToPage("/Admin/Login");
            }

            Admin = _context.Users.FirstOrDefault(u =>
                u.Email.ToLower() == sessionEmail.ToLower());

            if (Admin == null)
            {
                return RedirectToPage("/Admin/Login");
            }

            // Validate Full Name
            if (string.IsNullOrWhiteSpace(FullName))
            {
                LoadProfileData();
                ErrorMessage = "Please enter your full name.";
                return Page();
            }

            // Validate Email
            if (string.IsNullOrWhiteSpace(Email))
            {
                LoadProfileData();
                ErrorMessage = "Please enter your email address.";
                return Page();
            }

            if (!new System.ComponentModel.DataAnnotations.EmailAddressAttribute()
                    .IsValid(Email))
            {
                LoadProfileData();
                ErrorMessage = "Please enter a valid email address.";
                return Page();
            }

            // Check whether another account already uses this email
            var emailExists = _context.Users.Any(u =>
                u.UserID != Admin.UserID &&
                u.Email.ToLower() == Email.Trim().ToLower());

            if (emailExists)
            {
                LoadProfileData();
                ErrorMessage = "This email address is already registered.";
                return Page();
            }

            // Update admin information
            Admin.FullName = FullName.Trim();
            Admin.Email = Email.Trim();
            Admin.Phone = string.IsNullOrWhiteSpace(Phone)
                ? null
                : Phone.Trim();

            _context.SaveChanges();

            // Update session values so the changed information
            // is immediately reflected throughout the admin panel.
            HttpContext.Session.SetString("UserEmail", Admin.Email);
            HttpContext.Session.SetString("UserFullName", Admin.FullName);

            if (!string.IsNullOrWhiteSpace(Admin.Phone))
            {
                HttpContext.Session.SetString("UserPhone", Admin.Phone);
            }
            else
            {
                HttpContext.Session.Remove("UserPhone");
            }

            return RedirectToPage(new
            {
                updated = true
            });
        }

        public IActionResult OnGetUpdated()
        {
            return OnGet();
        }

        private void LoadProfileData()
        {
            if (Admin == null)
                return;

            FullName = Admin.FullName;
            Email = Admin.Email;
            Phone = Admin.Phone;
        }
    }
}