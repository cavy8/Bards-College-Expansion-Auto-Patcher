# Bards College Expansion Auto Patcher — Implementation Plan

A Synthesis patcher that reconciles third-party plugin changes with the Bards College Expansion (BCE) mod by KingGath. BCE creates a duplicate of the vanilla `SolitudeBardsCollege` cell (`kgcBardHijackSolitudeBardsCollege`) for compatibility. This patcher ensures that other plugins' additions and modifications to the original cell are correctly propagated to the BCE cell.

## User Review Required

> [!IMPORTANT]
> **Questions that will affect design — please answer before I proceed:**
>
> 1. **BCE plugin filename** — confirmed as `kinggathcreations_bard.esm`.
2. **Cell FormKeys** — Vanilla: `0x16A0C`, BCE: `0x1F0F8`.
3. **Original Skyrim masters** — `Skyrim.esm`, `Update.esm`, `Dawnguard.esm`, `HearthFires.esm`, `Dragonborn.esm`.
4. **Move semantics** — Literally change location to the same x/y/z in the BCE cell.
5. **FormLink swap scope** — Bulk remap across all record types.
6. **Equality Strategy** — Use Mutagen's record equality checks first.
7. **Persistence** — Persistent should win.

---

## Proposed Changes

### Overview of Phases

| Phase | Goal | Testable Milestone |
|-------|------|--------------------|
| **1** | Project setup, CSV loading, BCE mod detection | Build succeeds; CSV parses correctly; logs BCE mod found |
| **2** | Move new cell references into BCE cell | New refs appear in BCE cell in xEdit; removed from vanilla cell |
| **3** | Sync changes on matching references | Modified props on Cell1 ref → same props on Cell2 ref |
| **4** | Swap cell/ref references in other plugins | Conditions, packages etc. now point to BCE equivalents |
| **5** | Remap NavigationDoorLinks to BCE navmeshes | Doors link to new BCE navmesh triangles correctly |

Each phase produces a working patcher you can run and inspect in xEdit before moving on.

---

### Phase 1 — Project Setup & CSV Loading

#### [MODIFY] [BardsCollegeExpansionAutoPatcher.csproj](file:///D:/Skyrim%20-%20Dev/Bards%20College%20Expansion%20Auto%20Patcher/Bards%20College%20Expansion%20Auto%20Patcher/BardsCollegeExpansionAutoPatcher/BardsCollegeExpansionAutoPatcher.csproj)

- Add `Mutagen.Bethesda.FormKeys.SkyrimSE` package (for named cell constants)
- Add `CsvHelper` or use manual CSV parsing (manual is simpler for this format)
- Embed `BardsCollegeMatchingRefs.csv` as a Content/resource file

#### [NEW] [Settings.cs](file:///D:/Skyrim%20-%20Dev/Bards%20College%20Expansion%20Auto%20Patcher/Bards%20College%20Expansion%20Auto%20Patcher/BardsCollegeExpansionAutoPatcher/Settings.cs)

Settings exposed in the Synthesis UI:
- `List<string> BlacklistedPlugins` — plugins to skip entirely
- `bool Debug` — verbose logging

#### [NEW] [RefMapping.cs](file:///D:/Skyrim%20-%20Dev/Bards%20College%20Expansion%20Auto%20Patcher/Bards%20College%20Expansion%20Auto%20Patcher/BardsCollegeExpansionAutoPatcher/RefMapping.cs)

