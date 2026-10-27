# Adapting a template into usable code

## Standard file framework (keep it verbatim)

Every generated file must be a complete, compilable unit:

```
<19 top-level using directives - copy them from the template, do not retype>
namespace CustomFunctions
{
using Medidata.Core.Objects.DCFs;
#region System Class - Do Not Modify
... public class _SystemClass { ... static void Main() ... }   // reproduce verbatim
#endregion
public class <ClassName> : CustomFunctionBase
{
public object Eval(object ThisObject)
{
<adapted body>
}
}
}
```

The templates are already in this shape. When editing by hand, change only what the task
requires and leave the `_SystemClass` region untouched.

## What typically changes when specialising a template

| Change | Where it lives |
| --- | --- |
| Field OIDs (query field, date field, compare field) | local strings like `qTfield`, `qDfield`, `sTfield`, `sDfield` |
| Form / matrix OIDs | `qForm`, `SForm`, or literals in `FindByFormOID(...)` / `MergeMatrix(...)` |
| Tolerance window / thresholds | `AddMinutes(30)`, `AddDays(...)`, numeric range constants |
| Query text | `string querytext = "..."` - keep it investigator-readable English |
| Target visit / cycle OIDs | literals in `new Instance(...)`, `AddnextCycle` calls |
| Default / derived values | the argument of `dp.Enter(...)` and the computing expression |

Never invent OIDs. If the user has not supplied a field or form OID, ask for it.

## Naming rules (mandatory)

1. **Class name** = the CF name, with characters illegal in a C# identifier - `( ) . - =` and
   space - replaced by `_` (e.g. `Contra_AETOXGR(PRCDTH)` -> `Contra_AETOXGR_PRCDTH`).
2. **File name** = the plain CF name, no project code prefix, no combined-operation prefix.
3. **No header banner.** Do not add `// ===` blocks or a `/// <summary>` naming banner above the
   class; they were deliberately removed from the whole library.
4. **English only** in every deliverable: file names, comments, query text, query folder names.
5. Keep the template's existing inner comment style (e.g. `/* Programmer Name ... */`) if the
   user wants attribution; otherwise leave the body comment-free.

## Mechanics

- Source `.cs` files historically use `CR CR LF` line endings. Read bytes, normalise with
  `\r+\n|\r -> \n`, emit single `CRLF`; UTF-8, BOM only when non-ASCII characters are written.
- Prefer `scripts/instantiate.py` for pure parameter substitutions (it handles encoding, banner
  stripping, class renaming and validation in one step). Edit by hand only when the change is
  structural (new loop, new helper, different fetch scope).
- Validate every result with `scripts/validate_cf.py` before returning it. It checks brace
  balance with a real C# lexer, framework markers, class-name legality, and stale banners.
- Do not count braces with regex - string literals and comments cause false imbalances.

## Hard boundaries

- Never modify anything under `CF Code Lib/` when generating user code - emit to a separate
  output file or return the code inline.
- Never edit the `CF_for_*` study folders; they are the source of truth.
- Do not merge two templates by hand into a half-structured file; if no single template fits,
  say so, show the closest candidates, and ask whether to adapt one or compose a new CF.
- Do not guess Rave API behaviour beyond what the templates demonstrate.
