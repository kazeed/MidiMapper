# MidiMapper

MidiMapper converts drum MIDI files from the General MIDI percussion layout to the Addictive Drums 2 Standard layout. It is a small cross-platform .NET 10 command-line application built on [Melanchall.DryWetMidi](https://github.com/melanchall/drywetmidi).

## Features

- Remaps both note-on and note-off events through an injectable `INoteMap`.
- Includes a documented GM → AD2 Standard map.
- Preserves timing, velocity, channels, track structure, controllers, tempo, and other MIDI events.
- Leaves unknown notes unchanged and reports their played-hit counts.
- Supports replacement JSON maps for other AD2 presets or drum instruments.
- Does not modify the input file.

The converter processes every MIDI channel. Use a drum-only file, or isolate the GM percussion channel before conversion, so pitched instrument tracks are not remapped accidentally.

## Requirements

- .NET 10 SDK to build and run from source.
- A standard `.mid` input file.

## Build and test

```bash
dotnet restore src/MidiMapper.slnx
dotnet build src/MidiMapper.slnx
dotnet test src/MidiMapper.slnx
```

## Usage

```text
MidiMapper input.mid
MidiMapper input.mid output.mid
MidiMapper input.mid --map custom-map.json
MidiMapper --help
```

When no output is supplied, the result is written beside the input as `<inputname>_AD2.mid`. Existing output files require `--overwrite`:

```bash
dotnet run --project src/MidiMapper -- input.mid output.mid --overwrite
```

`--help` prints the complete command syntax and exit-code summary. Paths containing spaces should be quoted. Use `--` before positional paths beginning with a dash.

## Custom maps

The built-in map is in [`GeneralMidiToAd2Map.cs`](src/MidiMapper/GeneralMidiToAd2Map.cs). A custom JSON object replaces it:

```json
{ "36": 36, "38": 38, "42": 49 }
```

Keys and values must be integer MIDI pitches from 0 through 127. Missing keys pass through unchanged. The empty object is valid and performs a pass-through conversion.

The AD2 target assignments are based on [XLN Audio's published AD2 Keymap](https://support.xlnaudio.com/hc/en-us/articles/16925247222045-Addictive-Drums-2-Keymap). The built-in map covers the practical GM percussion inputs available for this conversion: direct kit pieces, Hand Clap and Electric Snare fallbacks, plus Tambourine, Cowbell, and Claves routed to AD2 Flexi articulations. Remaining GM sounds without a useful AD2 Standard target are intentionally left unmapped rather than assigned an inaccurate target. AD2 kits and presets can vary, so verify the result against the preset you use.

### Mapping limits

The built-in map intentionally leaves these GM percussion notes unchanged and reports their played hits:

- `58`: Vibraslap
- `60–74`: Bongos, congas, timbales, agogos, cabasa, maracas, whistles, and guiros
- `76–81`: Wood blocks, cuica, and triangle

These sounds do not have a sufficiently reliable one-to-one destination in the AD2 Standard keymap. Use `--map` for a deliberate project- or kit-specific choice. Notes outside the GM percussion range are also preserved unless a custom map explicitly assigns them. Since every MIDI channel is processed, use a drum-only file when the source contains pitched instruments.

## Publish a single executable

Release publishing defaults to a framework-dependent single-file executable. Supply the runtime identifier for the target platform:

```bash
# Windows x64
dotnet publish src/MidiMapper/MidiMapper.csproj -c Release -r win-x64

# Linux x64
dotnet publish src/MidiMapper/MidiMapper.csproj -c Release -r linux-x64

# macOS ARM64
dotnet publish src/MidiMapper/MidiMapper.csproj -c Release -r osx-arm64
```

The executable requires the matching .NET 10 runtime because it is framework-dependent. The Windows profile writes to `artifacts/win-x64/`.

## Contributing

Contributions are welcome:

1. Open an issue describing a bug, mapping question, or proposed feature.
2. Create a focused branch from `main`.
3. Add or update tests for behavior changes.
4. Run `dotnet build` and `dotnet test` locally.
5. Open a pull request with a clear description and verification details.

Please keep changes focused, avoid committing generated `bin/`, `obj/`, or `artifacts/` files, and do not include copyrighted MIDI content without permission. Mapping changes should include the source keymap or other evidence and explain any fallback choices. The GitHub Actions canary build must pass before merge.

## License

MidiMapper is licensed under the [GNU Affero General Public License v3.0](LICENSE).
