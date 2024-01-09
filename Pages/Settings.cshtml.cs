using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using static Calendar;
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
        public static string? selectedConfiguration { get; set; }
        public void OnGet()
        {
            Config config = null;
            try
            {
                config = Config.LoadConfigFromFile("Settings.xml");
            }
            catch (Exception ex)
            {
                // Handle the exception (e.g., log the error) and provide a default configuration
                Console.WriteLine("Error loading configuration file: " + ex.Message);
            }

            // If the file was successfully loaded, use the configurations from the loaded config object
            if (config != null)
            {
                Configurations = config.Configurations;
            }
            else
            {
                // If the file was not loaded, provide a default configuration
                Configurations = new List<string> { "DefaultConfiguration" };
            }

            // Retrieve the selected configuration from the query parameters
            selectedConfiguration = HttpContext.Request.Query["configuration"];

            selectedConfiguration ??= Configurations[0];


            var myCalendar = CalendarMap.LoadFromFile(selectedConfiguration + ".xml");
            var now = DateTime.Today;
            var id = ID.DateToID(now);
            var retrievedNotes = myCalendar.GetDayNotes(id);
            //TrackersValues = retrievedNotes.TrackersData ?? new SerializableDictionary<int, TrackerComponentData>();

            Options currentSettings = Options.LoadFromFile(selectedConfiguration + ".Settings.xml");
            Trackers = currentSettings.TrackersOption ?? new SerializableDictionary<int, TrackerData>();




            // Set Options from config file
            GreetingValue = currentSettings.GreetingOption;
            LongMonthNamesValue = currentSettings.LongMonthNamesOption;
            //Trackers = currentSettings.TrackersOption ?? new SerializableDictionary<int, TrackerData>();
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

            Config config = new();
            config.Configurations = model.Configurations ?? new List<string> { "DefaultCalendar" };

            Options currentSettings = new();
            currentSettings.GreetingOption = model.GreetingValue;
            currentSettings.LongMonthNamesOption = model.LongMonthNames;
            currentSettings.TrackersOption = model.Trackers;

            /*
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
            */


            // Save to file
            Config.SaveConfigToFile("Settings.xml", config);
            Console.WriteLine("Saved to: " + selectedConfiguration + ".Settings.xml");
            Options.SaveToFile(selectedConfiguration + ".Settings.xml", currentSettings);

            return new JsonResult(new { success = true });
        }
    }
}
