PAGES = {
    "": {
        "title": "DictaMute — automatic audio ducking, mute and pause for Windows",
        "description": "DictaMute automatically ducks, mutes or pauses selected Windows applications when configured microphone activity crosses a signal threshold. Free and open source.",
        "h1": "Make background audio get out of the way when your microphone is active.",
        "body": """
<section class="hero-grid">
  <div>
    <p class="eyebrow">Windows audio automation · free and open source</p>
    <p class="lead">DictaMute watches configured microphone sessions and a signal threshold. When the threshold is crossed, it can duck, mute or pause selected playback applications, then restore them after a configurable hold period.</p>
    <p class="boundary">DictaMute uses microphone activity and a signal meter. It does not perform speech recognition.</p>
    <div class="actions">
      <a class="button primary" href="{base}releases/">View release status</a>
      <a class="button" href="https://github.com/KeyffMS/DictaMute">View source on GitHub</a>
    </div>
  </div>
  <div class="flow-card" aria-label="DictaMute workflow">
    <div class="flow-node">Source X<br><span>active microphone session</span></div>
    <div class="flow-arrow">↓</div>
    <div class="flow-node">Signal above threshold</div>
    <div class="flow-arrow">↓</div>
    <div class="flow-node accent">Duck · Mute · Pause target Y</div>
    <div class="flow-arrow">↓</div>
    <div class="flow-node">Hold time → restore</div>
  </div>
</section>

<section>
  <h2>Choose what triggers the rule and what reacts.</h2>
  <div class="cards two">
    <article class="card"><h3>X — Sources</h3><p>Source applications are the microphone-using processes allowed to trigger the selected profile.</p></article>
    <article class="card"><h3>Y — Targets</h3><p>Target applications are the playback processes DictaMute may duck, mute or pause while the rule is active.</p></article>
  </div>
</section>

<section>
  <h2>Three explicit actions.</h2>
  <div class="cards three">
    <article class="card"><h3>Duck</h3><p>Reduce the target session to a configured level, then restore its previous level.</p></article>
    <article class="card"><h3>Mute</h3><p>Mute the target session while the gate is open and restore its prior state when possible.</p></article>
    <article class="card"><h3>Pause</h3><p>Pause a uniquely matched Windows media session. If the match is ambiguous, DictaMute falls back to Duck.</p></article>
  </div>
</section>

<section>
  <h2>Profiles for different situations.</h2>
  <p>Profiles keep source applications, target actions, threshold, hold time, duck level and global behavior together. A profile can be disabled independently from the global automation switch.</p>
</section>

<section class="trust">
  <h2>Local settings. No raw microphone recording stream.</h2>
  <p>The current implementation reads Windows audio-session state and signal-level metering. It does not create a raw microphone recording stream, upload audio, send behavioral telemetry or perform an automatic network update check.</p>
  <p><a href="{base}privacy/">Read privacy details</a> · <a href="{base}security/">Review security boundaries</a></p>
</section>

<section>
  <h2>Current limitations</h2>
  <ul>
    <li>The threshold is not a human-voice classifier.</li>
    <li>Noise or keyboard sound can cross the threshold.</li>
    <li>Application and media-session compatibility varies.</li>
    <li>Protected, elevated, exclusive or unusual audio paths may behave differently.</li>
    <li>Pause requires a compatible uniquely matched media session.</li>
    <li>Compatibility and performance evidence is still limited during alpha development.</li>
  </ul>
</section>
"""
    },
    "features/": {
        "title": "DictaMute features — sources, targets, profiles and audio actions",
        "description": "Explore DictaMute source/target rules, Duck/Mute/Pause actions, profiles, signal threshold, hold time, hotkeys and notification-area controls.",
        "h1": "Audio automation built around explicit source and target applications.",
        "body": """
<section><h2>Sources X</h2><p>Configure the microphone-using applications allowed to trigger the selected profile. A source session must be active and the signal must cross the threshold unless the profile uses the any-microphone option.</p></section>
<section><h2>Targets Y</h2><p>Choose playback applications and assign Duck, Mute or Pause. A source is never targeted by the same rule. Optional global mode can apply one action to everything except X.</p></section>
<div class="cards two">
  <article class="card"><h2>Threshold</h2><p>The signal level required to open the gate. Lower values react more easily. It is not speech classification.</p></article>
  <article class="card"><h2>Hold time</h2><p>Keeps the gate open briefly after the signal falls so short pauses do not constantly restore audio.</p></article>
  <article class="card"><h2>Duck level</h2><p>Sets the target Duck volume. DictaMute avoids raising a session that is already quieter.</p></article>
  <article class="card"><h2>Fade</h2><p>Controls how quickly Duck moves toward and restores volume. No guaranteed latency is claimed.</p></article>
</div>
<section><h2>Profiles</h2><p>Create, rename, select, enable/disable and delete profiles. Both the selected profile and global automation must be enabled for automation to run.</p></section>
<section><h2>Global hotkeys</h2><ul><li><code>Ctrl+Alt+X</code> — add foreground app to X.</li><li><code>Ctrl+Alt+Y</code> — add foreground app to Y.</li><li><code>Ctrl+Alt+M</code> — toggle global automation.</li></ul><p>Registration can fail if another application already owns a shortcut.</p></section>
<section><h2>Notification area</h2><p>Open configuration, toggle automation, choose profiles and exit from the DictaMute tray menu. The tray icon reflects readiness, active automation and disabled/error states.</p></section>
<section><h2>What DictaMute does not do</h2><ul><li>No speech recognition or transcription.</li><li>No guarantee that a trigger is human voice.</li><li>No process injection, kernel driver or Windows service.</li><li>No DRM or privilege bypass.</li><li>No universal compatibility guarantee.</li><li>No current telemetry or audio/settings upload.</li></ul></section>
"""
    },
    "download/": {
        "title": "Download DictaMute for Windows",
        "description": "Download the official DictaMute Windows x64 release from GitHub Releases, verify its SHA-256 checksum and review system requirements and known limitations.",
        "h1": "Download the current DictaMute release.",
        "body": """
<section class="notice"><h2>No official binary release has been published yet.</h2><p>The first public alpha is planned as <code>v0.0.107-alpha</code>. Until issue #7 is completed, temporary GitHub Actions artifacts are not official downloads.</p><p><a class="button" href="https://github.com/KeyffMS/DictaMute">Follow the repository</a></p></section>
<section><h2>System requirements</h2><ul><li>Windows 10 build 17763 or later, or Windows 11.</li><li>x64 for the initial official binary package.</li></ul></section>
<section><h2>Install</h2><ol><li>Download the official ZIP from the GitHub Release linked here.</li><li>Optionally verify SHA-256.</li><li>Extract the complete ZIP.</li><li>Start <code>DictaMute.exe</code>.</li><li>Configure X, Y and threshold, then Save and apply.</li></ol></section>
<section><h2>Verify SHA-256</h2><pre><code>Get-FileHash .\DictaMute-v0.0.107-alpha-win-x64.zip -Algorithm SHA256</code></pre><p>Compare the result with <code>SHA256SUMS.txt</code> from the same release.</p></section>
<section><h2>Windows reputation</h2><p>New or unsigned open-source binaries can trigger Microsoft Defender SmartScreen or another reputation warning. Download only from project-controlled links and verify the release, tag and checksum. Do not disable system protection globally.</p></section>
<section><h2>Remove DictaMute</h2><p>Exit DictaMute, delete its application directory and optionally delete <code>%LOCALAPPDATA%\DictaMute\</code> to remove saved profiles and logs.</p></section>
"""
    },
    "docs/": {
        "title": "DictaMute documentation — setup, profiles and troubleshooting",
        "description": "Set up DictaMute sources and targets, configure threshold and hold time, use Duck/Mute/Pause, manage profiles and troubleshoot Windows audio sessions.",
        "h1": "Set up DictaMute and understand each control.",
        "body": """
<section><h2>Three-step quick start</h2><ol><li><strong>Add source X.</strong> Start the microphone-using application and select it or use the source hotkey.</li><li><strong>Add target Y.</strong> Start playback, select the target and choose Duck, Mute or Pause.</li><li><strong>Set threshold and apply.</strong> Configure threshold/hold time, enable the profile and global automation, then save/apply.</li></ol></section>
<div class="cards two">
<article class="card"><h2>Profiles</h2><p>Create, rename, select, enable/disable and delete profiles. A disabled selected profile does not run automation.</p></article>
<article class="card"><h2>Sensitivity</h2><p>Threshold opens the gate; hold time delays restoration; Duck level and fade control volume behavior.</p></article>
<article class="card"><h2>Actions</h2><p>Duck changes volume, Mute changes mute state, and Pause uses a uniquely matched Windows media session with Duck fallback.</p></article>
<article class="card"><h2>Global behavior</h2><p>Use everything-except-X or any-microphone modes when explicit lists are not the intended workflow.</p></article>
</div>
<section><h2>Troubleshooting</h2><ul><li>Source not appearing: confirm the application has an active microphone session.</li><li>Meter at zero: verify the capture device, Windows level and driver.</li><li>Pause falls back: assign a media-session ID or use Duck/Mute.</li><li>Elevated target: Windows process/audio visibility can differ across integrity levels.</li><li>Reset configuration: exit DictaMute and remove <code>%LOCALAPPDATA%\DictaMute\settings.json</code>.</li></ul></section>
<section><h2>Developer documentation</h2><p><a href="https://github.com/KeyffMS/DictaMute">Repository</a> · <a href="https://github.com/KeyffMS/DictaMute/blob/first-attempt/docs/IMPLEMENTATION.md">Implementation</a> · <a href="https://github.com/KeyffMS/DictaMute/blob/first-attempt/docs/TESTING.md">Testing</a></p></section>
"""
    },
    "how-it-works/": {
        "title": "How DictaMute works with Windows audio sessions",
        "description": "Learn how DictaMute reads Windows audio-session activity and signal level, resolves X/Y rules, applies Duck/Mute/Pause and restores audio state.",
        "h1": "Windows audio-session automation, not speech recognition.",
        "body": """
<section><ol class="steps"><li>DictaMute enumerates active Windows capture and render endpoints and sessions.</li><li>It associates sessions with process/application identity where Windows permits.</li><li>For capture endpoints it reads session activity and endpoint signal metering.</li><li>The selected profile resolves which active capture apps count as X sources.</li><li>The gate compares peak signal with threshold and hold time.</li><li>Target rules resolve Y sessions and their Duck/Mute/Pause action.</li><li>DictaMute tracks the state it changes so it can restore controlled audio.</li><li>Pause uses Windows media controls and requires a unique match; otherwise it falls back to Duck.</li></ol></section>
<section class="flow-card wide"><h2>Data flow</h2><p>Capture endpoint/session → active source X → meter threshold/gate → target Y resolver → Duck/Mute/Pause → restore.</p></section>
<section><h2>Boundary</h2><p>DictaMute does not need to inject code into another application to change its Windows audio session. The current implementation also does not create a raw microphone recording stream.</p></section>
"""
    },
    "faq/": {
        "title": "DictaMute FAQ",
        "description": "Answers about DictaMute microphone handling, speech detection, application compatibility, privacy, profiles, tray operation and building from source.",
        "h1": "Frequently asked questions about DictaMute.",
        "body": """
<section class="faq">
<h2>Is DictaMute free?</h2><p>Yes. DictaMute is free and open source under the MIT License.</p>
<h2>Does DictaMute record my microphone?</h2><p>The current implementation does not create a raw microphone capture stream or persist microphone audio samples. It reads Windows audio-session state and signal-level metering.</p>
<h2>Does DictaMute recognize when I am speaking?</h2><p>No. It does not perform speech recognition or human-voice classification. Noise or keyboard sound can also cross the threshold.</p>
<h2>Does it work with every media player?</h2><p>No universal compatibility guarantee is made. Duck/Mute depend on Windows audio sessions; Pause also depends on a compatible uniquely matched media session.</p>
<h2>What if Pause cannot identify the player?</h2><p>DictaMute falls back to Duck rather than broadcasting an unaddressed global Play/Pause command.</p>
<h2>Profile enabled vs global automation?</h2><p>Both matter. A disabled selected profile does not run automation even when the global switch is enabled.</p>
<h2>Where are settings stored?</h2><p><code>%LOCALAPPDATA%\DictaMute\settings.json</code>. Logs are stored in the same directory.</p>
<h2>Does DictaMute send telemetry?</h2><p>The current implementation has no behavioral telemetry, analytics, remote crash reporting or automatic network update check.</p>
<h2>How do I completely remove it?</h2><p>Exit it, remove the program directory and optionally delete <code>%LOCALAPPDATA%\DictaMute\</code>.</p>
<h2>Can I build it myself?</h2><p>Yes. Use the .NET 8 SDK on supported Windows and follow the repository build instructions.</p>
<h2>Where do I report a problem?</h2><p>Use <a href="https://github.com/KeyffMS/DictaMute/issues">GitHub Issues</a> for non-sensitive reports and follow the security page for sensitive vulnerabilities.</p>
</section>
"""
    },
    "releases/": {
        "title": "DictaMute release history",
        "description": "Public DictaMute release history with versions, dates, official Windows downloads, checksums, source tags and release status.",
        "h1": "Public, verifiable DictaMute releases.",
        "body": """
<section class="notice"><h2>Planned: v0.0.107-alpha</h2><p>Status: not published. The first alpha remains behind the manual smoke-test and release-integrity gates in issue #7.</p></section>
<section><h2>Release authority</h2><p>GitHub Releases are the authoritative source for public DictaMute binaries. Temporary Actions artifacts are never official downloads.</p><p><a class="button" href="https://github.com/KeyffMS/DictaMute/releases">View GitHub Releases</a></p></section>
"""
    },
    "privacy/": {
        "title": "DictaMute privacy",
        "description": "How DictaMute processes Windows audio-session metadata and signal level locally, what it stores, and how to remove configuration and logs.",
        "h1": "DictaMute privacy and local data.",
        "body": """
<section class="trust"><p class="lead">DictaMute operates locally. The current implementation does not create a raw microphone recording stream, upload audio, send behavioral telemetry or perform an automatic network update check.</p></section>
<section><h2>What it reads</h2><p>Windows audio-session state, signal-level metering, process identity used for matching and compatible media-session identifiers.</p></section>
<section><h2>What it stores</h2><p>Profiles, app identities, paths when available, actions, thresholds, hotkeys and diagnostics under <code>%LOCALAPPDATA%\DictaMute\</code>.</p></section>
<section><h2>Delete local data</h2><p>Exit DictaMute, then delete the DictaMute directory under <code>%LOCALAPPDATA%</code>. Removing <code>settings.json</code> deletes saved profiles and assignments.</p></section>
<section><h2>Bug reports</h2><p>Redact usernames, local paths, organization/client names, tokens and confidential screenshots before posting publicly.</p><p><a href="https://github.com/KeyffMS/DictaMute/blob/first-attempt/PRIVACY.md">Read the complete source privacy notice</a>.</p></section>
"""
    },
    "legal/": {
        "title": "DictaMute license and legal notices",
        "description": "DictaMute MIT License, third-party notices, dependency information and third-party trademark/no-affiliation statement.",
        "h1": "License and legal notices.",
        "body": """
<section><h2>License</h2><p>DictaMute is distributed under the MIT License.</p><p><a href="https://github.com/KeyffMS/DictaMute/blob/first-attempt/LICENSE">Read LICENSE</a></p></section>
<section><h2>Third-party notices</h2><p>The current application uses NAudio and self-contained .NET runtime components. Official packages retain required notices.</p><p><a href="https://github.com/KeyffMS/DictaMute/blob/first-attempt/THIRD-PARTY-NOTICES.md">Read third-party notices</a></p></section>
<section><h2>No affiliation</h2><p>Third-party product names are used only for factual interoperability examples. DictaMute is not affiliated with, sponsored by, certified by or endorsed by Microsoft, Spotify, TIDAL, Discord, Google, OpenAI or other referenced third parties unless an explicit documented relationship exists.</p></section>
<section><h2>Protected content and platform boundaries</h2><p>DictaMute does not attempt to circumvent DRM, protected-media restrictions, application access controls or Windows security boundaries.</p></section>
"""
    },
    "security/": {
        "title": "DictaMute security and vulnerability reporting",
        "description": "DictaMute security boundaries, current-user privilege model, audio restoration limitations and vulnerability-reporting guidance.",
        "h1": "Security boundaries and reporting.",
        "body": """
<section><h2>Current-user desktop utility</h2><p>DictaMute runs in the current user's Windows session. It does not install a driver or service, inject code into target processes, write target-process memory or bypass DRM/privilege boundaries.</p></section>
<section><h2>Restoration is best effort</h2><p>DictaMute tracks controlled audio state and restores it on normal gate close, configuration changes and shutdown. Forced process termination, device/driver failure or disappearing sessions can prevent restoration; users must be able to restore audio manually.</p></section>
<section><h2>Report a vulnerability</h2><p>Do not publish sensitive exploit details in a public issue. Use GitHub private vulnerability reporting when available, otherwise request a private contact route without including exploit details.</p><p><a href="https://github.com/KeyffMS/DictaMute/blob/first-attempt/SECURITY.md">Read SECURITY.md</a></p></section>
"""
    },
    "support/": {
        "title": "DictaMute support and contact",
        "description": "Get DictaMute usage help, report reproducible bugs, request features or find the private security-reporting route.",
        "h1": "Get help with DictaMute.",
        "body": """
<div class="cards two">
<article class="card"><h2>Usage questions</h2><p>Start with the documentation and FAQ.</p><p><a href="{base}docs/">Documentation</a></p></article>
<article class="card"><h2>Bugs and feature requests</h2><p>Use GitHub Issues for non-sensitive reports.</p><p><a href="https://github.com/KeyffMS/DictaMute/issues">Open GitHub Issues</a></p></article>
<article class="card"><h2>Security</h2><p>Do not post sensitive vulnerability details publicly. Follow the security reporting instructions.</p><p><a href="{base}security/">Security</a></p></article>
<article class="card"><h2>Publisher</h2><p>KeyffMS / aiteracja.pl</p></article>
</div>
<section><h2>Useful bug-report details</h2><ul><li>DictaMute version and Windows build.</li><li>Source and target application.</li><li>Action mode and threshold/hold settings.</li><li>Exact reproduction steps and restoration result.</li><li>Only sanitized log excerpts.</li></ul></section>
<section class="notice"><h2>Privacy warning</h2><p>Do not publish screenshots containing private information or unredacted paths, usernames, organization/client names, tokens or confidential application content.</p></section>
"""
    },
}

NAV = [
    ("Features", "features/"),
    ("Download", "download/"),
    ("Docs", "docs/"),
    ("How it works", "how-it-works/"),
    ("FAQ", "faq/"),
    ("Releases", "releases/"),
]
