using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SkillBridge.Data;

namespace SkillBridge.Pages.Account
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

        public string SuccessMessage { get; set; } = string.Empty;

        public void OnGet()
        {
            // Show registration success message
            if (TempData["RegisterSuccess"] != null)
            {
                SuccessMessage = TempData["RegisterSuccess"]!.ToString()!;
            }
        }

        public IActionResult OnPost()
        {
            // Check email
            if (string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = "Please enter your email address.";
                return Page();
            }

            // Check password
            if (string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Please enter your password.";
                return Page();
            }

            // Find matching user
            var user = _context.Users.FirstOrDefault(u =>
                u.Email.ToLower() == Email.Trim().ToLower()
                && u.Password == Password);

            // Invalid login
            if (user == null)
            {
                ErrorMessage = "Invalid email or password.";
                return Page();
            }

            // Store logged-in user's information in session
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserFullName", user.FullName);
            HttpContext.Session.SetString("UserPhone", user.Phone ?? "");
            HttpContext.Session.SetString("UserRole", user.Role ?? "User");

            // Send success message
            TempData["LoginSuccess"] =
                "Login successful! Welcome back to SkillBridge.";

            // Check user role
            if (string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToPage("/Admin/Login");
            }

            // Normal user
            return RedirectToPage("/Dashboard");
        }
    }
}