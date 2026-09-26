# DictaMute release withdrawal procedure

A published release may need to be withdrawn because of a security issue, destructive audio-state behavior, corrupted packaging, missing legal material or another serious defect.

## Rules

1. Do not silently replace the binary behind an existing published tag.
2. Preserve the public history and explain that the affected version is withdrawn.
3. Remove or disable the unsafe binary download only when necessary to protect users, while preserving release notes/evidence where the platform permits.
4. Publish a clear notice describing the affected version and recommended action without disclosing sensitive exploit detail prematurely.
5. Fix the problem in a new version and publish under a new tag.
6. Update the canonical website and package/directory listings so they no longer recommend the withdrawn version.
7. If a checksum or artifact was incorrect, do not reuse the old tag to publish corrected bytes.
8. Record the decision and replacement version in `docs/RELEASE-HISTORY.md`.

## Security-sensitive withdrawal

Coordinate disclosure using `SECURITY.md`.

Do not put private vulnerability details into a public withdrawal notice before coordinated disclosure is appropriate.
