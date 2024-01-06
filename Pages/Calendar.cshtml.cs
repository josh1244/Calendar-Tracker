using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static Calendar;
using static Options;


namespace Calendar_Tracker.Pages
{
    public class CalendarModel : PageModel
    {
        public SerializableDictionary<int, TrackerData> Trackers { get; set; } = new SerializableDictionary<int, TrackerData>();
        public SerializableDictionary<int, TrackerComponentData> TrackersValues { get; set; } = new SerializableDictionary<int, TrackerComponentData>();
        public Options? currentSettings { get; set; }

        private static CalendarMap? MyCalendar;
        private static int EditedMonth;
        private static int EditedDay;
        private static int EditedYear;

        public class WeekModel
        {
            public int[] Days { get; set; } = new int[7];
        }

        public int[] CurrentWeekDays { get; set; } = new int[7];
        public void OnGet()
        {
            Console.WriteLine("OnGet method executed.");

            // Load Calendar data
            MyCalendar = CalendarMap.LoadFromFile("CalendarData.xml");

            Console.WriteLine(EditedYear);
            EditedMonth = DateTime.Now.Month;
            EditedDay = DateTime.Now.Day;
            EditedYear = DateTime.Now.Year;
            Console.WriteLine(EditedYear);

            CalculateDate();

            currentSettings = Options.LoadFromFile("SettingsData.xml");
        }

        public void OnPost()
        {
            Console.WriteLine("OnPost method executed.");
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
            Console.WriteLine("OnPostNextWeek method executed.");

            NextWeek(model.Days); //Add or remove one week from current day
            CalculateDate(); // Update Date

            return new JsonResult(new
            {
                success = true,
                message = "Update successful",
                month = EditedMonth,
                day = EditedDay,
                year = EditedYear,
                days = CurrentWeekDays
            }); //Return new data for date display
        }

        public IActionResult OnPostUpdateDate([FromBody] UpdateDateModel model)
        {
            Console.WriteLine("OnPostUpdateDate method executed.");

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

            DateTime EditDate = new(EditedYear, EditedMonth, EditedDay);

            // Convert Time to ID
            string Id = ID.DateToID(EditDate);

            Console.WriteLine(Id);

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
                        // Debugging output
                        Console.WriteLine($"Updating tracker with Id {trackerData.Id}");

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
            Console.Write("Saving to file...");
            CalendarMap.SaveToFile("CalendarData.xml", newCalendar);
            Console.WriteLine("Save complete.");

            return new JsonResult(new { success = true, message = "Form submitted successfully" });
        }

        public void NextWeek(int days)
        {
            DateTime EditDate = new(EditedYear, EditedMonth, EditedDay);

            DateTime newWeek = EditDate.AddDays(days);

            EditedMonth = newWeek.Month;
            EditedDay = newWeek.Day;
            EditedYear = newWeek.Year;
        }

        public void CalculateDate()
        {
            Console.WriteLine($"CalculateDate executed.");

            // Convert Time to ID
            Console.WriteLine(EditedYear);
            DateTime EditDate = new(EditedYear, EditedMonth, EditedDay);
            string todayID = ID.DateToID(EditDate);

            //Figure out the days of the week
            int dayOfWeek = (int)EditDate.DayOfWeek;

            // Populate the Days array of the CurrentWeek
            CurrentWeekDays = Enumerable.Range(0, 7)
                .Select(i => (int)EditDate.AddDays(i - dayOfWeek).Day)
                .ToArray();

            Console.WriteLine("ID of today is " + todayID);
            ViewData["todayID"] = todayID;

            // Draw Calendar
            //UI.DrawCalendar(todayID);

            // Display Notes
            DayNotes retrievedNotes = MyCalendar.GetDayNotes(todayID);
        }

        public IActionResult OnPostLoadTrackers([FromBody] UpdateDateModel model)
        {
            Console.WriteLine("OnPostLoadTrackers");

            EditedMonth = model.MonthAJAX;
            EditedDay = model.DayAJAX;
            EditedYear = model.YearAJAX;

            DateTime EditDate = new(EditedYear, EditedMonth, EditedDay);

            // Convert Time to ID
            string Id = ID.DateToID(EditDate);

            DayNotes RetrievedNotes = MyCalendar.GetDayNotes(Id);

            // Retrieve or create a new DayNotes object
            RetrievedNotes = MyCalendar.GetDayNotes(Id);

            currentSettings = Options.LoadFromFile("SettingsData.xml");

            // Set Options from config file
            Trackers = currentSettings.TrackersOption ?? new SerializableDictionary<int, TrackerData>();
            //Console.WriteLine($"Loaded Trackers: {JsonConvert.SerializeObject(Trackers)}");


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

            Console.WriteLine("Pretend Trackers Updated------------------------------------------");
            //return new JsonResult(new { success = true, message = "Update successful" });

            // Return only the necessary data for dynamic HTML generation
            return new JsonResult(new
            {
                success = true,
                message = "Update successful",
                trackers = Trackers,
                trackersData = RetrievedNotes.TrackersData
            });
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
