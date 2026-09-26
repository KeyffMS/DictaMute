# DictaMute — public brand and claims standard

This document is the canonical public identity contract for DictaMute. README, application UI, website, releases, package listings, screenshots and launch copy should follow it.

## Canonical identity

- **Product name:** `DictaMute`
- **Publisher:** `KeyffMS / aiteracja.pl`
- **Repository:** `https://github.com/KeyffMS/DictaMute`
- **Canonical product URL:** `https://aiteracja.pl/dictamute/`
- **Optional convenience redirect:** `https://dictamute.aiteracja.pl/`
- **License:** MIT
- **Primary brand asset:** `assets/brand/dictamute-mark.svg`

Use the exact spelling `DictaMute`. Do not write `Dict Mute`, `Dicta Mute` or `Dictamute` as the product identity.

Until issue #3 records a separate name/logo risk decision, use the plain product name without `®`. Do not imply trademark registration or legal clearance.

## Canonical descriptions

### One-line

> DictaMute is a free, open-source Windows utility that automatically ducks, mutes or pauses selected applications when configured microphone activity exceeds a chosen threshold.

### Extended

> DictaMute is a free, open-source audio utility for Windows 10 and Windows 11. It monitors configured microphone sessions and a user-selected signal threshold, then ducks, mutes or pauses selected playback applications and restores audio after a configurable hold period. It supports profiles, global hotkeys and notification-area operation, and stores its configuration locally.

### Polish short description

> DictaMute to darmowe, open-source'owe narzędzie dla Windows 10/11, które automatycznie ścisza, wycisza lub pauzuje wybrane aplikacje, gdy aktywność skonfigurowanej sesji mikrofonu przekroczy ustawiony próg sygnału.

Translations may be adapted for natural language, but must preserve the same functional boundaries.

## Approved terminology

Prefer:

- `microphone activity`
- `microphone session activity`
- `signal level`
- `signal threshold`
- `duck`, `mute`, `pause`
- `hold time`
- `source application` / group X
- `target application` / group Y
- `profile`
- `notification area` or `system tray`

For Polish copy prefer:

- `aktywność mikrofonu`
- `aktywność sesji mikrofonowej`
- `poziom sygnału`
- `próg sygnału`
- `ściszanie`, `wyciszanie`, `pauza`

## Claims boundaries

DictaMute's threshold logic is **not speech recognition** and does not determine whether a sound is human speech.

Do not market DictaMute as:

- speech recognition;
- AI voice detection;
- a classifier that reacts only to human voice;
- audio recording or transcription software;
- guaranteed to suppress false triggers from noise or keyboard sound;
- compatible with every Windows application, media player or audio configuration;
- zero-latency or real-time in a guaranteed timing sense;
- affiliated with or endorsed by Microsoft, Spotify, TIDAL, Discord, Google, OpenAI or any other third party.

Do not claim that raw microphone audio is or is not captured, persisted or transmitted from marketing copy independently. Those implementation/privacy statements must remain synchronized with the reviewed privacy authority in `PRIVACY.md` once issue #4 is completed.

## Third-party product names

Third-party application names may be used factually to explain tested or typical scenarios, for example to describe that a user may configure a browser, media player or meeting application.

Rules:

- use third-party names only when they add useful factual context;
- do not use third-party logos as decorative brand elements;
- do not imply partnership, certification, sponsorship or endorsement;
- do not describe compatibility as universal unless it has actually been verified;
- prefer generic categories in primary marketing copy and specific names in documentation/testing context.

## Visual identity

The current product mark is stored as SVG in `assets/brand/dictamute-mark.svg`.

The application visual language intentionally aligns with SightAdapt as part of the same publisher family:

- dark neutral surfaces;
- accent `#708BFF`;
- success `#4DD3A9`;
- danger `#FF6F80`;
- Segoe UI on Windows;
- rounded controls and cards.

The products remain separate identities. Do not reuse the SightAdapt name or eye mark as DictaMute branding.

## Public URL rules

The canonical public product root is:

`https://aiteracja.pl/dictamute/`

The optional `https://dictamute.aiteracja.pl/` address should redirect permanently to the canonical path if it is enabled.

Before the canonical site is live, repository and release copy may mention it as the planned canonical product URL but should not present a dead link as the primary user action.

GitHub Releases are planned as the authoritative binary source. Website and directory listings should link to or verify against those releases rather than maintain divergent binaries.

## Publisher wording

Use:

> KeyffMS / aiteracja.pl

Do not invent a corporate entity, legal registration, certification or partnership that is not documented.

## Synchronization checklist

When public product behavior or positioning changes, review:

- `README.md`
- application About/footer/product copy;
- `docs/BRAND.md`;
- `PRIVACY.md`;
- `SECURITY.md`;
- release notes and package metadata;
- canonical website copy;
- package/directory listings;
- social and launch profiles.

A future website or release must consume this contract rather than redefine product identity locally.
