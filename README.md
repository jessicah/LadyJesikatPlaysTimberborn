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
the steam workshop.

Create `SharedDependencies\TimberbornDependencies.targets.user`:
```xml
<Project>
	<PropertyGroup Label="Timberborn">
		<TimberbornGame>Y:\SteamLibrary\steamapps\common\Timberborn\Timberborn_Data\Managed</TimberbornGame>
		<TimberbornWorkshop>Y:\SteamLibrary\steamapps\workshop\content\1062090</TimberbornWorkshop>
	</PropertyGroup>
</Project>
```

This will overwrite the values in `TimberbornDependencies.targets`.
