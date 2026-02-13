# Bards College Expansion Auto Patcher

## Planning
- [x] Research reference project (SR-Exterior-Cities-Patcher)
- [x] Research Mutagen API and cheat sheet
- [x] Analyze CSV mapping file structure
- [x] Write implementation plan
- [x] Get user approval on plan

## Phase 1: Project Setup & CSV Loading
- [x] Update csproj with required packages
- [x] Create Settings.cs with blacklist/debug options
- [x] Create RefMapping.cs to load CSV
- [x] Create Program.cs scaffolding with BCE mod detection
- [x] Verify build

## Phase 2: Move New References
- [x] Identify refs in vanilla bards college cell NOT from original ESMs and NOT in CSV
- [x] Move (not copy) them into the BCE cell
- [ ] Verify with test load order

## Phase 3: Sync Changes to Matching References
- [ ] Compare winning overrides of Cell1 refs against their original ESM definitions
- [ ] Detect changes to base record, placement, enable parent, flags, etc.
- [ ] Apply same changes to corresponding Cell2 (BCE) refs
- [ ] Verify

## Phase 4: Swap Cell/Ref References in Other Plugins
- [ ] Scan conditions, packages, etc. for references to SolitudeBardsCollege
- [ ] Replace with kgcBardHijackSolitudeBardsCollege
- [ ] Scan for references to Cell1 placed refs and swap to Cell2
- [ ] Verify

## Phase 5: Cleanup & Polish
- [ ] Final testing
- [ ] README with credits
- [ ] Walkthrough artifact
