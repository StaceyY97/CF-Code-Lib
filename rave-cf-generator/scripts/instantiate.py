# -*- coding: utf-8 -*-
"""
instantiate.py - produce a new custom-function source from a template with literal replacements.

Usage:
  python instantiate.py --template "<path>.cs" --class-name MY_CF_001 \
      --replace "ADATIM=>XXXTIM" --replace "ADA=>XXX" \
      --replace "30=>60" --out "D:/out/MY_CF_001.cs"

Behaviour:
  * reads the template byte-safe, normalises CR CR LF / CR -> single CRLF
  * strips the auto-generated header banner if present
  * applies --replace old=>new literally, in the given order (never use regex)
  * renames the class to --class-name (must be a legal C# identifier)
  * writes the output as UTF-8 with CRLF line endings
  * runs the same structural checks as validate_cf.py and reports them
Exits 1 when the generated file fails a check.
"""
import os
import re
import sys
import argparse

BANNER = re.compile(r"// ={10,}\r?\n// Custom function : [^\n]*\r?\n// Class name      : [^\n]*\r?\n"
                    r"// Source category : [^\n]*\r?\n(?:// [^\n]*\r?\n)*?// ={10,}\r?\n"
                    r"/// <summary>\r?\n/// Custom function: [^\n]*\r?\n/// </summary>\r?\n")
IDENT = re.compile(r"^[A-Za-z_][A-Za-z0-9_]*$")


def brace_scan(s):
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


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--template", required=True)
    ap.add_argument("--class-name", required=True)
    ap.add_argument("--replace", action="append", default=[],
                    help="literal replacement, format old=>new (repeatable)")
    ap.add_argument("--out", required=True)
    args = ap.parse_args()

    if not IDENT.match(args.class_name):
        print("ERROR: --class-name '%s' is not a legal C# identifier" % args.class_name)
        sys.exit(2)
    if not os.path.exists(args.template):
        print("ERROR: template not found: %s" % args.template)
        sys.exit(2)

    raw = open(args.template, "rb").read()
    if raw.startswith(b"\xef\xbb\xbf"):
        raw = raw[3:]
    text = raw.decode("utf-8", "replace")
    text = text.replace("\r\n", "\n").replace("\r", "\n")
    text = BANNER.sub("", text)

    applied = []
    for spec in args.replace:
        if "=>" not in spec:
            print("ERROR: --replace must use old=>new, got: %s" % spec)
            sys.exit(2)
        old, new = spec.split("=>", 1)
        if not old:
            print("ERROR: empty old part in --replace: %s" % spec)
            sys.exit(2)
        cnt = text.count(old)
        if cnt == 0:
            print("WARNING: '%s' not found in template (check spelling/case)" % old)
        text = text.replace(old, new)
        applied.append((old, new, cnt))

    old_cls = re.search(r"public class (\S+) : CustomFunctionBase", text)
    if old_cls:
        text = text.replace("public class %s : CustomFunctionBase" % old_cls.group(1),
                            "public class %s : CustomFunctionBase" % args.class_name, 1)

    if not text.endswith("\n"):
        text += "\n"
    out = os.path.abspath(args.out)
    os.makedirs(os.path.dirname(out), exist_ok=True)
    with open(out, "w", encoding="utf-8", newline="\r\n") as fh:
        fh.write(text)

    depth, op, cl = brace_scan(text)
    problems = []
    if depth != 0:
        problems.append("brace imbalance (%d open / %d close)" % (op, cl))
    for mk in ["namespace CustomFunctions", "public class _SystemClass",
               "public object Eval(object ThisObject)", ": CustomFunctionBase"]:
        if mk not in text:
            problems.append("missing marker: %s" % mk)
    if BANNER.search(text):
        problems.append("banner still present")

    print("written : %s" % out)
    print("class   : %s" % args.class_name)
    for old, new, cnt in applied:
        print("replace : %r -> %r  (%d occurrence(s))" % (old, new, cnt))
    if problems:
        for pr in problems:
            print("FAIL    : %s" % pr)
        sys.exit(1)
    print("checks  : OK (brace-balanced, framework markers present, no banner)")


if __name__ == "__main__":
    main()
