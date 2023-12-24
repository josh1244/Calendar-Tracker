using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using static Calendar_Tracker.Pages.CalendarModel;
using static Options;

namespace Calendar_Tracker.Pages
{
    public class SettingsModel : PageModel
    {
        [BindProperty]
        public bool LongMonthNamesValue { get; set; }

        public void OnGet()
        {
            Console.WriteLine("OnGet method executed.");

            Options currentSettings = Options.LoadFromFile("SettingsData.xml");

            //Set Options from config file
            LongMonthNamesValue = currentSettings.LongMonthNamesOption;
        }

        public void OnPost()
        {
            Console.WriteLine("OnPost method executed.");
        }


        public class FormData
        {
            public bool LongMonthNames { get; set; }
        }
        public IActionResult OnPostSubmit([FromForm] FormData model)
        {
            Console.WriteLine("OnPostSubmit method executed.");

            Options currentSettings = new();
            
            currentSettings.LongMonthNamesOption = model.LongMonthNames;
            
            // Save to file
            Console.Write("Saving to file...");
            Options.SaveToFile("SettingsData.xml", currentSettings);
            Console.WriteLine("Save complete.");

            // Log values for debugging
            Console.WriteLine($"Options: {JsonConvert.SerializeObject(currentSettings)}");

            return new JsonResult(new { success = true });
        }
    }
}
