using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;

namespace MidiMapper;

public sealed class MidiConverter(INoteMap noteMap)
{
    public ConversionResult Convert(string inputPath, string outputPath, bool overwrite = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        inputPath = Path.GetFullPath(inputPath);
        outputPath = Path.GetFullPath(outputPath);

        ArgumentNullException.ThrowIfNull(noteMap);
        if (string.Equals(inputPath, outputPath,
            OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
            throw new ArgumentException("Input and output paths must be different.");

        if (!File.Exists(inputPath))
            throw new FileNotFoundException("Input MIDI file not found.", inputPath);

        using var input = File.OpenRead(inputPath);
        if (input.Length == 0) throw new FormatException("The MIDI file is empty.");
        var midiFile = MidiFile.Read(input, new ReadingSettings { SilentNoteOnPolicy = SilentNoteOnPolicy.NoteOn });

        var mappedNotes = 0;
        var unmappedNotes = new Dictionary<byte, int>();

        foreach (var trackChunk in midiFile.GetTrackChunks())
        {
            foreach (var midiEvent in trackChunk.Events)
            {
                if (midiEvent is not NoteEvent noteEvent)
                    continue;

                var sourceNote = (byte)noteEvent.NoteNumber;

                var isPlayedNote = noteEvent is NoteOnEvent on && on.Velocity > 0;
                if (noteMap.TryMap(sourceNote, out var targetNote))
                {
                    noteEvent.NoteNumber = (SevenBitNumber)targetNote;
                    if (isPlayedNote) mappedNotes++;
                }
                else if (isPlayedNote)
                {
                    unmappedNotes[sourceNote] =
                        unmappedNotes.GetValueOrDefault(sourceNote) + 1;
                }
            }
        }

        // Serialize completely before replacing the destination, so failed writes
        // cannot leave an existing output partially overwritten.
        var temporaryPath = Path.Combine(Path.GetDirectoryName(outputPath)!, $".{Guid.NewGuid():N}.tmp");
        try
        {
            midiFile.Write(temporaryPath);
            File.Move(temporaryPath, outputPath, overwrite);
        }
        finally
        {
            if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
        }

        return new ConversionResult(
            inputPath,
            outputPath,
            mappedNotes,
            unmappedNotes);
    }
}


