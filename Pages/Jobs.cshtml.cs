using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SkillBridge.Data;
using System.Text.Json;

namespace SkillBridge.Pages
{
    public class JobsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public JobsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<RecommendedJob> RecommendedJobs { get; set; } = new();

        public bool HasResumeData { get; set; }

        public string Message { get; set; } =
            "Analyze your resume first to receive personalized job recommendations.";

        // =========================================================
        // GET
        // =========================================================

        public async Task<IActionResult> OnGetAsync()
        {
            var userEmail =
                HttpContext.Session.GetString("UserEmail");

            // -----------------------------------------------------
            // LOGIN CHECK
            // -----------------------------------------------------

            if (string.IsNullOrWhiteSpace(userEmail))
            {
                return RedirectToPage("/Account/Login");
            }


            // -----------------------------------------------------
            // FIND CURRENT USER
            // -----------------------------------------------------

            var currentUser =
                await _context.Users
                    .FirstOrDefaultAsync(u =>
                        u.Email.ToLower() ==
                        userEmail.Trim().ToLower());

            if (currentUser == null)
            {
                return RedirectToPage("/Account/Login");
            }


            // -----------------------------------------------------
            // CHECK ACTUAL RESUME ANALYSIS
            // -----------------------------------------------------

            bool hasResume =
                !string.IsNullOrWhiteSpace(currentUser.ResumePath);

            bool hasSkills =
                !string.IsNullOrWhiteSpace(currentUser.Skills);

            bool hasResumeAnalysis = false;

            if (hasResume)
            {
                hasResumeAnalysis =
                    await _context.ResumeAnalysis
                        .AnyAsync(analysis =>
                            _context.Resumes.Any(resume =>
                                resume.ResumeID == analysis.ResumeID &&
                                resume.UserID == currentUser.UserID));
            }


            // -----------------------------------------------------
            // NO RESUME ANALYSIS
            // -----------------------------------------------------

            if (!hasResume ||
                !hasSkills ||
                !hasResumeAnalysis)
            {
                HasResumeData = false;

                RecommendedJobs.Clear();

                Message =
                    "Analyze your resume first to receive personalized job recommendations.";

                return Page();
            }


            // -----------------------------------------------------
            // RESUME ANALYSIS EXISTS
            // -----------------------------------------------------

            HasResumeData = true;

            Message =
                "These job recommendations are based on the skills identified from your resume.";


            // -----------------------------------------------------
            // GET PERSONALIZED JOBS
            // -----------------------------------------------------

            var jobsJson =
                TempData.Peek("RecommendedJobs") as string;

            if (!string.IsNullOrWhiteSpace(jobsJson))
            {
                try
                {
                    RecommendedJobs =
                        JsonSerializer.Deserialize<List<RecommendedJob>>(
                            jobsJson)
                        ?? new List<RecommendedJob>();
                }
                catch
                {
                    RecommendedJobs = new List<RecommendedJob>();
                }
            }


            // -----------------------------------------------------
            // IF ANALYSIS EXISTS BUT NO JOBS WERE GENERATED
            // -----------------------------------------------------

            if (RecommendedJobs.Count == 0)
            {
                Message =
                    "Your resume has been analyzed, but no matching jobs are currently available.";
            }


            return Page();
        }
    }
}