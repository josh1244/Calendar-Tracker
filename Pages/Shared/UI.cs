using System;
using static System.Net.Mime.MediaTypeNames;

internal class UI
{
    // Function to display notes
    internal static void DisplayNotes(Calendar.DayNotes notes)
    {
        if (notes.exists)
        {
            Console.WriteLine("Day Quality: " + notes.dayQuality);
            Console.WriteLine("Sleep Quality: " + notes.sleepQuality);
            Console.WriteLine("Took Meds: " + (notes.tookMeds ? "Yes" : "No"));
            Console.WriteLine();
        }
        else
        {
            Console.WriteLine("No notes yet");
            Console.WriteLine();
        }
    }

    // Function to draw Calendar
    internal static void DrawCalendar(string id)
    {
        DateTime date = ID.IDToDate(id);

        int weekNumber = date.DayOfYear / 7 + 1;
        int day = date.Day; // Day of month
        int dayOfWeek = (int)date.DayOfWeek; // Week begins at 0 on Sunday
        int year = date.Year; // Year

        //Figure out the days of the week
        int[] days = new int[7];
        DateTime prev;
        for (int i = 0; i < 7; i++)
        {
            prev = date.AddDays(i - dayOfWeek);
            days[i] = (int)prev.Day;
        }

        // Asign month name to month
        string month = date.Month switch
        {
            1 => "January",
            2 => "Febuary",
            3 => "March",
            4 => "April",
            5 => "May",
            6 => "June",
            7 => "July",
            8 => "August",
            9 => "September",
            10 => "October",
            11 => "November",
            12 => "December",
            //_ => "MONTH ERROR",
            _ => throw new ArgumentException($"MONTH ERROR: {date.Month}", nameof(date.Month)),
        };

        Console.WriteLine("Week: " + weekNumber + "                         " + year);
        Console.WriteLine(month);
        Console.WriteLine("_______________________________________________________________________");
        Console.WriteLine("|  Sunday |  Monday | Tuesday |Wednesday| Thursday| Friday  | Saturday|");
        Console.WriteLine("|   " + days[0].ToString().PadLeft(2) +
            "    |   " + days[1].ToString().PadLeft(2) +
            "    |   " + days[2].ToString().PadLeft(2) +
            "    |   " + days[3].ToString().PadLeft(2) +
            "    |   " + days[4].ToString().PadLeft(2) +
            "    |   " + days[5].ToString().PadLeft(2) +
            "    |   " + days[6].ToString().PadLeft(2) +
            "    |");
        Console.WriteLine("|_________|_________|_________|_________|_________|_________|_________|");
    }

}
