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

public class IndexModel : PageModel
{
    [BindProperty]
    public int SliderValue1 { get; set; }
    public int SliderValue2 { get; set; }

    

    







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
        if (RetrievedNotes.exists)
        {
            SliderValue1 = RetrievedNotes.dayQuality;
            SliderValue2 = RetrievedNotes.sleepQuality;
        }
        else
        {
            // Only set the initial value if it hasn't been set yet
            SliderValue1 = -1; // Set initial value to DayNote's value;
            SliderValue2 = -1; // Set initial value to DayNote's value
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
        //DayNotes note = RetrievedNotes;

        if (!string.IsNullOrEmpty(Request.Form["DayQualitySubmit"]))
        {
            // Bind the value from the form to the SliderValue property
            if (int.TryParse(Request.Form["DayQuality"], out int sliderValue))
            {
                note.exists = true;
                note.dayQuality = sliderValue;
                Console.WriteLine($"dayQuality: {sliderValue}");
            }
        }

        if (!string.IsNullOrEmpty(Request.Form["SleepQualitySubmit"]))
        {
            // Bind the value from the form to the SliderValue property
            if (int.TryParse(Request.Form["SleepQuality"], out int sliderValue))
            {
                note.exists = true;
                note.sleepQuality = sliderValue;
                Console.WriteLine($"sleepQuality: {sliderValue}");
            }
        }
        Console.WriteLine($"note after editing directly: {JsonConvert.SerializeObject(note)}");

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
        if (note.exists)
        {
            SliderValue1 = note.dayQuality;
            SliderValue2 = note.sleepQuality;
        }
        else
        {
            // Only set the initial value if it hasn't been set yet
            SliderValue1 = -1; // Set initial value to DayNote's value;
            SliderValue2 = -1; // Set initial value to DayNote's value
        }
    }

    public IActionResult OnPostUpdateSlider(int sliderNumber, int sliderValue)
    {
        Console.WriteLine("OnPostUpdateSlider method executed.");

        // Handle the slider update here
        if (sliderNumber == 1)
        {
            SliderValue1 = sliderValue;
        }
        else if (sliderNumber == 2)
        {
            SliderValue2 = sliderValue;
        }

        // Update the notes and save
        UpdateNotesAndSave();

        return new EmptyResult(); // Return an empty result for AJAX requests
    }
}
