using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SkillBridge.Data;
using SkillBridge.Models;

namespace SkillBridge.Pages.Admin
{
    public class EditUserModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditUserModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public User User { get; set; } = new User();

        public string ErrorMessage { get; set; } = string.Empty;


        // LOAD USER
        public IActionResult OnGet(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToPage("/Admin/Login");
            }

            var user = _context.Users.FirstOrDefault(u => u.UserID == id);

            if (user == null)
            {
                return NotFound();
            }

            User = user;

            return Page();
        }


        // SAVE CHANGES
        public IActionResult OnPost()
        {
            if (!IsAdmin())
            {
                return RedirectToPage("/Admin/Login");
            }

            // Password is not being edited on this page.
            // Remove its validation requirement for this POST.
            ModelState.Remove("User.Password");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var existingUser = _context.Users
                .FirstOrDefault(u => u.UserID == User.UserID);

            if (existingUser == null)
            {
                return NotFound();
            }

            // Update only the fields that this page is supposed to edit.
            existingUser.FullName = User.FullName;
            existingUser.Email = User.Email;
            existingUser.Phone = User.Phone;
            existingUser.Role = User.Role;

            _context.SaveChanges();

            return RedirectToPage("/Admin/Users");
        }


        // ADMIN CHECK
        private bool IsAdmin()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var userRole = HttpContext.Session.GetString("UserRole");

            return !string.IsNullOrEmpty(userEmail)
                && string.Equals(
                    userRole,
                    "Admin",
                    StringComparison.OrdinalIgnoreCase);
        }
        public IActionResult OnPostDelete(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToPage("/Admin/Login");
            }

            var user = _context.Users
                .FirstOrDefault(u => u.UserID == id);

            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            _context.SaveChanges();

            return RedirectToPage("/Admin/Users");
        }
    }
}