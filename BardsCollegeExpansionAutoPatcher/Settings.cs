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

    [SynthesisTooltip("Required when Mode=SourcePlugin. Example: Lux.esp")]
    public string SourcePlugin { get; set; } = string.Empty;

    [SynthesisTooltip("Plugins to ignore entirely. Example: SomePatch.esp")]
    public List<string> BlacklistedPlugins { get; set; } = [];

    [SynthesisTooltip("Enable verbose diagnostic logging.")]
    public bool Debug { get; set; } = false;
}
