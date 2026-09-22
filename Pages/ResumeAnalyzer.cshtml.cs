using SkillBridge.Data;
using SkillBridge.Models;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using UglyToad.PdfPig;

namespace SkillBridge.Pages
{
    public class ResumeAnalyzerModel : PageModel
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ApplicationDbContext _context;

        public ResumeAnalyzerModel(
            IWebHostEnvironment environment,
            ApplicationDbContext context)
        {
            _environment = environment;
            _context = context;
        }

        [BindProperty]
        public IFormFile? ResumeFile { get; set; }

        public string? UploadedFileName { get; set; }

        public string? SuccessMessage { get; set; }

        public string? ErrorMessage { get; set; }

        // =========================================================
        // ANALYSIS RESULTS
        // =========================================================

        public int ResumeScore { get; set; }

        public List<string> DetectedSkills { get; set; } = new();

        public List<string> MissingSkills { get; set; } = new();

        public List<string> Suggestions { get; set; } = new();

        public List<RecommendedJob> RecommendedJobs { get; set; } = new();

        public bool AnalysisCompleted { get; set; }


        // =========================================================
        // GET
        // =========================================================

        public void OnGet()
        {
        }


        // =========================================================
        // POST - ANALYZE RESUME
        // =========================================================

        public async Task<IActionResult> OnPostAsync()
        {
            if (ResumeFile == null || ResumeFile.Length == 0)
            {
                ErrorMessage = "Please choose a resume before analyzing.";
                return Page();
            }

            var allowedExtensions = new[]
            {
                ".pdf",
                ".doc",
                ".docx"
            };

            var extension = Path.GetExtension(ResumeFile.FileName)
                .ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                ErrorMessage =
                    "Invalid file format. Please upload a PDF, DOC, or DOCX file.";

                return Page();
            }

            if (ResumeFile.Length > 10 * 1024 * 1024)
            {
                ErrorMessage =
                    "The resume must be smaller than 10 MB.";

                return Page();
            }


            // =====================================================
            // CHECK LOGGED-IN USER
            // =====================================================

