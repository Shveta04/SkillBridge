using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillBridge.Models
{
    public class ResumeAnalysis
    {
        [Key]
        public int AnalysisID { get; set; }

        public int ResumeID { get; set; }

        public int ATSScore { get; set; }

        public string? ExtractedSkills { get; set; }

        public string? MissingSkills { get; set; }

        public string? ExperienceSummary { get; set; }

        public string? EducationSummary { get; set; }

        public string? Suggestions { get; set; }

        public DateTime AnalysisDate { get; set; } = DateTime.Now;

        // Relationship with Resume
        [ForeignKey("ResumeID")]
        public Resume? Resume { get; set; }
    }
}