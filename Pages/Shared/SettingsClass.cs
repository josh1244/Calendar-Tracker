using Newtonsoft.Json;
using System.Reflection.PortableExecutable;
using System.Xml.Serialization;
using static Calendar_Tracker.Pages.SettingsModel;

[Serializable]
public class Options // Class to manage the Options
{
    [Serializable] 
    public class TrackerData
    {
        public int Id { get; set; }
        public int Order { get; set; }
        public string? Name { get; set; }
        public string? Type { get; set; }
    }

    public string? GreetingOption { get; set; }
    public bool LongMonthNamesOption { get; set; }
    public SerializableDictionary<int, TrackerData> TrackersOption { get; set; } = new SerializableDictionary<int, TrackerData>();

    public Options()
    {
        GreetingOption = "Hello!";
        LongMonthNamesOption = false;
    }

    // Function to load Options from file
    public static Options LoadFromFile(string fileName)
    {
        // Check if the file exists
        if (!File.Exists(fileName))
        {
            // Return a new CalendarMap object
            return new Options();
        }

        using TextReader reader = new StreamReader(fileName);
        var serializer = new XmlSerializer(typeof(Options));

        // Check if the file is empty
        if (reader.Peek() == -1)
        {
            // Return a new Options object
            return new Options();
        }
        else
        {
            // Deserialize the Options object from the file
            try
            {
                object deserializedObject = serializer.Deserialize(reader)!;

                if (deserializedObject is Options calendarMap)
                {
                    // Successfully deserialized into a Options object
                    return calendarMap;
                }
                else
                {
                    // Handling the case where deserialization was not successful
                    return new Options();
                }
            }
            catch (Exception ex)
            {
                // Log or handle the exception
                Console.WriteLine($"Error deserializing from file: {ex.Message}");
                return new Options();
            }

        }
    }

    // Function to save Options to file
    public static void SaveToFile(string fileName, Options data)
    {
        XmlSerializer serializer = new(typeof(Options));
        using StreamWriter writer = new(fileName);
        serializer.Serialize(writer, data);
    }
}