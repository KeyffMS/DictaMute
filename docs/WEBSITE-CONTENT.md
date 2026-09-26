# DictaMute website content contract

This document is the content authority for the public DictaMute website tracked by issues #8–#13.

It consumes:

- `docs/BRAND.md` for product identity and claims;
- `PRIVACY.md` for privacy/data-flow statements;
- `SECURITY.md` for security boundaries and reporting;
- `docs/RELEASING.md` and GitHub Releases for release/download facts;
- `LICENSE` and `THIRD-PARTY-NOTICES.md` for legal content.

Website implementation must not redefine those authorities locally.

## 1. Canonical information architecture

Canonical public root:

`https://aiteracja.pl/dictamute/`

Required routes:

| Page | Canonical path | Primary purpose |
|---|---|---|
| Home | `/dictamute/` | Explain the problem, product and primary actions |
| Features | `/dictamute/features/` | Explain X/Y rules, actions, profiles and controls |
| Download | `/dictamute/download/` | Verified release download and integrity guidance |
| Documentation | `/dictamute/docs/` | Getting started and configuration hub |
| How it works | `/dictamute/how-it-works/` | Explain Windows audio/session behavior without overselling |
| FAQ | `/dictamute/faq/` | Answer common product, privacy and compatibility questions |
| Releases | `/dictamute/releases/` | Public release history derived from GitHub Releases |
| Privacy | `/dictamute/privacy/` | Public rendering of the approved privacy authority |
| Legal | `/dictamute/legal/` | MIT, notices and third-party/no-affiliation statements |
| Security | `/dictamute/security/` | Security boundaries and reporting |
| Support | `/dictamute/support/` | Usage help, bugs, features and security routing |

Optional convenience host:

`https://dictamute.aiteracja.pl/`

If enabled, it must permanently redirect to the matching canonical `aiteracja.pl/dictamute/` path and must not host a separately indexed copy.

## 2. Global navigation and footer

### Primary navigation

Recommended desktop order:

1. Features
2. Download
3. Docs
4. How it works
5. FAQ
6. Releases
7. GitHub

On mobile, use one accessible disclosure/menu control. All links remain ordinary crawlable links.

### Footer

Required content:

- `DictaMute`
- `KeyffMS / aiteracja.pl`
- MIT License
- GitHub repository
- Privacy
- Security
- Legal
- Support
- release/download link when a public release exists

Do not add social links until issue #17 confirms the relevant profile is official and maintained.

## 3. Canonical product copy

### One-line

> DictaMute is a free, open-source Windows utility that automatically ducks, mutes or pauses selected applications when configured microphone activity exceeds a chosen threshold.

### Extended

> DictaMute is a free, open-source audio utility for Windows 10 and Windows 11. It monitors configured microphone sessions and a user-selected signal threshold, then ducks, mutes or pauses selected playback applications and restores audio after a configurable hold period. It supports profiles, global hotkeys and notification-area operation, and stores its configuration locally.

### Plain-language constraint

Do not replace `microphone activity` / `signal threshold` with:

- AI voice detection;
- speech recognition;
- human-voice detection;
- intelligent listening;
- automatic transcription.

The product reacts to session activity and an audio meter threshold. Keyboard noise, room noise or other captured sound can also cross that threshold.

# 4. Home — `/dictamute/`

## Metadata

**Title:** `DictaMute — automatic audio ducking, mute and pause for Windows`

**Meta description:** `DictaMute automatically ducks, mutes or pauses selected Windows applications when configured microphone activity crosses a signal threshold. Free and open source.`

**H1:** `Make background audio get out of the way when your microphone is active.`

**Primary CTA after a public release:** `Download DictaMute`

Before the first public release exists, use `View the project on GitHub` and do not link temporary Actions artifacts.

**Secondary CTA:** `View source on GitHub`

## Hero copy

> DictaMute watches configured microphone sessions and a signal threshold. When the threshold is crossed, it can duck, mute or pause selected playback applications, then restore them after a configurable hold period.

