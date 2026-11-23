# Mods for Timberborn

## Mods

### ExtraDebugLogging

Should only be used when running into issues with blueprints and missing dependencies, adds
a load of debug output to try and narrow what is exactly missing. Once the issues have been
resolved, it is recommended to disable this mod to avoid conflicts.

### MultiFactionsOfMadness

A replacement to the Adaptors set of mods, that uses the filenames of specs to create the
multi-faction environment.

### SoundScale

This adjusts the pitch of all sound sources to 25% when the game is also running at 0.25x
speed, which allows normal audio when sped up 4x in a video editor.

### UiScale

Shouldn't be needed by most; this forces the scale to 70%, which is not possible in the
game. A mods setting panel could be useful here.

## Building

Add a user file in SharedDependencies with the path to the Timberborn managed folder and
the steam workshop. This file is ignored by git.

Create `SharedDependencies\TimberbornDependencies.targets.user`:
```xml
<Project>
	<PropertyGroup Label="Timberborn">
		<TimberbornGame>Y:\SteamLibrary\steamapps\common\Timberborn\Timberborn_Data\Managed</TimberbornGame>
		<TimberbornWorkshop>Y:\SteamLibrary\steamapps\workshop\content\1062090</TimberbornWorkshop>
		<TimberbornUserMods>O:\Onedrive\Documents\Timberborn\Mods</TimberbornUserMods>
	</PropertyGroup>
</Project>
```

`TimberbornGame` is where the game DLLs are found under your Steam Library, `TimberbornWorkshop`
is the Steam Workshop folder, and `TimberbornUserMods` is in your Documents folder, where
the built mods get written to.

If you don't want to automatically update the mods in your Timberborn folder, specify an
alternate location, and then manually copy into your Timberborn Mods folder.

Projects can opt in/out of Harmony and ModSettings by defining property items:

```xml
<PropertyGroup>
	<RequireModSettings>true</RequireModSettings>
	<RequireHarmony>false</RequireHarmony>
</PropertyGroup>
```

By default, these are set to `true`. It is recommended to set to `false` to reduce unneeded
dependencies.
