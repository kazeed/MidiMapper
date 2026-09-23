using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Xunit;

namespace MidiMapper.Tests;

public sealed class MidiConverterTests
{
    [Fact]
    public void Convert_RemapsMappedNotes()
    {
        var inputPath = TempMidiPath("input.mid");
        var outputPath = TempMidiPath("output.mid");

        try
        {
            WriteMidi(
                inputPath,
                new NoteOnEvent((SevenBitNumber)36, (SevenBitNumber)100),
                new NoteOffEvent((SevenBitNumber)36, (SevenBitNumber)0),
                new NoteOnEvent((SevenBitNumber)42, (SevenBitNumber)90),
                new NoteOffEvent((SevenBitNumber)42, (SevenBitNumber)0));

            var sut = new MidiConverter(new GeneralMidiToAd2Map());

            var result = sut.Convert(inputPath, outputPath);

            var output = MidiFile.Read(outputPath);

            var notes = output
                .GetTrackChunks()
                .SelectMany(x => x.Events)
                .OfType<NoteEvent>()
                .Select(x => (byte)x.NoteNumber)
                .ToArray();

#pragma warning disable IDE0230 // Use UTF-8 string literal
            Assert.Equal(
                new byte[] { 36, 36, 49, 49 },
                notes);
#pragma warning restore IDE0230 // Use UTF-8 string literal

            Assert.Equal(2, result.MappedNotes);
            Assert.Empty(result.UnmappedNotes);
        }
        finally
        {
            DeleteIfExists(inputPath);
            DeleteIfExists(outputPath);
        }
    }

    [Fact]
    public void Convert_UnmappedGmNote_IsLeftUnchangedAndReported()
    {
        var inputPath = TempMidiPath("input.mid");
        var outputPath = TempMidiPath("output.mid");

        try
        {
            WriteMidi(
                inputPath,
                new NoteOnEvent((SevenBitNumber)58, (SevenBitNumber)100),
                new NoteOffEvent((SevenBitNumber)58, (SevenBitNumber)0));

            var sut = new MidiConverter(new GeneralMidiToAd2Map());

            var result = sut.Convert(inputPath, outputPath);

            var output = MidiFile.Read(outputPath);

            var notes = output
                .GetTrackChunks()
                .SelectMany(x => x.Events)
                .OfType<NoteEvent>()
                .Select(x => (byte)x.NoteNumber)
                .ToArray();

#pragma warning disable IDE0230 // Use UTF-8 string literal
                Assert.Equal(new byte[] { 58, 58 }, notes);
#pragma warning restore IDE0230 // Use UTF-8 string literal

            Assert.True(result.UnmappedNotes.ContainsKey(58));
            Assert.Equal(1, result.UnmappedNotes[58]);
        }
        finally
        {
            DeleteIfExists(inputPath);
            DeleteIfExists(outputPath);
        }
    }

    [Fact]
    public void Convert_MissingInputFile_ThrowsFileNotFoundException()
    {
        var sut = new MidiConverter(new GeneralMidiToAd2Map());

        var missingInput = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid() + ".mid");

        var output = TempMidiPath("output.mid");

        Assert.Throws<FileNotFoundException>(
            () => sut.Convert(missingInput, output));
    }

    private static void WriteMidi(
        string path,
        params MidiEvent[] events)
    {
        var track = new TrackChunk(events);
        var midi = new MidiFile(track);

        midi.Write(path, overwriteFile: true);
    }

    private static string TempMidiPath(string suffix)
    {
        return Path.Combine(
            Path.GetTempPath(),
            $"{Guid.NewGuid()}_{suffix}");
    }

    private static void DeleteIfExists(string path)
    {
        if (File.Exists(path))
            File.Delete(path);
    }
}
