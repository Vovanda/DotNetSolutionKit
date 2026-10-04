# Contributing

## Changes

Every change reaches `master` through a pull request; `master` takes no direct pushes.

- The pull request runs [template.yml](.github/workflows/template.yml): it generates a solution for each
  combination of flags, builds it, runs its tests and scans it for secrets. It has to be green before the
  merge.
- What the change generates has to be green too, and template.yml checks that as well. In the
  combinations with `--GitHubCiCd` it runs the workflows the solution ships - `ci.yml` with its test
  servers, integration tests and coverage threshold, `secret-scan.yml`, `api-diff.yml` - with
  [scripts/run-workflow.py](scripts/run-workflow.py), on the runner and nothing published. The `uses:`
  steps are left to the job around it; that they run on GitHub itself is what the `nightly` branch of
  DotNetSolutionKit.Samples shows: a daily check regenerates it the day after `master` moves, and its CI
  runs then. `master` stays ready to release.
- A change in what a generated solution does comes with a test of that behaviour in the template's tests.
- Documentation and the site (`docs/`, `index.html`, `docs.html`, `site/`) go through a pull request too.
  They are not part of the template's package and need no new version.

After a release, [DotNetSolutionKit.Samples](https://github.com/sawking-tech/DotNetSolutionKit.Samples)
regenerates its branches from it within a day and runs their CI; its `nightly` branch follows `master`.

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
- the tag sits on `master`, or on `2.x` for a fix of the .NET 8 line once 3.0 is out. `2.x` takes
  pull requests the way `master` does, its pushes run `template.yml`, and it releases `2.*` tags only;
  their GitHub release is not marked latest.

The release also packs the template, `DotNetSolutionKit.Templates.csproj`, and attaches the package to the
GitHub release. With the repository secret `NUGET_API_KEY` set, it publishes the package to nuget.org as
`SawKing.DotNetSolutionKit`. A version there cannot be deleted, only unlisted.
