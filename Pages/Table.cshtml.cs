using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using static Calendar;
using static Options;
using System.Globalization;
using static ID;

namespace Calendar_Tracker.Pages
{
    public class TableModel : PageModel
    {
        public SerializableDictionary<int, TrackerData> Trackers { get; set; } = new SerializableDictionary<int, TrackerData>();
        public SerializableDictionary<int, TrackerComponentData> TrackersValues { get; set; } = new SerializableDictionary<int, TrackerComponentData>();
        public List<string>? Configurations { get; set; }
        public static string? selectedConfiguration { get; set; }
        public CalendarMap MyCalendar { get; set; }

        public void OnGet()
        {
            Config? config = null;
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
            Configurations = config?.Configurations ?? new List<string> { "DefaultConfiguration" };

            // Retrieve the selected configuration from the query parameters
            selectedConfiguration = HttpContext.Request.Query["configuration"];

            selectedConfiguration ??= Configurations[0];


            MyCalendar = CalendarMap.LoadFromFile(selectedConfiguration + ".xml");
            var now = DateTime.Today;
            var id = ID.DateToID(now);
            var retrievedNotes = MyCalendar.GetDayNotes(id);
            TrackersValues = retrievedNotes.TrackersData ?? new SerializableDictionary<int, TrackerComponentData>();

            Options currentSettings = Options.LoadFromFile(selectedConfiguration + ".Settings.xml");
            Trackers = currentSettings.TrackersOption ?? new SerializableDictionary<int, TrackerData>();

            foreach (var (trackerId, trackerValues) in TrackersValues)
            {
                var newTrackerData = new TrackerComponentData
                {
                    Id = trackerValues.Id,
                    SliderValue = trackerValues.SliderValue,
                    CheckboxValue = trackerValues.CheckboxValue,
                    TextValue = trackerValues.TextValue,
                    NumberValue = trackerValues.NumberValue,
                    DropdownValue = trackerValues.DropdownValue,
                };

                if (retrievedNotes.Exists)
                {
                    if (retrievedNotes.TrackersData.TryGetValue(trackerValues.Id, out var existingTrackerData))
                    {
                        existingTrackerData.SliderValue = newTrackerData.SliderValue;
                        existingTrackerData.CheckboxValue = newTrackerData.CheckboxValue;
                        existingTrackerData.TextValue = newTrackerData.TextValue;
                        existingTrackerData.NumberValue = newTrackerData.NumberValue;
                        existingTrackerData.DropdownValue = newTrackerData.DropdownValue;
                    }
                    else
                    {
                        retrievedNotes.TrackersData.Add(trackerValues.Id, newTrackerData);
                    }
                }
            }
        }
    }
}
