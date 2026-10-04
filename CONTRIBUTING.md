# Contributing

## Branches

- `dev` is where the work goes. Every change reaches it through a pull request; it takes no direct pushes.
- `master` is the last release. It takes only the release pull request from `dev`, and a patch for the
  release when one cannot wait for the next.
- `2.x` is the .NET 8 line once 3.0 is out: its fixes go there through pull requests, until support for
  .NET 8 ends on 10 November 2026.

## Changes

A pull request into `dev` publishes nothing, so a small one is fine. Each pull request waits for a full run
of the checks, though, so it pays either to put related pieces of work into one pull request, or to keep
several pull requests going at once and work on one while the checks of another run.

- The pull request runs [template.yml](.github/workflows/template.yml): it generates a solution for each
  combination of flags, builds it, runs its tests and scans it for secrets. It has to be green before the
  merge.
- What the change generates has to be green too, and template.yml checks that as well. In the
  combinations with `--GitHubCiCd` it runs the workflows the solution ships - `ci.yml` with its test
  servers, integration tests and coverage threshold, `secret-scan.yml`, `api-diff.yml` - with
  [scripts/run-workflow.py](scripts/run-workflow.py), on the runner and nothing published. That their
  `uses:` steps run on GitHub itself is what the `nightly` branch of
  [DotNetSolutionKit.Samples](https://github.com/sawking-tech/DotNetSolutionKit.Samples) shows: it is
  regenerated from `dev` the day after `dev` moves, and its CI runs then.
- A change in what a generated solution does comes with a test of that behaviour in the template's tests.
- A change to a workflow of this repository is run on a temporary branch first, for each case it
  handles - the one that succeeds and each one that refuses - before its pull request: a workflow that
  does not work never reaches `dev`, let alone `master`. A rehearsal publishes nothing.
- A change a user of the template sees raises `version` in [version.json](version.json), in the same commit:
  minor for a feature, patch for a fix. The release notes name what the version is about, for whoever
  decides to update, in a sentence each; they are not a log of every change. The details are in the commits
  and the documentation, the steps to update in [upgrading](docs/getting-started/upgrading.md). On `dev`
  raising it publishes nothing; what is published is the version `dev` carries when it reaches `master`.
- Documentation and the site (`docs/`, `index.html`, `docs.html`, `site/`) change in the same pull request
  as what they describe. The site is served from `master`, so it describes the released version.

## Releases

The template follows semantic versioning. Its version and release notes are in `version.json`, in the same
shape as a generated solution's ([ADR-004](docs/adr/004-product-version-by-hand.md)).

What `master` receives is published: the GitHub release, the package on nuget.org and in GitHub Packages.
A release comes about once a week, at the end of a sprint, when the work done is worth one, and only
then:

- inside the template: `template.yml` is green on the last commit of `dev`;
- outside it: `nightly` in DotNetSolutionKit.Samples is regenerated from that commit, and its CI is green;
- the batch is finished: nothing begun and left undone is in `dev`.

Before the merge, `version` on `dev` is checked against the last release. If nothing raised it, one more
commit on `dev` does, with the release notes. The notes of the versions `dev` went through since the last
release are gathered into the one released; a change that breaks solutions of the current major version
and comes without a short way to update them makes it a major, with the update described in
[upgrading](docs/getting-started/upgrading.md). The notes have a headline about what matters most to a
user of the template, and lines checked against the commits since the last release.

The release is then one pull request from `dev` into `master`.

Merged into `master`, it makes [release.yml](.github/workflows/release.yml) tag `v<version>`, wait for
`template.yml` on that commit and publish: the GitHub release with the version's notes
(`scripts/release-notes.sh <version>` prints the same text), the package
`DotNetSolutionKit.Templates.csproj` packs, attached to the release, on nuget.org as
`SawKing.DotNetSolutionKit` and in GitHub Packages. A version on nuget.org cannot be deleted, only
unlisted. DotNetSolutionKit.Samples regenerates its release branches from the new tag within a day.

A patch that cannot wait is a pull request into `master` that raises the patch version; it is merged back
into `dev`. A fix of the .NET 8 line is a pull request into `2.x`; `2.x` releases `2.*` versions only, and
their GitHub release is not marked latest.
