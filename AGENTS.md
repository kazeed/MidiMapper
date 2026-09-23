# AI contribution guide

This repository contains a small .NET 10 console application for converting drum MIDI note numbers from General MIDI to Addictive Drums 2 Standard.

## Required workflow

1. Read `README.md` and the relevant tests before changing behavior.
2. Keep changes focused on the requested task.
3. Run `dotnet build src/MidiMapper.slnx` and `dotnet test src/MidiMapper.slnx` before handing off.
4. If publishing is affected, verify at least one runtime-specific publish and report any unavailable runtime packs or network failures.
5. Do not commit, push, or alter Git history unless the user explicitly requests it.

## Architecture rules

- `Program.cs` owns argument parsing, help text, console output, and exit-code handling.
- `MidiConverter` contains MIDI transformation logic only and must not write to the console.
- `INoteMap` is the mapping extension point; do not add a container, repository, factory, or service layer.
- `ConversionResult` remains an immutable result type.
- Use Melanchall.DryWetMidi for MIDI I/O; do not add another MIDI library.
- Preserve all non-note MIDI data, timing, velocity, channels, track structure, and the source file.
- Keep nullable reference types enabled and avoid warnings where reasonably possible.

## Mapping rules

- Keep the built-in map easy to inspect in `GeneralMidiToAd2Map.cs`.
- Every mapping change must include or update a focused unit test.
- Do not invent AD2 targets for sounds without a direct equivalent. Leave them unmapped or document an explicit fallback.
- Cite the AD2 keymap or other evidence in the change description and explain collisions or compression choices.
- Remember that the converter currently processes all MIDI channels; do not silently introduce channel filtering.

## Dependency and portability rules

- Prefer the existing .NET base class library and current package references.
- Add a dependency only when the standard library cannot reasonably solve the problem, and explain why.
- Keep the app runnable on Windows and Unix-like systems; avoid OS-specific APIs in core code.
- Release publishing is framework-dependent and single-file. Runtime identifiers are supplied at publish time.
- Never commit `bin/`, `obj/`, `artifacts/`, local MIDI files, secrets, or generated archives.
