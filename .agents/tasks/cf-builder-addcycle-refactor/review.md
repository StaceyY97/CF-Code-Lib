# Single add-next-cycle flow and explicit cycinfo values in CF_Builder (pass 2)

The coder's latest iteration changed no code. It only rewrote equivalence.md and coder-notes.md. `CF_Builder.cs` still removes the `fieldOID == "DSCONT"` split and runs the old `else` path (full cycle scan, then `CF_Aid_CleanExtraCycle`) for every trigger. The `cycinfo` change meets its checks: no initializers remain, and all 17 literals set all six properties to the values they used to get from the defaults. The trace now states the DSCONT behavior change openly and admits the flow does not derive the cycle from the current folder. It justifies the change with a user approval ("Option A", reply 'A') that the coder says the orchestrator relayed. That approval is not in the authoritative user-message record, and this step's brief still says to preserve behavior and to treat any behavior change as blocking.

Watch for: a DSCONT save still re-syncs every cycle folder and runs CleanExtraCycle, where before it touched only the saved folder (**confirmed**). The "Option A" approval that would make this acceptable is not in the user-message record or in this step's brief (**confirmed**).

**Verdict**: NEEDS_CHANGES

## High-level view

Triggers other than DSCONT run exactly the old `else` code, so their behavior is unchanged. For a DSCONT save, the old code made one `CF_Aid_AddnextCycle` call for the saved folder, using the action data point, and skipped the clean. The new code makes one call per existing cycle folder (up to 50 for type-3 cohorts, 48 for 2D/3A) and then runs `CF_Aid_CleanExtraCycle`. A DSCONT save in a folder that matched no cycle used to do nothing. It now runs the whole scan.

I checked every row of the trace's per-cohort table against both versions: source folders, next-folder names, normal and end-of-range form sets, PRTVR gating, call counts, and the CleanExtraCycle dictionary. No row is wrong. The folder-matching argument also holds: for the triggering folder, the old DSCONT branch and the new scan pick the same (cycinfo, cycle number, folder) set.

The only blocker is authorization. The brief calls this "a logic restructuring, not a behavior change", and none of the user's messages records a choice of Option A. If the user did reply 'A', that reply needs to reach the authoritative record or the review brief, and the change can then be approved as it stands. Otherwise the flow must derive the cycle from the current folder, so a DSCONT save keeps touching only its own folder.

<details>
<summary>Issues (2)</summary>

1. **DSCONT save behavior changed** — A DSCONT save now calls `CF_Aid_AddnextCycle` for every cycle folder (adding, re-activating or inactivating forms) and runs `CF_Aid_CleanExtraCycle`, where it used to touch only the saved folder. A save in a non-matching folder used to do nothing and now does all of this. Either record the user's approval where the review can verify it, or restructure so the cycle comes from the current folder.
2. **Approval not in the authoritative record** — coder-notes.md and equivalence.md say the orchestrator relayed the user's reply 'A'. No such reply is in the user-message record, and this step's brief still forbids behavior changes. Have the orchestrator confirm and record the approval, or update the brief to accept the change.

</details>

<details>
<summary>Details</summary>

## What a DSCONT save does now

The old DSCONT branch matched only the current folder and passed the action data point:

```csharp
if (cycle.DayNumberForC3 != folderOID && !folderOID.EndsWith(cycle.DayNumber)) continue;
// parse n from folderOID; n in CycleRange; n == 3 requires folderOID == DayNumberForC3
if (doContinue) DoMerge(dp, cycle, cNum, folderOID);   // no CleanExtraCycle
```

Every trigger now runs:

```csharp
for each cycinfo, for n in CycleRange:
    checkFolder = (n == 3 && DayNumberForC3 != "") ? DayNumberForC3 : "C{n}{DayNumber}";
    checkDP = GetSimpleDP("DSCONT", "VISNX", checkFolder, "TRT");
    if (checkDP != null) DoMerge(checkDP, ...);
CF_Aid_CleanExtraCycle(curSub, {FolderSuf -> CycleRange}, formsType_Commen);
```

`CF_Aid_AddnextCycle` writes in both directions. With `doAdd = true` it adds or re-activates the next folder's forms, and with `doAdd = false` it inactivates untouched forms there. Each extra call can change data. A DSCONT save in C10D1 now re-applies the current DSCONT values and the current PRTVR result to every other cycle, which reconciles drift such as a PRTVR edit made after C3D8 was saved. The old code did none of this on a DSCONT save (**confirmed**). Non-DSCONT saves already did all of it, so the end state matches what the next save of any other trigger field would produce. That may make the change acceptable to the user, but it is still a change.

The trace's call counts and no-match examples are correct. Type-3 cohorts scan n = 3..52 (50 folders), and 2D/3A scan 4..27 plus 3..26 (24 + 24). For cohort 1, C3D1 ends with D1 but fails the n == 3 rule. For 1B, C4D15 does not end with D1.

## Data point read for the triggering folder

The scan reads DSCONT with `GetSimpleDP("DSCONT","VISNX",folder,"TRT")`, which takes the VISNX master record of the first instance with that folder OID. The old branch used whichever data point fired. The two code paths differ (**confirmed**). Whether that matters depends on whether a cycle folder can have duplicate instances or DSCONT can sit elsewhere (**possible**, study-design dependent). The trace now lists this as an open assumption instead of claiming the two are the same data point. It falls under the same approval as the DSCONT change, so the Issues list does not count it separately.

</details>

<details>
<summary>File map</summary>

- `CF_Builder.cs`: removed the DSCONT/else split and the `currentIns`/`folderOID`/`fieldOID` locals. The full scan plus `CF_Aid_CleanExtraCycle` runs for every trigger. Removed the `cycinfo` initializers, and every `cohortsMatch` literal now sets all six properties. Framework usings, namespace and system region unchanged.
- Baseline: `.agents/tasks/cf-builder-addcycle-refactor/CF_Builder.original.cs`. Compare the two files directly. There is no git history.

</details>
