using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SkillBridge.Pages
{
    public class DashboardModel : PageModel
    {
        public string Greeting { get; set; } = "";
        public string WelcomeMessage { get; set; } = "";

        public void OnGet()
        {
            int hour = DateTime.Now.Hour;

            if (hour < 12)
            {
                Greeting = "☀ Good Morning";
            }
            else if (hour < 17)
            {
                Greeting = "🌤 Good Afternoon";
            }
            else
            {
                Greeting = "🌙 Good Evening";
            }

            WelcomeMessage = "Let's build your career today.";
        }
    }
}