---
name: rave-cf-generator
description: "Generate ready-to-use Medidata Rave custom-function (.cs) code from the curated template library 'CF Code Lib'. This skill should be used when the user describes a custom function they want (open query, set visible, merge matrix, add form, add folder, enter value, dynamic search list, combined function, email/derivation/validation), gives a CF name plus field/form OIDs or thresholds and wants working code returned, asks to find a similar function in the CF template library, or names a retired CF that must be reproduced from its pattern representative. Queries may be in Chinese (开质疑, 设可见, 合并矩阵, 加表单, 加文件夹, 填值, 动态检索, 组合功能). Not for editing the CF_for_* study source folders. IMPORTANT: Always write generated CF code directly into CF_Builder.cs file for user review."
agent_created: true
---

# Rave CF Generator

Match a user's description to a template in the curated `CF Code Lib` library, specialise it,
validate it, and return complete usable code.

## Code Generation Rules (MANDATORY)

1. **Always write generated CF code directly into CF_Builder.cs file** for user review.
2. **CF_Builder.cs editable region**:
   - Only edit content **after** `public object Eval(object ThisObject)` `{` and **before** the matching closing braces `}` `}` `}` at the end of the class
   - **DO NOT modify** the standard framework: using statements, `_SystemClass` region, class declaration, or the final closing braces
   - Lambda expressions and helper methods should be placed either:
     - **Inside the Eval method** (for simple helpers), OR
     - **After the Eval method's closing brace `}`** but before the class's closing brace `}` (when there are multiple helper methods)
   - **Comment conventions**:
     - CF summary/description goes inside Eval method as a HEADER INFORMATION comment block at the very beginning
     - Helper methods should NOT have XML documentation comments (`/// <summary>`)
     - Use inline comments (`//`) for brief explanations within code
3. **Use interface types instead of concrete classes**:
   - Use `ISubject` instead of `Subject`
   - Use `IDataPoint` instead of `DataPoint`
   - Use `IReadOnlyList<IDataPoint>` instead of `DataPoints`
   - Use `IInstance` instead of `Instance`
   - Use `IDataPage` instead of `DataPage`
   - Use `IActionFunctionParams` instead of `ActionFunctionParams`
   - Use `IRecord` instead of `Record`
   - Use `IMatrix` instead of `Matrix`
   - Interface definitions are available in `Medidata.Interfaces.dll` located in `Program Files/Medidata Solutions/CustomFunction Development Utility/bin/`

4. **Use Basical_Func.cs helper methods** (located in `Lib_newAPI/Basical_Func.cs`):
   - `GetDP(field_Oid, form_Oid, folder_Oid, ISubject)` - Get non-log DataPoint
   - `subject.GetDataPointsByOidPath(oid_field, oid_form, folder_oid)` - Get log DataPoints (can also apply to non-log)
   - `DoCheck(IDataPoint dp, bool IsDate, bool checkEmpty = true)` - Validate DataPoint (date or non-date)
   - `IsValidDp(IDataPoint dp, bool checkEmpty)` - Check if DataPoint is valid
   - `IsValidDate(IDataPoint dp)` - Check if DataPoint has valid date value

5. **Use Rave static context** instead of casting ThisObject:
   - Use `Rave.Context.ActionDataPoint` to get the action DataPoint
   - Use `Rave.Marking.PerformQueryAction()` instead of `CustomFunction.PerformQueryAction()`
   - Use `Rave.Architect.FetchMatrixByOid()` for matrix operations

6. **Use interface-based data retrieval**:
   - Use `subject.GetDataPointsByOidPath(fieldOID, formOID, folderOID)` instead of `CustomFunction.FetchAllDataPointsForOIDPath()`
   - Use `subject.Instances.FindByFolderOid(folderOID)` for instance lookup
   - Use `instance.DataPages.FindByFormOid(formOID)` for data page lookup
   - Use `record.AllDataPoints.FindByFieldOid(fieldOID)` for data point lookup within a record

