# LethalAPI.DevTools
A Lethal Company plugin utilizing the LethalAPI framework.

## Template Usage
Use a mass replace function to replace all of the following variables. In most IDE's this option can be opened with `[Ctrl] + [Shift] + [H]`.
- Replace all instances of "`LethalAPI.DevTools`" with the assembly name. Note - This will also be the root namespace, so be careful with the naming scheme.
- Replace all instances of "`A collection of tools for LethalAPI to help make the job of developers easier.`" with a description of the plugin.
- Replace all instances of "`LethalAPI.DevTools`" with the name of the plugin.
- Replace all instances of "`LethalAPI Modding Community`" with the name(s) of the author(s) - or community.
- Select a plugin type.
  - Replace all instances of "`PLUGIN-TYPE`" with the plugin type you wish to use for the main plugin.
> Optional plugin types include `Inherited`, `Attribute`, and `MinimalAttribute`.


Plugin Types:
In order to allow choosing between multiple plugin types while keeping the "Plugin.cs" filename, we use some trickery to accomplish the renaming and inclusion via the csproj file. More documentation can be found about this inside the csproj file.