            var userEmail = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(userEmail))
            {
                ErrorMessage =
                    "Your session has expired. Please login again.";

                return Page();
            }

            var currentUser = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Email.ToLower() == userEmail.Trim().ToLower());

            if (currentUser == null)
            {
                ErrorMessage =
                    "User account could not be found.";

                return Page();
            }


            // =====================================================
            // CREATE UPLOAD DIRECTORY
            // =====================================================

            var uploadFolder = Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "resumes"
            );

            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }


            // =====================================================
            // SAFE FILE NAME
            // =====================================================

            var savedFileName =
                $"{Guid.NewGuid()}{extension}";

            var physicalFilePath =
                Path.Combine(
                    uploadFolder,
                    savedFileName
                );


            // =====================================================
            // SAVE PHYSICAL RESUME FILE
            // =====================================================

            using (var stream = new FileStream(
                physicalFilePath,
                FileMode.Create))
            {
                await ResumeFile.CopyToAsync(stream);
            }

            UploadedFileName = ResumeFile.FileName;


            // =====================================================
            // SAVE RESUME PATH TO USER
            // =====================================================

            currentUser.ResumePath = Path.Combine(
                "uploads",
                "resumes",
                savedFileName
            ).Replace("\\", "/");

            _context.Users.Update(currentUser);

            await _context.SaveChangesAsync();


            // =====================================================
            // CREATE RESUME DATABASE RECORD
            // =====================================================

            var resume = new Resume
            {
                UserID = currentUser.UserID,
                FileName = ResumeFile.FileName,
                FilePath = currentUser.ResumePath,
                UploadDate = DateTime.Now
            };

            _context.Resumes.Add(resume);

            await _context.SaveChangesAsync();


            // =====================================================
            // EXTRACT RESUME TEXT
            // =====================================================

            string resumeText;

            try
            {
                resumeText = ExtractResumeText(
                    physicalFilePath,
                    extension
                );
            }
            catch (Exception ex)
            {
                ErrorMessage =
                    "We could not read the contents of your resume. " +
                    "Please make sure the file is not corrupted.";

                Console.WriteLine(ex);

                return Page();
            }


            if (string.IsNullOrWhiteSpace(resumeText))
            {
                ErrorMessage =
                    "We could not find readable text in your resume.";

                return Page();
            }


            // =====================================================
            // ANALYZE RESUME
            // =====================================================

            await AnalyzeResume(resumeText);

            AnalysisCompleted = true;

            SuccessMessage =
                "Resume uploaded and analyzed successfully.";


            // =====================================================
            // SAVE RESUME ANALYSIS TO DATABASE
            // =====================================================

            var resumeAnalysis = new ResumeAnalysis
            {
                ResumeID = resume.ResumeID,

                ATSScore = ResumeScore,

                ExtractedSkills =
                    string.Join(", ", DetectedSkills),

                MissingSkills =
                    string.Join(", ", MissingSkills),

                ExperienceSummary =
                    resumeText.ToLowerInvariant().Contains("experience")
                        ? "Experience section detected in resume."
                        : "Experience section not detected.",

                EducationSummary =
                    resumeText.ToLowerInvariant().Contains("education")
                    || resumeText.ToLowerInvariant().Contains("degree")
                    || resumeText.ToLowerInvariant().Contains("bachelor")
                    || resumeText.ToLowerInvariant().Contains("master")
                        ? "Education information detected in resume."
                        : "Education information not detected.",

                Suggestions =
                    string.Join(" | ", Suggestions),

                AnalysisDate = DateTime.Now
            };

            _context.ResumeAnalysis.Add(resumeAnalysis);

            await _context.SaveChangesAsync();


            // =====================================================
            // SAVE RECOMMENDATIONS FOR JOBS PAGE
            // =====================================================

            TempData["RecommendedJobs"] =
                JsonSerializer.Serialize(RecommendedJobs);

            TempData["DetectedSkills"] =
                JsonSerializer.Serialize(DetectedSkills);

            TempData["ResumeScore"] =
                ResumeScore;


            return Page();
        }


        // =========================================================
        // EXTRACT RESUME TEXT
        // =========================================================

        private string ExtractResumeText(
            string filePath,
            string extension)
        {
            return extension switch
            {
                ".pdf" => ExtractPdfText(filePath),
                ".docx" => ExtractDocxText(filePath),
                ".doc" => string.Empty,
                _ => string.Empty
            };
        }


        // =========================================================
        // PDF TEXT EXTRACTION
        // =========================================================

        private string ExtractPdfText(string filePath)
        {
            var text = new System.Text.StringBuilder();

            using var document =
                PdfDocument.Open(filePath);

            foreach (var page in document.GetPages())
            {
                text.AppendLine(page.Text);
            }

            return text.ToString();
        }


        // =========================================================
        // DOCX TEXT EXTRACTION
        // =========================================================

        private string ExtractDocxText(string filePath)
        {
            var text = new System.Text.StringBuilder();

            using var document =
                WordprocessingDocument.Open(
                    filePath,
                    false
                );

            var body =
                document.MainDocumentPart?
                    .Document?
                    .Body;

            if (body == null)
            {
                return string.Empty;
            }

            foreach (var paragraph in body.Elements<
                DocumentFormat.OpenXml.Wordprocessing.Paragraph>())
            {
                text.AppendLine(
                    paragraph.InnerText
                );
            }

            return text.ToString();
        }


        // =========================================================
        // RESUME ANALYZER
        // =========================================================

        private async Task AnalyzeResume(string resumeText)
        {
            var text =
                resumeText.ToLowerInvariant();


            // =====================================================
            // SKILL DATABASE
            // =====================================================

            var skillDatabase =
                new Dictionary<string, string[]>
                {
                    {
                        "C#",
                        new[]
                        {
                            "c#",
                            "c sharp"
                        }
                    },

                    {
                        "ASP.NET Core",
                        new[]
                        {
                            "asp.net core",
                            "aspnet core"
                        }
                    },

                    {
                        "ASP.NET MVC",
                        new[]
                        {
                            "asp.net mvc",
                            "aspnet mvc"
                        }
                    },

                    {
                        "SQL",
                        new[]
                        {
                            "sql",
                            "sql server",
                            "mysql",
                            "postgresql"
                        }
                    },

                    {
                        "Java",
                        new[]
                        {
                            "java",
                            "java se",
                            "java ee"
                        }
                    },

                    {
                        "JavaScript",
                        new[]
                        {
                            "javascript",
                            "java script"
                        }
                    },

                    {
                        "TypeScript",
                        new[]
                        {
                            "typescript"
                        }
                    },

                    {
                        "HTML",
                        new[]
                        {
                            "html",
                            "html5"
                        }
                    },

                    {
                        "CSS",
                        new[]
                        {
                            "css",
                            "css3"
                        }
                    },

                    {
                        "Bootstrap",
                        new[]
                        {
                            "bootstrap"
                        }
                    },

                    {
                        "React",
                        new[]
                        {
                            "react",
                            "react.js",
                            "reactjs"
                        }
                    },

                    {
                        "Angular",
                        new[]
                        {
                            "angular",
                            "angularjs"
                        }
                    },

                    {
                        "Vue.js",
                        new[]
                        {
                            "vue.js",
                            "vuejs"
                        }
                    },

                    {
                        "Python",
                        new[]
                        {
                            "python"
                        }
                    },

                    {
                        "Django",
                        new[]
                        {
                            "django"
                        }
                    },

                    {
                        "Flask",
                        new[]
                        {
                            "flask"
                        }
                    },

                    {
                        "Entity Framework",
                        new[]
                        {
                            "entity framework",
                            "entityframework",
                            "ef core"
                        }
                    },

                    {
                        "REST API",
                        new[]
                        {
                            "rest api",
                            "restful api",
                            "web api"
                        }
                    },

                    {
                        "Git",
                        new[]
                        {
                            "git"
                        }
                    },

                    {
                        "GitHub",
                        new[]
                        {
                            "github"
                        }
                    },

                    {
                        "Docker",
                        new[]
                        {
                            "docker"
                        }
                    },

                    {
                        "Kubernetes",
                        new[]
                        {
                            "kubernetes",
                            "k8s"
                        }
                    },

                    {
                        "Azure",
                        new[]
                        {
                            "azure",
                            "microsoft azure"
                        }
                    },

                    {
                        "AWS",
                        new[]
                        {
                            "aws",
                            "amazon web services"
                        }
                    },

                    {
                        "Google Cloud",
                        new[]
                        {
                            "google cloud",
                            "gcp"
                        }
                    },

                    {
                        "CI/CD",
                        new[]
                        {
                            "ci/cd",
                            "continuous integration",
                            "continuous deployment"
                        }
                    },

                    {
                        "Machine Learning",
                        new[]
                        {
                            "machine learning",
                            "ml"
                        }
                    },

                    {
                        "Artificial Intelligence",
                        new[]
                        {
                            "artificial intelligence",
                            "ai"
                        }
                    },

                    {
                        "Data Analysis",
                        new[]
                        {
                            "data analysis",
                            "data analytics"
                        }
                    },

                    {
                        "Power BI",
                        new[]
                        {
                            "power bi"
                        }
                    },

                    {
                        "Excel",
                        new[]
                        {
                            "microsoft excel",
                            "excel"
                        }
                    },

                    {
                        "Communication",
                        new[]
                        {
                            "communication skills",
                            "communication"
                        }
                    },

                    {
                        "Leadership",
                        new[]
                        {
                            "leadership",
                            "team leader"
                        }
                    }
                };


            // =====================================================
            // DETECT SKILLS
            // =====================================================

            DetectedSkills.Clear();

            foreach (var skill in skillDatabase)
            {
                foreach (var keyword in skill.Value)
                {
                    if (text.Contains(keyword))
                    {
                        if (!DetectedSkills.Contains(skill.Key))
                        {
                            DetectedSkills.Add(skill.Key);
                        }

                        break;
                    }
                }
            }


            // =====================================================
            // REQUIRED SKILLS
            // =====================================================

            var importantSkills =
                new List<string>
                {
                    "Git",
                    "GitHub",
                    "SQL",
                    "REST API",
                    "Communication"
                };


            // =====================================================
            // MISSING SKILLS
            // =====================================================

            MissingSkills =
                importantSkills
                    .Where(skill =>
                        !DetectedSkills.Contains(skill))
                    .ToList();


            // =====================================================
            // RESUME SCORE
            // =====================================================

            int score = 40;

            score += Math.Min(
                DetectedSkills.Count * 2,
                20
            );

            if (text.Contains("project") ||
                text.Contains("projects"))
            {
                score += 10;
            }

            if (text.Contains("experience") ||
                text.Contains("work experience"))
            {
                score += 10;
            }

            if (text.Contains("education") ||
                text.Contains("degree") ||
                text.Contains("bachelor") ||
                text.Contains("master"))
            {
                score += 5;
            }

            if (text.Contains("github"))
            {
                score += 5;
            }

            if (text.Contains("linkedin"))
            {
                score += 5;
            }

            ResumeScore =
                Math.Min(score, 100);


            // =====================================================
            // SUGGESTIONS
            // =====================================================

            Suggestions.Clear();

            if (!text.Contains("summary") &&
                !text.Contains("profile"))
            {
                Suggestions.Add(
                    "Add a short professional summary describing your career direction."
                );
            }

            if (!text.Contains("project"))
            {
                Suggestions.Add(
                    "Add practical projects that demonstrate your technical skills."
                );
            }

            if (!text.Contains("github"))
            {
                Suggestions.Add(
                    "Add your GitHub profile so employers can review your projects."
                );
            }

            if (!text.Contains("linkedin"))
            {
                Suggestions.Add(
                    "Add your LinkedIn profile to strengthen your professional presence."
                );
            }

            if (!text.Contains("achievement") &&
                !text.Contains("increased") &&
                !text.Contains("improved") &&
                !text.Contains("%"))
            {
                Suggestions.Add(
                    "Add measurable achievements such as percentages, numbers, or performance improvements."
                );
            }

            if (MissingSkills.Count > 0)
            {
                Suggestions.Add(
                    "Consider learning " +
                    string.Join(", ", MissingSkills) +
                    " to strengthen your employability."
                );
            }

            if (Suggestions.Count == 0)
            {
                Suggestions.Add(
                    "Your resume has a strong foundation. Continue adding measurable achievements and relevant projects."
                );
            }


            // =====================================================
            // SAVE DETECTED SKILLS TO USER PROFILE
            // =====================================================

            var userEmail =
                HttpContext.Session.GetString("UserEmail");

            if (!string.IsNullOrEmpty(userEmail))
            {
                var currentUser =
                    await _context.Users
                        .FirstOrDefaultAsync(u =>
                            u.Email.ToLower() ==
                            userEmail.Trim().ToLower());

                if (currentUser != null)
                {
                    currentUser.Skills =
                        string.Join(
                            ", ",
                            DetectedSkills
                        );

                    await _context.SaveChangesAsync();
                }
            }


            // =====================================================
            // GENERATE PERSONALIZED JOBS FROM SQL DATABASE
            // =====================================================

            await GenerateRecommendedJobs();
        }


        // =========================================================
        // PERSONALIZED JOB RECOMMENDATIONS FROM SQL
        // =========================================================

        private async Task GenerateRecommendedJobs()
        {
            RecommendedJobs.Clear();


            // =====================================================
            // GET MASTER JOBS FROM SQL DATABASE
            // =====================================================

            var jobs =
                await _context.Jobs
                    .AsNoTracking()
                    .ToListAsync();


            // =====================================================
            // MATCH SQL JOBS WITH RESUME SKILLS
            // =====================================================

            foreach (var job in jobs)
            {
                if (string.IsNullOrWhiteSpace(job.RequiredSkills))
                {
                    continue;
                }

                var requiredSkills =
                    job.RequiredSkills
                        .Split(
                            ',',
                            StringSplitOptions.RemoveEmptyEntries
                        )
                        .Select(skill => skill.Trim())
                        .Where(skill => !string.IsNullOrWhiteSpace(skill))
                        .ToList();


                if (requiredSkills.Count == 0)
                {
                    continue;
                }


                // -------------------------------------------------
                // FIND MATCHED SKILLS
                // -------------------------------------------------

                var matchedSkills =
                    requiredSkills
                        .Where(requiredSkill =>
                            DetectedSkills.Any(detectedSkill =>
                                detectedSkill.Equals(
                                    requiredSkill,
                                    StringComparison.OrdinalIgnoreCase
                                )))
                        .ToList();


                // -------------------------------------------------
                // ONLY RECOMMEND JOBS WITH AT LEAST ONE MATCH
                // -------------------------------------------------

                if (matchedSkills.Count == 0)
                {
                    continue;
                }


                // -------------------------------------------------
                // CALCULATE MATCH PERCENTAGE
                // -------------------------------------------------

                int matchPercentage =
                    (int)Math.Round(
                        (double)matchedSkills.Count /
                        requiredSkills.Count *
                        100
                    );

                matchPercentage =
                    Math.Max(
                        40,
                        Math.Min(
                            matchPercentage,
                            99
                        )
                    );


                // -------------------------------------------------
                // FIND MISSING SKILLS
                // -------------------------------------------------

                var missingSkills =
                    requiredSkills
                        .Where(requiredSkill =>
                            !DetectedSkills.Any(detectedSkill =>
                                detectedSkill.Equals(
                                    requiredSkill,
                                    StringComparison.OrdinalIgnoreCase
                                )))
                        .ToList();


                // -------------------------------------------------
                // WHY RECOMMENDED
                // -------------------------------------------------

                var whyRecommended =
                    "Your resume matches skills required for this role. " +
                    "Matching skills found in your resume: " +
                    string.Join(", ", matchedSkills) +
                    ".";


                // -------------------------------------------------
                // CREATE RECOMMENDED JOB DTO
                // -------------------------------------------------

                RecommendedJobs.Add(
                    new RecommendedJob
                    {
                        Id =
                            job.JobID.ToString(),

                        Title =
                            job.JobTitle,

                        Company =
                            job.CompanyName,

                        Location =
                            job.Location,

                        JobType =
                            job.JobType,

                        MatchPercentage =
                            matchPercentage,

                        Skills =
                            requiredSkills,

                        JobUrl =
                            job.ApplyLink ?? "",

                        Description =
                            job.Description,

                        MissingSkills =
                            missingSkills,

                        WhyRecommended =
                            whyRecommended
                    }
                );
            }


            // =====================================================
            // SORT BY MATCH PERCENTAGE
            // =====================================================

            RecommendedJobs =
                RecommendedJobs
                    .OrderByDescending(
                        job => job.MatchPercentage
                    )
                    .Take(6)
                    .ToList();


            // =====================================================
            // FALLBACK
            // =====================================================

            if (RecommendedJobs.Count == 0 && jobs.Count > 0)
            {
                var fallbackJob =
                    jobs
                        .OrderBy(job => job.JobID)
                        .First();


                var requiredSkills =
                    fallbackJob.RequiredSkills
                        .Split(
                            ',',
                            StringSplitOptions.RemoveEmptyEntries
                        )
                        .Select(skill => skill.Trim())
                        .ToList();


                RecommendedJobs.Add(
                    new RecommendedJob
                    {
                        Id =
                            fallbackJob.JobID.ToString(),

                        Title =
                            fallbackJob.JobTitle,

                        Company =
                            fallbackJob.CompanyName,

                        Location =
                            fallbackJob.Location,

                        JobType =
                            fallbackJob.JobType,

                        MatchPercentage =
                            40,

                        Skills =
                            requiredSkills,

                        JobUrl =
                            fallbackJob.ApplyLink ?? "",

                        Description =
                            fallbackJob.Description,

                        MissingSkills =
                            requiredSkills,

                        WhyRecommended =
                            "This general job recommendation is shown because no specific role matched the skills detected in your resume."
                    }
                );
            }
        }
    }


    // =============================================================
    // RECOMMENDED JOB MODEL
    // =============================================================

    public class RecommendedJob
    {
        public string Id { get; set; } = "";

        public string Title { get; set; } = "";

        public string Company { get; set; } = "";

        public string Location { get; set; } = "";

        public string JobType { get; set; } = "";

        public int MatchPercentage { get; set; }

        public List<string> Skills { get; set; } = new();

        public string JobUrl { get; set; } = "";

        public string Description { get; set; } = "";

        public List<string> MissingSkills { get; set; } = new();

        public string WhyRecommended { get; set; } = "";
    }
}