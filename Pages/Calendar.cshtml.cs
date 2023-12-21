using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System;
using System.Reflection;
using System.Security.Cryptography;
using static Calendar;
using static System.Net.Mime.MediaTypeNames;
public static class SessionExtensions
{
    public static T GetObject<T>(this ISession session, string key)
    {
        var data = session.GetString(key);
        return data == null ? default : JsonConvert.DeserializeObject<T>(data);
    }

    public static void SetObject(this ISession session, string key, object value)
    {
        session.SetString(key, JsonConvert.SerializeObject(value));
    }
}

namespace Calendar_Tracker.Pages
{
    public class CalendarModel : PageModel
    {
        // Properties to hold the date values
        [BindProperty]
        public int EditedMonth { get; set; }

        [BindProperty]
        public int EditedDay { get; set; }

        [BindProperty]
        public int EditedYear { get; set; }


        public void OnGet()
        {
            Console.WriteLine("OnGet method executed.");

            int EditedMonth = DateTime.Now.Month;
            int EditedDay = DateTime.Now.Day;
            int EditedYear = DateTime.Now.Year;

    
            HttpContext.Session.SetObject("EditedMonth", EditedMonth);
            HttpContext.Session.SetObject("EditedDay", EditedDay);
            HttpContext.Session.SetObject("EditedYear", EditedYear);

            CalculateDate();
        }

        public void OnPost()
        {
            Console.WriteLine("OnPost method executed.");
        }

        [HttpPost]
        public IActionResult UpdateDate(int editedMonth, int editedDay, int editedYear)
        {
            Console.WriteLine("OnPostUpdateDate method executed.");

            

            // Convert the abbreviated month name to its numerical representation
            //string[] Months = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
            //int MonthNumber = Array.IndexOf(Months, editedMonth) + 1;
            EditedMonth = editedMonth;
            HttpContext.Session.SetObject("EditedMonth", EditedMonth);

            //int DayNumber = int.Parse(editedDay.Trim(',')); // Remove comma from day
            EditedDay = editedDay;
            HttpContext.Session.SetObject("EditedDay", EditedDay);

            //int YearNumber = int.Parse(model.PropertyValue);
            EditedYear = editedYear;
            HttpContext.Session.SetObject("EditedYear", EditedYear);

            
            
            // Perform any common processing
            CalculateDate();

            // Return a JSON response using JsonResult
            return new JsonResult(new { success = true, message = "Update successful" });
        }

        public class UpdateDateModel
        {
            public string PropertyName { get; set; }
            public string PropertyValue { get; set; }
        }

        public void CalculateDate()
        {
            EditedMonth = HttpContext.Session.GetObject<int>("EditedMonth");
            EditedDay = HttpContext.Session.GetObject<int>("EditedDay");
            EditedYear = HttpContext.Session.GetObject<int>("EditedYear");

            Console.WriteLine($"EditedYear: {EditedYear}");
            Console.WriteLine($"EditedMonth: {EditedMonth}");
            Console.WriteLine($"EditedDay: {EditedDay}");
            DateTime EditDate = new DateTime(EditedYear, EditedMonth, EditedDay);
        }

        public void OnGetTerminal()
        {
            // Load Calendar data
            CalendarMap myCalendar = CalendarMap.LoadFromFile("calendarData.xml");
            DayNotes retrievedNotes;

            // Get the current time
            DateTime now = DateTime.Today;

            // Convert Time to ID
            string todayID = ID.DateToID(now);

            // Write it to console and page
            Console.WriteLine("ID of today is " + todayID);
            ViewData["todayID"] = todayID;

            // Draw Calendar
            UI.DrawCalendar(todayID);
            // Display Notes
            retrievedNotes = myCalendar.GetDayNotes(todayID);
            UI.DisplayNotes(retrievedNotes);

            // Input Date
            DateTime inputDateTime;
            while (true)
            { // Make sure input is valid
                Console.Write("Input date (mm/dd/yyyy): ");
                string sinput = Console.ReadLine() ?? "mm/dd/yyyy";

                //cin >> get_time(&inputDateTime, "%m/%d/%Y");
                if (DateTime.TryParse(sinput, out inputDateTime))
                //inputDateTime = ConvertToDateTime(input); standalone method of getting DateTime from string
                {
                    //Console.WriteLine("You entered: " + inputDateTime);
                    break; // Continue
                }
                else
                {
                    Console.WriteLine("Invalid input"); // Try Again
                }
            }
            string inputID = ID.DateToID(inputDateTime);


            // Draw Calendar
            UI.DrawCalendar(inputID);

            // Retrieve and print notes for the day
            Console.WriteLine("Current Notes: ");
            retrievedNotes = myCalendar.GetDayNotes(inputID);
            UI.DisplayNotes(retrievedNotes);


            // Add notes for a day
            DayNotes notes = new();


            // Day Quality
            int input;
            while (true)
            { // Make sure input is valid
                Console.Write("Input Day Quality: ");

                string sinput = Console.ReadLine() ?? "day quality";
                if (int.TryParse(sinput, out _))
                {
                    // Valid Integer Input
                    input = int.Parse(sinput); // Convert the input to an integer
                    input = int.Clamp(input, 0, 10); // Clamp the input to the range of 0 to 10
                    break;
                }
                Console.WriteLine("Invalid input");
            }
            // Success
            Console.WriteLine("You entered: " + input);
            notes.DayQuality = input;

            // Sleep Quality
            while (true)
            { // Make sure input is valid
                Console.Write("Input Sleep Quality: ");

                string sinput = Console.ReadLine() ?? "sleep quality";
                if (int.TryParse(sinput, out _))
                {
                    // Valid Integer Input
                    input = int.Parse(sinput); // Convert the input to an integer
                    input = int.Clamp(input, 0, 10); // Clamp the input to the range of 0 to 10
                    break;
                }
                Console.WriteLine("Invalid input");
            }
            // Success
            Console.WriteLine("You entered: " + input);
            notes.SleepQuality = input;

            // Took Meds
            while (true)
            {
                bool binput;
                // Make sure input is valid
                Console.Write("Took Meds?: ");

                string sinput = Console.ReadLine() ?? "took meds";
                sinput = sinput.ToLower();

                if (sinput == "1" || sinput == "yes" || sinput == "true" || sinput == "y")
                {
                    binput = true;
                }
                else if (sinput == "0" || sinput == "no" || sinput == "false" || sinput == "n")
                {
                    binput = false;
                }
                else
                {
                    // Invalid input, try again
                    Console.WriteLine("Invalid input");
                    continue; // Skip the rest of the loop
                }
                // Success
                notes.TookMeds = binput;
                break;
            }
            // Success
            Console.WriteLine("You entered: " + (notes.TookMeds ? "Yes" : "No"));

            notes.Exists = true;


            myCalendar.AddDay(inputID, notes);

            // Retrieve and print notes for the day
            Console.WriteLine("New Notes: ");
            retrievedNotes = myCalendar.GetDayNotes(inputID);
            UI.DisplayNotes(retrievedNotes);

            CalendarMap.SaveToFile("calendarData.xml", myCalendar);
        }
    }
}
