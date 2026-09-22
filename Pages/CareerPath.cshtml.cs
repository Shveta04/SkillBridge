using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SkillBridge.Data;

namespace SkillBridge.Pages
{
    public class CareerPathModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CareerPathModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // CAREER PATH DATA
        // =========================================================

        public string CareerGoal { get; set; } = "Software Developer";

        public string CareerDescription { get; set; } =
            "A recommended career direction based on the skills identified in your resume.";

        public List<CareerStep> CareerSteps { get; set; } = new();

        public bool HasResumeData { get; set; }


        // =========================================================
        // GET
        // =========================================================

        public async Task<IActionResult> OnGetAsync()
        {
            var userEmail =
                HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrWhiteSpace(userEmail))
            {
                return RedirectToPage("/Account/Login");
            }

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
            // CHECK WHETHER RESUME SKILLS EXIST
            // -----------------------------------------------------

            if (string.IsNullOrWhiteSpace(currentUser.Skills))
            {
                HasResumeData = false;

                CreateDefaultCareerPath();

                return Page();
            }

            HasResumeData = true;

            // -----------------------------------------------------
            // GET STORED SKILLS
            // -----------------------------------------------------

            var detectedSkills =
                currentUser.Skills
                    .Split(
                        ',',
                        StringSplitOptions.RemoveEmptyEntries)
                    .Select(skill => skill.Trim())
                    .ToList();

            // -----------------------------------------------------
            // DETERMINE BEST CAREER
            // -----------------------------------------------------

            var career = DetermineCareer(detectedSkills);

            CareerGoal = career.Name;

            CareerDescription = career.Description;

            CareerSteps = career.Steps;

