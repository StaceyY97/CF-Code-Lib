# CF Code Lib

A curated **template library** of Medidata Rave custom functions (`.cs`), organised by the
operation each one performs. Every file is a **complete, standalone custom-function source
file**, and each category keeps **one representative per reusable pattern family**.

- **280** files across 9 categories.
- Sources: `CF_for_CRN_12`, `CF_for_CTMX-2051`, `CF_for_OP_3136` (originals untouched).
- Filenames carry **no project code** — just the function name.
- The auto-generated header banner has been **removed** from every file.
- Near-duplicate functions have been collapsed, both **across** projects (earlier pass) and
  **within** each category (this pass).

## Categories

| Folder | Files | Operation detected |
| --- | --- | --- |
| `01_OpenQuery` | 70 | `CustomFunction.PerformQueryAction(...)` |
| `02_SetVisible` | 24 | `dp.IsVisible = ...` / `.IsVisibleRaw` |
| `03_MergeMatrix` | 7 | `subject.MergeMatrix()` / `UnMergeMatrix()` |
| `04_AddForm` | 15 | `new DataPage(...)` + `DataPages.Add(...)` |
| `05_AddFolder` | 2 | `new Instance(...)` / `CF_Aid_AddnextCycle` |
| `06_EnterValue` | 34 | `dp.Enter(value, ...)` |
| `07_DynamicSearchList` | 9 | `DynamicSearchParams` + `KeyValueCollection` |
| `08_Combined` | 27 | 2 or more of the above in one CF |
| `09_Other` | 92 | pure validation / email / derivation |

Total: **280**.

## File layout

Each `.cs` file is a complete unit built from the standard Rave framework:

```
19 top-level using directives
namespace CustomFunctions
{
    using Medidata.Core.Objects.DCFs;
    #region System Class - Do Not Modify
    ... public class _SystemClass { ... Main() ... }   (reproduced verbatim)
    #endregion

    public class <CF name> : CustomFunctionBase
    {
        public object Eval(object ThisObject)
        {
            <original function body, inserted verbatim>
        }
    }
}
```

The original library files were **method-body fragments** (no usings / namespace / class).
All fragments were verified brace-balanced before wrapping, so the wrapper supplies exactly
the 3 opening braces (namespace / class / `Eval`) and the 3 matching closing braces; a fragment
that ends with helper methods or nested classes still lands at the correct scope level.

### Class naming

The class name is the CF name. Rave CF names may contain characters that are **not legal in a
C# identifier** — `(`, `)`, `.`, `-`, `=`, space. Those files have the offending characters
replaced by `_` (e.g. `Contra_AETOXGR(PRCDTH)` -> `Contra_AETOXGR_PRCDTH`). The full mapping is in
`_class_name_map.csv`.

### Line endings

The source `.cs` files were encoded with `CR CR LF` line endings (a text-mode round-trip artifact
— 497 of 500 originals). Read as text that doubles every line, so this library re-emits each
fragment with a single `CRLF` per logical line. No code was changed — only whitespace between
lines. Files are UTF-8 (with BOM where the source had one).

## New API Templates (Lib_newAPI)

The `Lib_newAPI/` folder contains templates that use **interface-based programming** instead of
concrete classes, following modern Rave development best practices.

### Key features:

1. **Interface types** instead of concrete classes:
   - `ISubject` instead of `Subject`
   - `IDataPoint` instead of `DataPoint`
   - `IReadOnlyList<IDataPoint>` instead of `DataPoints`
   - `IActionFunctionParams` instead of `ActionFunctionParams`
   - `IInstance`, `IDataPage`, `IRecord`, `IMatrix` for their respective concrete types

2. **Rave static context**:
   - `Rave.Context.ActionDataPoint` to get the action DataPoint
   - `Rave.Marking.PerformQueryAction()` for query operations
   - `Rave.Architect.FetchMatrixByOid()` for matrix operations

3. **Interface-based data retrieval**:
   - `subject.GetDataPointsByOidPath(fieldOID, formOID, folderOID)` for log/non-log data
   - `subject.Instances.FindByFolderOid(folderOID)` for instance lookup
   - `instance.DataPages.FindByFormOid(formOID)` for data page lookup
   - `record.AllDataPoints.FindByFieldOid(fieldOID)` for data point lookup

4. **Property access**:
   - `dp.StandardValue` (property) instead of `dp.StandardValue()` (method)
   - `dp.IsVisibleRaw` for visibility check
   - `dp.Record.RecordPosition` for log form position

### Basic Helper Methods

`Lib_newAPI/Basical_Func.cs` provides reusable helper methods:

