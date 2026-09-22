using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SkillBridge.Data;

namespace SkillBridge.Pages.Admin
{
    public class LoginModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public LoginModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        public string ErrorMessage { get; set; } = string.Empty;

        public IActionResult OnPost()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = "Please enter the admin email.";
                return Page();
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Please enter the admin password.";
                return Page();
            }

            var email = Email.Trim();

            var user = _context.Users.FirstOrDefault(u =>
                u.Email == email &&
                u.Password == Password);

            if (user == null)
            {
                ErrorMessage = "Invalid admin email or password.";
                return Page();
            }

            if (!string.Equals(
                    user.Role,
                    "Admin",
                    StringComparison.OrdinalIgnoreCase))
            {
                ErrorMessage = "You do not have administrator access.";
                return Page();
            }

            // Create the admin session
            HttpContext.Session.SetString(
                "UserEmail",
                user.Email);

            HttpContext.Session.SetString(
                "UserRole",
                user.Role);

            return RedirectToPage("/Admin/Dashboard");
        }
    }
}