using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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

        public string CareerGoal { get; set; } = "Software Developer";

        public string RecommendationMessage { get; set; } =
            "Learning resources are selected based on the skills identified from your resume.";

        public List<LearningRecommendation> Recommendations { get; set; }
            = new List<LearningRecommendation>();

        public void OnGet()
        {
            // Check login
            string? email = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(email))
            {
                Response.Redirect("/Account/Login");
                return;
            }

            // Get current user
            User? currentUser = _context.Users
                .FirstOrDefault(u => u.Email == email);

            if (currentUser == null)
            {
                Response.Redirect("/Account/Login");
                return;
            }

            // If no resume skills are available
            if (string.IsNullOrWhiteSpace(currentUser.Skills))
            {
                CareerGoal = "Software Developer";

                RecommendationMessage =
                    "Analyze your resume first so SkillBridge can recommend learning resources based on your skills.";

                Recommendations = GetDefaultRecommendations();

                return;
            }

            // Read skills saved by Resume Analyzer
            List<string> userSkills = currentUser.Skills
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim().ToLower())
                .Distinct()
                .ToList();

            // Determine career direction
            CareerGoal = DetermineCareerGoal(userSkills);

            // Generate personalized recommendations from SQL
            Recommendations = GetRecommendations(userSkills, CareerGoal);

            RecommendationMessage =
                $"These learning resources are recommended based on your resume skills and your {CareerGoal} career direction.";
        }


        // =========================================================
        // DETERMINE CAREER GOAL
        // =========================================================

        private string DetermineCareerGoal(List<string> skills)
        {
            int frontendScore = CountMatches(
                skills,
                "html",
                "css",
                "javascript",
                "typescript",
                "react",
                "angular",
                "vue"
            );

            int dotnetScore = CountMatches(
                skills,
                "c#",
                "asp.net core",
                "asp.net",
                "entity framework",
                "sql"
            );

            int backendScore = CountMatches(
                skills,
                "c#",
                "java",
                "python",
                "sql",
                "rest api",
                "entity framework"
            );

            int dataScore = CountMatches(
                skills,
                "python",
                "sql",
                "data analysis",
                "power bi",
                "excel"
            );

            int aiScore = CountMatches(
                skills,
                "python",
                "machine learning",
                "ml",
                "artificial intelligence",
                "ai",
                "data analysis"
            );

            int cloudScore = CountMatches(
                skills,
                "docker",
                "kubernetes",
                "azure",
                "aws",
                "gcp",
                "ci/cd",
                "linux"
            );

            var scores = new Dictionary<string, int>
            {
                { "Frontend Developer", frontendScore },
                { ".NET Developer", dotnetScore },
                { "Backend Developer", backendScore },
                { "Data Analyst", dataScore },
                { "AI/ML Developer", aiScore },
                { "Cloud / DevOps Engineer", cloudScore }
            };

            return scores
                .OrderByDescending(x => x.Value)
                .First()
                .Key;
        }


        // =========================================================
        // GET PERSONALIZED RECOMMENDATIONS FROM SQL DATABASE
        // =========================================================

        private List<LearningRecommendation> GetRecommendations(
            List<string> skills,
            string careerGoal)
        {
            // Skills generally useful for each career direction
            var careerSkills = new Dictionary<string, List<string>>
            {
                {
                    ".NET Developer",
                    new List<string>
                    {
                        "c#",
                        "asp.net core",
                        "entity framework",
                        "sql",
                        "rest api",
                        "git",
                        "github",
                        "azure"
                    }
                },

                {
                    "Frontend Developer",
                    new List<string>
                    {
                        "javascript",
                        "react",
                        "typescript",
                        "html",
                        "css",
                        "git",
                        "github",
                        "rest api"
                    }
                },

                {
                    "Backend Developer",
                    new List<string>
                    {
                        "c#",
                        "java",
                        "python",
                        "sql",
                        "rest api",
                        "entity framework",
                        "git",
                        "github"
                    }
                },

                {
                    "Data Analyst",
                    new List<string>
                    {
                        "python",
                        "sql",
                        "data analysis",
                        "power bi",
                        "excel"
                    }
                },

                {
                    "AI/ML Developer",
                    new List<string>
                    {
                        "python",
                        "machine learning",
                        "artificial intelligence",
                        "data analysis",
                        "git"
                    }
                },

                {
                    "Cloud / DevOps Engineer",
                    new List<string>
                    {
                        "docker",
                        "kubernetes",
                        "azure",
                        "aws",
                        "gcp",
                        "ci/cd",
                        "linux",
                        "git"
                    }
                }
            };

            // Get required skills for selected career
            List<string> requiredSkills;

            if (careerSkills.ContainsKey(careerGoal))
            {
                requiredSkills = careerSkills[careerGoal];
            }
            else
            {
                requiredSkills = new List<string>
                {
                    "c#",
                    "sql",
                    "rest api",
                    "git",
                    "github"
                };
            }

            // Find missing skills
            List<string> missingSkills = requiredSkills
                .Where(required =>
                    !skills.Any(userSkill =>
                        userSkill.Contains(required.ToLower())))
                .ToList();

            // Get courses from SQL Server
            List<Course> courses = _context.Courses
                .ToList();

            var matchedCourses = new List<(Course Course, int MatchCount)>();

            foreach (Course course in courses)
            {
                if (string.IsNullOrWhiteSpace(course.SkillsCovered))
                {
                    continue;
                }

                List<string> courseSkills = course.SkillsCovered
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim().ToLower())
                    .ToList();

                int matchCount = 0;

                foreach (string missingSkill in missingSkills)
                {
                    if (courseSkills.Any(courseSkill =>
                        courseSkill.Contains(missingSkill) ||
                        missingSkill.Contains(courseSkill)))
                    {
                        matchCount++;
                    }
                }

                if (matchCount > 0)
                {
                    matchedCourses.Add((course, matchCount));
                }
            }

            // Sort resources according to skill-gap matches
            var orderedCourses = matchedCourses
                .OrderByDescending(x => x.MatchCount)
                .ThenBy(x => x.Course.CourseID)
                .Take(6)
                .ToList();

            var recommendations = new List<LearningRecommendation>();

            foreach (var item in orderedCourses)
            {
                Course course = item.Course;

                recommendations.Add(
                    new LearningRecommendation
                    {
                        Provider = "External Learning Resource",

                        Title = course.CourseName,

                        Description =
                            $"This learning resource can help improve skills related to your {careerGoal} career direction.",

                        RecommendedFor =
                            $"Recommended to strengthen skills required for {careerGoal}.",

                        Priority =
                            item.MatchCount >= 2
                                ? "HIGH"
                                : "RECOMMENDED",

                        Icon = "bi-book",

                        ResourceLabel = "External Learning Resource",

                        ResourceUrl = course.CourseLink ?? string.Empty
                    }
                );
            }

            // If no matching resources were found,
            // show general resources from SQL.
            if (recommendations.Count == 0)
            {
                recommendations = GetDefaultRecommendations();
            }

            return recommendations;
        }


        // =========================================================
        // DEFAULT RECOMMENDATIONS FROM SQL DATABASE
        // =========================================================

        private List<LearningRecommendation> GetDefaultRecommendations()
        {
            List<Course> courses = _context.Courses
                .OrderBy(c => c.CourseID)
                .Take(4)
                .ToList();

            var recommendations = new List<LearningRecommendation>();

            foreach (Course course in courses)
            {
                recommendations.Add(
                    new LearningRecommendation
                    {
                        Provider = "External Learning Resource",

                        Title = course.CourseName,

                        Description =
                            "This external learning resource can help you build and improve your technical skills.",

                        RecommendedFor =
                            "Software developers and learners building professional skills.",

                        Priority = "RECOMMENDED",

                        Icon = "bi-book",

                        ResourceLabel = "External Learning Resource",

                        ResourceUrl = course.CourseLink ?? string.Empty
                    }
                );
            }

            return recommendations;
        }


        // =========================================================
        // COUNT SKILL MATCHES
        // =========================================================

        private int CountMatches(
            List<string> skills,
            params string[] targetSkills)
        {
            int count = 0;

            foreach (string target in targetSkills)
            {
                if (skills.Any(skill =>
                    skill.Contains(target.ToLower())))
                {
                    count++;
                }
            }

            return count;
        }
    }


    // =============================================================
    // LEARNING RECOMMENDATION MODEL
    // =============================================================

    public class LearningRecommendation
    {
        public string Provider { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string RecommendedFor { get; set; } = string.Empty;

        public string Priority { get; set; } = "RECOMMENDED";

        public string Icon { get; set; } = "bi-book";

        public string ResourceLabel { get; set; } = "Learning Resource";

        public string ResourceUrl { get; set; } = string.Empty;
    }
}