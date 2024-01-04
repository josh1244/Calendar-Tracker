using Newtonsoft.Json;
using System.Reflection.PortableExecutable;
using System.Xml.Serialization;


public class Calendar
{
    public class TrackerComponentData
    {
        public int Id { get; set; }
        public int SliderValue { get; set; }
        public bool CheckboxValue { get; set; }
        public string TextValue { get; set; }
        public string DropdownValue { get; set; }
    }

    public class DayNotes // Class DayNotes to keep notes
    {
        public bool Exists { get; set; }
        public SerializableDictionary<int, TrackerComponentData> TrackersData { get; set; } = new SerializableDictionary<int, TrackerComponentData>();


        // Define a constructor for DayNotes
        public DayNotes()
        {
            Exists = false;
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
                return value!;
            }
            else
            {
                // Key wasn't in dictionary; create a new DayNotes object with the specified ID
                DayNotes newNotes = new();
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
            using TextReader reader = new StreamReader(fileName);
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
                object deserializedObject = serializer.Deserialize(reader)!;

                if (deserializedObject is CalendarMap calendarMap)
                {
                    // Successfully deserialized into a CalendarMap object
                    return calendarMap;
                }
                else
                {
                    // Handling the case where deserialization was not successful
                    return new CalendarMap();
                }
            }
        }

        // Function to save calendar to file
        public static void SaveToFile(string fileName, CalendarMap data)
        {
            XmlSerializer serializer = new(typeof(CalendarMap));
            using StreamWriter writer = new(fileName);
            serializer.Serialize(writer, data);
        }
    }
}