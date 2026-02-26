# Bards College Expansion Auto Patcher

This is a Synthesis patcher for the Bards College Expansion mod for Skyrim Special Edition.

## Process

### Bards College Cell
- Phase 2: Move New References into BCE Cell
- Phase 3: Sync changes to matching references and cell records
- Phase 4: Swap cell/reference pointers in other plugins
- Phase 5: Sync changes to doors

### Winking Skeever Cell
- Phase 2: Copy (not move) new references into BCE cell
- Phase 3: Sync changes to matching references and cell records
- Phase 4: Swap cell/reference pointers only within cloned records and/or bce cell
- Phase 5: skip