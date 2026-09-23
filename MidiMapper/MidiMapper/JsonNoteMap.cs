using System.Globalization;
using System.Text.Json;

namespace MidiMapper;

/// <summary>A complete replacement map; missing pitches remain unchanged.</summary>
public sealed class JsonNoteMap : INoteMap
{
    private readonly Dictionary<byte, byte> notes = new();

    public JsonNoteMap(string path)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        if (document.RootElement.ValueKind != JsonValueKind.Object)
            throw new FormatException("The note map must be a JSON object.");
        foreach (var property in document.RootElement.EnumerateObject())
        {
            if (!byte.TryParse(property.Name, NumberStyles.None, CultureInfo.InvariantCulture, out var source)
                || source > 127 || property.Value.ValueKind != JsonValueKind.Number
                || !property.Value.TryGetByte(out var target) || target > 127)
                throw new FormatException("Map keys and values must be integer MIDI pitches from 0 to 127.");
            if (!notes.TryAdd(source, target))
                throw new FormatException($"Duplicate source pitch: {source}.");
        }
    }

    public bool TryMap(byte source, out byte target) => notes.TryGetValue(source, out target);
}
