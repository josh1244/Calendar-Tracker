using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using static Calendar;
using static System.Runtime.InteropServices.JavaScript.JSType;



namespace Calendar_Tracker.Pages
{
    public class CalendarModel : PageModel
    {
        public List<WeekModel> Weeks { get; set; } = new List<WeekModel>();

        public void OnGet()
        {
            Console.WriteLine("OnGet method executed.");

            // Load Calendar data
            CalendarMap MyCalendar = CalendarMap.LoadFromFile("calendarData.xml");
            HttpContext.Session.SetObject("MyStoredCalendar", MyCalendar);


            int EditedMonth = DateTime.Now.Month;
            int EditedDay = DateTime.Now.Day;
            int EditedYear = DateTime.Now.Year;


            HttpContext.Session.SetObject("Month", EditedMonth);
            HttpContext.Session.SetObject("Day", EditedDay);
            HttpContext.Session.SetObject("Year", EditedYear);

            CalculateDate();
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

            // Return a JSON response using JsonResult
            return new JsonResult(new { success = true, message = "Update successful", weeks = Weeks });
        }

        public void CalculateDate()
        {
            Console.WriteLine($"CalculateDate executed.");

            CalendarMap MyCalendar = HttpContext.Session.GetObject<CalendarMap>("MyStoredCalendar");
            MyCalendar ??= CalendarMap.LoadFromFile("calendarData.xml"); // if null. Shouldnt be needed

            int EditedMonth = HttpContext.Session.GetObject<int>("Month");
            int EditedDay = HttpContext.Session.GetObject<int>("Day");
            int EditedYear = HttpContext.Session.GetObject<int>("Year");


            // Convert Time to ID
            DateTime EditDate = new(EditedYear, EditedMonth, EditedDay);
            string todayID = ID.DateToID(EditDate);

            //Figure out the days of the week
            int dayOfWeek = (int)EditDate.DayOfWeek;
            //Weeks = [];
            /*
            for (int i = 0; i < 7; i++)
            {
                var week = new WeekModel();
                DateTime prev = EditDate.AddDays(i - dayOfWeek);
                week.Days.Add((int)prev.Day);
                //Weeks[i] = (int)prev.Day;
                Weeks.Add(week);
            }
            */
            Weeks = Enumerable.Range(0, 7)
               .Select(i => new WeekModel
               {
                   Days = { (int)EditDate.AddDays(i - dayOfWeek).Day }
               })
               .ToList();


            Console.WriteLine("ID of today is " + todayID);
            ViewData["todayID"] = todayID;

            // Draw Calendar
            UI.DrawCalendar(todayID);
            // Display Notes
            DayNotes retrievedNotes = MyCalendar.GetDayNotes(todayID);
            UI.DisplayNotes(retrievedNotes);



            HttpContext.Session.SetObject("retrievedNotes", retrievedNotes);
            HttpContext.Session.SetObject("todayID", todayID);

        }

        public class WeekModel
        {
            public List<int> Days { get; set; } = new List<int>();
        }
    }
}
