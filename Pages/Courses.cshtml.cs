
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SkillBridge.Data;
using SkillBridge.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SkillBridge.Pages
{
    public class CoursesModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CoursesModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public string CareerGoal { get; set; } = "";

        public string RecommendationMessage { get; set; } =
            "Analyze your resume first so SkillBridge can recommend learning resources.";

        public List<LearningRecommendation> Recommendations { get; set; }
            = new();

        public bool HasResumeData { get; set; }

        public IActionResult OnGet()
        {
            string? email = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrWhiteSpace(email))
                return RedirectToPage("/Account/Login");

            User? currentUser = _context.Users
                .AsNoTracking()
                .FirstOrDefault(u =>
                    u.Email.ToLower() == email.Trim().ToLower());

            if (currentUser == null)
                return RedirectToPage("/Account/Login");

            Resume? latestResume = _context.Resumes
                .AsNoTracking()
                .Where(r => r.UserID == currentUser.UserID)
                .OrderByDescending(r => r.UploadDate)
                .FirstOrDefault();

            if (latestResume == null)
            {
                SetNoResumeMessage();
                return Page();
            }

            ResumeAnalysis? latestAnalysis = _context.ResumeAnalysis
                .AsNoTracking()
                .Where(a => a.ResumeID == latestResume.ResumeID)
                .OrderByDescending(a => a.AnalysisDate)
                .FirstOrDefault();

            if (latestAnalysis == null)
            {
                SetNoResumeMessage();
                return Page();
            }

            // Prefer skills saved in the latest analysis.
            // Fall back to the user's stored skills if necessary.
            string storedSkills =
                !string.IsNullOrWhiteSpace(latestAnalysis.ExtractedSkills)
                    ? latestAnalysis.ExtractedSkills
                    : currentUser.Skills ?? "";

            List<string> userSkills = SplitSkills(storedSkills);

            HasResumeData = true;

            CareerGoal = DetermineCareerGoal(userSkills);

            List<string> analysisMissingSkills =
                SplitSkills(latestAnalysis.MissingSkills ?? "");

            Recommendations = GetRecommendations(
                userSkills,
                analysisMissingSkills,
                CareerGoal);

            if (Recommendations.Count > 0)
            {
                RecommendationMessage =
                    $"These learning resources are selected using your resume analysis, skill gaps, and {CareerGoal} career direction.";
            }
            else
            {
                RecommendationMessage =
                    "Your resume has been analyzed, but no relevant learning resources were found. Please check the available records in the Courses database.";
            }

            return Page();
        }

        private void SetNoResumeMessage()
        {
            HasResumeData = false;
            CareerGoal = "";
            Recommendations.Clear();

            RecommendationMessage =
                "Analyze your latest resume first to receive personalized learning resources.";
        }

        private List<string> SplitSkills(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return new List<string>();

            return value
                .Split(
                    new[] { ',', ';', '|', '\n', '\r' },
                    StringSplitOptions.RemoveEmptyEntries)
                .Select(NormalizeSkill)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct()
                .ToList();
        }

        private string NormalizeSkill(string skill)
        {
            return skill.Trim()
                .ToLowerInvariant()
                .Replace("asp.net", "asp.net")
                .Replace("machine-learning", "machine learning")
                .Replace("data-analysis", "data analysis")
                .Trim();
        }

        private string DetermineCareerGoal(List<string> skills)
        {
            var scores = new Dictionary<string, int>
            {
                ["Frontend Developer"] = CountMatches(
                    skills, "html", "css", "javascript",
                    "typescript", "react", "angular", "vue"),

                [".NET Developer"] = CountMatches(
                    skills, "c#", "asp.net core", "asp.net",
                    "entity framework", "sql"),

                ["Backend Developer"] = CountMatches(
                    skills, "c#", "java", "python",
                    "sql", "rest api", "entity framework"),

                ["Data Analyst"] = CountMatches(
                    skills, "python", "sql", "data analysis",
                    "power bi", "excel"),

                ["AI/ML Developer"] = CountMatches(
                    skills, "python", "machine learning",
                    "artificial intelligence", "data analysis"),

                ["Cloud / DevOps Engineer"] = CountMatches(
                    skills, "docker", "kubernetes", "azure",
                    "aws", "gcp", "ci/cd", "linux")
            };

            return scores
                .OrderByDescending(x => x.Value)
                .First().Key;
        }

        private List<LearningRecommendation> GetRecommendations(
            List<string> userSkills,
            List<string> analysisMissingSkills,
            string careerGoal)
        {
            var careerSkills = new Dictionary<string, List<string>>
            {
                [".NET Developer"] = new()
                {
                    "c#", "asp.net core", "entity framework",
                    "sql", "rest api", "git", "github", "azure"
                },

                ["Frontend Developer"] = new()
                {
                    "javascript", "react", "typescript",
                    "html", "css", "git", "github", "rest api"
                },

                ["Backend Developer"] = new()
                {
                    "c#", "java", "python", "sql",
                    "rest api", "entity framework", "git", "github"
                },

                ["Data Analyst"] = new()
                {
                    "python", "sql", "data analysis",
                    "power bi", "excel"
                },

                ["AI/ML Developer"] = new()
                {
                    "python", "machine learning",
                    "artificial intelligence", "data analysis", "git"
                },

                ["Cloud / DevOps Engineer"] = new()
                {
                    "docker", "kubernetes", "azure", "aws",
                    "gcp", "ci/cd", "linux", "git"
                }
            };

            List<string> targetSkills =
                careerSkills.TryGetValue(careerGoal, out var selectedSkills)
                    ? selectedSkills
                    : new List<string>();

            // Calculate career skills not found in the user's skills.
            List<string> careerGaps = targetSkills
                .Where(required =>
                    !userSkills.Any(userSkill =>
                        SkillsMatch(userSkill, required)))
                .ToList();

            // Combine saved analysis gaps and career gaps.
            List<string> missingSkills = analysisMissingSkills
                .Concat(careerGaps)
                .Distinct()
                .ToList();

            List<Course> courses = _context.Courses
                .AsNoTracking()
                .ToList();

            var scoredCourses = new List<(Course Course, int GapScore, int CareerScore)>();

            foreach (Course course in courses)
            {
                List<string> coveredSkills =
                    SplitSkills(course.SkillsCovered);

                if (coveredSkills.Count == 0)
                    continue;

                int gapScore = coveredSkills
                    .Distinct()
                    .Count(covered =>
                        missingSkills.Any(missing =>
                            SkillsMatch(covered, missing)));

                int careerScore = coveredSkills
                    .Distinct()
                    .Count(covered =>
                        targetSkills.Any(target =>
                            SkillsMatch(covered, target)));

                // Only recommend resources relevant to a skill gap
                // or to the user's recommended career direction.
                if (gapScore > 0 || careerScore > 0)
                {
                    scoredCourses.Add(
                        (course, gapScore, careerScore));
                }
            }

            return scoredCourses
                .OrderByDescending(x => x.GapScore)
                .ThenByDescending(x => x.CareerScore)
                .ThenBy(x => x.Course.CourseID)
                .Take(6)
                .Select(item => new LearningRecommendation
                {
                    Provider = "External Learning Resource",
                    Title = item.Course.CourseName,

                    Description = item.GapScore > 0
                        ? "This resource can help strengthen one or more skills identified as areas for improvement in your resume or career path."
                        : $"This resource covers skills relevant to your {careerGoal} career direction.",

                    RecommendedFor = item.GapScore > 0
                        ? "Skills to improve: " +
                          string.Join(", ",
                              SplitSkills(item.Course.SkillsCovered)
                                  .Where(covered =>
                                      missingSkills.Any(missing =>
                                          SkillsMatch(covered, missing))))
                        : $"Career-related skills for {careerGoal}.",

                    Priority = item.GapScore >= 2
                        ? "HIGH"
                        : item.GapScore == 1
                            ? "RECOMMENDED"
                            : "CAREER RELATED",

                    Icon = "bi-book",
                    ResourceLabel = "External Learning Resource",
                    ResourceUrl = item.Course.CourseLink ?? ""
                })
                .ToList();
        }

        private bool SkillsMatch(string first, string second)
        {
            string a = NormalizeSkill(first);
            string b = NormalizeSkill(second);

            if (a == b)
                return true;

            // Avoid treating short skill names such as "AI"
            // as matches inside unrelated words.
            if (a.Length <= 2 || b.Length <= 2)
                return false;

            return a.Contains(b) || b.Contains(a);
        }

        private int CountMatches(
            List<string> skills,
            params string[] targetSkills)
        {
            return targetSkills
                .Count(target =>
                    skills.Any(skill =>
                        SkillsMatch(skill, target)));
        }
    }

    public class LearningRecommendation
    {
        public string Provider { get; set; } = "";
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string RecommendedFor { get; set; } = "";
        public string Priority { get; set; } = "RECOMMENDED";
        public string Icon { get; set; } = "bi-book";
        public string ResourceLabel { get; set; } = "Learning Resource";
        public string ResourceUrl { get; set; } = "";
    }
}
