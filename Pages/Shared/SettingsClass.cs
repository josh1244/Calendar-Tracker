using Newtonsoft.Json;
using System.Reflection.PortableExecutable;
using System.Xml.Serialization;


[Serializable]
public class Options // Class to manage the Options
{
    public bool LongMonthNamesOption { get; set; }

    public Options()
    {
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
    }

    // Function to save Options to file
    public static void SaveToFile(string fileName, Options data)
    {
        XmlSerializer serializer = new(typeof(Options));
        using StreamWriter writer = new(fileName);
        serializer.Serialize(writer, data);
    }
}