7. **DataPoint property access on interfaces**:
   - Use `dp.StandardValue` (property) instead of `dp.StandardValue()` (method)
   - Use `dp.IsVisibleRaw` for visibility check
   - Use `dp.Record.RecordPosition` for log form position

## Workflow

1. **Locate the library.** Run `scripts/match_template.py --discover`. It resolves the library
   from `$CFLIB_PATH`, the current workspace (or its parents), or the default path
   `D:/AI/Workbudy/06_Project Code/CF Code Lib`. If it cannot be found, ask the user for the path.

2. **Read the operation map.** Load `references/categories.md` to map the description to the
   right category folder and to understand the fetch-scope and log/non-log semantics before
   choosing a template.

3. **Match templates.** Run
   `scripts/match_template.py "<description>" --top 5`
   (optionally `--category 01_OpenQuery` to restrict, `--show 1` to print the best match's code).
   The matcher handles Chinese descriptions, boosts OID-like tokens, resolves retired family
   names through `_removed_from_lib/template_lib_manifest.csv`, and prints matched keywords so
   the ranking can be judged.

4. **Understand the template.** Read the top 1-2 candidate files. Identify its local helpers,
   the fetch scope (`InstCurrent.Folder.OID` = same visit, `null` = subject-wide), thresholds,
   and query text. Read `references/customization-guide.md` for the adaptation rules.

5. **Collect the missing parameters.** Field/form OIDs, thresholds, query wording and the
   fetch scope must come from the user - never invent OIDs or silently switch between
   same-visit and subject-wide. Ask when unclear, showing the template's current values.

6. **Generate the code.** For pure parameter substitutions use
   `scripts/instantiate.py --template <file> --class-name <CF_NAME> --replace "old=>new" ... --out <path>`
   (handles CR-CR-LF normalisation, banner stripping, class renaming, CRLF output, validation).
   For structural changes edit a copy by hand, preserving the standard framework
   (19 usings, `_SystemClass` region, `public class <Name> : CustomFunctionBase`).
   
   **Apply Code Generation Rules above** - use interfaces, Basical_Func helpers, and Rave static context.

7. **Validate.** Run `scripts/validate_cf.py <output.cs>`. Fix any FAIL before returning.
   Never count braces with regex; the validator uses a C# lexer.

8. **Deliver.** Write the complete code directly into `CF_Builder.cs` file, and state:
   which template was used, every replacement applied, the fetch scope chosen, and any
   assumption still needing confirmation.

## Conventions (mandatory)

- Class name = CF name with `( ) . - =` and space replaced by `_`; file name = plain CF name.
- **No header banner** (`// ===` blocks, naming `/// <summary>` lines) in generated files.
- **English only** in file names, comments and query text, even when the user writes Chinese.
- Emit new code to a separate output location; never write into `CF Code Lib/` or `CF_for_*/`
  while generating user code.

## When nothing matches

If the best score is low or no template implements the requested rule, say so plainly, list the
closest candidates with their matched keywords, and ask the user whether to adapt the closest
one or write a new CF from scratch following the framework and the operation fingerprints in
`references/categories.md`. Do not silently return a poor match.

## Resources

- `scripts/match_template.py` - rank library templates against a description; `--discover`,
  `--top N`, `--category`, `--show K`.
- `scripts/instantiate.py` - template + literal `old=>new` replacements -> validated output file.
- `scripts/validate_cf.py` - brace-balance (C# lexer), framework markers, class-name legality,
  banner and line-ending checks; accepts files or `--dir`.
- `references/categories.md` - category map, API fingerprints, fetch-scope semantics,
  log/non-log pattern, family-representative resolution.
- `references/customization-guide.md` - standard framework, what typically changes, naming
  rules, encoding mechanics, hard boundaries.
- `Lib_newAPI/Basical_Func.cs` - basic helper methods for DataPoint retrieval and validation.
