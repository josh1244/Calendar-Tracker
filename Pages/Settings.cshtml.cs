using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Reflection;
using static Calendar_Tracker.Pages.CalendarModel;
using static Options;

namespace Calendar_Tracker.Pages
{
    public class SettingsModel : PageModel
    {
        [BindProperty]
        public string? GreetingValue { get; set; }
        public bool LongMonthNamesValue { get; set; }

        public SerializableDictionary<int, TrackerData> Trackers { get; set; } = new SerializableDictionary<int, TrackerData>();

        public void OnGet()
        {
            Console.WriteLine("OnGet method executed.");

            Options currentSettings = Options.LoadFromFile("SettingsData.xml");

            // Set Options from config file
            GreetingValue = currentSettings.GreetingOption;
            LongMonthNamesValue = currentSettings.LongMonthNamesOption;
            Trackers = currentSettings.TrackersOption ?? new SerializableDictionary<int, TrackerData>();
            Console.WriteLine($"Loaded Trackers: {JsonConvert.SerializeObject(Trackers)}");
        }

        public void OnPost()
        {
            Console.WriteLine("OnPost method executed.");
        }

        public class FormData
        {
            public string? GreetingValue { get; set; }
            public bool LongMonthNames { get; set; }
            public SerializableDictionary<int, TrackerData> Trackers { get; set; } = new SerializableDictionary<int, TrackerData>();
        }

        public IActionResult OnPostSubmit([FromForm] FormData model)
        {
            Console.WriteLine("OnPostSubmit method executed.");

            if (!ModelState.IsValid)
            {
                // Log or handle validation errors
                Console.WriteLine("Model state is not valid.");
                return BadRequest(ModelState);
            }

            Options currentSettings = new();

            currentSettings.GreetingOption = model.GreetingValue;
            currentSettings.LongMonthNamesOption = model.LongMonthNames;
            currentSettings.TrackersOption = model.Trackers;


            // Save to file
            Console.Write("Saving to file...");
            Options.SaveToFile("SettingsData.xml", currentSettings);
            Console.WriteLine("Save complete.");

            // Log values for debugging
            Console.WriteLine($"Options: {JsonConvert.SerializeObject(currentSettings)}");
            Console.WriteLine($"Trackers from Form Data: {JsonConvert.SerializeObject(model.Trackers)}");

            return new JsonResult(new { success = true });
        }
    }
}
