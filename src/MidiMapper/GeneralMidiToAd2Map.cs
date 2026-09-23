using System.Collections.Frozen;

namespace MidiMapper;

/// <summary>
/// Maps General MIDI percussion notes to the corresponding AD2 Standard keymap.
/// Notes without a direct AD2 Standard equivalent are intentionally left unmapped.
/// </summary>
public sealed class GeneralMidiToAd2Map : INoteMap
{
    // AD2 target names follow XLN Audio's published AD2 Keymap PDF.
    private static readonly FrozenDictionary<byte, byte> Map = new Dictionary<byte, byte>
    {
        [35] = 36, // GM Acoustic Bass Drum 2 -> Kick
        [36] = 36, // GM Bass Drum 1 -> Kick
        [37] = 42, // GM Side Stick -> Snare SideStick
        [38] = 38, // GM Acoustic Snare -> Snare Open Hit
        [39] = 42, // GM Hand Clap -> Snare SideStick
        [40] = 37, // GM Electric Snare -> Snare Rimshot
        [41] = 65, // GM Low Floor Tom -> Tom 4
        [42] = 49, // GM Closed Hi-Hat -> HiHat Closed 1 Tip
        [43] = 65, // GM High Floor Tom -> Tom 4
        [44] = 48, // GM Pedal Hi-Hat -> HiHat Pedal Closed
        [45] = 67, // GM Low Tom -> Tom 3
        [46] = 54, // GM Open Hi-Hat -> HiHat Open A
        [47] = 69, // GM Low-Mid Tom -> Tom 2
        [48] = 71, // GM Hi-Mid Tom -> Tom 1
        [49] = 77, // GM Crash Cymbal 1 -> Cymbal 1 Hit
        [50] = 71, // GM High Tom -> Tom 1
        [51] = 60, // GM Ride Cymbal 1 -> Ride 1 Tip
        [52] = 79, // GM Chinese Cymbal -> Cymbal 2 Hit
        [53] = 61, // GM Ride Bell -> Ride 1 Bell
        [54] = 47, // GM Tambourine -> Flexi 1 Hit A
        [55] = 81, // GM Splash Cymbal -> Cymbal 3 Hit
        [56] = 73, // GM Cowbell -> Flexi 1 Hit B
        [57] = 79, // GM Crash Cymbal 2 -> Cymbal 2 Hit
        [59] = 84, // GM Ride Cymbal 2 -> Ride 2 Tip
        [75] = 74, // GM Claves (sticks) -> Flexi 1 Hit C
    }.ToFrozenDictionary();

    public bool TryMap(byte source, out byte target)
        => Map.TryGetValue(source, out target);
}
