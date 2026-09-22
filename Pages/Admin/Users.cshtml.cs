using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SkillBridge.Data;
using SkillBridge.Models;

namespace SkillBridge.Pages.Admin
{
    public class UsersModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public UsersModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<User> Users { get; set; } = new List<User>();

        public IActionResult OnGet()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userEmail) ||
                !string.Equals(userRole, "Admin",
                    StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToPage("/Admin/Login");
            }

            Users = _context.Users
                .OrderBy(u => u.UserID)
                .ToList();

            return Page();
        }
    }
}