using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SkillBridge.Data;

namespace SkillBridge.Pages.Admin
{
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DashboardModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            // Check whether a user is logged in
            var userEmail = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrWhiteSpace(userEmail))
            {
                return RedirectToPage("/Account/Login");
            }

            // Find the logged-in user
            var user = _context.Users
                .FirstOrDefault(u => u.Email == userEmail);

            // User does not exist
            if (user == null)
            {
                return RedirectToPage("/Account/Login");
            }

            // User exists but is not an administrator
            if (user.Role != "Admin")
            {
                return RedirectToPage("/Dashboard");
            }

            // Authorized administrator
            return Page();
        }
    }
}