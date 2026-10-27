# -*- coding: utf-8 -*-
"""
match_template.py - rank CF Code Lib templates against a natural-language description.

Usage:
  python match_template.py "compare time in same visit, open query if sample not 30 min before infusion"
  python match_template.py "<desc>" --top 5 --category 01_OpenQuery
  python match_template.py "<desc>" --show 1        # print the full code of the best match
  python match_template.py --discover               # locate the library and print counts

Library auto-discovery order: $CFLIB_PATH -> cwd and parents -> the default project path.
Exits 2 when the library cannot be found.
"""
import os
import re
import sys
import csv
import argparse

DEFAULT_LIB = "D:/AI/Workbudy/06_Project Code/CF Code Lib"
CATEGORY_DIRS = ["01_OpenQuery", "02_SetVisible", "03_MergeMatrix", "04_AddForm",
                 "05_AddFolder", "06_EnterValue", "07_DynamicSearchList",
                 "08_Combined", "09_Other"]

# --- category detection (English + Chinese description phrases) -------------
CATEGORY_KEYWORDS = {
    "01_OpenQuery": ["open query", "query", "querytext", "firequery", "discrepancy", "质疑", "开质疑"],
    "02_SetVisible": ["set visible", "visible", "visibility", "hide", "show", "display", "设可见", "可见", "显示", "隐藏"],
    "03_MergeMatrix": ["merge matrix", "mergematrix", "unmerge", "matrix", "合并矩阵", "矩阵"],
    "04_AddForm": ["add form", "addform", "create form", "new form", "datapage", "加表单", "新增表单", "建表单"],
    "05_AddFolder": ["add folder", "addfolder", "instance", "nextcycle", "cycle", "加文件夹", "文件夹", "周期"],
    "06_EnterValue": ["enter value", "entervalue", "set value", "pre-fill", "prefill", "default value",
                      "填值", "赋值", "设值", "默认值"],
    "07_DynamicSearchList": ["dynamic search", "searchlist", "search list", "dropdown", "keyvaluecollection",
                             "动态检索", "动态搜索", "下拉"],
    "08_Combined": ["combined", "multiple operations", "组合", "组合功能", "多个操作"],
    "09_Other": ["email", "e-mail", "mail", "notification", "derivation", "derive",
                 "邮件", "通知", "派生", "校验", "验证"],
}

# Chinese -> English keyword expansion (files are English-only, so map first)
CN_TO_EN = {
    "开质疑": ["query"], "质疑": ["query"], "质询": ["query"],
    "设可见": ["visible"], "可见": ["visible"], "显示": ["visible", "show"], "隐藏": ["hide"],
    "合并": ["merge"], "矩阵": ["matrix"],
    "加表单": ["add", "form"], "新增表单": ["add", "form"], "建表单": ["add", "form"],
    "加文件夹": ["folder", "instance"], "文件夹": ["folder"], "周期": ["cycle", "visit"],
    "填值": ["enter", "value"], "赋值": ["enter", "value"], "设值": ["enter", "value"],
    "默认值": ["default", "value"],
    "动态检索": ["searchlist", "dynamic"], "下拉": ["dropdown", "searchlist"],
    "组合": ["combined"], "多个操作": ["combined"],
    "邮件": ["email", "mail"], "通知": ["notification", "email"], "派生": ["derive"],
    "校验": ["check"], "验证": ["validation"],
    "同一访视": ["same", "visit"], "同访视": ["same", "visit"], "访视": ["visit"],
    "时间": ["time"], "日期": ["date"], "比较": ["compare"], "范围": ["range"],
    "重复": ["duplicate"], "必填": ["required"], "缺失": ["missing"], "逻辑": ["logic"],
    "提前": ["prior", "before"], "之前": ["before", "prior"], "之后": ["after"],
}

PHRASES = ["same visit", "prior to", "after", "time range", "date comparison", "at least one",
           "log form", "non-log", "duplicate", "missing", "required", "range check",
           "consent date", "dosing", "infusion", "treatment", "default value", "next cycle",
           "search list", "key value", "out of range", "not equal", "greater than", "less than"]


def read_text(p):
    b = open(p, "rb").read()
    if b.startswith(b"\xef\xbb\xbf"):
        b = b[3:]
    return b.decode("utf-8", "replace")


def discover_lib():
    env = os.environ.get("CFLIB_PATH")
    if env and os.path.isdir(env):
        return env
    d = os.getcwd()
    for _ in range(6):
        cand = os.path.join(d, "CF Code Lib")
        if os.path.isdir(cand) and os.path.isdir(os.path.join(cand, CATEGORY_DIRS[0])):
            return cand
        parent = os.path.dirname(d)
        if parent == d:
            break
        d = parent
    return DEFAULT_LIB if os.path.isdir(DEFAULT_LIB) else None


