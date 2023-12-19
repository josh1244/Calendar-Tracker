using Microsoft.AspNetCore.Mvc.RazorPages;
using static Calendar;

namespace Calendar_Tracker.Pages
{
    public class CalendarModel : PageModel
    {
        public void OnGet()
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
                string sinput = Console.ReadLine();

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
            DayNotes notes = new DayNotes();


            // Day Quality
            int input;
            while (true)
            { // Make sure input is valid
                Console.Write("Input Day Quality: ");

                string sinput = Console.ReadLine();
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
            notes.dayQuality = input;

            // Sleep Quality
            while (true)
            { // Make sure input is valid
                Console.Write("Input Sleep Quality: ");

                string sinput = Console.ReadLine();
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
            notes.sleepQuality = input;

            // Took Meds
            while (true)
            {
                bool binput;
                // Make sure input is valid
                Console.Write("Took Meds?: ");

                string sinput = Console.ReadLine();
                sinput.ToLower();

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
                notes.tookMeds = binput;
                break;
            }
            // Success
            Console.WriteLine("You entered: " + (notes.tookMeds ? "Yes" : "No"));

            notes.exists = true;


            myCalendar.AddDay(inputID, notes);

            // Retrieve and print notes for the day
            Console.WriteLine("New Notes: ");
            retrievedNotes = myCalendar.GetDayNotes(inputID);
            UI.DisplayNotes(retrievedNotes);

            CalendarMap.SaveToFile("calendarData.xml", myCalendar);
        }
    }
}