```csharp
// Get non-log DataPoint
IDataPoint GetDP(string field_Oid, string form_Oid, string folder_Oid, ISubject subj) 
    => subj.Instances.FindByFolderOid(folder_Oid)?.DataPages.FindByFormOid(form_Oid)?.MasterRecord.AllDataPoints.FindByFieldOid(field_Oid);

// Get log DataPoints (can also apply to non-log)
IReadOnlyList<IDataPoint> dps = subject.GetDataPointsByOidPath(oid_field, oid_form, folder_oid);

// Validation helpers
bool DoCheck(IDataPoint dp, bool IsDate, bool checkEmpty = true) 
    => IsDate ? IsValidDp(dp, checkEmpty) && IsValidDate(dp) : IsValidDp(dp, checkEmpty);
bool IsValidDp(IDataPoint dp, bool checkEmpty) 
    => dp != null && dp.Active && dp.IsVisibleRaw && (checkEmpty ? dp.Data != string.Empty : true);
bool IsValidDate(IDataPoint dp) 
    => dp != null && dp.Active && dp.IsVisibleRaw && (dp.StandardValue is DateTime || dp.StandardValue is TimeSpan);
```

## CF Builder Workflow

`CF_Builder.cs` is the **working file** for generating and testing new custom functions.

### Usage:
1. Describe the CF requirement to the AI assistant
2. AI generates CF code directly into `CF_Builder.cs` (replacing any existing CF)
3. Review and test the generated code
4. Move validated CF to the appropriate category folder

### Code Generation Rules:
- **Delete and replace**: New CF replaces all existing CFs in `CF_Builder.cs`
- **Interface-based**: Use `ISubject`, `IDataPoint`, etc. instead of concrete classes
- **Header in Eval**: CF description goes inside `Eval()` as `HEADER INFORMATION` comment block
- **No XML docs on helpers**: Helper methods use inline comments, not `/// <summary>`
- **Helper placement**: Simple helpers inside `Eval()`, multiple helpers after `Eval()` before class close

## Rave CF Generator Skill

The `rave-cf-generator/` folder contains the AI skill for generating custom functions.

### Key files:
- `SKILL.md` - Skill definition and code generation rules
- `references/categories.md` - Category map and matching hints
- `references/customization-guide.md` - Standard framework and adaptation rules
- `scripts/match_template.py` - Template matching utility
- `scripts/instantiate.py` - Template instantiation utility
- `scripts/validate_cf.py` - CF validation utility

## Changes applied, in order

1. **Classified** all custom functions into the 9 categories above (500 files).
2. **Deduplicated** exact logic duplicates; named each file after the function only — no project prefix.
3. **Collapsed cross-project similar functions** (>=85% similar after comment/whitespace
   normalisation) to a single representative. 13 files removed (484 left).
4. **Wrapped every fragment in the standard framework and re-saved in place** (484 files).
5. **Removed the `TimeCheck_SameFolder` family from `01_OpenQuery`** — 25 files implementing the same
   rule ("compare a sample/assessment date-time against a dosing date-time inside a tolerance window").
   Kept: `01_OpenQuery/TimeCheck_SameFolder.cs`. 459 left.
6. **Curated into a template library** (this pass):
   - Clustered files **within each category** by token similarity on the comment-stripped body;
     every cluster at **>= 0.90** keeps **one representative** (the medoid — the member most similar
     to all the others in its cluster). **178 files removed**, leaving **280**.
   - **Stripped the auto-generated header banner** (the `// ===` block plus the `/// <summary>` line)
     from all 280 surviving files.
   - Verified afterwards: 0 surviving pairs remain >= 0.90 similar inside a category; all 280 files
     are brace-balanced (checked with a C# lexer) and structurally identical to their pre-pass
     versions apart from the deleted banner.

## Removed files

All removed files are **archived, never hard-deleted**; the originals also still exist in the
`CF_for_*` folders.

| Pass | Count | Archive | Manifest |
| --- | --- | --- | --- |
| Step 3 — cross-project duplicates | 13 | `../_removed_from_lib/` | `removed_manifest.csv` |
| Step 5 — `TimeCheck_SameFolder` family | 25 | `../_removed_from_lib/` | `timecheck_family_manifest.csv` |
| Step 6 — within-category pattern families | 178 | `../_removed_from_lib/` | `template_lib_manifest.csv` |

`template_lib_manifest.csv` columns: `category, kept_representative, removed_file, similarity, note`.
There are **67 clusters** and **67 kept representatives** among the 280 files.

## Related artefacts

| Path | Content |
| --- | --- |
| `_class_name_map.csv` | CF name -> C# class name, flag for sanitised names |
| `CF_Builder.cs` | Working file for CF generation and testing |
| `Lib_newAPI/` | Interface-based templates and helper methods |
| `rave-cf-generator/` | AI skill for generating custom functions |
| `../_backup_CFCodeLib_pre_template/` | full snapshot taken immediately before step 6 (458 files) |
| `../_backup_CFCodeLib_fragments/` | pre-wrap fragments (same 9-folder layout) |
| `../_removed_from_lib/` | all archived removals + the three manifests |
| `../_tools/build_template_lib.py` | step 6 tool (idempotent, re-runnable) |
| `../_tools/wrap_into_framework.py` | the wrapper (re-runnable, refuses to double-wrap) |
| `../_tools/rebuild_fragments.py` | rebuilds fragments from the pristine sources |
| `../_tools/fragment_source_map.csv` | library file -> source file mapping |

## Classification rule

Detection runs on the **comment-stripped source**, so text inside header comments
(e.g. "Edit Checks: ...") is never mistaken for an operation.
