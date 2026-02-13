using System.Globalization;
using Mutagen.Bethesda.Plugins;

namespace BardsCollegeExpansionAutoPatcher;

public sealed class RefMapping
{
    public IReadOnlyDictionary<FormKey, FormKey> Cell1ToCell2 => _cell1ToCell2;
    public IReadOnlyDictionary<FormKey, FormKey> Cell2ToCell1 => _cell2ToCell1;
    public IReadOnlySet<FormKey> AllCell1Refs => _allCell1Refs;
    public IReadOnlySet<string> Cell1LoadOrderPrefixes => _cell1LoadOrderPrefixes;
    public IReadOnlySet<string> Cell2LoadOrderPrefixes => _cell2LoadOrderPrefixes;

    private readonly Dictionary<FormKey, FormKey> _cell1ToCell2 = [];
    private readonly Dictionary<FormKey, FormKey> _cell2ToCell1 = [];
    private readonly HashSet<FormKey> _allCell1Refs = [];
    private readonly HashSet<string> _cell1LoadOrderPrefixes = [];
    private readonly HashSet<string> _cell2LoadOrderPrefixes = [];

    private RefMapping()
    {
    }

    public static RefMapping Load(string csvPath)
    {
        if (!File.Exists(csvPath))
        {
            throw new FileNotFoundException("Reference mapping CSV was not found.", csvPath);
        }

        var mapping = new RefMapping();

        foreach (var line in File.ReadLines(csvPath).Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var parts = line.Split(',', StringSplitOptions.TrimEntries);
            if (parts.Length < 3)
            {
                continue;
            }

            var cell1Ref = ParseReferenceFormKey(parts[0], "Skyrim.esm", out var cell1Prefix);
            var cell2Ref = ParseReferenceFormKey(parts[2], "kinggathcreations_bard.esm", out var cell2Prefix);

            mapping._cell1ToCell2[cell1Ref] = cell2Ref;
            mapping._cell2ToCell1[cell2Ref] = cell1Ref;
            mapping._allCell1Refs.Add(cell1Ref);
            mapping._cell1LoadOrderPrefixes.Add(cell1Prefix);
            mapping._cell2LoadOrderPrefixes.Add(cell2Prefix);
        }

        return mapping;
    }

    private static FormKey ParseReferenceFormKey(string rawFormId, string modName, out string loadOrderPrefix)
    {
        var cleaned = rawFormId.Trim();
        if (cleaned.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            cleaned = cleaned[2..];
        }

        if (cleaned.Length < 8)
        {
            cleaned = cleaned.PadLeft(8, '0');
        }

        if (cleaned.Length != 8)
        {
            throw new InvalidDataException($"Unexpected FormID format: '{rawFormId}'.");
        }

        loadOrderPrefix = cleaned[..2].ToUpperInvariant();
        var localIdHex = cleaned[2..];
        if (!uint.TryParse(localIdHex, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var localId))
        {
            throw new InvalidDataException($"Unable to parse FormID: '{rawFormId}'.");
        }

        return new FormKey(ModKey.FromNameAndExtension(modName), localId);
    }
}
