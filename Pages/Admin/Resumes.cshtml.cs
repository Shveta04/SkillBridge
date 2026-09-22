using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SkillBridge.Data;
using SkillBridge.Models;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;

namespace SkillBridge.Pages.Admin
{
    public class ResumesModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ResumesModel(
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public List<User> UsersWithResumes { get; set; } = new();

        public IActionResult OnGet()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userEmail) ||
                !string.Equals(userRole, "Admin",
                    StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToPage("/Admin/Login");
            }

            UsersWithResumes = _context.Users
                .Where(u => !string.IsNullOrEmpty(u.ResumePath))
                .OrderBy(u => u.UserID)
                .ToList();

            return Page();
        }

        public IActionResult OnGetViewResume(int id)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userEmail) ||
                !string.Equals(userRole, "Admin",
                    StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToPage("/Admin/Login");
            }

            var user = _context.Users.FirstOrDefault(u => u.UserID == id);

            if (user == null || string.IsNullOrWhiteSpace(user.ResumePath))
            {
                return NotFound("Resume not found.");
            }

            var relativePath = user.ResumePath.TrimStart(
                '/',
                '\\'
            ).Replace('/', Path.DirectorySeparatorChar);

            var filePath = Path.Combine(
                _environment.WebRootPath,
                relativePath
            );

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Resume file could not be found.");
            }

            var extension = Path.GetExtension(filePath)
                .ToLowerInvariant();

            // PDF files can be displayed directly in the browser.
            if (extension == ".pdf")
            {
                return PhysicalFile(
                    filePath,
                    "application/pdf"
                );
            }

            // DOCX files are converted to PDF.
            if (extension == ".docx")
            {
                using var inputStream = new FileStream(
                    filePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read
                );

                using var wordDocument = new WordDocument(
                    inputStream,
                    FormatType.Docx
                );

                using var renderer = new DocIORenderer();

                using var pdfDocument =
                    renderer.ConvertToPDF(wordDocument);

                using var outputStream = new MemoryStream();

                pdfDocument.Save(outputStream);
                outputStream.Position = 0;

                wordDocument.Close();

                return File(
                    outputStream.ToArray(),
                    "application/pdf"
                );
            }

            return BadRequest(
                "This resume format is not supported for browser viewing."
            );
        }
    }
}