using System.Text.Json;
using Melanchall.DryWetMidi.Common;

namespace MidiMapper;

public static class Program
{
    public static string Version => typeof(Program).Assembly.GetName().Version?.ToString(3) ?? "0.0.0";

    public const string Usage = """
        Usage: MidiMapper <input.mid> [output.mid] [--map mapping.json] [--overwrite]
               MidiMapper --help
               MidiMapper --version
        Default output: <input>_AD2.mid alongside the input.
        --map replaces the built-in GM-to-AD2 map with a JSON pitch-to-pitch object.
        --overwrite permits replacing an existing output. Input must be a different path.
        Use -- before positional paths beginning with a dash.
        Counts are positive-velocity note-ons. Unmapped notes are preserved.
        All MIDI channels are converted; use files containing only the intended drum parts.
        Exit codes: 0 success/help, 1 usage, 2 missing file, 3 invalid MIDI/map,
                    4 I/O or access error, 99 unexpected failure.
        """;

    public static int Main(string[] args) => Run(args, Console.Out, Console.Error);

    public static int Run(string[] args, TextWriter output, TextWriter error)
    {
        if (args.Length == 1 && args[0] is "--help" or "-h")
        {
            output.WriteLine(Usage);
            return 0;
        }
        if (args.Length == 1 && args[0] == "--version")
        {
            output.WriteLine($"MidiMapper {Version}");
            return 0;
        }

        try
        {
            var paths = new List<string>();
            string? mapPath = null;
            var overwrite = false;
            var positionalOnly = false;

            for (var i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                if (!positionalOnly && arg == "--") { positionalOnly = true; continue; }
                if (!positionalOnly && arg == "--overwrite" && !overwrite) { overwrite = true; continue; }
                if (!positionalOnly && arg == "--map" && mapPath is null)
                {
                    if (++i == args.Length || args[i].StartsWith("--"))
                        throw new ArgumentException("--map requires a JSON file path.");
                    mapPath = args[i];
                    continue;
                }
                if (!positionalOnly && arg.StartsWith('-'))
                    throw new ArgumentException($"Unknown or repeated option: {arg}");
                ArgumentException.ThrowIfNullOrWhiteSpace(arg);
                paths.Add(arg);
            }

            if (paths.Count is < 1 or > 2)
                throw new ArgumentException("Provide one input path and at most one output path.");

            var input = Path.GetFullPath(paths[0]);
            var destination = paths.Count == 2 ? Path.GetFullPath(paths[1])
                : Path.Combine(Path.GetDirectoryName(input)!, Path.GetFileNameWithoutExtension(input) + "_AD2.mid");
            INoteMap map = mapPath is null ? new GeneralMidiToAd2Map() : new JsonNoteMap(mapPath);
            var result = new MidiConverter(map).Convert(input, destination, overwrite);

            output.WriteLine($"Created: {result.OutputPath}");
            output.WriteLine($"Mapped notes: {result.MappedNotes}");
            if (result.UnmappedNotes.Count > 0)
            {
                output.WriteLine("Unmapped notes left unchanged:");
                foreach (var note in result.UnmappedNotes.OrderBy(x => x.Key))
                    output.WriteLine($"  {note.Key}: {note.Value} note(s)");
            }
            return 0;
        }
        catch (ArgumentException ex) { error.WriteLine($"{ex.Message}\n{Usage}"); return 1; }
        catch (FileNotFoundException ex) { error.WriteLine($"File not found: {ex.FileName}"); return 2; }
        catch (Exception ex) when (ex is MidiException or JsonException or FormatException)
        { error.WriteLine($"Invalid MIDI or map: {ex.Message}"); return 3; }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        { error.WriteLine($"I/O error: {ex.Message}"); return 4; }
        catch (Exception ex) { error.WriteLine($"Unexpected error: {ex.Message}"); return 99; }
    }
}
