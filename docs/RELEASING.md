# DictaMute release process

Issue #7 is the release authority for the first public DictaMute publication.

## Version and tag policy

DictaMute currently uses a monotonic development version in the form:

`0.0.NNN`

The application footer displays this numeric product version.

Public GitHub Release tags use:

- alpha: `v0.0.NNN-alpha`
- beta: `v0.0.NNN-beta`
- release candidate: `v0.0.NNN-rc`
- stable: `v0.0.NNN`

The first planned public release is:

`v0.0.107-alpha`

This does **not** mean it is published yet. Publication remains blocked until the release request is explicitly armed and all gates pass.

## Authoritative sources

- source: `https://github.com/KeyffMS/DictaMute`
- binary publication: GitHub Releases
- release configuration: `release/release-request.json`
- release notes: path named by `notesFile` in the release request
- release history: `docs/RELEASE-HISTORY.md`
- public identity/claims: `docs/BRAND.md`
- privacy: `PRIVACY.md`
- security: `SECURITY.md`
- third-party notices: `THIRD-PARTY-NOTICES.md`

The future product website must link to and verify GitHub Release assets rather than maintain a separate binary authority.

## Release request

`release/release-request.json` is deliberately committed to the repository so publication intent is reviewable.

The initial state is disarmed:

- `publish: false`
- `manualWindowsSmokeAccepted: false`
- `releaseImmutabilityConfirmed: false`

Do not change `publish` to `true` until the exact candidate has passed the manual Windows smoke checklist and GitHub release immutability is confirmed for the repository.

## Required release artifacts

A public Windows x64 release must contain at least:

- `DictaMute-<tag>-win-x64.zip`
- `SHA256SUMS.txt`
- `DictaMute-<tag>-dependencies.json`
- `DictaMute-<tag>-release-manifest.json`

The ZIP must contain:

- `DictaMute.exe` and its complete self-contained runtime payload;
- `LICENSE`;
- `PRIVACY.md`;
- `SECURITY.md`;
- `THIRD-PARTY-NOTICES.md`.

GitHub-generated source archives are not the official Windows binary package.

## Automated gates

The normal build workflow verifies:

- restore/build;
- deterministic core tests;
- WPF/UI/brand/privacy contract tests;
- release metadata consistency;
- self-contained Windows x64 publish;
- required packaged public documents.

The manual release workflow repeats the source/build/test gates and then creates a release candidate archive, checksum, dependency inventory and release manifest.

## Manual Windows smoke test

Record a smoke test using `release/manual-smoke-template.md`.

The exact release candidate must be checked on supported Windows before publication.

Minimum cases:

1. application starts and reports the expected version;
2. source X is detected while its microphone session is active;
3. threshold behavior is understandable and does not claim speech classification;
4. Duck reduces target volume and restores it;
5. Mute mutes and restores prior mute state where applicable;
6. Pause pauses a uniquely matched media session or safely falls back to Duck;
7. hold time delays restoration as configured;
8. profile switching works;
9. disabled profile does not run automation;
10. global automation enable/disable restores audio;
11. tray open/profile/exit actions work;
12. normal exit restores controlled audio state;
13. settings survive restart;
14. no unexpected network prompt/traffic is introduced by DictaMute itself.

Record Windows edition/build, architecture, test date, release tag, source commit and tester.

## Release immutability gate

Before the first public release, confirm GitHub Release immutability or an equivalent repository policy that prevents silent replacement of published release assets/tags.

Record non-secret evidence in issue #7.

If release immutability cannot be enabled, #7 must explicitly document the alternative integrity policy before publication.

## Publishing

The `.github/workflows/release.yml` workflow is manually triggered.

When `publish=false`, it produces a candidate artifact only.

When `publish=true`, the workflow refuses to continue unless:

- `manualWindowsSmokeAccepted=true`;
- `releaseImmutabilityConfirmed=true`;
- project version equals release-request version;
- tag matches channel policy;
- release notes exist;
- all automated tests pass;
- required legal/privacy/security files are present.

Publication uses the exact workflow commit as the tag/release target.

Never reuse a published tag for different source bytes.

## Checksums

The release workflow computes SHA-256 for the official ZIP and writes it to `SHA256SUMS.txt`.

Users can verify a downloaded release with:

```powershell
Get-FileHash .\DictaMute-<tag>-win-x64.zip -Algorithm SHA256
```

The output must match the published checksum.

## Dependency inventory and SBOM

The release workflow records the transitive NuGet package inventory as JSON using the .NET CLI.

Before the first stable release, evaluate a maintained SPDX or CycloneDX SBOM generator and add an SBOM only if the output is reproducible and its generating tool/version is pinned.

Do not label a simple package list as an SBOM.

## Release notes

Every public release note must include:

- product/version/channel;
- source commit;
- supported Windows/architecture;
- concise change summary;
- known limitations;
- privacy/security changes, if any;
- upgrade/configuration notes;
- official ZIP name;
- checksum link/file;
- privacy/security/license/notices links;
- canonical website once live;
- repository/source tag.

## Withdrawal

See `docs/RELEASE-WITHDRAWAL.md`.

Published history must not be silently rewritten to hide a withdrawn build.

## Future website synchronization

The website download page must derive or verify:

- current recommended release;
- tag/version;
- release date;
- artifact name and size;
- SHA-256;
- release notes/source tag;
- dependency/legal/privacy/security links.

GitHub Releases remain authoritative.
