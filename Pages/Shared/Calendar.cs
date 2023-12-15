using System.Reflection.PortableExecutable;
using System.Xml.Serialization;

public class Calendar
{
    public class DayNotes // Class DayNotes to keep notes
    {
        public bool exists;
        public int dayQuality;
        public int sleepQuality;
        public bool tookMeds;

        // Define a constructor for DayNotes
        public DayNotes()
        {
            exists = false;
            dayQuality = 0;
            sleepQuality = 0;
            tookMeds = false;
        }
    }

    public class CalendarMap // Class to manage the calendar
    {
        public SerializableDictionary<string, DayNotes> days;  // Using string to store the unique ID

        public CalendarMap()
        {
            days = new SerializableDictionary<string, DayNotes>();
        }

        // Function to add a day with notes to the calendar
        public void AddDay(string id, DayNotes notes)
        {
            days[id] = notes;
        }

        // Function to get notes for a specific day
        public DayNotes GetDayNotes(string id)
        {
            DayNotes value;
            if (days.TryGetValue(id, out value))
            {
                // Key was in dictionary; "value" contains corresponding value
                //return days[id];
                return value;
            }
            else
            {
                // Key wasn't in dictionary; "value" is now 0
                return new DayNotes { };
            }
        }
    }

    // Function to load calendar from file
    public static CalendarMap LoadFromFile(string fileName)
    {
        // Check if the file exists
            if (!File.Exists(fileName))
        {
            // Return a new CalendarMap object
            return new CalendarMap();
        }
        using (TextReader reader = new StreamReader(fileName))
        {
                var serializer = new XmlSerializer(typeof(CalendarMap));
            // Check if the file is empty
            if (reader.Peek() == -1)
            {
                // Return a new CalendarMap object
                return new CalendarMap();
            }
            else
            {
                // Deserialize the CalendarMap object from the file
                return (CalendarMap)serializer.Deserialize(reader);
            }
        }
    }

    // Function to save calendar to file
    public static void SaveToFile(string fileName, CalendarMap data)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(CalendarMap));
        using (StreamWriter writer = new StreamWriter(fileName))
        {
            serializer.Serialize(writer, data);
        }
    }
}