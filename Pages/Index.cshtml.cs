using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Globalization;
using static Calendar;

namespace Calendar_Tracker.Pages
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public int SliderValue { get; set; }

        public void OnGet()
        {
            CalendarMap myCalendar = LoadFromFile("calendarData.xml");
            DayNotes retrievedNotes;

            // Get the current time
            DateTime now = DateTime.Today;

            // Convert Time to ID
            string todayID = ID.DateToID(now);
            retrievedNotes = myCalendar.GetDayNotes(todayID);

            if (retrievedNotes.exists)
            {
                SliderValue = retrievedNotes.dayQuality;
                ViewData["Message"] = $"Day Quality: {SliderValue}";
            }
            else
            {
                // Only set the initial value if it hasn't been set yet
                SliderValue = 5; // Set initial value to DayNote's value
                ViewData["Message"] = $"Day Quality: {SliderValue}";
            }            
        }

        public void OnPost()
        {
            // Bind the value from the form to the SliderValue property
            if (int.TryParse(Request.Form["DayQuality"], out int sliderValue))
            {
                SliderValue = sliderValue;
            }

            ViewData["Message"] = $"Day Quality: {SliderValue}";
        }
    }
}
