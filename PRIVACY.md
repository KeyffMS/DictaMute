# DictaMute Privacy Notice

**Effective date:** 2026-09-26  
**Publisher / data controller for voluntarily submitted support material:** KeyffMS / aiteracja.pl

This notice describes the current DictaMute implementation on the `first-attempt` development line. If product behavior changes, especially if networking, telemetry, crash reporting, upload or automatic update features are introduced, this notice must be reviewed before release.

## Summary

DictaMute is designed to operate locally on Windows.

The current application:

- reads Windows audio endpoint/session metadata and signal-level metering values;
- reads process identity needed to match configured source and target applications;
- reads and changes per-session playback volume/mute state;
- may send pause/play commands to an explicitly matched Windows media session;
- stores its configuration and diagnostic log locally;
- does **not** instantiate an audio-capture stream, read microphone sample buffers or save microphone recordings;
- does **not** send telemetry, analytics, crash reports or application data over the network;
- does **not** perform an automatic network update check.

The microphone trigger is based on session activity plus an audio **meter value**. It is not speech recognition and does not determine whether a sound is human speech.

## Runtime data read by DictaMute

### Audio devices and sessions

DictaMute uses Windows Core Audio / NAudio APIs to read:

- active capture and render endpoints;
- audio-session state;
- process ID associated with an audio session;
- capture-device friendly name for display/diagnostics;
- endpoint mute state and master volume scalar;
- `AudioMeterInformation.MasterPeakValue`;
- playback session volume/mute state.

The current source code does not create `WasapiCapture`, `WaveIn` or another raw-audio capture stream and does not read PCM/sample buffers.

The meter value is used transiently by the automation engine and UI. DictaMute does not intentionally persist a history of microphone level readings.

### Process and application identity

To match configured applications, DictaMute may read:

- process name;
- full executable path when Windows permits access;
- AppUserModelID when available;
- application icon from the executable.

An executable path can contain a Windows account name, organization name or other information specific to the local computer.

### Windows media sessions

For the optional `Pause` action, DictaMute may read:

- Global System Media Transport Controls session identifiers;
- playback status;
- whether pause/play commands are supported.

A selected media-session identifier can be stored in the profile when the user assigns it explicitly.

## Data stored locally

DictaMute stores configuration in:

`%LOCALAPPDATA%\DictaMute\settings.json`

The settings file can contain:

- global enabled/disabled state;
- selected profile index;
- profile names and enabled/disabled state;
- source application identities;
- target application identities;
- executable paths when available;
- AppUserModelIDs when available;
- target action (`Duck`, `Mute` or `Pause`);
- assigned media-session ID when configured;
- threshold, hold time, duck level and fade duration;
- global-mode options;
- configured global hotkeys.

When malformed settings are detected, DictaMute can create a local backup next to the settings file using a name similar to:

`settings.json.corrupt-<timestamp>.bak`

## Diagnostic log

DictaMute writes a local diagnostic log to:

`%LOCALAPPDATA%\DictaMute\DictaMute.log`

When the log grows beyond approximately 1 MiB it can be rotated to:

`DictaMute.log.1`

The log contains timestamps, operation descriptions and exception details. Exception text can contain local environment details. The application does not intentionally log microphone recordings or a continuous history of meter values.

Before sharing logs publicly, review and redact:

- usernames;
- local file paths;
- organization names;
- application names that reveal confidential work;
- device names;
- any other sensitive context present in exception messages.

## Network activity, telemetry and analytics

A review of the current application source found no application networking API usage such as `HttpClient`, sockets or web-request clients.

The current DictaMute application therefore does not intentionally:

- transmit settings;
- transmit microphone data;
- upload logs;
- send behavioral telemetry;
- send analytics events;
- use session replay;
- upload crash reports;
- check a remote update service automatically.

This statement applies to DictaMute's own current code. Windows, GitHub, package managers, third-party applications and operating-system services have their own behavior and privacy policies.

## Data sharing

DictaMute does not intentionally share locally processed application/audio-session data with the publisher or third parties.

Data leaves the local machine only when the user separately chooses to share information, for example by creating a GitHub issue or using a future support channel.

## Public bug reports and support data

The public GitHub issue tracker is public. Do not post confidential information there.

Before submitting a bug report:

1. remove or replace usernames in paths;
2. redact organization/client/project names;
3. crop screenshots to remove private content;
4. include only the smallest relevant log excerpt;
5. do not attach microphone recordings or unrelated application content;
6. do not publish secrets, access tokens or credentials.

For ordinary support, use the project's public GitHub issue tracker with sanitized information.

A private support channel is not currently promised. Sensitive security reports must follow the private reporting route documented in `SECURITY.md` once that document and route are published. Until then, do not place sensitive vulnerability details in a public issue.

## Support-data retention

Local DictaMute files remain on the user's computer until the user deletes them.

Public GitHub issues and comments can remain as part of the public project history and are subject to GitHub's platform controls.

If the publisher accepts support material through a private project-controlled channel in the future:

- collect only material needed to investigate the report;
- do not repurpose it for advertising or profiling;
- remove diagnostic attachments and private support material within 30 days after the report is closed, unless a longer retention period is necessary for an active security/legal investigation;
- honor earlier deletion requests where technically and legally practical;
- document platform limitations if a third-party support service prevents selective deletion.

## Inspecting and deleting local DictaMute data

To remove local DictaMute configuration and logs:

1. exit DictaMute from the notification-area menu;
2. open `%LOCALAPPDATA%\DictaMute\`;
3. optionally copy `settings.json` if a backup is wanted;
4. delete the DictaMute directory.

Deleting `settings.json` removes saved profiles, application assignments and configured hotkeys. DictaMute recreates default settings when started again.

Removing the application executable/package does not necessarily remove `%LOCALAPPDATA%\DictaMute\`; delete that directory separately if desired.

## Privacy review gate

Before merging or releasing any feature that adds one of the following, update this notice and the related technical controls:

- network requests;
- automatic update checks;
- telemetry or analytics;
- remote crash reporting;
- log upload;
- cloud configuration;
- account/login features;
- recording or persistence of microphone/audio samples;
- remote support or diagnostics.

Marketing copy must not make privacy claims that are broader than the behavior documented here.

## Contact and responsibility

For project-controlled support data voluntarily submitted to the publisher, the responsible publisher is:

**KeyffMS / aiteracja.pl**

Repository:

`https://github.com/KeyffMS/DictaMute`

Canonical product URL:

`https://aiteracja.pl/DictaMute/`

Use the public issue tracker only for non-sensitive, redacted reports. Security/private reporting instructions are maintained separately in `SECURITY.md`.
