using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using static Options;

namespace Calendar_Tracker.Pages
{
    public class SettingsModel : PageModel
    {
        [BindProperty]
        public string? GreetingValue { get; set; }
        public bool LongMonthNamesValue { get; set; }
        public List<string>? Configurations { get; set; }
        public SerializableDictionary<int, TrackerData> Trackers { get; set; } = new SerializableDictionary<int, TrackerData>();

        public void OnGet()
        {
            Options currentSettings = Options.LoadFromFile("SettingsData.xml");

            // Set Options from config file
            GreetingValue = currentSettings.GreetingOption;
            LongMonthNamesValue = currentSettings.LongMonthNamesOption;
            Configurations = currentSettings.Configurations ?? new List<string> { "DefaultCalendar" };
            Trackers = currentSettings.TrackersOption ?? new SerializableDictionary<int, TrackerData>();
        }

        public class FormData
        {
            public string? GreetingValue { get; set; }
            public bool LongMonthNames { get; set; }
            public List<string>? Configurations { get; set; }
            public SerializableDictionary<int, TrackerData> Trackers { get; set; } = new SerializableDictionary<int, TrackerData>();
        }

        public IActionResult OnPostSubmit([FromForm] FormData model)
        {
            if (!ModelState.IsValid)
            {
                // Log or handle validation errors
                Console.WriteLine("Model state is not valid.");
                return BadRequest(ModelState);
            }

            Options currentSettings = new();

            currentSettings.GreetingOption = model.GreetingValue;
            currentSettings.LongMonthNamesOption = model.LongMonthNames;
            currentSettings.Configurations = model.Configurations ?? new List<string> { "DefaultCalendar" };
            currentSettings.TrackersOption = model.Trackers;

            foreach (var trackerEntry in model.Trackers)
            {
                Console.WriteLine($"Tracker Id: {trackerEntry.Key}");

                var trackerData = trackerEntry.Value;
                Console.WriteLine($"  Order: {trackerData.Order}");
                Console.WriteLine($"  Name: {trackerData.Name}");
                Console.WriteLine($"  Type: {trackerData.Type}");
                Console.WriteLine($"  DefaultText: {trackerData.DefaultText}");

                if (trackerData.DropdownOptions != null)
                {
                    Console.WriteLine("  Dropdown Options:");
                    foreach (var option in trackerData.DropdownOptions)
                    {
                        Console.WriteLine($"    {option}");
                    }
                }
            }



            // Save to file
            Options.SaveToFile("SettingsData.xml", currentSettings);

            return new JsonResult(new { success = true });
        }
    }
}