- Loads [BardsCollegeMatchingRefs.csv](file:///D:/Skyrim%20-%20Dev/Bards%20College%20Expansion%20Auto%20Patcher/BardsCollegeMatchingRefs.csv) at runtime
- Parses hex FormIDs, strips the load-order prefix (first 2 hex digits), and constructs `FormKey` objects using the known source mod names (`Skyrim.esm` for Cell1, `kinggathcreations_bard.esm` for Cell2)
- Exposes:
  - `Dictionary<FormKey, FormKey> Cell1ToCell2` (vanilla ref → BCE ref)
  - `Dictionary<FormKey, FormKey> Cell2ToCell1` (reverse lookup)
  - `HashSet<FormKey> AllCell1Refs` (for quick membership checks)

**CSV Parsing Strategy:** The CSV has format `Cell1_RefFormID,Cell1_BaseFormID,Cell2_RefFormID,Cell2_BaseFormID`. The first 2 hex characters are the load-order index; we strip them and use the remaining 6 hex characters as the FormKey ID, paired with the source mod filename.

- Cell1 refs starting with `00` → `Skyrim.esm`
- Cell1 refs starting with other values → we need to determine which ESM (could be from `Update.esm`, `Dawnguard.esm`, etc.). Looking at the data, all Cell1 refs start with `00` (they're vanilla references). We'll validate this assumption.
- Cell2 refs starting with `05` → `kinggathcreations_bard.esm` (to be confirmed)

#### [MODIFY] [Program.cs](file:///D:/Skyrim%20-%20Dev/Bards%20College%20Expansion%20Auto%20Patcher/Bards%20College%20Expansion%20Auto%20Patcher/BardsCollegeExpansionAutoPatcher/Program.cs)

- Use user-defined patch name from Synthesis
- Detect BCE plugin (`kinggathcreations_bard.esm`) presence in load order
- Add it as a master reference
- Load `RefMapping` (mapping CSV entries to `Skyrim.esm` and `kinggathcreations_bard.esm`)
- Resolve Cell FormID/FormKeys:
  - Vanilla: `0x16A0C`
  - BCE: `0x1F0F8`
- Log diagnostic info

---

### Phase 2 — Move New References into BCE Cell

#### [MODIFY] [Program.cs](file:///D:/Skyrim%20-%20Dev/Bards%20College%20Expansion%20Auto%20Patcher/Bards%20College%20Expansion%20Auto%20Patcher/BardsCollegeExpansionAutoPatcher/Program.cs)

**Logic:**
1. Iterate all placed objects, NPCs, and traps whose winning override places them in `SolitudeBardsCollege`
2. For each reference:
   - Skip if the reference's originating mod is one of the vanilla ESMs (BCE already handles these)
   - Skip if the reference's FormKey is in the CSV mapping (it already has a BCE equivalent — handled in Phase 3)
   - Skip if originating from the BCE plugin itself
   - Skip if originating from a blacklisted plugin
3. If the reference passes all filters, **move it** from the vanilla cell to the BCE cell:
   - Get/add the vanilla cell as an override in the patch
   - Get/add the BCE cell as an override in the patch
   - Remove from vanilla cell's `Persistent` or `Temporary` list
   - Add to BCE cell's corresponding list

**What we move:** `IPlacedObject`, `IPlacedNpc`, `IAPlacedTrap`

---

### Phase 3 — Sync Changes to Matching References

#### [MODIFY] [Program.cs](file:///D:/Skyrim%20-%20Dev/Bards%20College%20Expansion%20Auto%20Patcher/Bards%20College%20Expansion%20Auto%20Patcher/BardsCollegeExpansionAutoPatcher/Program.cs)

**Logic:**
1. For each entry in the CSV mapping (`cell1Ref → cell2Ref`):
   - Resolve the Cell1 ref's winning override (across entire load order, excluding blacklisted plugins)
   - Resolve the Cell1 ref's *original definition* from its originating ESM
   - Skip if the winning override is from a vanilla ESM or the BCE plugin (no third-party changes)
   - Compare the winning override against the original definition
   - If changes are detected, resolve the Cell2 ref context and apply the same deltas

**Properties to sync:**
- `Base` record (if the base form was swapped)
- `Placement` (Position + Rotation)
- `Scale`
- `EnableParent`
- `MajorRecordFlags` / `SkyrimMajorRecordFlags`
- `Ownership`
- `LockData`
- `LinkedReferences`
- `ActivateParents`
- `Primitive`
- All other subrecords that could be modified

**Approach:** Deep copy the winning Cell1 ref, then selectively apply changes to the Cell2 ref override. We compare field-by-field against the original definition to detect what changed, and apply only those changes.

> [!NOTE]
> An alternative to field-by-field comparison is to use Mutagen's record equality checks. Fields not equal to the original definition are considered "changed" and are applied to the Cell2 ref. This keeps the logic simpler and automatically handles future field additions.

---

### Phase 4 — Swap Cell & Reference Pointers in Other Plugins

#### [MODIFY] [Program.cs](file:///D:/Skyrim%20-%20Dev/Bards%20College%20Expansion%20Auto%20Patcher/Bards%20College%20Expansion%20Auto%20Patcher/BardsCollegeExpansionAutoPatcher/Program.cs)

**Logic build a swap map:**
```
FormKey vanillaCell  → FormKey bceCell
FormKey cell1Ref[0]  → FormKey cell2Ref[0]
FormKey cell1Ref[1]  → FormKey cell2Ref[1]
...
```

**Where to scan:** All records in the load order from non-vanilla, non-BCE, non-blacklisted plugins. Specifically:

1. **Packages** — Conditions referencing the cell or placed refs, package data locations
2. **Quests** — Stage conditions, aliases with forced/specific references, conditions on quest fragments
3. **Scenes / DialogTopics / DialogResponses** — Conditions
4. **AI Packages** — Linked references inside package data
5. **Any record with FormLinks** — Use `EnumerateFormLinks()` to find and replace

**Strategy:** Use Mutagen's `IFormLinkContainerGetter` / `RemapLinks()` method which provides a built-in mechanism to remap FormLinks across an entire record. We build a `Dictionary<FormKey, FormKey>` swap map and call `record.RemapLinks(swapMap)` on each winning override from qualifying plugins.

> [!WARNING]
> `RemapLinks` is powerful but replaces *all* FormLink occurrences within a record. This is exactly what we want here, but we need to be careful not to remap records from vanilla ESMs or the BCE plugin itself.

---

## Edge Cases & Considerations

| Edge Case | Handling |
|-----------|----------|
| A mod adds a **new** NPC to the bards college AND references it in packages | Phase 2 moves the NPC; Phase 4 does NOT remap (it's a new FormKey, not in the mapping) — this is correct, the NPC just lives in the BCE cell now |
| A mod changes both a Cell1 ref AND references it elsewhere | Phase 3 syncs the ref properties; Phase 4 swaps any pointers to it |
| The BCE plugin itself modifies a matched ref | We skip BCE plugin changes (they're the baseline) |
| Cell1 ref has been deleted by a mod | Check for deletion flags and skip or handle accordingly |
| CSV has entries where Cell1 and Cell2 base records differ (rows 30, 31, etc.) | These are intentional BCE changes; when syncing, we keep BCE's base record unless the *vanilla* base was also changed by a third-party mod |
| Persistent vs Temporary | Persistent should win |
| Load order variability of the `05` prefix in CSV | We strip the load-order index and use mod name; load-order position doesn't matter |

---

## File Structure After Implementation

```
BardsCollegeExpansionAutoPatcher/
├── Program.cs              — Main patcher logic (4 phases)
├── Settings.cs             — Synthesis UI settings
├── RefMapping.cs           — CSV loader + FormKey mapping
├── BardsCollegeExpansionAutoPatcher.csproj
└── data/
    └── BardsCollegeMatchingRefs.csv  (embedded resource)
```

---

## Verification Plan

### Automated Checks (Build + Runtime Logging)
1. **Build:** `dotnet build` in the project directory — must succeed with no errors
2. **Dry-run logging:** Run the patcher with `debug = true` and inspect console output for:
   - CSV loaded with expected number of entries (~1483)
   - BCE plugin detected
   - Correct number of refs moved / synced / swapped

### Manual Verification (xEdit Inspection)
After each phase, run the patcher against your Skyrim load order via Synthesis, then open the generated `BardsCollegeExpansionPatch.esp` in xEdit:

1. **Phase 1:** Verify the patch ESP exists, has BCE as a master, and contains no records yet
2. **Phase 2:** Check that new references (from non-vanilla mods) in `SolitudeBardsCollege` have been moved to `kgcBardHijackSolitudeBardsCollege`
3. **Phase 3:** Pick a ref that was modified by a third-party mod → verify the BCE equivalent has the same modifications
4. **Phase 4:** Find a package/quest/condition that originally referenced `SolitudeBardsCollege` → verify it now points to the BCE cell

> [!TIP]
> For Phase 2 and 3 testing, I recommend having at least one small test mod that adds a reference to the vanilla bards college cell and one that modifies an existing reference. If you don't have one, I can create a dummy test plugin.

---

### Phase 5 — Remap NavigationDoorLinks to BCE Navmeshes

**Logic:**
1. Build navmesh centroid maps for both the vanilla cell and the BCE cell.
2. Match vanilla navmeshes to BCE navmeshes based on the closest distance between their centroids.
3. Iterate all placed doors that have been patched/moved.
4. If a door has a `NavigationDoorLink` pointing to a vanilla navmesh, remap it to the matched BCE navmesh.

## Navmesh Centroid Matching Approach

To identify which vanilla navmesh matches which BCE navmesh without a hardcoded CSV, we use the following programmatic approach:

1.  **Centroid Calculation**: For every `NAVM` record in the cell, we compute its **centroid** by averaging the X, Y, and Z coordinates of all vertices in its vertex list (`Data.Vertices`).
2.  **Distance Matching**: For each navmesh in the vanilla cell, we find the navmesh in the BCE cell with the smallest Euclidean distance between their centroids.
3.  **Validation**: Since BCE preserves the original Bards College geometry (it is an expansion, not a redesign), the centroids of corresponding navmeshes should be nearly identical (distance close to 0).
4.  **Remapping**: Once the pairs are matched, any door's `NavigationDoorLink` that points to a vanilla navmesh is updated to reference the corresponding matched BCE navmesh.
