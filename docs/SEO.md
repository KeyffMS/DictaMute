# DictaMute SEO and indexing policy

Canonical origin: `https://aiteracja.pl/DictaMute/`

## URL policy

- public route segments are lowercase;
- directory pages use trailing slashes;
- every public HTML page declares the equivalent Aiteracja canonical URL;
- GitHub Pages/fallback internal navigation may use `/DictaMute/`, but canonical metadata still points to Aiteracja;
- fallback builds publish `robots.txt` with `Disallow: /` to avoid duplicate indexing;
- canonical builds allow indexing and reference the canonical sitemap.

## Metadata

Every generated page includes:

- unique title;
- unique meta description;
- canonical link;
- Open Graph title/description/URL/image/locale;
- Twitter-compatible summary-large-image metadata;
- `SoftwareApplication` JSON-LD with factual product identity.

The structured data intentionally contains no ratings, review counts, awards, medical categories or universal compatibility claims.

## Social preview

Source asset:

`site/assets/social-preview.svg`

It uses the approved DictaMute mark and project palette. A raster PNG export may be added under issue #14 for platforms that do not render SVG social cards reliably; metadata must then be updated to the maintained raster asset.

## Sitemap and robots

The site generator creates:

- `sitemap.xml` with all 11 canonical pages;
- canonical `robots.txt` allowing intended crawling and referencing the sitemap;
- fallback `robots.txt` disallowing indexing.

## External verification still required after #10

Repository CI can validate structure but cannot establish live indexing.

After production deployment:

1. verify every sitemap URL returns HTTPS 200;
2. verify canonical/link-preview rendering against the live origin;
3. verify Google Search Console ownership for the canonical property;
4. submit/verify the canonical sitemap;
5. verify Bing Webmaster Tools ownership;
6. evaluate/enable IndexNow if it matches the maintained hosting model;
7. record verification dates and non-secret ownership evidence in issue #13.

Do not commit Search Console/Bing verification secrets or private account recovery information.
