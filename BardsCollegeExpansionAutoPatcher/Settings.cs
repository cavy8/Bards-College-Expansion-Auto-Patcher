using Mutagen.Bethesda.Synthesis.Settings;

namespace BardsCollegeExpansionAutoPatcher;

public sealed class Settings
{
    [SynthesisTooltip("Plugins to ignore entirely. Example: SomePatch.esp")]
    public List<string> BlacklistedPlugins { get; set; } = [];

    [SynthesisTooltip("Enable verbose diagnostic logging.")]
    public bool Debug { get; set; } = false;
}
