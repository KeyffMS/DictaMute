# DictaMute static website source

The maintained website source lives in this repository under `site/`.

This is the current source-of-truth for issue #9. A separate website repository can be introduced later only if the hosting workflow needs it; the content contract remains `docs/WEBSITE-CONTENT.md`.

## Local build

Canonical production-path build:

```powershell
python tools/build-site.py --output site/_build/canonical --base-path /dictamute/ --canonical-base-path /dictamute/
python tools/verify-site.py --root site/_build/canonical --base-path /dictamute/ --canonical-base-path /dictamute/
```

GitHub Pages-style fallback build:

```powershell
python tools/build-site.py --output site/_build/pages --base-path /DictaMute/ --canonical-base-path /dictamute/
python tools/verify-site.py --root site/_build/pages --base-path /DictaMute/ --canonical-base-path /dictamute/
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
- canonical URLs always use `https://aiteracja.pl/dictamute/`;
- GitHub Pages fallback builds use `/DictaMute/` for internal navigation but keep Aiteracja canonical metadata;
- fallback `robots.txt` disallows indexing so it cannot become a competing indexed origin;
- canonical builds generate `sitemap.xml` and an indexing-enabled `robots.txt`.
