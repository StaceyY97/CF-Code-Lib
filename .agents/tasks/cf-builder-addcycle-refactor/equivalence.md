# Behavior-equivalence trace: CF_Builder.cs vs CF_Builder.original.cs

## What changed in the code
1. The `if (fieldOID == "DSCONT") { ... } else { ... }` split (original lines 91-120) is gone. The body of the original `else` branch (lines 107-119: full scan of every cycle + `CF_Aid_CleanExtraCycle`) now runs for every trigger field. The unused locals `currentIns`, `folderOID`, `fieldOID` (original lines 80-82) were removed together with the branch.
2. `DoMerge` (original lines 123-136) is unchanged, including the PRTVR check.
3. `cycinfo` properties have no initializers. Every `new cycinfo { ... }` sets all six properties. Values filled in from the old defaults: `CycleRange = cycleType_2`, `DayNumber = "D1"`, `DayNumberForC3 = string.Empty`, `FolderSuf = "D1"`, `Forms = formsType_Commen`, `Forms_End = formsType_Commen_End`.

## Behavior change for DSCONT saves (Option A, user-approved)
Approval: User explicitly chose Option A in the orchestrator chat (reply: 'A'), after being told that DSCONT saves will re-sync all cycles and run CF_Aid_CleanExtraCycle. Relayed by the orchestrator.

Step (a) of the user's request, "取cycle", is implemented as iterating all cycles in each cycinfo's CycleRange. That is what Option A means. The flow does not derive the cycle from the current folder.

- **Before:** when DSCONT triggered the CF, only the current folder was evaluated: one `DoMerge(dp, ...)` call with the action data point, and no `CF_Aid_CleanExtraCycle`. A DSCONT save in a folder that matched no cycle (e.g. C3D1 for cohort 1, C4D15 for 1B) did nothing.
- **After:** a DSCONT save runs the same full scan as any other trigger. It re-evaluates `DSCONT` on `VISNX` in every cycle folder under `TRT` (up to 50 `CF_Aid_AddnextCycle` calls for type-3 cohorts, 48 for 2D/3A), then runs `CF_Aid_CleanExtraCycle` once. This also applies to DSCONT saves in folders that match no cycle.
- **Effect for the triggering folder:** the same `DoMerge` arguments (cycinfo, cycle number, folder OID, target folder, forms, PRTVR check). The DSCONT value is read with `GetSimpleDP("DSCONT","VISNX",folder,"TRT")` instead of the action data point (see the open assumption below).
- **Extra effects:** every other cycle folder is re-synced with its own current DSCONT value and the current PRTVR result, and out-of-range cycles are cleaned.
- **Triggers other than DSCONT:** no change. They already ran this exact code.

## Open assumption (for the user to confirm)
DSCONT is read from the VISNX master record of the first TRT instance of each cycle folder (`FindByFolderOid` returns the first match). The old DSCONT branch used the action data point. Behavior may differ if a cycle folder has duplicate instances (e.g. re-added after inactivation), or if DSCONT sits on another form, record or parent folder. The code was not changed for this. It needs confirming against the study design.

## Folder matching check (old DSCONT branch vs scan, for the triggering folder)
In the old branch, a folder F matched a cycinfo when: F == DayNumberForC3, or F ends with DayNumber; the cycle number n parsed from F is within CycleRange; and when n == 3, F == DayNumberForC3.
The scan visits `DayNumberForC3` for n == 3 (when it is set) and `"C{n}{DayNumber}"` otherwise. Every cycinfo with a cycle 3 in its range has a non-empty DayNumberForC3. The only cycinfo with an empty DayNumberForC3 (2D/3A #1) starts at cycle 4. So both versions pick the same (cycinfo, n, folder) set. Example: for 1B, C3D1 is rejected by both; C3D8 is accepted by both.

## Per-cohort trace (both versions, via DoMerge)
DoMerge: next folder = `C{n}{FolderSuf}` if FolderSuf == "D15", otherwise `C{n+1}{FolderSuf}`. The forms are `Forms_End` when n == CycleRange[1], otherwise `Forms`. doAdd = DSCONT == "Y". For cohorts 1B/2B/2D/3A and source folder C3D15/C3D8, doAdd also requires PRTVR (SCR/DSCONS) to be V1..V6.

| Cohort | cycinfo (all props) | Source folders | Next folder added | Forms (normal / last) | PRTVR check | Diff |
|---|---|---|---|---|---|---|
| 1 | type3 {3,52}, D1, C3D15, D1, Commen, Commen_End | C3D15, C4D1..C52D1 | C4D1..C53D1 | VISIT,EX,VISNX / VISIT,EX at n=52 | no | none |
| 2A | type1 {3,34}, D1, C3D15, D1, Commen, Commen_End | C3D15, C4D1..C34D1 | C4D1..C35D1 | Commen / Commen_End at n=34 | no | none |
| 1B | type2 {3,26}, D1, C3D8, D1, Commen, Commen_End | C3D8, C4D1..C26D1 | C4D1..C27D1 | Commen / Commen_End at n=26 | yes, at C3D8 | none |
| 2B | type2, D1, C3D8, D1, Commen, Commen_End | C3D8, C4D1..C26D1 | C4D1..C27D1 | Commen / Commen_End | yes, at C3D8 | none |
| 2D #1 | type4 {4,27}, D1, "", D15, D15, D15_End | C4D1..C27D1 | C4D15..C27D15 (same cycle) | VISIT,VISNX,EXBEV / VISIT,EXBEV at n=27 | no (no C3 folder) | none |
| 2D #2 | type2, D15, C3D15, D1, EXBEV, EXBEV | C3D15, C4D15..C26D15 | C4D1..C27D1 | VISIT,EX,EXBEV,VISNX both (Forms_End = formsType_EXBEV, kept as in original) | yes, at C3D15 | none |
| 3A #1 / #2 | same as 2D | same as 2D | same as 2D | same as 2D | same as 2D | none |
| 1C | type3, D1, C3D8, D1, Commen, Commen_End | C3D8, C4D1..C52D1 | C4D1..C53D1 | Commen / Commen_End | no | none |
| 2C | same as 1C | same | same | same | no | none |
| 2E | type3, D1, C3D8, D1, EXBEV, EXBEV_End | C3D8, C4D1..C52D1 | C4D1..C53D1 | EXBEV / EXBEV_End at n=52 | no | none |
| 3B | same as 2E | same | same | same | no | none |
| 2F | type3, D1, C3D8, D1, Commen, Commen_End | C3D8, C4D1..C52D1 | C4D1..C53D1 | Commen / Commen_End | no | none |
| 2I | type2, D15, C3D22, D1, Commen, Commen_End | C3D22, C4D15..C26D15 | C4D1..C27D1 | Commen / Commen_End at n=26 | no | none |
| 2G | type1, D15, C3D15, D1, Commen, Commen_End | C3D15, C4D15..C34D15 | C4D1..C35D1 | Commen / Commen_End at n=34 | no | none |
| 2K | same as 2F | same | same | same | no | none |
| 2J | same as 2I | same | same | same | no | none |

CleanExtraCycle dictionary (`FolderSuf -> CycleRange`): it is built from the same FolderSuf/CycleRange values as before. For 2D/3A it is {D15: {4,27}, D1: {3,26}}, and for the others {D1: range}. No change.

The PRTVR check (DoMerge line `new List<string> { "1B", "2B", "2D", "3A" }` ... `vNum < 7`) is byte-identical to the original.

Unchanged edge case: a null/missing COHORT still makes `cohortsMatch.ContainsKey(null)` throw, the same as the original.
