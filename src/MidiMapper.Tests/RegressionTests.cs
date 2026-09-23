using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;

namespace MidiMapper.Tests;

public sealed class RegressionTests : IDisposable
{
    private readonly string directory = Path.Combine(Path.GetTempPath(), "MidiMapperTests-" + Guid.NewGuid());
    public RegressionTests() => Directory.CreateDirectory(directory);
    private string PathFor(string name) => Path.Combine(directory, name);
    public void Dispose() => Directory.Delete(directory, recursive: true);
    private (int Code, string Output, string Error) Run(params string[] args)
    {
        using var output = new StringWriter();
        using var error = new StringWriter();
        var code = CommandLine.Run(args, output, error);
        return (code, output.ToString(), error.ToString());
    }
    private string WriteMidi()
    {
        var path = PathFor("input.mid");
        new MidiFile(new TrackChunk(
            new NoteOnEvent((SevenBitNumber)42, (SevenBitNumber)100) { DeltaTime = 12, Channel = (FourBitNumber)9 },
            new NoteOnEvent((SevenBitNumber)42, (SevenBitNumber)0) { DeltaTime = 24, Channel = (FourBitNumber)9 },
            new NoteOnEvent((SevenBitNumber)10, (SevenBitNumber)80),
            new NoteOffEvent((SevenBitNumber)10, (SevenBitNumber)0),
            new NoteOnEvent((SevenBitNumber)58, (SevenBitNumber)80),
            new NoteOnEvent((SevenBitNumber)58, (SevenBitNumber)0),
            new TextEvent("preserved"))).Write(path);
        return path;
    }
    [Fact]
    public void CountsAttacksAndPreservesTimingChannelsAndOtherEvents()
    {
        var output = PathFor("out.mid");
        var result = new MidiConverter(new GeneralMidiToAd2Map()).Convert(WriteMidi(), output);
        Assert.Equal(1, result.MappedNotes);
        Assert.Equal(1, result.UnmappedNotes[10]);
        Assert.Equal(1, result.UnmappedNotes[58]);
        var events = MidiFile.Read(output, new ReadingSettings { SilentNoteOnPolicy = SilentNoteOnPolicy.NoteOn }).GetTrackChunks().SelectMany(x => x.Events).ToArray();
        var notes = events.OfType<NoteEvent>().OrderByDescending(x => (int)x.Channel).ToArray();
        Assert.Equal(new byte[] {49,49,10,10,58,58}, notes.Select(x => (byte)x.NoteNumber));
        Assert.Equal(12, notes[0].DeltaTime);
        Assert.Equal(24, notes[1].DeltaTime);
        Assert.Equal(9, (int)notes[1].Channel);
        Assert.Equal(0, (int)Assert.IsType<NoteOnEvent>(notes[1]).Velocity);
        Assert.Equal("preserved", events.OfType<TextEvent>().Single().Text);
    }
    [Theory]
    [InlineData("")]
    [InlineData("not MIDI")]
    [InlineData("MThd\0\0\0\u0006\0")]
    public void MalformedMidiReturnsInvalidDataAndPreservesOutput(string content)
    {
        var input = PathFor("bad.mid");
        var output = PathFor("out.mid");
        File.WriteAllText(input, content);
        File.WriteAllText(output, "keep me");
        Assert.Equal(3, Run(input, output, "--overwrite").Code);
        Assert.Equal("keep me", File.ReadAllText(output));
    }
    [Fact]
    public void CliHandlesHelpUsageAndMissingFiles()
    {
        Assert.Equal(0, Run("--help").Code);
        Assert.Equal(1, Run().Code);
        Assert.Equal(1, Run("a", "b", "c").Code);
        Assert.Equal(1, Run("--unknown").Code);
        Assert.Equal(1, Run("a", "--map").Code);
        Assert.Equal(1, Run("a", "--overwrite", "--overwrite").Code);
        Assert.Equal(1, Run("\0").Code);
        Assert.Equal(2, Run(PathFor("missing.mid")).Code);
    }
    [Fact]
    public void CliDefaultOutputAndOverwriteAreExplicit()
    {
        var input = WriteMidi();
        Assert.Equal(0, Run(input).Code);
        var output = PathFor("input_AD2.mid");
        Assert.True(File.Exists(output));
        var bytes = File.ReadAllBytes(output);
        Assert.Equal(4, Run(input).Code);
        Assert.Equal(bytes, File.ReadAllBytes(output));
        Assert.Equal(0, Run(input, "--overwrite").Code);
        Assert.Equal(1, Run(input, input, "--overwrite").Code);
        Assert.Empty(Directory.GetFiles(directory, "*.tmp"));
    }
    [Theory]
    [InlineData("[]")]
    [InlineData("null")]
    [InlineData("{")]
    [InlineData("{\"128\":36}")]
    [InlineData("{\"36\":128}")]
    [InlineData("{\"36\":-1}")]
    [InlineData("{\"36\":1.5}")]
    [InlineData("{\"36\":\"42\"}")]
    [InlineData("{\"36\":42,\"36\":43}")]
    public void InvalidJsonMapsReturnInvalidData(string json)
    {
        var map = PathFor("map.json");
        File.WriteAllText(map, json);
        Assert.Equal(3, Run(WriteMidi(), "--map", map).Code);
        Assert.False(File.Exists(PathFor("input_AD2.mid")));
    }
    [Fact]
    public void CustomMapReplacesDefaultAndSupportsFullMidiRange()
    {
        var map = PathFor("map.json");
        File.WriteAllText(map, "{\"10\":127}");
        var result = Run(WriteMidi(), "--map", map);
        Assert.Equal(0, result.Code);
        Assert.Empty(result.Error);
        Assert.Contains("Mapped notes: 1", result.Output);
        Assert.Equal(new byte[] {42,42,127,127,58,58}, MidiFile.Read(PathFor("input_AD2.mid"))
            .GetTrackChunks().SelectMany(x => x.Events).OfType<NoteEvent>().OrderByDescending(x => (int)x.Channel).Select(x => (byte)x.NoteNumber));
    }
}



