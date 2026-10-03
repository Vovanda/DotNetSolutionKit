# CI on GitHub Actions

Generated with `--GitHubCiCd`, off by default: each team has its own CI, and these workflows only run on
GitHub. Pass it with `-M false`; the workflows cover every service in `All.sln`, so a later service needs
nothing more.

## `.github/workflows/ci.yml`

Runs on every pull request, on a push to `main` or `master`, and by hand:

1. Fails when a service project is missing from `All.sln`. A service is added there by
   `src/services/manual-add-projects.sh`; forgotten, the service would drop out of the build and its tests
   without anything failing.
2. Builds `All.sln` in Release.
3. Runs the unit tests: everything not marked `[Category(TestCategories.Integration)]`.
4. Runs the integration tests against real servers started in the job: PostgreSQL always, ClickHouse with
   `--ClickHouse`, SeaweedFS with `--Storage`. PostgreSQL keeps its data on a ramdisk with durability off,
   which is safe only because the data dies with the job. See [testing](../architecture/testing.md).
5. Merges the coverage of both runs into one report: a summary on the run page, the HTML report as an
   artifact. Migrations, model configuration, DI composition, host setup and seeding are left out.

Branch coverage fails the run when it is below the repository variable `COVERAGE_MIN_BRANCH`, in
percent. Without the variable the report is shown and nothing fails on it, so a team sets the threshold
once its tests exist, and raises it without editing the workflow.

## `.github/workflows/secret-scan.yml`

Runs [gitleaks](https://github.com/gitleaks/gitleaks) over the commits a pull request or a push brings in,
with `tools/gitleaks/.gitleaks.toml`: the gitleaks rule set plus connection-string passwords, URIs with
credentials and signing keys in JSON settings. Values marked `test-do-not-use`, `placeholder` or
`changeme`, and `${VARIABLE}` references, are allowed.

Only incoming commits are scanned, so a finding in old history does not fail every later run. To scan the
whole history once, for example when adopting the workflow in an existing repository:

```bash
gitleaks detect --config tools/gitleaks/.gitleaks.toml --redact
```
