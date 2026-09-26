# DictaMute Security Policy

This document describes the current security boundaries and vulnerability-reporting process for DictaMute.

## Supported versions

DictaMute is currently in active development before its first maintained public GitHub Release.

Until a release policy is published under issue #7:

- the latest `first-attempt` development build is the only development line expected to receive fixes;
- old local builds and temporary GitHub Actions artifacts should not be treated as supported distributions;
- do not rely on a development build for security-critical or safety-critical audio workflows.

Once public releases exist, this section must be replaced with an explicit supported-version table.

## Reporting a vulnerability

Do **not** publish exploit details, secrets, private logs or sensitive screenshots in a public GitHub issue.

Preferred route:

1. If GitHub displays **Report a vulnerability** / private vulnerability reporting for this repository, use that private route.
2. If a private route is not available, create a minimal public issue stating only that you need a private security contact route. Do not include reproduction details that would expose users or enable abuse.
3. The publisher can then establish an appropriate private channel before sensitive material is shared.

For ordinary non-sensitive defects and feature requests, use the public issue tracker:

`https://github.com/KeyffMS/DictaMute/issues`

No guaranteed response-time or security SLA is currently offered.

## Security boundaries

### Current-user desktop application

DictaMute is a normal Windows desktop application running in the current user's context.

The current implementation does not install or require:

- a kernel driver;
- a Windows service;
- DLL/process injection;
- code modification in target applications;
- target-process memory writes;
- elevation or UAC bypass;
- DRM/access-control bypass.

DictaMute uses documented Windows/Core Audio and Global System Media Transport Controls APIs to observe and control audio sessions available to the current process.

### Process identity

To match configured applications, DictaMute may query:

- process name;
- executable path where permitted;
- AppUserModelID where available.

Windows can deny access to process metadata. DictaMute treats inaccessible/exited processes as unavailable rather than trying to bypass the restriction.

### Audio control

DictaMute can:

- read audio-session state;
- read endpoint signal-level metering;
- change per-session volume/mute state;
- send pause/play commands to a uniquely matched Windows media session.

The application does not create a raw microphone capture stream in the current implementation. See `PRIVACY.md` for the data-flow details.

### Restoration and failure behavior

DictaMute tracks audio state it changes and attempts to restore that state when:

- the signal gate closes;
- automation/profile configuration changes;
- automation is disabled;
- the application shuts down normally;
- the automation worker stops after a handled error.

The application also avoids overwriting certain manual volume/unmute changes made while it is active.

These controls are best-effort, not a transactional OS guarantee. A forced process kill, power loss, driver failure, disappearing audio session or target-application failure can prevent restoration. Users must be able to restore volume/playback manually through Windows or the target application.

### Elevated/protected applications

Compatibility can differ when the target application:

- runs elevated while DictaMute does not;
- exposes incomplete process metadata;
- uses protected/DRM-controlled media;
- does not expose a compatible Windows media session;
- uses an exclusive or unusual audio path;
- changes or replaces audio/media sessions rapidly.

DictaMute must not attempt to bypass integrity levels, DRM or Windows security boundaries to improve compatibility.

### Media-session matching

The `Pause` action is sent only when DictaMute identifies exactly one matching media session. If matching is ambiguous or unsupported, DictaMute falls back to Duck rather than broadcasting an unaddressed global media key.

A user can explicitly assign a media-session identifier to a target profile. Such identifiers are local configuration data and can change when applications change implementation.

### Global hotkeys

DictaMute registers global hotkeys in the user's desktop session. Registration can fail if another application already owns the shortcut. Hotkeys do not grant additional privileges.

## Network boundary

The current DictaMute source does not intentionally use networking APIs, telemetry, remote analytics, crash upload or automatic network update checks.

Adding any network feature requires:

- privacy review and update of `PRIVACY.md`;
- threat/security review;
- clear user-facing purpose and controls;
- review of third-party services/dependencies;
- updated website/release claims.

## Local files

DictaMute stores settings and logs below:

`%LOCALAPPDATA%\DictaMute\`

These files are protected only by the normal Windows user/account/file-system boundary. They are not separately encrypted by DictaMute.

Do not store secrets in profile names or other DictaMute settings.

See `PRIVACY.md` for exact file names and deletion guidance.

## Third-party software and affiliation

DictaMute can be configured to interact with many third-party applications through Windows audio/media APIs.

References to Microsoft, Windows, Spotify, TIDAL, Discord, Google, OpenAI or other products are factual interoperability examples only.

DictaMute is not affiliated with, sponsored by, certified by or endorsed by those third parties unless an explicit documented relationship is established.

DictaMute does not attempt to circumvent DRM, protected-media restrictions, application access controls or platform security features.

## Dependencies and redistribution

Current maintained application dependencies include:

- .NET 8 / Windows Desktop runtime components in self-contained distributions;
- NAudio.Wasapi / NAudio.Core 2.2.1.

Third-party notices are maintained in `THIRD-PARTY-NOTICES.md`.

Official packages must include:

- `LICENSE`;
- `PRIVACY.md`;
- `SECURITY.md`;
- `THIRD-PARTY-NOTICES.md`;
- any additional runtime/dependency license or notice files required by the final publish output.

Dependency/license inventory must be reviewed again before each public release.

## Out of scope for current security claims

The project does not claim to protect against:

- a malicious local administrator;
- malware already running as the user;
- kernel/driver compromise;
- malicious audio drivers;
- a malicious target application intentionally interfering with Windows audio/session APIs;
- security failures inside Windows or third-party applications.

DictaMute should not be presented as a security boundary between local applications.

## Release security gate

Before a public release:

- CI/build/tests must pass for the exact source commit;
- release artifacts must come from an approved build, not an arbitrary temporary file;
- legal/privacy/security documents must be present in the package;
- dependency notices must be reviewed;
- release checksums must be published;
- a Windows smoke test must cover enable/disable, Duck/Mute/Pause, profile switching, tray exit and audio restoration;
- known security-relevant limitations must be included in release notes.

Issue #7 tracks the maintained release process.
