using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillBridge.Models
{
    public class Resume
    {
        [Key]
        public int ResumeID { get; set; }

        public int UserID { get; set; }

        [Required]
        public string FileName { get; set; } = string.Empty;

        [Required]
        public string FilePath { get; set; } = string.Empty;

        public DateTime UploadDate { get; set; } = DateTime.Now;

        // Relationship with User
        [ForeignKey("UserID")]
        public User? User { get; set; }
    }
}