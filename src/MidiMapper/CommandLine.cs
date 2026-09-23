namespace MidiMapper;

// Kept as a small test-facing compatibility wrapper; the application CLI lives in Program.
public static class CommandLine
{
    public const string Usage = Program.Usage;

    public static int Run(string[] args, TextWriter output, TextWriter error)
        => Program.Run(args, output, error);
}
