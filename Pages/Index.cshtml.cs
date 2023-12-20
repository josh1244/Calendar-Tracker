using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System;
using System.Globalization;
using static Calendar;

/*
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
*/

#pragma warning disable CA1050 // Declare types in namespaces
public class IndexModel : PageModel
#pragma warning restore CA1050 // Declare types in namespaces
{
    [BindProperty]
    public int SliderValue1 { get; set; }
    public int SliderValue2 { get; set; }
    public bool CheckValue3 { get; set; }











    public void OnGet()
    {
        Console.WriteLine("OnGet method executed.");

        CalendarMap MyCalendar = CalendarMap.LoadFromFile("calendarData.xml");

        // Get the current time
        DateTime now = DateTime.Today;

        // Convert Time to ID
        string Id = ID.DateToID(now);

        // Retrieve or create a new DayNotes object
        DayNotes RetrievedNotes = MyCalendar.GetDayNotes(Id);

        //Set Sliders
        if (RetrievedNotes.Exists)
        {
            SliderValue1 = RetrievedNotes.DayQuality;
            SliderValue2 = RetrievedNotes.SleepQuality;
            CheckValue3 = RetrievedNotes.TookMeds;
        }
        else
        {
            // Only set the initial value if it hasn't been set yet
            SliderValue1 = -1; // Set initial value to DayNote's value;
            SliderValue2 = -1; // Set initial value to DayNote's value
            CheckValue3 = false; // Set initial value to DayNote's value
        }
    }

    public void OnPost()
    {
        Console.WriteLine("OnPost method executed.");

        UpdateNotesAndSave();
    }

    private void UpdateNotesAndSave()
    {
        Console.WriteLine("UpdateNotesAndSave method executed.");
        CalendarMap MyCalendar = CalendarMap.LoadFromFile("calendarData.xml");

        // Get the current time
        DateTime now = DateTime.Today;

        // Convert Time to ID
        string Id = ID.DateToID(now);

        // Retrieve or create a new DayNotes object
        DayNotes note = MyCalendar.GetDayNotes(Id);

        // Update Note in calendar
        if (!string.IsNullOrEmpty(Request.Form["Submit"]))
        {
            if (int.TryParse(Request.Form["DayQuality"], out int sliderValue1))
            {
                note.Exists = true;
                note.DayQuality = sliderValue1;
                Console.WriteLine($"dayQuality: {sliderValue1}");
            }

            if (int.TryParse(Request.Form["SleepQuality"], out int sliderValue2))
            {
                note.Exists = true;
                note.SleepQuality = sliderValue2;
                Console.WriteLine($"sleepQuality: {sliderValue2}");
            }

            // Check if the checkbox is checked
            string? tookMedsValue = Request.Form["TookMeds"];
            if (tookMedsValue == "on")
            {
                note.Exists = true;
                note.TookMeds = true;
                Console.WriteLine("tookMeds: true");
            }
            else
            {
                // Checkbox is unchecked
                note.Exists = true;
                note.TookMeds = false; // Set to your default value for unchecked state
                Console.WriteLine("tookMeds: false");
            }
        }

        // Update MyCalendar
        CalendarMap newCalendar = MyCalendar;
        newCalendar.AddDay(Id, note);

        // Retrieve notes for day
        note = newCalendar.GetDayNotes(Id);

        // Save to file
        Console.Write("Saving to file...");
        CalendarMap.SaveToFile("calendarData.xml", newCalendar);
        Console.WriteLine("Save complete.");

        // Log values for debugging
        Console.WriteLine($"note: {JsonConvert.SerializeObject(note)}");
        Console.WriteLine($"newCalendar: {JsonConvert.SerializeObject(newCalendar)}");

        //Set Sliders
        if (note.Exists)
        {
            SliderValue1 = note.DayQuality;
            SliderValue2 = note.SleepQuality;
            CheckValue3 = note.TookMeds;
        }
        else
        {
            // Only set the initial value if it hasn't been set yet
            SliderValue1 = -1; // Set initial value to DayNote's value;
            SliderValue2 = -1; // Set initial value to DayNote's value
            CheckValue3 = false; // Set initial value to DayNote's value
        }
    }
}
