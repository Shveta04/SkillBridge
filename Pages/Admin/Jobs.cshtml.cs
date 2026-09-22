using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SkillBridge.Data;
using SkillBridge.Models;

namespace SkillBridge.Pages.Admin
{
    public class JobsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public JobsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Job> Jobs { get; set; } = new List<Job>();

        public IActionResult OnGet()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userEmail) ||
                !string.Equals(userRole, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToPage("/Admin/Login");
            }

            Jobs = _context.Jobs
                .OrderBy(j => j.JobID)
                .ToList();

            return Page();
        }
    }
}