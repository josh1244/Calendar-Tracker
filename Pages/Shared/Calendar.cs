using Newtonsoft.Json;
using System.Reflection.PortableExecutable;
using System.Xml.Serialization;
using static Calendar;

public class Calendar
{
    [Serializable]
    public class DayNotes // Class DayNotes to keep notes
    {
        public bool exists { get; set; }
        public int dayQuality { get; set; }
        public int sleepQuality { get; set; }
        public bool tookMeds { get; set; }

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
            if (days.ContainsKey(id))
            {
                // Day note already exists, update it
                days[id] = notes;
            }
            else
            {
                // Day note doesn't exist, add a new one
                days.Add(id, notes);
            }
        }

        // Function to get notes for a specific day
        public DayNotes GetDayNotes(string id)
        {
            if (days.TryGetValue(id, out DayNotes value))
            {
                // Key was in dictionary; "value" contains corresponding value
                return value;
            }
            else
            {
                // Key wasn't in dictionary; create a new DayNotes object with the specified ID
                DayNotes newNotes = new DayNotes();
                days.Add(id, newNotes);
                return newNotes;
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
}