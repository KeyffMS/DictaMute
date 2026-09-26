#!/usr/bin/env python3
from __future__ import annotations

import argparse
import html
import importlib.util
import json
import shutil
from pathlib import Path
from xml.sax.saxutils import escape as xml_escape

ROOT = Path(__file__).resolve().parents[1]

def load_content():
    path = ROOT / "site" / "content.py"
    spec = importlib.util.spec_from_file_location("dictamute_site_content", path)
    module = importlib.util.module_from_spec(spec)
    assert spec and spec.loader
    spec.loader.exec_module(module)
    return module.PAGES, module.NAV

def norm_path(value: str) -> str:
    value = "/" + value.strip("/") + "/"
    return "/" if value == "//" else value

def link(base: str, route: str) -> str:
    if route == "":
        return base
    return base + route

def page_html(page, nav, base_path, canonical_origin, canonical_base, route):
    canonical = canonical_origin.rstrip("/") + canonical_base + route
    body = page["body"].format(base=base_path)
    nav_html = "".join(
        f'<a href="{html.escape(link(base_path, target))}">{html.escape(label)}</a>'
        for label, target in nav
    )
    title = html.escape(page["title"])
    desc = html.escape(page["description"])
    h1 = html.escape(page["h1"])
    home = html.escape(base_path)
    icon = html.escape(base_path + "assets/dictamute-mark.svg")
    social = html.escape(canonical_origin.rstrip("/") + canonical_base + "assets/social-preview.svg")
    structured = {
        "@context": "https://schema.org",
        "@type": "SoftwareApplication",
        "name": "DictaMute",
        "applicationCategory": "UtilitiesApplication",
        "operatingSystem": "Windows 10, Windows 11",
        "softwareVersion": "0.0.107",
        "isAccessibleForFree": True,
        "license": "https://github.com/KeyffMS/DictaMute/blob/first-attempt/LICENSE",
        "codeRepository": "https://github.com/KeyffMS/DictaMute",
        "url": canonical_origin.rstrip("/") + canonical_base,
        "releaseNotes": "https://github.com/KeyffMS/DictaMute/releases",
        "publisher": {
            "@type": "Organization",
            "name": "KeyffMS / aiteracja.pl",
            "url": "https://aiteracja.pl/"
        }
    }
    structured_json = html.escape(json.dumps(structured, ensure_ascii=False), quote=False)
    return f"""<!doctype html>
<html lang="en">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <title>{title}</title>
  <meta name="description" content="{desc}">
  <link rel="canonical" href="{html.escape(canonical)}">
  <meta property="og:type" content="website">
  <meta property="og:site_name" content="DictaMute">
  <meta property="og:title" content="{title}">
  <meta property="og:description" content="{desc}">
  <meta property="og:url" content="{html.escape(canonical)}">
  <meta property="og:image" content="{social}">
  <meta property="og:locale" content="en_US">
  <meta name="twitter:card" content="summary_large_image">
  <meta name="twitter:title" content="{title}">
  <meta name="twitter:description" content="{desc}">
  <meta name="twitter:image" content="{social}">
  <script type="application/ld+json">{structured_json}</script>
  <link rel="icon" href="{icon}" type="image/svg+xml">
  <link rel="stylesheet" href="{html.escape(base_path + 'assets/styles.css')}">
</head>
<body>
  <a class="skip-link" href="#content">Skip to content</a>
  <header class="site-header">
    <div class="shell header-inner">
      <a class="brand" href="{home}" aria-label="DictaMute home">
        <img src="{icon}" alt="" width="36" height="36">
        <span>DictaMute</span>
      </a>
      <nav class="desktop-nav" aria-label="Primary">{nav_html}<a href="https://github.com/KeyffMS/DictaMute">GitHub</a></nav>
      <details class="mobile-nav">
        <summary>Menu</summary>
        <nav aria-label="Mobile primary">{nav_html}<a href="https://github.com/KeyffMS/DictaMute">GitHub</a></nav>
      </details>
    </div>
  </header>
  <main id="content" class="shell">
    <header class="page-title"><h1>{h1}</h1></header>
    {body}
  </main>
  <footer class="site-footer">
    <div class="shell footer-grid">
      <div><strong>DictaMute</strong><p>KeyffMS / aiteracja.pl · MIT</p></div>
      <nav aria-label="Footer">
        <a href="{html.escape(base_path + 'privacy/')}">Privacy</a>
        <a href="{html.escape(base_path + 'security/')}">Security</a>
        <a href="{html.escape(base_path + 'legal/')}">Legal</a>
        <a href="{html.escape(base_path + 'support/')}">Support</a>
      </nav>
    </div>
  </footer>
</body>
</html>
"""

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--output", default="site/_build/canonical")
    parser.add_argument("--base-path", default="/dictamute/")
    parser.add_argument("--canonical-base-path", default="/dictamute/")
    parser.add_argument("--canonical-origin", default="https://aiteracja.pl")
    args = parser.parse_args()

    output = (ROOT / args.output).resolve() if not Path(args.output).is_absolute() else Path(args.output)
    base = norm_path(args.base_path)
    canonical_base = norm_path(args.canonical_base_path)
    pages, nav = load_content()

    if output.exists():
        shutil.rmtree(output)
    output.mkdir(parents=True)

    assets = output / "assets"
    assets.mkdir()
    shutil.copy2(ROOT / "site" / "assets" / "styles.css", assets / "styles.css")
    shutil.copy2(ROOT / "assets" / "brand" / "dictamute-mark.svg", assets / "dictamute-mark.svg")
    shutil.copy2(ROOT / "site" / "assets" / "social-preview.svg", assets / "social-preview.svg")

    for route, page in pages.items():
        destination = output if route == "" else output / route
        destination.mkdir(parents=True, exist_ok=True)
        (destination / "index.html").write_text(
            page_html(page, nav, base, args.canonical_origin, canonical_base, route),
            encoding="utf-8",
        )

    canonical_urls = [
        args.canonical_origin.rstrip("/") + canonical_base + route
        for route in pages
    ]
    sitemap = ['<?xml version="1.0" encoding="UTF-8"?>', '<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">']
    for url in canonical_urls:
        sitemap.append(f"  <url><loc>{xml_escape(url)}</loc></url>")
    sitemap.append("</urlset>")
    (output / "sitemap.xml").write_text("\n".join(sitemap) + "\n", encoding="utf-8")

    if base == canonical_base:
        robots = f"User-agent: *\nAllow: /\nSitemap: {args.canonical_origin.rstrip('/')}{canonical_base}sitemap.xml\n"
    else:
        robots = "User-agent: *\nDisallow: /\n"
    (output / "robots.txt").write_text(robots, encoding="utf-8")

    (output / "404.html").write_text(
        page_html(
            {
                "title": "Page not found — DictaMute",
                "description": "The requested DictaMute page was not found.",
                "h1": "Page not found.",
                "body": f'<section><p>The requested page does not exist.</p><p><a class="button" href="{html.escape(base)}">Back to DictaMute</a></p></section>',
            },
            nav, base, args.canonical_origin, canonical_base, "",
        ),
        encoding="utf-8",
    )
    print(f"Built {len(pages)} pages in {output}")

if __name__ == "__main__":
    main()