Supporting line:

> Built for Windows 10 and Windows 11. Free, open source and designed to run from the notification area.

Visible boundary note:

> DictaMute uses microphone activity and a signal meter. It does not perform speech recognition.

## X → Y explanation

Heading: `Choose what triggers the rule and what reacts.`

### X — Sources

> Source applications are the microphone-using processes allowed to trigger the selected profile.

### Y — Targets

> Target applications are the playback processes DictaMute may duck, mute or pause while the rule is active.

Diagram copy:

`Source X active + signal above threshold → Duck / Mute / Pause target Y → hold time → restore`

## Three action cards

### Duck

> Reduce the target application's volume to a configured level while the rule is active, then restore the previous level.

### Mute

> Mute the target audio session while the rule is active, then restore its prior mute state when possible.

### Pause

> Ask a uniquely matched Windows media session to pause. If DictaMute cannot identify a safe unique session, it falls back to Duck instead of sending an unaddressed global media command.

## Profiles section

Heading: `Save different setups for different situations.`

> Profiles keep source applications, target actions, threshold, hold time, duck level and global behavior together. A profile can be enabled or disabled independently from the application's global automation switch.

Illustrative names: Dictation, Meetings, Recording, Focus.

## Privacy section

Heading: `Local settings. No raw microphone recording stream.`

> The current implementation reads Windows audio-session state and signal-level metering. It does not create a raw microphone recording stream, upload audio, send behavioral telemetry or perform an automatic network update check. Settings and diagnostic logs are stored locally.

Links: `Read the privacy notice`, `Review security boundaries`.

## Open-source section

Heading: `Open source and inspectable.`

> DictaMute is distributed under the MIT License. Source code, issues, release history and implementation documentation are public on GitHub.

## Limitations section

Visible bullets:

- the threshold is not a human-voice classifier;
- noise or keyboard sound can trigger the threshold;
- media-session matching varies between applications;
- protected, elevated, exclusive or unusual audio paths may behave differently;
- Pause requires a compatible uniquely matched media session and can fall back to Duck;
- broad compatibility and performance testing remain limited during alpha development.

# 5. Features — `/dictamute/features/`

## Metadata

**Title:** `DictaMute features — sources, targets, profiles and audio actions`

**Meta description:** `Explore DictaMute source/target rules, Duck/Mute/Pause actions, profiles, signal threshold, hold time, hotkeys and notification-area controls.`

**H1:** `Audio automation built around explicit source and target applications.`

**Primary CTA:** `Read the quick start`

## Required sections

### Sources X

Explain configured microphone-using applications, active-session requirement, threshold behavior and optional any-microphone mode.

### Targets Y

Explain explicit targets, source exclusion, per-target action and optional everything-except-X mode.

### Threshold

> Threshold is the signal level required to open the gate. Lower values react more easily; higher values ignore more quiet input. It is not speech classification.

### Hold time

> Hold time keeps the rule active briefly after the signal falls below threshold so short pauses do not constantly restore and reapply audio.

### Duck level and fade

Explain target Duck volume, the rule that already-quieter sessions are not raised, and the fade duration. Do not promise guaranteed timing.

### Profiles

Explain create, rename, select, enable/disable and delete. The selected profile and global automation must both be enabled for automation to operate.

### Global hotkeys

Current defaults:

- `Ctrl+Alt+X` — add foreground application to sources X;
- `Ctrl+Alt+Y` — add foreground application to targets Y;
- `Ctrl+Alt+M` — toggle global automation.

State that registration can fail if another program owns the combination.

### Notification area

Explain open configuration, global enable/disable, profile selection, exit and the stateful tray icon.

### What DictaMute does not do

- no speech recognition/transcription;
- no guarantee that input is human voice;
- no target-process injection;
- no kernel driver or Windows service;
- no DRM/privilege bypass;
- no universal application compatibility guarantee;
- no current audio/settings/telemetry upload.

# 6. Download — `/dictamute/download/`

