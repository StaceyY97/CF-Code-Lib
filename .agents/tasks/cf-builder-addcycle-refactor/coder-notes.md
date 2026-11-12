# Coder notes (current state of CF_Builder.cs)

## Approval
User explicitly chose Option A in the orchestrator chat (reply: 'A'), after being told that DSCONT saves will re-sync all cycles and run CF_Aid_CleanExtraCycle. Relayed by the orchestrator.

Step (a) of the user's request, "取cycle", is implemented as iterating all cycles in CycleRange, which is what Option A means.

This iteration made no code changes. Only equivalence.md and these notes were updated to address review issues 1-4.

## New flow (Option A)
For every trigger field, with no branch on the field OID or the folder:
1. Read COHORT (SCR/ENROLL) and look up the `cycinfo[]` in `cohortsMatch`.
2. Get the cycle: for each cycinfo and each cycle n in CycleRange, the folder is `DayNumberForC3` for n == 3, otherwise `C{n}{DayNumber}`.
3. Read DSCONT on VISNX in that folder under TRT. If it exists, call `DoMerge`. DoMerge works out the next visit name, decides doAdd (DSCONT == "Y" plus the unchanged PRTVR <V7 check), and calls `CF_Aid_AddnextCycle`.
4. Run `CF_Aid_CleanExtraCycle` once at the end.

The `cycinfo` class has no property initializers. All 17 instances set all 6 properties, using the values the old defaults gave.

## Checks run
Command:
```
python "d:\AI\Workbudy\06_Project Code\CF Code Lib\rave-cf-generator\scripts\validate_cf.py" "d:\AI\Workbudy\06_Project Code\CF Code Lib\CF_Builder.cs"
```
Output:
```
OK    d:\AI\Workbudy\06_Project Code\CF Code Lib\CF_Builder.cs
      * file stem 'CF_Builder' != class name 'CustomFunction1'
1 file(s), 0 problem(s)
```
(The file-stem note is informational. The class name was left unchanged on purpose.)

Inspection (PowerShell Select-String on CF_Builder.cs):
- Pattern `Field\.OID|fieldOID ==|== "DSCONT"`: 0 matches.
- Pattern `\{ get; set; \} =` (property initializers): 0 matches.
- Lines matching `new cycinfo \{`: 17. Lines missing any of CycleRange, `DayNumber =`, DayNumberForC3, FolderSuf, `Forms =`, Forms_End: 0.

No compile was run (there is no project/csproj for this snippet).

## Open questions
- Open assumption for the user: DSCONT is read from the VISNX master record of the first TRT instance of each cycle folder. Behavior may differ from the old DSCONT branch (which used the action data point) if a folder has duplicate instances or DSCONT sits elsewhere. The code was not changed for this.
- Left as in the original: 2D/3A #2 uses `Forms_End = formsType_EXBEV` (not `_End`), and a null COHORT still throws in `ContainsKey`.
