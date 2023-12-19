using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System;
using System.Globalization;
using static Calendar;

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

public class IndexModel : PageModel
{
    [BindProperty]
    public int SliderValue1 { get; set; }
    public int SliderValue2 { get; set; }

    [BindProperty]
    public string Id
    {
        get => HttpContext.Session.GetString("Id");
        set => HttpContext.Session.SetString("Id", value);
    }

    public DayNotes RetrievedNotes
    {
        get => HttpContext.Session.GetObject<DayNotes>("RetrievedNotes") ?? new DayNotes();
        set => HttpContext.Session.SetObject("RetrievedNotes", value);
    }

    public CalendarMap MyCalendar
    {
        get => HttpContext.Session.GetObject<CalendarMap>("MyCalendar") ?? new CalendarMap();
        set => HttpContext.Session.SetObject("MyCalendar", value);
    }

    







    public void OnGet()
    {
        Console.WriteLine("OnGet method executed.");

        MyCalendar = CalendarMap.LoadFromFile("calendarData.xml");

        // Get the current time
        DateTime now = DateTime.Today;

        // Convert Time to ID
        Id = ID.DateToID(now);

        // Retrieve or create a new DayNotes object
        RetrievedNotes = MyCalendar.GetDayNotes(Id);

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

        if (!string.IsNullOrEmpty(Request.Form["DayQualitySubmit"]))
        {
            // Bind the value from the form to the SliderValue property
            if (int.TryParse(Request.Form["DayQuality"], out int sliderValue))
            {
                SliderValue1 = sliderValue;
                UpdateNotesAndSave(SliderValue1, null);
            }
        }

        if (!string.IsNullOrEmpty(Request.Form["SleepQualitySubmit"]))
        {
            // Bind the value from the form to the SliderValue property
            if (int.TryParse(Request.Form["SleepQuality"], out int sliderValue))
            {
                SliderValue2 = sliderValue;
                UpdateNotesAndSave(null, SliderValue2);
            }
        }
    }

    private void UpdateNotesAndSave(int? dayQuality, int? sleepQuality)
    {
        Console.WriteLine("UpdateNotesAndSave method executed.");
        // Update Note in calendar
        DayNotes note = RetrievedNotes;
        if (dayQuality.HasValue)
        {
            note.exists = true;
            note.dayQuality = dayQuality.Value;
        }

        if (sleepQuality.HasValue)
        {
            note.exists = true;
            note.sleepQuality = sleepQuality.Value;
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
    }
}
