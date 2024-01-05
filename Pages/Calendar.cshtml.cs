using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using static Calendar;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.Extensions.Configuration;
using static Options;
using Newtonsoft.Json;
using System.Globalization;




namespace Calendar_Tracker.Pages
{
    public class CalendarModel : PageModel
    {
        public SerializableDictionary<int, TrackerData> Trackers { get; set; } = new SerializableDictionary<int, TrackerData>();
        public SerializableDictionary<int, TrackerComponentData> TrackersValues { get; set; } = new SerializableDictionary<int, TrackerComponentData>();
        public Options currentSettings { get; set; }

        public class WeekModel
        {
            public int[] Days { get; set; } = new int[7];
        }

        public int[] CurrentWeekDays { get; set; } = new int[7];
        public void OnGet()
        {
            Console.WriteLine("OnGet method executed.");

            // Load Calendar data
            CalendarMap MyCalendar = CalendarMap.LoadFromFile("CalendarData.xml");
            HttpContext.Session.SetObject("MyStoredCalendar", MyCalendar);


            int EditedMonth = DateTime.Now.Month;
            int EditedDay = DateTime.Now.Day;
            int EditedYear = DateTime.Now.Year;


            HttpContext.Session.SetObject("Month", EditedMonth);
            HttpContext.Session.SetObject("Day", EditedDay);
            HttpContext.Session.SetObject("Year", EditedYear);

            CalculateDate();

            // Get the current time
            //DateTime now = DateTime.Today;
            // Convert Time to ID
            //string Id = ID.DateToID(now);
            // Retrieve or create a new DayNotes object
            
            currentSettings = Options.LoadFromFile("SettingsData.xml");

            LoadTrackers();
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
                month = HttpContext.Session.GetObject<int>("Month"),
                day = HttpContext.Session.GetObject<int>("Day"),
                year = HttpContext.Session.GetObject<int>("Year"),
                days = CurrentWeekDays
            }); //Return new data for date display
        }

        public IActionResult OnPostUpdateDate([FromBody] UpdateDateModel model)
        {
            Console.WriteLine("OnPostUpdateDate method executed.");

            int EditedMonth = model.MonthAJAX;
            int EditedDay = model.DayAJAX;
            int EditedYear = model.YearAJAX;

            HttpContext.Session.SetObject("Month", EditedMonth);
            HttpContext.Session.SetObject("Day", EditedDay);
            HttpContext.Session.SetObject("Year", EditedYear);

            CalculateDate();

            return new JsonResult(new { success = true, message = "Update successful", days = CurrentWeekDays });
        }

        public void NextWeek(int days)
        {
            int EditedMonth = HttpContext.Session.GetObject<int>("Month");
            int EditedDay = HttpContext.Session.GetObject<int>("Day");
            int EditedYear = HttpContext.Session.GetObject<int>("Year");

            DateTime EditDate = new(EditedYear, EditedMonth, EditedDay);


            DateTime newWeek = EditDate.AddDays(days);

            EditedMonth = newWeek.Month;
            EditedDay = newWeek.Day;
            EditedYear = newWeek.Year;

            HttpContext.Session.SetObject("Month", EditedMonth);
            HttpContext.Session.SetObject("Day", EditedDay);
            HttpContext.Session.SetObject("Year", EditedYear);


        }

        public void CalculateDate()
        {
            Console.WriteLine($"CalculateDate executed.");

            CalendarMap MyCalendar = HttpContext.Session.GetObject<CalendarMap>("MyStoredCalendar");
            MyCalendar ??= CalendarMap.LoadFromFile("CalendarData.xml"); // if null. Shouldnt be needed

            int EditedMonth = HttpContext.Session.GetObject<int>("Month");
            int EditedDay = HttpContext.Session.GetObject<int>("Day");
            int EditedYear = HttpContext.Session.GetObject<int>("Year");

            // Convert Time to ID
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


            HttpContext.Session.SetObject("retrievedNotes", retrievedNotes);
            HttpContext.Session.SetObject("todayID", todayID);

            LoadTrackers();
            //Cause html to regenerate trackers form
        }

        public void LoadTrackers()
        {
            int EditedMonth = HttpContext.Session.GetObject<int>("Month");
            int EditedDay = HttpContext.Session.GetObject<int>("Day");
            int EditedYear = HttpContext.Session.GetObject<int>("Year");

            DateTime EditDate = new(EditedYear, EditedMonth, EditedDay);

            // Convert Time to ID
            string Id = ID.DateToID(EditDate);

            CalendarMap MyCalendar = HttpContext.Session.GetObject<CalendarMap>("MyStoredCalendar");

            DayNotes RetrievedNotes = MyCalendar.GetDayNotes(Id);

            // Retrieve or create a new DayNotes object
            RetrievedNotes = MyCalendar.GetDayNotes(Id);

            currentSettings = Options.LoadFromFile("SettingsData.xml");

            // Set Options from config file
            Trackers = currentSettings.TrackersOption ?? new SerializableDictionary<int, TrackerData>();
            Console.WriteLine($"Loaded Trackers: {JsonConvert.SerializeObject(Trackers)}");


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
                    //ValueExists = trackerValues.ValueExists
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
                        //existingTrackerData.ValueExists = newTrackerData.ValueExists;
                    }
                    else
                    {
                        // Tracker doesn't exist, add a new one
                        RetrievedNotes.TrackersData.Add(trackerValues.Id, newTrackerData);
                    }
                }
            }
            Console.WriteLine($"Loaded TrackersValues: {JsonConvert.SerializeObject(RetrievedNotes.TrackersData)}");
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
