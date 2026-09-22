using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace SkillBridge.Pages
{
    public class JobDetailsModel : PageModel
    {
        public RecommendedJob? Job { get; set; }

        public void OnGet(string? id)
        {
            var jobsJson =
                TempData.Peek("RecommendedJobs") as string;

            if (string.IsNullOrWhiteSpace(jobsJson))
            {
                Job = null;
                return;
            }

            try
            {
                var jobs =
                    JsonSerializer.Deserialize<List<RecommendedJob>>(
                        jobsJson
                    ) ?? new List<RecommendedJob>();

                if (!string.IsNullOrWhiteSpace(id))
                {
                    Job = jobs.FirstOrDefault(j =>
                        string.Equals(
                            j.Id,
                            id,
                            StringComparison.OrdinalIgnoreCase
                        )
                    );
                }
            }
            catch
            {
                Job = null;
            }
        }
    }
}