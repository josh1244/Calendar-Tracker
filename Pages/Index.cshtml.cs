using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using static Calendar;
using static Options;


namespace Calendar_Tracker.Pages
{
    public class IndexModel : PageModel
    {
        public SerializableDictionary<int, TrackerData> Trackers { get; set; } = new SerializableDictionary<int, TrackerData>();
        public SerializableDictionary<int, TrackerComponentData> TrackersValues { get; set; } = new SerializableDictionary<int, TrackerComponentData>();
        public string? GreetingValue { get; set; }

        public void OnGet()
        {            
            CalendarMap MyCalendar = CalendarMap.LoadFromFile("CalendarData.xml");

            // Get the current time
            DateTime now = DateTime.Today;

            // Convert Time to ID
            string Id = ID.DateToID(now);

            // Retrieve or create a new DayNotes object
            DayNotes RetrievedNotes = MyCalendar.GetDayNotes(Id);

            Options currentSettings = Options.LoadFromFile("SettingsData.xml");

            // Setup Greeting
            GreetingValue = currentSettings.GreetingOption;

            // Set Options from config file
            Trackers = currentSettings.TrackersOption ?? new SerializableDictionary<int, TrackerData>();

            // Populate TrackersValues or perform necessary initialization
            TrackersValues = RetrievedNotes.TrackersData ?? new SerializableDictionary<int, TrackerComponentData>();

            // Iterate through all trackers and update settings and values
            foreach (var (trackerId, trackerValues) in TrackersValues)
            {
                // Assuming you have corresponding properties in TrackerComponentData
                var newTrackerData = new TrackerComponentData
                {
                    Id = trackerValues.Id,
                    SliderValue = trackerValues.SliderValue,
                    CheckboxValue = trackerValues.CheckboxValue,
                    TextValue = trackerValues.TextValue,
                    DropdownValue = trackerValues.DropdownValue,
                };

                if (RetrievedNotes.Exists)
                {
                    if (RetrievedNotes.TrackersData.TryGetValue(trackerValues.Id, out var existingTrackerData))
                    {
                        // Tracker already exists, update its values
                        existingTrackerData.SliderValue = newTrackerData.SliderValue;
                        existingTrackerData.CheckboxValue = newTrackerData.CheckboxValue;
                        existingTrackerData.TextValue = newTrackerData.TextValue;
                        existingTrackerData.DropdownValue = newTrackerData.DropdownValue;
                    }
                    else
                    {
                        // Tracker doesn't exist, add a new one
                        RetrievedNotes.TrackersData.Add(trackerValues.Id, newTrackerData);
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
                foreach (var modelStateValue in ModelState.Values)
                {
                    foreach (var error in modelStateValue.Errors)
                    {
                        // Log or print the error messages
                        Console.WriteLine(error.ErrorMessage);
                    }
                }

                // Handle the errors or return an error response
                return BadRequest(ModelState);
            }

            CalendarMap MyCalendar = CalendarMap.LoadFromFile("CalendarData.xml");

            // Get the current time
            DateTime now = DateTime.Today;

            // Convert Time to ID
            string Id = ID.DateToID(now);

            // Retrieve or create a new DayNotes object
            DayNotes note = MyCalendar.GetDayNotes(Id);

            // Update Note in calendar
            if (model != null && model.TrackersValues != null && note != null && note.TrackersData != null)
            {
                note.Exists = true;

                foreach (var trackerData in model.TrackersValues.Values)
                {
                    if (note.TrackersData.TryGetValue(trackerData.Id, out var existingTrackerData))
                    {
                        // Tracker already exists, update its values
                        existingTrackerData.SliderValue = trackerData.SliderValue;
                        existingTrackerData.CheckboxValue = trackerData.CheckboxValue;
                        existingTrackerData.TextValue = trackerData.TextValue;
                        existingTrackerData.DropdownValue = trackerData.DropdownValue;
                    }
                    else
                    {
                        // Tracker not found, create a new entry
                        note.TrackersData[trackerData.Id] = trackerData;

                        // Debugging output
                        Console.WriteLine($"Created a new tracker with Id {trackerData.Id}");
                    }
                }
            }

            // Update MyCalendar
            CalendarMap newCalendar = MyCalendar;
            newCalendar.AddDay(Id, note);

            // Save to file
            CalendarMap.SaveToFile("CalendarData.xml", newCalendar);

            return new JsonResult(new { success = true, message = "Form submitted successfully" });
        }

        public IActionResult OnPostLoadSettings()
        {
            Options currentSettings = Options.LoadFromFile("SettingsData.xml");

            //Set Options from config file
            bool longMonthNamesValue = currentSettings.LongMonthNamesOption;

            return new JsonResult(new { success = true, message = "Update successful", longMonthNamesValue });
        }
    }
}
