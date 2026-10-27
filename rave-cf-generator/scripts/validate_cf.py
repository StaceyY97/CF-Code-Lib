# -*- coding: utf-8 -*-
"""
validate_cf.py - structural checks for a generated / wrapped Rave custom-function source.

Usage:
  python validate_cf.py <file.cs> [more.cs ...]
  python validate_cf.py --dir <folder>          # every .cs under the folder

Checks:
  1. brace balance with a real C# lexer (comments, strings, verbatim, chars handled)
  2. framework markers present, exactly one 'namespace CustomFunctions'
  3. class declaration 'public class X : CustomFunctionBase' with a legal C# identifier
  4. no CR CR LF line endings, no leftover header banner
  5. non-ASCII character count (informational)
Exit code 0 when all files pass, 1 otherwise.
"""
import os
import re
import sys
import argparse

BANNER = re.compile(r"// ={10,}\r?\n// Custom function : ")
MARKERS = ["namespace CustomFunctions", "#region System Class - Do Not Modify",
           "public class _SystemClass", "public object Eval(object ThisObject)",
           ": CustomFunctionBase"]
IDENT = re.compile(r"^[A-Za-z_][A-Za-z0-9_]*$")


def read_bytes(p):
    with open(p, "rb") as fh:
        return fh.read()


def brace_scan(s):
    """(final_depth, open, close) counting braces in code only."""
    i, n, depth, op, cl = 0, len(s), 0, 0, 0
    while i < n:
        c = s[i]
        if c == "/" and i + 1 < n and s[i + 1] == "/":
            j = s.find("\n", i)
            i = n if j < 0 else j + 1
            continue
        if c == "/" and i + 1 < n and s[i + 1] == "*":
            j = s.find("*/", i + 2)
            i = n if j < 0 else j + 2
            continue
        if c == "@" and i + 1 < n and s[i + 1] == '"':
            i += 2
            while i < n:
                if s[i] == '"':
                    if i + 1 < n and s[i + 1] == '"':
                        i += 2
                        continue
                    i += 1
                    break
                i += 1
            continue
        if c == "$" and i + 1 < n and s[i + 1] == '"':
            i += 2
            while i < n:
                if s[i] == "\\":
                    i += 2
                    continue
                if s[i] == '"':
                    i += 1
                    break
                if s[i] == "{":
                    if i + 1 < n and s[i + 1] == "{":
                        i += 2
                        continue
                    op += 1
                    depth += 1
                    i += 1
                    continue
                if s[i] == "}":
                    if i + 1 < n and s[i + 1] == "}":
                        i += 2
                        continue
                    cl += 1
                    depth -= 1
                    i += 1
                    continue
                i += 1
            continue
        if c == '"':
            i += 1
            while i < n:
                if s[i] == "\\":
                    i += 2
                    continue
                if s[i] == '"':
                    i += 1
                    break
                i += 1
            continue
        if c == "'":
            i += 1
            while i < n:
                if s[i] == "\\":
                    i += 2
                    continue
                if s[i] == "'":
                    i += 1
                    break
                i += 1
            continue
        if c == "{":
            op += 1
            depth += 1
        elif c == "}":
            cl += 1
            depth -= 1
        i += 1
    return depth, op, cl


def check_file(p):
    raw = read_bytes(p)
    problems, notes = [], []

    if b"\xef\xbb\xbf" in raw[3:]:
        problems.append("BOM in the middle of the file")
    if b"\r\r\n" in raw:
        problems.append("CR CR LF line endings (doubled lines when read as text)")

    text = raw.decode("utf-8-sig" if raw.startswith(b"\xef\xbb\xbf") else "utf-8", "replace")
    norm = text.replace("\r\n", "\n").replace("\r", "\n")

    depth, op, cl = brace_scan(text)
    if depth != 0:
        problems.append("brace imbalance: %d open / %d close (net %+d)" % (op, cl, depth))

    for mk in MARKERS:
        if mk not in norm:
            problems.append("missing marker: %s" % mk)
    if norm.count("namespace CustomFunctions") != 1:
        problems.append("'namespace CustomFunctions' count != 1")

    m = re.search(r"public class (\S+) : CustomFunctionBase", norm)
    if not m:
        problems.append("no 'public class X : CustomFunctionBase' declaration")
    elif not IDENT.match(m.group(1)):
        problems.append("class name '%s' is not a legal C# identifier" % m.group(1))

    if BANNER.search(text):
        problems.append("header banner still present")

    nonascii = sum(1 for ch in norm if ord(ch) > 127)
    if nonascii:
        notes.append("%d non-ASCII characters (fine inside strings/comments only)" % nonascii)

    stem = os.path.splitext(os.path.basename(p))[0]
    if m and stem != m.group(1):
        notes.append("file stem '%s' != class name '%s'" % (stem, m.group(1)))

    return problems, notes


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("files", nargs="*")
    ap.add_argument("--dir", default=None)
    args = ap.parse_args()

    targets = list(args.files)
    if args.dir:
        for dp, _, fs in os.walk(args.dir):
            targets += [os.path.join(dp, f) for f in fs if f.endswith(".cs")]
    if not targets:
        print("nothing to validate")
        sys.exit(2)

    failed = 0
    for p in targets:
        problems, notes = check_file(p)
        status = "OK  " if not problems else "FAIL"
        print("%s  %s" % (status, p))
        for pr in problems:
            print("      - %s" % pr)
            failed += 1
        for nt in notes:
            print("      * %s" % nt)
    print("")
    print("%d file(s), %d problem(s)" % (len(targets), failed))
    sys.exit(1 if failed else 0)


if __name__ == "__main__":
    main()
