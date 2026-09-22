using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SkillBridge.Data;
using SkillBridge.Models;

namespace SkillBridge.Pages.Account
{
    public class ProfileModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ProfileModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public User? CurrentUser { get; set; }

        public IActionResult OnGet()
        {
            var email = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(email))
            {
                return RedirectToPage("/Account/Login");
            }

            CurrentUser = _context.Users.FirstOrDefault(u =>
                u.Email.ToLower() == email.ToLower());

            if (CurrentUser == null)
            {
                HttpContext.Session.Clear();
                return RedirectToPage("/Account/Login");
            }

            return Page();
        }
    }
}