## Metadata

**Title:** `Download DictaMute for Windows`

**Meta description:** `Download the official DictaMute Windows x64 release from GitHub Releases, verify its SHA-256 checksum and review system requirements and known limitations.`

**H1:** `Download the current DictaMute release.`

**Primary CTA:** `Download DictaMute for Windows x64`

The button must resolve to the official GitHub Release asset.

Before a release exists show:

> No official binary release has been published yet. Build from source or follow the repository for the first alpha.

## Release card

Derive or verify against GitHub Releases:

- version/tag/date/channel;
- Windows support and architecture;
- official archive filename/size;
- SHA-256;
- release notes;
- source tag/commit;
- dependency inventory;
- privacy/security/license/notices.

## System requirements

- Windows 10 build 17763 or later, or Windows 11;
- x64 for the initial official binary package.

## Installation

1. Download the official ZIP.
2. Optionally verify SHA-256.
3. Extract the complete ZIP.
4. Start `DictaMute.exe`.
5. Configure X sources, Y targets and threshold.
6. Use Save and apply.

## Checksum verification

```powershell
Get-FileHash .\DictaMute-v0.0.108-alpha-win-x64.zip -Algorithm SHA256
```

Compare with `SHA256SUMS.txt` on the same GitHub Release.

## Windows reputation warning

> New or unsigned open-source Windows binaries can trigger Microsoft Defender SmartScreen or another reputation warning. Download only from project-controlled links and verify the release/tag/checksum before choosing whether to run the file. Do not disable system protection globally.

## Removal

