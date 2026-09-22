namespace SkillBridge.Models
{
    public class Job
    {
        public int JobID { get; set; }

        public string JobTitle { get; set; } = string.Empty;

        public string CompanyName { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public string JobType { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string RequiredSkills { get; set; } = string.Empty;
        public string Salary { get; set; } = string.Empty;

        public string? ApplyLink { get; set; }
    }
}