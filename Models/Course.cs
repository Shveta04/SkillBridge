using System.ComponentModel.DataAnnotations;

namespace SkillBridge.Models
{
    public class Course
    {
        [Key]
        public int CourseID { get; set; }

        public string CourseName { get; set; } = string.Empty;

        public string SkillsCovered { get; set; } = string.Empty;

        public string? CourseLink { get; set; }
    }
}