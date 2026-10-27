# CF Code Lib - category map and matching hints

The library root contains one folder per operation. Files are complete, standalone Rave
custom-function sources (standard framework, no header banner). User descriptions may be in
Chinese; the mapping below links the phrases to the right folder.

## Operation fingerprints (detect on comment-stripped source, never on comments)

| Folder | Operation | Deciding API | Typical description phrases (EN / CN) |
| --- | --- | --- | --- |
| `01_OpenQuery` | Open a query on a data point | `CustomFunction.PerformQueryAction(...)` | open query, raise query, discrepancy check / 开质疑, 质疑 |
| `02_SetVisible` | Show / hide fields or forms | `dp.IsVisible = ...` / `.IsVisibleRaw` | set visible, hide field, show form / 设可见, 隐藏, 显示 |
| `03_MergeMatrix` | Merge / unmerge log matrices | `subject.MergeMatrix()` / `UnMergeMatrix()` | merge matrix, switch matrix / 合并矩阵, 矩阵切换 |
| `04_AddForm` | Create a form in a visit | `new DataPage(...)` + `DataPages.Add(...)` | add form, create form / 加表单, 新增表单 |
| `05_AddFolder` | Create a visit / folder | `new Instance(...)` or a call to `CF_Aid_AddnextCycle` | add folder, next cycle, new visit / 加文件夹, 下一周期 |
| `06_EnterValue` | Write a value into a field | `dp.Enter(value, ...)` | enter value, set value, default value, pre-fill / 填值, 赋值, 默认值 |
| `07_DynamicSearchList` | Build a dynamic search list | `DynamicSearchParams` + `KeyValueCollection` | dynamic search list, dropdown options / 动态检索, 下拉选项 |
| `08_Combined` | Two or more of the above in one CF | combination of the fingerprints above | combined function, add form and enter value / 组合功能 |
| `09_Other` | Pure validation, email, derivation - no structural action | none of the above | send email, derivation, duplicate check / 邮件, 派生, 查重 |

A `Lib_newAPI/` folder (if present) holds templates written with the newer Rave object
interfaces; search it like any other folder - the matcher does this automatically.

## Fetch scope - the single most important semantic choice

Look at the 3rd argument of `FetchAllDataPointsForOIDPath(fieldOID, formOID, <scope>, subject)`:

- `InstCurrent.Folder.OID` - only the **current visit**. Use when the rule is intra-visit
  (e.g. "all fields should be in the same visit").
- `null` - **subject-wide** (all visits). Use for cross-visit rules (e.g. compare against the
  earliest consent date, or a value recorded in the screening visit).

Never silently change this scope when adapting a template; confirm it with the user.

## Log-form vs non-log-form handling

Templates that loop over `FetchAllDataPointsForOIDPath(...)` results and check
`if (dpAction.Field.OID == qTfield) dpts.Add(dpAction);` deliberately handle **both** log and
non-log forms: when the action field *is* the query field, the loop processes just that data
point. Preserve this pattern when the user's rule must work for log forms.

## Well-known reference pattern

`01_OpenQuery/TimeCheck_SameFolder.cs` (class `ADA_008`): compare a sample date-time against a
dosing date-time inside a tolerance window, all fields in the same visit, works for log and
non-log forms. Helpers: `isValuedDate`, `isValuedTime`, `ConvertToDate(date, time)`,
`GetDataPointByOID`; window: `dt1.AddMinutes(30) > dt2`.

## Family representatives

Each folder keeps one representative per pattern family. A user may still name a retired CF
(e.g. `CM_017`); `match_template.py` resolves such names through
`_removed_from_lib/template_lib_manifest.csv` (`removed_file -> kept_representative`) and points
at the surviving representative.