def detect_categories(query_lower):
    hits = set()
    for cat, kws in CATEGORY_KEYWORDS.items():
        for kw in kws:
            if kw in query_lower:
                hits.add(cat)
    return hits


def expand_query(query):
    """Return (lower query with CN mapped, token list, phrase list)."""
    low = query.lower()
    for cn, ens in CN_TO_EN.items():
        if cn in low:
            low += " " + " ".join(ens)
    toks = set(re.findall(r"[A-Za-z_][A-Za-z0-9_]*", low))
    toks = {t for t in toks if len(t) >= 2}
    phrases = [p for p in PHRASES if p in low]
    return low, toks, phrases


def load_family_map(lib):
    """removed CF stem -> (category, kept representative file) from the curation manifest."""
    mpath = os.path.join(os.path.dirname(lib.rstrip("/\\")), "_removed_from_lib",
                         "template_lib_manifest.csv")
    fam = {}
    if os.path.exists(mpath):
        for r in csv.DictReader(open(mpath, encoding="utf-8-sig")):
            stem = r["removed_file"][:-3] if r["removed_file"].endswith(".cs") else r["removed_file"]
            fam[stem.lower()] = (r["category"], r["kept_representative"])
    return fam


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("query", nargs="*", help="natural-language description of the wanted CF")
    ap.add_argument("--lib", default=None, help="path to CF Code Lib")
    ap.add_argument("--top", type=int, default=5)
    ap.add_argument("--category", default=None, help="restrict to one category folder")
    ap.add_argument("--show", type=int, default=0, metavar="K",
                    help="print the full code of the K-th (1-based) candidate")
    ap.add_argument("--discover", action="store_true")
    args = ap.parse_args()

    lib = args.lib or discover_lib()
    if not lib or not os.path.isdir(lib):
        print("ERROR: CF Code Lib not found. Set --lib or $CFLIB_PATH.")
        sys.exit(2)
    if args.discover:
        print("library:", lib)
        for c in sorted(d for d in os.listdir(lib) if os.path.isdir(os.path.join(lib, d))):
            d = os.path.join(lib, c)
            print("  %-22s %d files" % (c, len([f for f in os.listdir(d) if f.endswith(".cs")])))
        return

    query = " ".join(args.query).strip()
    if not query:
        print("ERROR: empty description.")
        sys.exit(2)
    low, toks, phrases = expand_query(query)
    cats = detect_categories(low)
    if args.category:
        cats = {args.category}

    fam = load_family_map(lib)

    # family-name short-circuit: query mentions a removed family member
    direct = fam.get(query.strip().lower()) or fam.get(re.sub(r"\.cs$", "", query.strip()).lower())

    scored = []
    cat_dirs = sorted(d for d in os.listdir(lib) if os.path.isdir(os.path.join(lib, d)))
    for cat in cat_dirs:
        if cats and cat not in cats:
            continue
        d = os.path.join(lib, cat)
        if not os.path.isdir(d):
            continue
        for fn in sorted(os.listdir(d)):
            if not fn.endswith(".cs"):
                continue
            text = read_text(os.path.join(d, fn))
            tl = text.lower()
            score = 4.0 if cat in cats else 0.0
            matched = []
            for ph in phrases:
                if ph in tl:
                    score += 2.0
                    matched.append(ph)
            for t in toks:
                if t in tl:
                    bonus = 3.0 if (t.isupper() or any(ch.isdigit() for ch in t)) else 1.0
                    score += bonus
                    matched.append(t)
            # class-name mention bonus
            m = re.search(r"public class (\S+) : CustomFunctionBase", text)
            if m and m.group(1).lower() in toks:
                score += 3.0
            if direct and cat == direct[0] and fn == direct[1]:
                score += 100.0
                matched.insert(0, "family representative")
            scored.append((score, cat, fn, matched[:12]))

    scored.sort(key=lambda x: (-x[0], x[1], x[2]))

    print("library : %s" % lib)
    print("query   : %s" % query)
    if cats:
        print("category: %s" % ", ".join(sorted(cats)))
    if direct:
        print("family hit: %s/%s  <- the name you gave was merged into this representative"
              % (direct[0], direct[1]))
    print("")
    if not scored:
        print("no candidates (check --category spelling)")
        return
    for i, (sc, cat, fn, matched) in enumerate(scored[: args.top], 1):
        print("%d. [%5.2f] %s/%s" % (i, sc, cat, fn))
        if matched:
            print("     matched: %s" % ", ".join(matched))
    best = scored[0]
    print("")
    print("best: %s/%s" % (best[1], best[2]))
    print("next step: read that file, adapt OIDs/thresholds/query text, then validate with validate_cf.py")

    if args.show:
        sc, cat, fn, _ = scored[args.show - 1]
        print("\n" + "=" * 78)
        print("### %s/%s  (score %.2f)" % (cat, fn, sc))
        print("=" * 78)
        print(read_text(os.path.join(lib, cat, fn)))


if __name__ == "__main__":
    main()
