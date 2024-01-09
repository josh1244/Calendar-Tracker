using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static Calendar;
using static Options;

namespace Calendar_Tracker.Pages
{
    public class CalendarModel : PageModel
    {
        private static CalendarMap? MyCalendar;
        private static int EditedMonth;
        private static int EditedDay;
        private static int EditedYear;

        public SerializableDictionary<int, TrackerData> Trackers { get; set; } = new SerializableDictionary<int, TrackerData>();
        public SerializableDictionary<int, TrackerComponentData> TrackersValues { get; set; } = new SerializableDictionary<int, TrackerComponentData>();
        public Options? CurrentSettings { get; set; }
        public List<string>? Configurations { get; set; }
        public static string? selectedConfiguration { get; set; }

        public int[] CurrentWeekDays { get; set; } = new int[7];

        public class WeekModel
        {
            public int[] Days { get; set; } = new int[7];
        }

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

            CurrentSettings = Options.LoadFromFile(selectedConfiguration + ".Settings.xml");
            Trackers = CurrentSettings.TrackersOption ?? new SerializableDictionary<int, TrackerData>();


            EditedMonth = DateTime.Now.Month;
            EditedDay = DateTime.Now.Day;
            EditedYear = DateTime.Now.Year;
            CalculateDate();
        }

        public class UpdateDateModel
        {
            public int MonthAJAX { get; set; }
            public int DayAJAX { get; set; }
            public int YearAJAX { get; set; }
        }

        public class NextWeekModel
        {
            public int Days { get; set; }
        }

        public IActionResult OnPostNextWeek([FromBody] NextWeekModel model)
        {
            NextWeek(model.Days);
            CalculateDate();

            return new JsonResult(new
            {
                success = true,
                message = "Update successful",
                month = EditedMonth,
                day = EditedDay,
                year = EditedYear,
                days = CurrentWeekDays
            });
        }

        public IActionResult OnPostUpdateDate([FromBody] UpdateDateModel model)
        {
            EditedMonth = model.MonthAJAX;
            EditedDay = model.DayAJAX;
            EditedYear = model.YearAJAX;

            CalculateDate();

            return new JsonResult(new { success = true, message = "Update successful", days = CurrentWeekDays });
        }

        public class FormData
        {
            public SerializableDictionary<int, TrackerComponentData> TrackersValues { get; set; } = new SerializableDictionary<int, TrackerComponentData>();
        }

        public IActionResult OnPostSubmit([FromForm] FormData model)
        {
            if (!ModelState.IsValid)
            {
                LogModelStateErrors();
                return BadRequest(ModelState);
            }

            DateTime editDate = new DateTime(EditedYear, EditedMonth, EditedDay);
            string id = ID.DateToID(editDate);

            DayNotes note = MyCalendar.GetDayNotes(id);

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
                    }
                }
            }

            CalendarMap newCalendar = MyCalendar;
            newCalendar.AddDay(id, note);

            CalendarMap.SaveToFile(selectedConfiguration + ".xml", newCalendar);

            return new JsonResult(new { success = true, message = "Form submitted successfully" });
        }

        public void NextWeek(int days)
        {
            DateTime editDate = new DateTime(EditedYear, EditedMonth, EditedDay);
            DateTime newWeek = editDate.AddDays(days);
            EditedMonth = newWeek.Month;
            EditedDay = newWeek.Day;
            EditedYear = newWeek.Year;
        }

        public void CalculateDate()
        {
            DateTime editDate = new DateTime(EditedYear, EditedMonth, EditedDay);
            string todayId = ID.DateToID(editDate);
            int dayOfWeek = (int)editDate.DayOfWeek;

            CurrentWeekDays = Enumerable.Range(0, 7)
                .Select(i => (int)editDate.AddDays(i - dayOfWeek).Day)
                .ToArray();

            ViewData["todayID"] = todayId;
            DayNotes retrievedNotes = MyCalendar.GetDayNotes(todayId);
        }

        public IActionResult OnPostLoadTrackers([FromBody] UpdateDateModel model)
        {
            EditedMonth = model.MonthAJAX;
            EditedDay = model.DayAJAX;
            EditedYear = model.YearAJAX;
            Console.WriteLine(EditedMonth + "" + EditedDay + "" + EditedYear);

            DateTime editDate = new DateTime(EditedYear, EditedMonth, EditedDay);
            string id = ID.DateToID(editDate);

            DayNotes retrievedNotes = MyCalendar.GetDayNotes(id);


            CurrentSettings = Options.LoadFromFile(selectedConfiguration + ".Settings.xml");

            Trackers = CurrentSettings.TrackersOption ?? new SerializableDictionary<int, TrackerData>();
            TrackersValues = retrievedNotes.TrackersData ?? new SerializableDictionary<int, TrackerComponentData>();

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

            return new JsonResult(new
            {
                success = true,
                message = "Update successful",
                trackers = Trackers,
                trackersData = retrievedNotes.TrackersData
            });
        }

        public IActionResult OnPostLoadSettings()
        {
            Options currentSettings = Options.LoadFromFile(selectedConfiguration + ".Settings.xml");
            bool longMonthNamesValue = currentSettings.LongMonthNamesOption;

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
