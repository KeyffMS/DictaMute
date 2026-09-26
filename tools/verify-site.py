#!/usr/bin/env python3
from __future__ import annotations

import argparse
from html.parser import HTMLParser
from pathlib import Path
from urllib.parse import urlparse

REQUIRED = [
    "", "features/", "download/", "docs/", "how-it-works/", "faq/",
    "releases/", "privacy/", "legal/", "security/", "support/",
]

class Scan(HTMLParser):
    def __init__(self):
        super().__init__()
        self.title = ""
        self.in_title = False
        self.h1 = 0
        self.description = None
        self.canonical = None
        self.links = []
        self.images = []
        self.skip = False
        self.remote_resources = []

    def handle_starttag(self, tag, attrs):
        a = dict(attrs)
        if tag == "title":
            self.in_title = True
        elif tag == "h1":
            self.h1 += 1
        elif tag == "meta" and a.get("name") == "description":
            self.description = a.get("content")
        elif tag == "link":
            if a.get("rel") == "canonical":
                self.canonical = a.get("href")
            if a.get("rel") == "stylesheet" and a.get("href", "").startswith(("http://", "https://")):
                self.remote_resources.append(a.get("href"))
        elif tag == "a":
            href = a.get("href", "")
            self.links.append(href)
            if a.get("class") == "skip-link":
                self.skip = True
        elif tag == "img":
            self.images.append(a)
        elif tag == "script" and a.get("src", "").startswith(("http://", "https://")):
            self.remote_resources.append(a.get("src"))

    def handle_endtag(self, tag):
        if tag == "title":
            self.in_title = False

    def handle_data(self, data):
        if self.in_title:
            self.title += data

def norm(value):
    return "/" + value.strip("/") + "/"

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", required=True)
    parser.add_argument("--base-path", default="/dictamute/")
    parser.add_argument("--canonical-base-path", default="/dictamute/")
    parser.add_argument("--canonical-origin", default="https://aiteracja.pl")
    args = parser.parse_args()

    root = Path(args.root)
    base = norm(args.base_path)
    canonical_base = norm(args.canonical_base_path)
    errors = []

    expected_paths = set()
    for route in REQUIRED:
        path = root / route / "index.html" if route else root / "index.html"
        expected_paths.add(path.resolve())
        if not path.exists():
            errors.append(f"missing page: {route or '/'}")
            continue
        text = path.read_text(encoding="utf-8")
        scan = Scan()
        scan.feed(text)

        if not scan.title.strip():
            errors.append(f"{route}: missing title")
        if not scan.description:
            errors.append(f"{route}: missing meta description")
        if scan.h1 != 1:
            errors.append(f"{route}: expected exactly one H1, got {scan.h1}")
        expected_canonical = args.canonical_origin.rstrip("/") + canonical_base + route
        if scan.canonical != expected_canonical:
            errors.append(f"{route}: canonical mismatch {scan.canonical!r} != {expected_canonical!r}")
        if not scan.skip:
            errors.append(f"{route}: missing skip link")
        for image in scan.images:
            if "alt" not in image:
                errors.append(f"{route}: image without alt")
        if scan.remote_resources:
            errors.append(f"{route}: unsolicited remote resources: {scan.remote_resources}")

        forbidden = ["google-analytics", "googletagmanager", "hotjar", "segment.com", "facebook.com/tr", "doubleclick"]
        lower = text.lower()
        for marker in forbidden:
            if marker in lower:
                errors.append(f"{route}: forbidden tracking marker {marker}")

        for href in scan.links:
            if not href or href.startswith(("#", "mailto:", "https://", "http://")):
                continue
            if not href.startswith(base):
                errors.append(f"{route}: internal link outside base path: {href}")
                continue
            target = href[len(base):].split("#", 1)[0].split("?", 1)[0]
            target_path = root / target
            if target.endswith("/") or target == "":
                target_path = target_path / "index.html"
            if not target_path.exists():
                errors.append(f"{route}: broken internal link: {href}")

    if not (root / "404.html").exists():
        errors.append("missing 404.html")
    if not (root / "assets" / "styles.css").exists():
        errors.append("missing styles.css")
    if not (root / "assets" / "dictamute-mark.svg").exists():
        errors.append("missing brand SVG")

    if errors:
        for error in errors:
            print("FAIL", error)
        raise SystemExit(1)

    print(f"Verified {len(REQUIRED)} DictaMute pages at base {base}")

if __name__ == "__main__":
    main()
