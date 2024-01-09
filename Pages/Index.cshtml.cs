using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static Calendar;
using static Options;
using static Config;

namespace Calendar_Tracker.Pages
{
    public class IndexModel : PageModel
    {
        public SerializableDictionary<int, TrackerData> Trackers { get; set; } = new SerializableDictionary<int, TrackerData>();
        public SerializableDictionary<int, TrackerComponentData> TrackersValues { get; set; } = new SerializableDictionary<int, TrackerComponentData>();
        public string? GreetingValue { get; set; }
        public List<string>? Configurations { get; set; }
        public static string? selectedConfiguration { get; set; }
        public static CalendarMap MyCalendar { get; set; }

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
            Console.WriteLine(selectedConfiguration);

            selectedConfiguration ??= Configurations[0];


            Console.WriteLine(selectedConfiguration);

            MyCalendar = CalendarMap.LoadFromFile(selectedConfiguration + ".xml");
            var now = DateTime.Today;
            var id = ID.DateToID(now);
            var retrievedNotes = MyCalendar.GetDayNotes(id);
            TrackersValues = retrievedNotes.TrackersData ?? new SerializableDictionary<int, TrackerComponentData>();
            
            Options currentSettings = Options.LoadFromFile(selectedConfiguration + ".Settings.xml");
            GreetingValue = currentSettings.GreetingOption;
            Trackers = currentSettings.TrackersOption ?? new SerializableDictionary<int, TrackerData>();

            foreach (var (trackerId, trackerValues) in TrackersValues)
            {
                var newTrackerData = new TrackerComponentData
                {
                    Id = trackerValues.Id,
                    SliderValue = trackerValues.SliderValue,
                    CheckboxValue = trackerValues.CheckboxValue,
                    TextValue = trackerValues.TextValue,
                    DropdownValue = trackerValues.DropdownValue,
                };

                if (retrievedNotes.Exists)
                {
                    if (retrievedNotes.TrackersData.TryGetValue(trackerValues.Id, out var existingTrackerData))
                    {
                        existingTrackerData.SliderValue = newTrackerData.SliderValue;
                        existingTrackerData.CheckboxValue = newTrackerData.CheckboxValue;
                        existingTrackerData.TextValue = newTrackerData.TextValue;
                        existingTrackerData.DropdownValue = newTrackerData.DropdownValue;
                    }
                    else
                    {
                        retrievedNotes.TrackersData.Add(trackerValues.Id, newTrackerData);
                    }
                }
            }
        }

        public class FormData
        {
            public SerializableDictionary<int, TrackerComponentData> TrackersValues { get; set; } = new SerializableDictionary<int, TrackerComponentData>();
        }

        public IActionResult OnPostSubmit([FromForm] FormData model)
        {
            Console.WriteLine("OnPostSubmit method executed.");

            if (!ModelState.IsValid)
            {
                LogModelStateErrors();
                return BadRequest(ModelState);
            }

            //var myCalendar = CalendarMap.LoadFromFile("CalendarData.xml");
            var id = ID.DateToID(DateTime.Today);
            Console.WriteLine("Id:" + id);
            var note = MyCalendar.GetDayNotes(id);

            if (model != null && model.TrackersValues != null && note != null && note.TrackersData != null)
            {
                note.Exists = true;

                foreach (var trackerData in model.TrackersValues.Values)
                {
                    if (note.TrackersData.TryGetValue(trackerData.Id, out var existingTrackerData))
                    {
                        existingTrackerData.SliderValue = trackerData.SliderValue;
                        existingTrackerData.CheckboxValue = trackerData.CheckboxValue;
                        existingTrackerData.TextValue = trackerData.TextValue;
                        existingTrackerData.DropdownValue = trackerData.DropdownValue;
                    }
                    else
                    {
                        note.TrackersData[trackerData.Id] = trackerData;
                        Console.WriteLine($"Created a new tracker with Id {trackerData.Id}");
                    }
                }
            }

            var newCalendar = MyCalendar;
            newCalendar.AddDay(id, note);
            CalendarMap.SaveToFile(selectedConfiguration + ".xml", newCalendar);

            return new JsonResult(new { success = true, message = "Form submitted successfully" });
        }

        public IActionResult OnPostLoadSettings()
        {
            var currentSettings = Options.LoadFromFile(selectedConfiguration + ".Settings.xml");
            var longMonthNamesValue = currentSettings.LongMonthNamesOption;

            return new JsonResult(new { success = true, message = "Update successful", longMonthNamesValue });
        }

        private void LogModelStateErrors()
        {
            foreach (var modelStateValue in ModelState.Values)
            {
                foreach (var error in modelStateValue.Errors)
                {
                    Console.WriteLine(error.ErrorMessage);
                }
            }
        }
    }
}
