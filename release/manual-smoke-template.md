# DictaMute manual Windows smoke test

- Release tag:
- Product version:
- Source commit:
- Candidate ZIP SHA-256:
- Tester:
- Date:
- Windows edition:
- Windows build:
- Architecture:
- Primary capture device:
- Target playback application(s):

## Checks

- [ ] Application launches and displays the expected version.
- [ ] Tray icon and menu open correctly.
- [ ] Configured X source appears while its microphone session is active.
- [ ] Meter/threshold reacts to signal and is understood as signal activity, not speech classification.
- [ ] Duck reduces target volume and restores it.
- [ ] Mute mutes and restores prior state where applicable.
- [ ] Pause works for a uniquely matched media session or safely falls back to Duck.
- [ ] Hold time delays restoration as configured.
- [ ] Profile switching works.
- [ ] Disabled profile does not trigger automation.
- [ ] Global automation enable/disable works and releases controlled audio.
- [ ] Settings survive application restart.
- [ ] Closing through the tray restores controlled audio state.
- [ ] No unexpected network behavior attributable to DictaMute is observed.
- [ ] LICENSE, PRIVACY.md, SECURITY.md and THIRD-PARTY-NOTICES.md exist in the extracted package.

## Result

- [ ] PASS — candidate accepted for publication.
- [ ] FAIL — do not publish.

Notes:
