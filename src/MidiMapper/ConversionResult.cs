namespace MidiMapper;

public sealed record ConversionResult(
    string InputPath,
    string OutputPath,
    int MappedNotes,
    IReadOnlyDictionary<byte, int> UnmappedNotes);
