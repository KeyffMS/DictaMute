# DictaMute static website source

This repository retains a reference/fallback static-site source under `site/`. The canonical production website source now lives in `KeyffMS/DictaMute-website`.

This is the current source-of-truth for issue #9. A separate website repository can be introduced later only if the hosting workflow needs it; the content contract remains `docs/WEBSITE-CONTENT.md`.

## Local build

Canonical production-path build:

```powershell
python tools/build-site.py --output site/_build/canonical --base-path /DictaMute/ --canonical-base-path /DictaMute/
python tools/verify-site.py --root site/_build/canonical --base-path /DictaMute/ --canonical-base-path /DictaMute/
```

GitHub Pages-style fallback build:

```powershell
python tools/build-site.py --output site/_build/pages --base-path /DictaMute/ --canonical-base-path /DictaMute/
python tools/verify-site.py --root site/_build/pages --base-path /DictaMute/ --canonical-base-path /DictaMute/
```

The canonical origin is `https://aiteracja.pl` unless explicitly overridden.

## Design constraints

- no runtime framework;
- no remote fonts;
- no cookies or analytics;
- no third-party embeds;
- GitHub Releases remain the binary authority;
- no official-download button until an official release exists;
- all public claims must remain synchronized with brand/privacy/security/release authorities.


## URL policy

- canonical public routes are lowercase;
- directory-style routes use a trailing slash;
- canonical URLs always use `https://aiteracja.pl/DictaMute/`;
- GitHub Pages fallback builds use `/DictaMute/` for internal navigation but keep Aiteracja canonical metadata;
- fallback `robots.txt` disallows indexing so it cannot become a competing indexed origin;
- canonical builds generate `sitemap.xml` and an indexing-enabled `robots.txt`.
