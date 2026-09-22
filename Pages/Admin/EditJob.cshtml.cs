using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SkillBridge.Data;
using SkillBridge.Models;

namespace SkillBridge.Pages.Admin
{
    public class EditJobModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditJobModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Job Job { get; set; } = new Job();

        public IActionResult OnGet(int id)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userEmail) ||
                !string.Equals(userRole, "Admin",
                    StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToPage("/Admin/Login");
            }

            var job = _context.Jobs.FirstOrDefault(j => j.JobID == id);

            if (job == null)
            {
                return RedirectToPage("/Admin/Jobs");
            }

            Job = job;

            return Page();
        }

        public IActionResult OnPost()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userEmail) ||
                !string.Equals(userRole, "Admin",
                    StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToPage("/Admin/Login");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var existingJob = _context.Jobs
                .FirstOrDefault(j => j.JobID == Job.JobID);

            if (existingJob == null)
            {
                return RedirectToPage("/Admin/Jobs");
            }

            existingJob.JobTitle = Job.JobTitle;
            existingJob.CompanyName = Job.CompanyName;
            existingJob.Location = Job.Location;
            existingJob.JobType = Job.JobType;
            existingJob.Description = Job.Description;
            existingJob.RequiredSkills = Job.RequiredSkills;
            existingJob.ApplyLink = Job.ApplyLink;

            _context.SaveChanges();

            return RedirectToPage("/Admin/Jobs");
        }

        public IActionResult OnPostDelete(int id)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userEmail) ||
                !string.Equals(userRole, "Admin",
                    StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToPage("/Admin/Login");
            }

            var job = _context.Jobs
                .FirstOrDefault(j => j.JobID == id);

            if (job != null)
            {
                _context.Jobs.Remove(job);
                _context.SaveChanges();
            }

            return RedirectToPage("/Admin/Jobs");
        }
    }
}