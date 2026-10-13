using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SkillBridge.Data;

namespace SkillBridge.Pages
{
    public class SkillGapModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public SkillGapModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // PAGE DATA
        // =========================================================

        public string CareerGoal { get; set; } = "";

        public string CareerDescription { get; set; } =
            "Analyze your resume to receive a personalized skill gap analysis.";

        public List<string> CurrentSkills { get; set; } = new();

        public List<SkillGapItem> SkillGaps { get; set; } = new();

        public int CurrentSkillCount { get; set; }

        public int SkillGapCount { get; set; }

        public bool HasResumeData { get; set; }


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
            // CHECK FOR ACTUAL RESUME ANALYSIS
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

                CareerGoal = "";

                CareerDescription =
                    "Analyze your resume first to receive a personalized skill gap analysis.";

                CurrentSkills.Clear();
                SkillGaps.Clear();

                CurrentSkillCount = 0;
                SkillGapCount = 0;

                return Page();
            }


            // -----------------------------------------------------
            // RESUME ANALYSIS EXISTS
            // -----------------------------------------------------

            HasResumeData = true;


            // -----------------------------------------------------
            // GET DETECTED SKILLS
            // -----------------------------------------------------

            CurrentSkills =
                currentUser.Skills!
                    .Split(
                        ',',
                        StringSplitOptions.RemoveEmptyEntries)
                    .Select(skill => skill.Trim())
                    .Where(skill => !string.IsNullOrWhiteSpace(skill))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();


            CurrentSkillCount =
                CurrentSkills.Count;


            // -----------------------------------------------------
            // DETERMINE CAREER
            // -----------------------------------------------------

            var career =
                DetermineCareer(CurrentSkills);


            CareerGoal =
                career.Name;


            CareerDescription =
                career.Description;


            // -----------------------------------------------------
            // GENERATE PERSONALIZED SKILL GAPS
            // -----------------------------------------------------

            GenerateSkillGaps(
                CurrentSkills,
                career
            );


            SkillGapCount =
                SkillGaps.Count;


            return Page();
        }


        // =========================================================
        // DETERMINE PERSONALIZED CAREER
        // =========================================================

        private CareerDefinition DetermineCareer(
            List<string> skills)
        {
            var careers =
                new List<CareerDefinition>
                {
                    CreateDotNetCareer(),
                    CreateFrontendCareer(),
                    CreateBackendCareer(),
                    CreateDataAnalystCareer(),
                    CreateAIMLCareer(),
                    CreateCloudDevOpsCareer()
                };


            CareerDefinition? bestCareer = null;

            int bestScore = 0;


            foreach (var career in careers)
            {
                int score =
                    career.RequiredSkills.Count(
                        requiredSkill =>
                            skills.Any(
                                userSkill =>
                                    string.Equals(
                                        userSkill,
                                        requiredSkill,
                                        StringComparison.OrdinalIgnoreCase
                                    )
                            )
                    );


                if (score > bestScore)
                {
                    bestScore = score;

                    bestCareer = career;
                }
            }


            return bestCareer ??
                   CreateDefaultCareer();
        }


        // =========================================================
        // GENERATE SKILL GAPS
        // =========================================================

        private void GenerateSkillGaps(
            List<string> currentSkills,
            CareerDefinition career)
        {
            SkillGaps.Clear();


            foreach (var skill in career.RequiredSkills)
            {
                bool alreadyHaveSkill =
                    currentSkills.Any(
                        current =>
                            string.Equals(
                                current,
                                skill,
                                StringComparison.OrdinalIgnoreCase
                            )
                    );


                if (!alreadyHaveSkill)
                {
                    string priority =
                        GetPriority(
                            skill,
                            career.Name
                        );


                    string explanation =
                        GetSkillExplanation(
                            skill,
                            career.Name
                        );


                    SkillGaps.Add(
                        new SkillGapItem
                        {
                            Skill = skill,

                            Priority = priority,

                            Explanation = explanation
                        }
                    );
                }
            }
        }


        // =========================================================
        // PRIORITY
        // =========================================================

        private string GetPriority(
            string skill,
            string career)
        {
            if (skill == "C#" ||
                skill == "ASP.NET Core" ||
                skill == "SQL" ||
                skill == "JavaScript" ||
                skill == "Python")
            {
                return "High";
            }


            return "Medium";
        }


        // =========================================================
        // SKILL EXPLANATION
        // =========================================================

        private string GetSkillExplanation(
            string skill,
            string career)
        {
            return skill switch
            {
                "C#" =>
                    "Important for building modern applications and backend solutions.",

                "ASP.NET Core" =>
                    "Useful for developing scalable web applications and APIs.",

                "SQL" =>
                    "Helps you work with databases and manage application data.",

                "Entity Framework" =>
                    "Useful for connecting .NET applications with databases.",

                "REST API" =>
                    "Important for connecting frontend applications with backend services.",

                "HTML" =>
                    "A fundamental technology for creating web page structure.",

                "CSS" =>
                    "Important for creating responsive and user-friendly interfaces.",

                "JavaScript" =>
                    "Essential for creating interactive and dynamic web applications.",

                "React" =>
                    "A popular framework library for building modern component-based interfaces.",

                "TypeScript" =>
                    "Improves large frontend applications with stronger type safety.",

                "Python" =>
                    "Widely used for backend development, data analysis and AI applications.",

                "Machine Learning" =>
                    "Important for developing predictive and intelligent applications.",

                "Artificial Intelligence" =>
                    "Helps in building intelligent systems and AI-powered applications.",

                "Data Analysis" =>
                    "Useful for finding insights and patterns from datasets.",

                "Power BI" =>
                    "Helps create dashboards and communicate data insights.",

                "Excel" =>
                    "Useful for data preparation, analysis and reporting.",

                "Docker" =>
                    "Important for packaging and deploying applications consistently.",

                "Kubernetes" =>
                    "Useful for managing containerized applications at scale.",

                "Azure" =>
                    "Provides cloud services for deploying and managing applications.",

                "AWS" =>
                    "A major cloud platform used for application deployment and infrastructure.",

                "CI/CD" =>
                    "Automates software testing and deployment processes.",

                _ =>
                    $"Developing {skill} can strengthen your readiness for the {career} career path."
            };
        }


        // =========================================================
        // .NET / SOFTWARE DEVELOPER
        // =========================================================

        private CareerDefinition CreateDotNetCareer()
        {
            return new CareerDefinition
            {
                Name = "Software Developer",

                Description =
                    "Your resume shows skills related to software and web application development. These additional skills can strengthen your preparation for a software developer role.",

                RequiredSkills = new List<string>
                {
                    "C#",
                    "ASP.NET Core",
                    "SQL",
                    "Entity Framework",
                    "REST API",
                    "Git",
                    "GitHub"
                }
            };
        }


        // =========================================================
        // FRONTEND
        // =========================================================

        private CareerDefinition CreateFrontendCareer()
        {
            return new CareerDefinition
            {
                Name = "Frontend Developer",

                Description =
                    "Your resume shows frontend-related skills. Strengthening these technologies can improve your readiness for frontend development roles.",

                RequiredSkills = new List<string>
                {
                    "HTML",
                    "CSS",
                    "JavaScript",
                    "React",
                    "TypeScript",
                    "Git",
                    "GitHub"
                }
            };
        }


        // =========================================================
        // BACKEND
        // =========================================================

        private CareerDefinition CreateBackendCareer()
        {
            return new CareerDefinition
            {
                Name = "Backend Developer",

                Description =
                    "Your resume contains programming or backend-related skills. These additional technologies can strengthen your backend development profile.",

                RequiredSkills = new List<string>
                {
                    "C#",
                    "SQL",
                    "REST API",
                    "Entity Framework",
                    "Git",
                    "GitHub"
                }
            };
        }


        // =========================================================
        // DATA ANALYST
        // =========================================================

        private CareerDefinition CreateDataAnalystCareer()
        {
            return new CareerDefinition
            {
                Name = "Data Analyst",

                Description =
                    "Your resume contains data-related skills. Strengthening the following areas can improve your readiness for data analyst roles.",

                RequiredSkills = new List<string>
                {
                    "SQL",
                    "Python",
                    "Data Analysis",
                    "Power BI",
                    "Excel"
                }
            };
        }


        // =========================================================
        // AI / ML
        // =========================================================

        private CareerDefinition CreateAIMLCareer()
        {
            return new CareerDefinition
            {
                Name = "AI / ML Developer",

                Description =
                    "Your resume contains skills related to programming or AI. Developing these additional areas can strengthen your AI/ML career profile.",

                RequiredSkills = new List<string>
                {
                    "Python",
                    "Data Analysis",
                    "Machine Learning",
                    "Artificial Intelligence",
                    "Git"
                }
            };
        }


        // =========================================================
        // CLOUD / DEVOPS
        // =========================================================

        private CareerDefinition CreateCloudDevOpsCareer()
        {
            return new CareerDefinition
            {
                Name = "Cloud / DevOps Engineer",

                Description =
                    "Your resume contains cloud or deployment-related skills. These additional skills can strengthen your preparation for cloud and DevOps roles.",

                RequiredSkills = new List<string>
                {
                    "Linux",
                    "Docker",
                    "Kubernetes",
                    "Azure",
                    "AWS",
                    "CI/CD",
                    "Git"
                }
            };
        }


        // =========================================================
        // DEFAULT CAREER
        // =========================================================

        private CareerDefinition CreateDefaultCareer()
        {
            return new CareerDefinition
            {
                Name = "Software Developer",

                Description =
                    "Your resume contains software development related skills. These additional skills can strengthen your preparation for software development roles.",

                RequiredSkills = new List<string>
                {
                    "C#",
                    "SQL",
                    "REST API",
                    "Git",
                    "GitHub"
                }
            };
        }
    }


    // =============================================================
    // SKILL GAP ITEM
    // =============================================================

    public class SkillGapItem
    {
        public string Skill { get; set; } = "";

        public string Priority { get; set; } = "";

        public string Explanation { get; set; } = "";
    }
}