            return Page();
        }


        // =========================================================
        // DETERMINE CAREER
        // =========================================================

        private CareerDefinition DetermineCareer(
            List<string> skills)
        {
            var careers = new List<CareerDefinition>
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
                            skills.Any(userSkill =>
                                string.Equals(
                                    userSkill,
                                    requiredSkill,
                                    StringComparison.OrdinalIgnoreCase)));

                if (score > bestScore)
                {
                    bestScore = score;
                    bestCareer = career;
                }
            }

            // -----------------------------------------------------
            // FALLBACK
            // -----------------------------------------------------

            return bestCareer ??
                   CreateDefaultCareer();
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
                    "A software development path focused on programming, web application development, backend technologies and practical software projects.",

                RequiredSkills = new List<string>
                {
                    "C#",
                    "ASP.NET Core",
                    "ASP.NET MVC",
                    "Entity Framework",
                    "SQL"
                },

                Steps = new List<CareerStep>
                {
                    new CareerStep
                    {
                        Label = "FOUNDATION",
                        Title = "Programming Fundamentals",
                        Description =
                            "Strengthen programming logic, problem solving, object-oriented programming and core software development concepts.",
                        Skills = new List<string>
                        {
                            "Programming",
                            "OOP",
                            "Problem Solving"
                        },
                        Icon = "bi-code-slash"
                    },

                    new CareerStep
                    {
                        Label = "DEVELOPMENT",
                        Title = "Web Application Development",
                        Description =
                            "Build modern web applications and strengthen your frontend and backend development knowledge.",
                        Skills = new List<string>
                        {
                            "HTML",
                            "CSS",
                            "JavaScript",
                            "ASP.NET Core"
                        },
                        Icon = "bi-laptop"
                    },

                    new CareerStep
                    {
                        Label = "ADVANCED SKILLS",
                        Title = "Backend & Database Development",
                        Description =
                            "Develop stronger backend skills by working with databases, APIs and application frameworks.",
                        Skills = new List<string>
                        {
                            "C#",
                            "SQL",
                            "Entity Framework",
                            "REST API"
                        },
                        Icon = "bi-database"
                    },

                    new CareerStep
                    {
                        Label = "PRACTICAL EXPERIENCE",
                        Title = "Build Real Projects",
                        Description =
                            "Apply your technical knowledge by creating complete projects that demonstrate your development abilities.",
                        Skills = new List<string>
                        {
                            "Projects",
                            "Git",
                            "GitHub",
                            "Problem Solving"
                        },
                        Icon = "bi-kanban"
                    }
                }
            };
        }


        // =========================================================
        // FRONTEND DEVELOPER
        // =========================================================

        private CareerDefinition CreateFrontendCareer()
        {
            return new CareerDefinition
            {
                Name = "Frontend Developer",

                Description =
                    "A career path focused on creating responsive, interactive and user-friendly web interfaces.",

                RequiredSkills = new List<string>
                {
                    "HTML",
                    "CSS",
                    "JavaScript",
                    "React",
                    "Angular",
                    "TypeScript"
                },

                Steps = new List<CareerStep>
                {
                    new CareerStep
                    {
                        Label = "FOUNDATION",
                        Title = "Web Fundamentals",
                        Description =
                            "Build a strong understanding of how websites work and learn the fundamental technologies used to create web pages.",
                        Skills = new List<string>
                        {
                            "HTML",
                            "CSS",
                            "Web Basics"
                        },
                        Icon = "bi-code-slash"
                    },

                    new CareerStep
                    {
                        Label = "DEVELOPMENT",
                        Title = "JavaScript Development",
                        Description =
                            "Develop interactive web applications using JavaScript and modern frontend development techniques.",
                        Skills = new List<string>
                        {
                            "JavaScript",
                            "DOM",
                            "ES6+"
                        },
                        Icon = "bi-laptop"
                    },

                    new CareerStep
                    {
                        Label = "FRAMEWORKS",
                        Title = "Modern Frontend Frameworks",
                        Description =
                            "Strengthen your skills with a modern frontend framework and learn component-based application development.",
                        Skills = new List<string>
                        {
                            "React",
                            "Angular",
                            "TypeScript"
                        },
                        Icon = "bi-window"
                    },

                    new CareerStep
                    {
                        Label = "PRACTICAL EXPERIENCE",
                        Title = "Build Web Projects",
                        Description =
                            "Create responsive websites and complete frontend projects to demonstrate your practical development skills.",
                        Skills = new List<string>
                        {
                            "Responsive Design",
                            "Projects",
                            "Git",
                            "GitHub"
                        },
                        Icon = "bi-kanban"
                    }
                }
            };
        }


        // =========================================================
        // BACKEND DEVELOPER
        // =========================================================

        private CareerDefinition CreateBackendCareer()
        {
            return new CareerDefinition
            {
                Name = "Backend Developer",

                Description =
                    "A career path focused on server-side development, APIs, databases and application logic.",

                RequiredSkills = new List<string>
                {
                    "C#",
                    "Java",
                    "Python",
                    "SQL",
                    "REST API"
                },

                Steps = new List<CareerStep>
                {
                    new CareerStep
                    {
                        Label = "FOUNDATION",
                        Title = "Programming Fundamentals",
                        Description =
                            "Strengthen programming logic, data structures and problem-solving skills.",
                        Skills = new List<string>
                        {
                            "Programming",
                            "Logic",
                            "Problem Solving"
                        },
                        Icon = "bi-code-slash"
                    },

                    new CareerStep
                    {
                        Label = "BACKEND",
                        Title = "Server-Side Development",
                        Description =
                            "Learn how backend applications process requests, implement business logic and communicate with clients.",
                        Skills = new List<string>
                        {
                            "C#",
                            "Java",
                            "Python",
                            "Backend"
                        },
                        Icon = "bi-server"
                    },

                    new CareerStep
                    {
                        Label = "DATABASE & API",
                        Title = "Databases & REST APIs",
                        Description =
                            "Develop skills for storing data, designing APIs and connecting applications with databases.",
                        Skills = new List<string>
                        {
                            "SQL",
                            "REST API",
                            "Database"
                        },
                        Icon = "bi-database"
                    },

                    new CareerStep
                    {
                        Label = "PRACTICAL EXPERIENCE",
                        Title = "Build Backend Projects",
                        Description =
                            "Create practical applications that demonstrate backend logic, database integration and API development.",
                        Skills = new List<string>
                        {
                            "Projects",
                            "Git",
                            "GitHub",
                            "APIs"
                        },
                        Icon = "bi-kanban"
                    }
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
                    "A career path focused on analyzing data, finding useful insights and presenting information through reports and visualizations.",

                RequiredSkills = new List<string>
                {
                    "SQL",
                    "Python",
                    "Data Analysis",
                    "Power BI",
                    "Excel"
                },

                Steps = new List<CareerStep>
                {
                    new CareerStep
                    {
                        Label = "FOUNDATION",
                        Title = "Data Fundamentals",
                        Description =
                            "Build a strong foundation in data concepts, spreadsheets and basic analytical thinking.",
                        Skills = new List<string>
                        {
                            "Excel",
                            "Data Fundamentals",
                            "Analytical Thinking"
                        },
                        Icon = "bi-bar-chart"
                    },

                    new CareerStep
                    {
                        Label = "DATA QUERYING",
                        Title = "SQL & Database Skills",
                        Description =
                            "Learn to retrieve, filter and organize information from databases using SQL.",
                        Skills = new List<string>
                        {
                            "SQL",
                            "Databases",
                            "Queries"
                        },
                        Icon = "bi-database"
                    },

                    new CareerStep
                    {
                        Label = "DATA ANALYSIS",
                        Title = "Analyze & Visualize Data",
                        Description =
                            "Develop the ability to analyze datasets and communicate insights using visualization tools.",
                        Skills = new List<string>
                        {
                            "Python",
                            "Data Analysis",
                            "Power BI"
                        },
                        Icon = "bi-graph-up"
                    },

                    new CareerStep
                    {
                        Label = "PRACTICAL EXPERIENCE",
                        Title = "Build Data Projects",
                        Description =
                            "Work with real datasets and create dashboards or analysis projects that demonstrate your skills.",
                        Skills = new List<string>
                        {
                            "Data Projects",
                            "Dashboards",
                            "Power BI",
                            "SQL"
                        },
                        Icon = "bi-kanban"
                    }
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
                    "A career path focused on Python, data analysis, machine learning and artificial intelligence applications.",

                RequiredSkills = new List<string>
                {
                    "Python",
                    "Machine Learning",
                    "Artificial Intelligence"
                },

                Steps = new List<CareerStep>
                {
                    new CareerStep
                    {
                        Label = "FOUNDATION",
                        Title = "Python & Data Fundamentals",
                        Description =
                            "Strengthen Python programming and develop a foundation in data handling and analytical thinking.",
                        Skills = new List<string>
                        {
                            "Python",
                            "Programming",
                            "Data"
                        },
                        Icon = "bi-code-slash"
                    },

                    new CareerStep
                    {
                        Label = "DATA",
                        Title = "Data Analysis",
                        Description =
                            "Learn to prepare, explore and analyze datasets before applying machine learning techniques.",
                        Skills = new List<string>
                        {
                            "Python",
                            "Data Analysis",
                            "SQL"
                        },
                        Icon = "bi-bar-chart"
                    },

                    new CareerStep
                    {
                        Label = "MACHINE LEARNING",
                        Title = "Machine Learning",
                        Description =
                            "Develop knowledge of machine learning concepts, algorithms and model development.",
                        Skills = new List<string>
                        {
                            "Machine Learning",
                            "Python",
                            "ML Algorithms"
                        },
                        Icon = "bi-cpu"
                    },

                    new CareerStep
                    {
                        Label = "PRACTICAL EXPERIENCE",
                        Title = "Build AI Projects",
                        Description =
                            "Apply your knowledge by creating practical machine learning or AI projects.",
                        Skills = new List<string>
                        {
                            "AI Projects",
                            "Machine Learning",
                            "Python",
                            "Git"
                        },
                        Icon = "bi-kanban"
                    }
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
                    "A career path focused on cloud platforms, containers, deployment automation and modern infrastructure.",

                RequiredSkills = new List<string>
                {
                    "Docker",
                    "Kubernetes",
                    "Azure",
                    "AWS",
                    "CI/CD"
                },

                Steps = new List<CareerStep>
                {
                    new CareerStep
                    {
                        Label = "FOUNDATION",
                        Title = "Infrastructure Fundamentals",
                        Description =
                            "Understand operating systems, networking, servers and the fundamentals of modern application infrastructure.",
                        Skills = new List<string>
                        {
                            "Linux",
                            "Networking",
                            "Servers"
                        },
                        Icon = "bi-hdd-network"
                    },

                    new CareerStep
                    {
                        Label = "CLOUD",
                        Title = "Cloud Fundamentals",
                        Description =
                            "Learn the fundamentals of cloud computing and work with a major cloud platform.",
                        Skills = new List<string>
                        {
                            "Azure",
                            "AWS",
                            "Cloud"
                        },
                        Icon = "bi-cloud"
                    },

                    new CareerStep
                    {
                        Label = "DEVOPS",
                        Title = "Containers & CI/CD",
                        Description =
                            "Develop skills in containerization, deployment automation and continuous integration.",
                        Skills = new List<string>
                        {
                            "Docker",
                            "Kubernetes",
                            "CI/CD"
                        },
                        Icon = "bi-box-seam"
                    },

                    new CareerStep
                    {
                        Label = "PRACTICAL EXPERIENCE",
                        Title = "Build & Deploy Projects",
                        Description =
                            "Deploy practical applications and demonstrate your ability to manage modern development environments.",
                        Skills = new List<string>
                        {
                            "Cloud Projects",
                            "Docker",
                            "CI/CD",
                            "Git"
                        },
                        Icon = "bi-kanban"
                    }
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
                    "A general software development roadmap designed to help you build programming fundamentals and practical development skills.",

                RequiredSkills = new List<string>(),

                Steps = new List<CareerStep>
                {
                    new CareerStep
                    {
                        Label = "FOUNDATION",
                        Title = "Programming Fundamentals",
                        Description =
                            "Start by strengthening programming logic, problem solving and fundamental programming concepts.",
                        Skills = new List<string>
                        {
                            "Programming",
                            "Logic Building",
                            "Problem Solving"
                        },
                        Icon = "bi-code-slash"
                    },

                    new CareerStep
                    {
                        Label = "DEVELOPMENT",
                        Title = "Web Development",
                        Description =
                            "Learn the fundamentals required to build modern web applications.",
                        Skills = new List<string>
                        {
                            "HTML",
                            "CSS",
                            "JavaScript"
                        },
                        Icon = "bi-laptop"
                    },

                    new CareerStep
                    {
                        Label = "ADVANCED SKILLS",
                        Title = "Backend & Database Skills",
                        Description =
                            "Develop backend, database and API development skills.",
                        Skills = new List<string>
                        {
                            "SQL",
                            "REST API",
                            "Git"
                        },
                        Icon = "bi-database"
                    },

                    new CareerStep
                    {
                        Label = "PRACTICAL EXPERIENCE",
                        Title = "Build Practical Projects",
                        Description =
                            "Apply your knowledge by creating projects that demonstrate your technical abilities.",
                        Skills = new List<string>
                        {
                            "Projects",
                            "Git",
                            "GitHub"
                        },
                        Icon = "bi-kanban"
                    }
                }
            };
        }


        // =========================================================
        // DEFAULT PATH
        // =========================================================

        private void CreateDefaultCareerPath()
        {
            var career = CreateDefaultCareer();

            CareerGoal = career.Name;

            CareerDescription = career.Description;

            CareerSteps = career.Steps;
        }
    }


    // =============================================================
    // CAREER DEFINITION
    // =============================================================

    public class CareerDefinition
    {
        public string Name { get; set; } = "";

        public string Description { get; set; } = "";

        public List<string> RequiredSkills { get; set; } = new();

        public List<CareerStep> Steps { get; set; } = new();
    }


    // =============================================================
    // CAREER STEP
    // =============================================================

    public class CareerStep
    {
        public string Label { get; set; } = "";

        public string Title { get; set; } = "";

        public string Description { get; set; } = "";

        public List<string> Skills { get; set; } = new();

        public string Icon { get; set; } = "bi-circle";
    }
}