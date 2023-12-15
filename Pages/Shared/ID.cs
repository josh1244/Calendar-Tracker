internal class ID
{
    // Function to convert string ID to DateTime date
    public static DateTime IDToDate(string id)
    {
        DateTime date;
        string[] parts = id.Split('-'); // split the string by the hyphen character
        string WeekDay = parts[0]; // Day of Week Ignored
        int month = int.Parse(parts[1]); // Month 1-12
        int day = int.Parse(parts[2]); // Day
        int year = int.Parse(parts[3]); // Year
        date = new DateTime(year, month, day); // Create a DateTime object
        return date;
    }

    // Function to convert tm date to string ID
    public static string DateToID(DateTime date)
    {
        // Extract week number, day of the week, and year
        int dayOfWeek = (int)date.DayOfWeek; // Week begins at 0 on Sunday
        int month = (int)date.Month; // Month is 1-12
        int dayOfMonth = (int)date.Day; // Day of month
        int year = (int)date.Year; // Year

        // DayOfWeek-Month-DayOfMonth-Year
        string id = "";

        id += dayOfWeek.ToString();
        id += "-";
        if (month.ToString().Length == 1)
        { // add zero if month is single digit
            id += "0";
        }
        id += month.ToString();
        id += "-";
        if (dayOfMonth.ToString().Length == 1)
        { // add zero if dayOfMonth is single digit
            id += "0";
        }
        id += dayOfMonth.ToString();
        id += "-";
        id += year.ToString();

        return id;
    }
}