using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SkillBridge.Data;
using SkillBridge.Models;

namespace SkillBridge.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public RegisterModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public User NewUser { get; set; } = new User();

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            // Check validation rules from User.cs
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Check whether email already exists
            bool emailExists = _context.Users
                .Any(u => u.Email.ToLower() == NewUser.Email.Trim().ToLower());

            if (emailExists)
            {
                ModelState.AddModelError(
                    "NewUser.Email",
                    "An account with this email already exists."
                );

                return Page();
            }

            // Clean user information
            NewUser.FullName = NewUser.FullName.Trim();
            NewUser.Email = NewUser.Email.Trim().ToLower();

            if (!string.IsNullOrWhiteSpace(NewUser.Phone))
            {
                NewUser.Phone = NewUser.Phone.Trim();
            }

            // Save the new user
            _context.Users.Add(NewUser);
            _context.SaveChanges();

            // Send success message to Login page
            TempData["RegisterSuccess"] =
                "Registration successful! You can now log in to your SkillBridge account.";

            // Redirect to Login
            return RedirectToPage("/Account/Login");
        }
    }
}