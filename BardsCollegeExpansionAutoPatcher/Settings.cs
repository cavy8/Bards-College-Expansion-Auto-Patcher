using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Synthesis.Settings;

namespace BardsCollegeExpansionAutoPatcher;

public enum PatchMode
{
    SourcePlugin,
    Global
}

public sealed class Settings
{
    [SynthesisTooltip("Patch execution mode. SourcePlugin runs per-plugin deltas; Global runs broad all-plugin patching.")]
    public PatchMode Mode { get; set; } = PatchMode.SourcePlugin;

    [SynthesisTooltip("Required when Mode=SourcePlugin. Pick from your loaded plugins.")]
    public ModKey SourcePlugin { get; set; } = ModKey.Null;

    [SynthesisTooltip("Plugins to ignore entirely. Pick one or more loaded plugins.")]
    public List<ModKey> BlacklistedPlugins { get; set; } = [];

    [SynthesisTooltip("Enable verbose diagnostic logging.")]
    public bool Debug { get; set; } = false;
}
