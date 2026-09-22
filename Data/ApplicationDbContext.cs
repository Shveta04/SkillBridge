using Microsoft.EntityFrameworkCore;
using SkillBridge.Models;

namespace SkillBridge.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        public DbSet<Job> Jobs { get; set; }

        public DbSet<Resume> Resumes { get; set; }

        public DbSet<ResumeAnalysis> ResumeAnalysis { get; set; }

        public DbSet<Course> Courses { get; set; }
    }
}