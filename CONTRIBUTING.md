# Contributing

## Versions and releases

The template follows semantic versioning. Its version and release notes are in [version.json](version.json),
in the same shape as a generated solution's ([ADR-004](docs/adr/004-product-version-by-hand.md)):

- a commit that adds or fixes something a user of the template sees adds a line to the `highlights` of the
  next version, in the same commit. Without a next version in the file yet, it adds one: a minor version
  for a feature, a patch for a fix;
- a change that breaks solutions generated from the current major version, and comes without a short way
  to update them, starts a new major version, with the update described in
  [upgrading](docs/getting-started/upgrading.md);
- a release is a tag `v<version>` on the commit where `version` names it. The tag starts
  `.github/workflows/release.yml`, which refuses a tag that does not match `version.json` and publishes a
  GitHub release with that version's notes. `scripts/release-notes.sh <version>` prints the same text.

The release also packs the template, `DotNetSolutionKit.Templates.csproj`, and attaches the package to the
GitHub release. With the repository secret `NUGET_API_KEY` set, it publishes the package to nuget.org as
`SawKing.DotNetSolutionKit`. A version there cannot be deleted, only unlisted.
