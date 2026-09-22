using System.ComponentModel.DataAnnotations;

namespace SkillBridge.Models
{
    public class User
    {
        public int UserID { get; set; }

        [Required(ErrorMessage = "Please enter your full name.")]
        [RegularExpression(
            @"^[A-Za-z ]+$",
            ErrorMessage = "Name can contain letters and spaces only.")]
        public string FullName { get; set; } = string.Empty;


        [Required(ErrorMessage = "Please enter your email address.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; } = string.Empty;


        [Required(ErrorMessage = "Please create a password.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        public string Password { get; set; } = string.Empty;


        [RegularExpression(
            @"^[0-9]{10}$",
            ErrorMessage = "Contact number must contain exactly 10 digits.")]
        public string? Phone { get; set; }


        public string? Skills { get; set; }

        public string? ResumePath { get; set; }

        public bool IsEmailVerified { get; set; } = false;

        public string? EmailVerificationCode { get; set; }

        public DateTime? EmailVerificationExpiry { get; set; }

        public string Role { get; set; } = "User";
    }
}