Exit DictaMute, remove the program directory, and optionally delete `%LOCALAPPDATA%\DictaMute\` to remove saved profiles/logs.

# 7. Documentation — `/dictamute/docs/`

## Metadata

**Title:** `DictaMute documentation — setup, profiles and troubleshooting`

**Meta description:** `Set up DictaMute sources and targets, configure threshold and hold time, use Duck/Mute/Pause, manage profiles and troubleshoot Windows audio sessions.`

**H1:** `Set up DictaMute and understand each control.`

**Primary CTA:** `Start with the three-step quick start`

## Three-step quick start

1. **Add source X:** start the microphone-using application and select it or use the source hotkey.
2. **Add target Y:** start playback, select the target and choose Duck, Mute or Pause.
3. **Set threshold and apply:** configure threshold/hold time, enable the profile and global automation, then save/apply.

## Documentation groups

- Getting started
- Profiles
- Duck / Mute / Pause
- Threshold / hold / duck / fade
- Global modes
- Hotkeys
- Tray controls
- Troubleshooting
- Developer documentation

Troubleshooting must cover source/target not appearing, zero meter, Pause fallback, elevated targets, settings/log location, corrupted settings recovery and tray behavior.

# 8. How it works — `/dictamute/how-it-works/`

## Metadata

**Title:** `How DictaMute works with Windows audio sessions`

**Meta description:** `Learn how DictaMute reads Windows audio-session activity and signal level, resolves X/Y rules, applies Duck/Mute/Pause and restores audio state.`

**H1:** `Windows audio-session automation, not speech recognition.`

**Primary CTA:** `Read the technical details on GitHub`

## Required explanation

1. Enumerate active capture/render endpoints and audio sessions.
2. Associate sessions with process/application identity where Windows permits.
3. Read capture endpoint signal metering and active capture-session state.
4. Resolve active X sources from the selected profile.
5. Compare peak meter with threshold/hold gate.
6. Resolve Y target rules.
7. Apply Duck/Mute state tracking or Pause.
8. Pause requires a unique Windows media-session match and otherwise falls back to Duck.
9. Restore controlled state when the gate closes, configuration changes, automation stops or normal shutdown occurs.

Boundary:

> DictaMute does not need to inject code into another application to change its Windows audio session. It also does not create a raw microphone recording stream in the current implementation.

Required accessible diagram text:

`Capture endpoint/session → active source X → meter threshold/gate → target Y resolver → Duck/Mute/Pause → restore`

# 9. FAQ — `/dictamute/faq/`

## Metadata

**Title:** `DictaMute FAQ`

**Meta description:** `Answers about DictaMute microphone handling, speech detection, application compatibility, privacy, profiles, tray operation and building from source.`

**H1:** `Frequently asked questions about DictaMute.`

**Primary CTA:** `Read the documentation`

## Required answers

### Is DictaMute free?

Yes. DictaMute is free and open source under the MIT License.

### Does DictaMute record my microphone?

The current implementation does not create a raw microphone capture stream or persist microphone audio samples. It reads Windows audio-session state and signal-level metering. See the privacy notice for exact current behavior.

### Does DictaMute recognize when I am speaking?

No. It does not perform speech recognition or human-voice classification. Noise or keyboard sound can also cross the configured signal threshold.

### Does it work with every player?

No universal compatibility guarantee is made. Duck/Mute depend on Windows audio sessions; Pause also depends on a compatible uniquely matched media session.

### What happens if Pause cannot identify the player?

DictaMute falls back to Duck instead of broadcasting an unaddressed global Play/Pause command.

### Profile enabled vs global automation?

Both matter. A disabled selected profile does not run automation even when the global switch is enabled.

### Where are settings stored?

`%LOCALAPPDATA%\DictaMute\settings.json`

### Does DictaMute send telemetry?

The current implementation has no behavioral telemetry, analytics, remote crash reporting or automatic network update check.

### Why might Windows warn about the download?

New or unsigned binaries can have limited reputation. Use official project-controlled links and verify the checksum.

### How do I remove it?

Exit, delete the program directory and optionally delete `%LOCALAPPDATA%\DictaMute\`.

### Can I build it myself?

Yes, with the .NET 8 SDK on supported Windows.

### Where do I report a problem?

Use GitHub Issues for non-sensitive reports and follow `SECURITY.md` for sensitive security details.

# 10. Releases — `/dictamute/releases/`

## Metadata

**Title:** `DictaMute release history`

**Meta description:** `Public DictaMute release history with versions, dates, official Windows downloads, checksums, source tags and release status.`

**H1:** `Public, verifiable DictaMute releases.`

**Primary CTA:** `View releases on GitHub`

GitHub Releases remain authoritative.

Show version/tag/date/channel/summary/platform, official ZIP, checksum, source tag, dependency inventory and withdrawal notice where applicable.

Before first publication show the planned alpha as not published without exposing temporary Actions artifacts.

# 11. Privacy — `/dictamute/privacy/`

## Metadata

**Title:** `DictaMute privacy`

**Meta description:** `How DictaMute processes Windows audio-session metadata and signal level locally, what it stores, and how to remove configuration and logs.`

**H1:** `DictaMute privacy and local data.`

**Primary CTA:** `Read the source privacy notice`

Authority: `PRIVACY.md`.

Required summary while current behavior remains true:

> DictaMute operates locally. The current implementation does not create a raw microphone recording stream, upload audio, send behavioral telemetry or perform an automatic network update check.

Cover data read/stored, signal metering, executable paths, settings/log deletion, reporting redaction and support-data retention.

# 12. Legal — `/dictamute/legal/`

## Metadata

**Title:** `DictaMute license and legal notices`

**Meta description:** `DictaMute MIT License, third-party notices, dependency information and third-party trademark/no-affiliation statement.`

**H1:** `License and legal notices.`

**Primary CTA:** `View source license`

Required:

- MIT License;
- NAudio notices;
- .NET/runtime notices from official release;
- dependency inventory;
- repository and publisher;
- no-affiliation wording.

Required text:

> Third-party product names are used only for factual interoperability examples. DictaMute is not affiliated with, sponsored by, certified by or endorsed by Microsoft, Spotify, TIDAL, Discord, Google, OpenAI or other referenced third parties unless an explicit documented relationship exists.

> DictaMute does not attempt to circumvent DRM, protected-media restrictions, application access controls or Windows security boundaries.

# 13. Security — `/dictamute/security/`

## Metadata

**Title:** `DictaMute security and vulnerability reporting`

**Meta description:** `DictaMute security boundaries, current-user privilege model, audio restoration limitations and vulnerability-reporting guidance.`

**H1:** `Security boundaries and reporting.`

**Primary CTA:** `Read SECURITY.md`

Authority: `SECURITY.md`.

Cover current-user model, no driver/service/injection, no bypass, restoration limitations, elevated/protected application limitations, local-file boundary and vulnerability reporting.

# 14. Support — `/dictamute/support/`

## Metadata

**Title:** `DictaMute support and contact`

**Meta description:** `Get DictaMute usage help, report reproducible bugs, request features or find the private security-reporting route.`

**H1:** `Get help with DictaMute.`

**Primary CTA:** `Open the documentation`

Routes:

- usage questions → docs/FAQ;
- reproducible defects → GitHub Issues;
- feature requests → GitHub Issues;
- security → `SECURITY.md`;
- publisher → KeyffMS / aiteracja.pl.

Bug report checklist:

- DictaMute version;
- Windows edition/build;
- source/target applications;
- action mode;
- device context where relevant;
- threshold/hold time;
- reproduction steps;
- restoration outcome;
- sanitized log excerpt.

Privacy warning:

> Do not publish screenshots containing private information or unredacted paths, usernames, organization names, client/project names, tokens or confidential application content.

# 15. Visual content inventory

Issue #14 owns production. Required authentic visuals:

1. main configuration window;
2. configured X and Y lists;
3. profile active/disabled state;
4. Duck/Mute/Pause selection;
5. threshold/hold/duck controls;
6. tray icon/menu;
7. active automation status;
8. simplified X→Y workflow diagram;
9. optional short demonstration.

Rules:

- record DictaMute version/build;
- remove private data;
- avoid unnecessary third-party logos;
- no implied endorsement;
- meaningful alt text;
- diagrams require text alternatives;
- no stock UI presented as the real application.

# 16. Limitations that must remain visible

- threshold is not speech/voice recognition;
- other microphone sound can trigger the threshold;
- application/session compatibility varies;
- Pause depends on media-session support and unique matching;
- elevated/protected/exclusive audio configurations can limit behavior;
- alpha testing breadth is limited until release evidence grows.

Do not advertise guaranteed CPU/RAM limits, latency, compatibility percentages or universal support without measured release evidence.

# 17. SEO handoff

Every page requires:

- unique title;
- unique meta description;
- one H1;
- canonical URL under `https://aiteracja.pl/dictamute/`;
- descriptive internal links;
- crawlable textual content.

Issue #12 owns Open Graph/social metadata and `SoftwareApplication` structured data.

Issue #13 owns sitemap, robots, Google/Bing verification and duplicate-origin handling.

Do not add fabricated ratings, review counts, awards or endorsements.

# 18. Accessibility

- semantic heading hierarchy;
- keyboard-reachable navigation;
- visible focus;
- skip navigation;
- sufficient contrast;
- no information only by color;
- descriptive links;
- zoom/reflow;
- reduced-motion handling;
- useful alt text;
- no autoplaying media;
- caption/text alternative for meaningful video/audio demos.

# 19. Analytics/privacy constraint

Initial website must not use cookies, ad networks, behavioral analytics, session replay, remote tracking fonts or unsolicited third-party embeds.

Privacy-respecting aggregate measurement requires a future explicit privacy/data-flow review.

# 20. Implementation handoff

Issue #9 implements this contract rather than redefining claims.

Issue #10 deploys it to the canonical Aiteracja path.

Issue #11 replaces pre-release placeholders with verified GitHub Release metadata.

Issues #12/#13 add discoverability without changing product meaning.

Issue #14 produces authentic visuals under section 15.

When behavior changes, update the underlying brand/privacy/security/release authority first, then this contract and the website.
