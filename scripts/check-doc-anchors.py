#!/usr/bin/env python3
#
# Checks the links to a section in the documentation:
#
#   python3 scripts/check-doc-anchors.py
#
# A link [text](path.md#section) in docs/ or the README must name a heading of that file, by the id GitHub
# gives it (and the site gives it too, site/docs.js slug()). A Russian document links the sections of
# Russian ones: the site shows a document in the reader's language, so an English section id would not be
# found in the Russian twin it opens.
import glob
import os
import re
import sys
from urllib.parse import unquote

os.chdir(os.path.join(os.path.dirname(__file__), ".."))


def slug(text):
    text = text.replace("`", "").strip().lower()
    text = re.sub(r"[^\w\s-]", "", text).replace("_", "")
    return re.sub(r"\s", "-", text)


def headings(path):
    ids, code = set(), False
    for line in open(path, encoding="utf-8"):
        if line.startswith("```"):
            code = not code
        match = None if code else re.match(r"#{1,6}\s+(.*)", line)
        if match:
            ids.add(slug(match.group(1)))
    return ids


failed = 0
for source in sorted(glob.glob("docs/**/*.md", recursive=True) + ["README.md"]):
    text = re.sub(r"```.*?```", "", open(source, encoding="utf-8").read(), flags=re.S)
    for match in re.finditer(r"\]\(([^)\s#]*)#([^)\s]+)\)", text):
        path, section = match.group(1), unquote(match.group(2))
        if re.match(r"[a-z]+:", path):
            continue
        target = os.path.normpath(os.path.join(os.path.dirname(source), path)) if path else source
        if not target.endswith(".md") or not os.path.exists(target):
            continue
        if source.endswith(".ru.md") and not target.endswith(".ru.md"):
            print(f"{source}: {match.group(0)} links a section of the English {target}")
            failed = 1
        elif section not in headings(target):
            print(f"{source}: {match.group(0)} names no heading of {target}")
            failed = 1

print("every link to a section names a heading" if not failed else "fix the links above")
sys.exit(failed)
