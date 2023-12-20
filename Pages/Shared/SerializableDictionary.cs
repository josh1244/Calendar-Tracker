using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

[XmlRoot("dictionary")]
#pragma warning disable CA1050 // Declare types in namespaces
public class SerializableDictionary<TKey, TValue>
#pragma warning restore CA1050 // Declare types in namespaces
    : Dictionary<TKey, TValue>, IXmlSerializable
    where TKey : notnull
{
    public SerializableDictionary() { }
    public SerializableDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary) { }
    public SerializableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) : base(dictionary, comparer) { }
    public SerializableDictionary(IEqualityComparer<TKey> comparer) : base(comparer) { }
    public SerializableDictionary(int capacity) : base(capacity) { }
    public SerializableDictionary(int capacity, IEqualityComparer<TKey> comparer) : base(capacity, comparer) { }

    #region IXmlSerializable Members
    public System.Xml.Schema.XmlSchema GetSchema()
    {
        return null!;
    }

    public void ReadXml(System.Xml.XmlReader reader)
    {
        XmlSerializer keySerializer = new(typeof(TKey));
        XmlSerializer valueSerializer = new(typeof(TValue));

        bool wasEmpty = reader.IsEmptyElement;
        reader.Read();

        if (wasEmpty)
            return;

        while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
        {
            reader.ReadStartElement("item");

            reader.ReadStartElement("key");
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            TKey key = (TKey)keySerializer.Deserialize(reader);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            reader.ReadEndElement();

            reader.ReadStartElement("value");
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            TValue value = (TValue)valueSerializer.Deserialize(reader);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            reader.ReadEndElement();

#pragma warning disable CS8604 // Possible null reference argument.
            this.Add(key, value);
#pragma warning restore CS8604 // Possible null reference argument.

            reader.ReadEndElement();
            reader.MoveToContent();
        }
        reader.ReadEndElement();
    }

    public void WriteXml(System.Xml.XmlWriter writer)
    {
        XmlSerializer keySerializer = new(typeof(TKey));
        XmlSerializer valueSerializer = new(typeof(TValue));

        foreach (TKey key in this.Keys)
        {
            writer.WriteStartElement("item");

            writer.WriteStartElement("key");
            keySerializer.Serialize(writer, key);
            writer.WriteEndElement();

            writer.WriteStartElement("value");
            TValue value = this[key];
            valueSerializer.Serialize(writer, value);
            writer.WriteEndElement();

            writer.WriteEndElement();
        }
    }
    #endregion
}