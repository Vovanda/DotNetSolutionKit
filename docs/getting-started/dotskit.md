# dotskit

`dotskit` is the command-line tool of DotNetSolutionKit. It generates a solution with the template, adds
services to it and flags to its services, and keeps what the team changed: where the team and the template
changed the same file, it merges the two the way git merges branches.

The template works without the tool, as [generating a solution](generating-a-solution.md) describes. The
tool adds what one run of `dotnet new` cannot do: add a flag to a service that has its folder already,
update a file the team has changed, and record in the solution what it was generated from.

## Install

```bash
dotnet tool install -g SawKing.DotsKit.Tool
```

The tool is published from version 2.8. dotskit X.Y.Z generates with the template X.Y.Z: it installs the
package `SawKing.DotNetSolutionKit` of its own version into a hive of its own, and the template installed
by `dotnet new install` is left as it is.

## Commands

```bash
# in an empty folder: a solution with its first service
dotskit new -N MyCompany -P MyProduct -S Orders

# in the solution: one more service, with the solution's names, database and deployment
dotskit new -S Billing --Storage true

# in the solution: a flag for a service it has
dotskit new -S Orders --MongoDB true

# in a solution made without dotskit: describe it in its manifest
dotskit init
```

`dotskit new` reads from the folder whether to make a solution or to add to one: it looks for
`src/services/*.All.sln` in the folder and its parents. `--Solution` is not needed and changes nothing.

The template's parameters are passed as they are, in short or long form, in any case: `-S`, `--Storage`,
`--storage`, `--api-gateway`. See the [parameters](generating-a-solution.md#parameters).

## What a command does

1. Checks the folder. A new solution goes into an empty folder or next to files in a clean git working
   tree. A solution to add to needs `.dotskit/manifest.json` of the tool's version and a clean working tree.
2. Generates in a temporary folder what the template gives for the solution before the command (the base)
   and after it.
3. Compares the base, the new template output and the solution file by file, and shows the plan: every
   file with what happens to it and why, and the projects added to `All.sln` or removed from it.
4. Asks before writing. `--yes` writes without asking.
5. Writes all files or none: an error or Ctrl+C in the middle puts every file back. A process killed while
   writing leaves `*.dotskit-new` and `*.dotskit-old` files beside the solution's; `git status` shows them,
   and the next command refuses the unclean tree until they are cleaned up with git.
6. Builds `All.sln`.

What happens to a file:

| In the plan | When | What is written |
|---|---|---|
| `Add` | the template adds a file the solution does not have | the template's file |
| `Update` | the team did not change the file | the template's new file, with the line endings of the solution's |
| `Merge` | both changed it, in lines apart | both changes |
| `Delete` | the template removed a file the team did not change | the file is deleted |
| `Kept` | one side removed the file and the other changed it, or both changed a binary file | nothing: the solution's file stays |
| `Conflict` | both changed the same lines, or lines right next to each other | the file with both versions between `<<<<<<< solution` and `>>>>>>> template` |

`All.sln` is changed by `dotnet sln`, project by project, and never merged as text: `dotnet sln add` gives
a project a new GUID each time.

## Manifest

`.dotskit/manifest.json` records what the solution was generated from: the template's version, the
arguments of the solution and the arguments of each service. Commit it with the solution: every later
command rebuilds the base from it.

The manifest keeps a port for each service. The template picks a random free port when `--HttpPort` is not
given, so the tool picks one once, the first port from 5000 that no service of the solution has, and a
service keeps its port when a later command adds a flag to it.

A flag with a part in `Common` or the root, such as `--Storage`, becomes the solution's when any of its
services has it. The solution's names, `--Database`, `--Deploy`, `--TestFramework`, `--Agent` and
`--GitHubCiCd` are set by the first command. A later command takes them from the manifest; a command that
gives one of them another value is refused.

## A solution made without dotskit

`dotskit new` works from the manifest. A solution made with the template alone, or before 2.8, has none, and
`dotskit init` writes it: it reads the solution, checks the reading and writes `.dotskit/manifest.json`, and
nothing else, after a yes.

What it reads:

- the version of the template from `DotNetSolutionKitVersion` in `Directory.Build.props`, there since 2.7.0;
- the names from `src/services/*.All.sln`; where a dot can belong to either name (`Acme.Corp.Shop`), from the
  image names of `deploy/build-images.sh`, or from `-N` and `-P` given to the command;
- the database, the deployment, the test framework and the agent by the files only one choice generates;
- the flags of `Common` and the root by the files each flag adds;
- each service under `src/services` with its own API project: its flags by the sections of its
  `appsettings.json` and the lines only a flag writes, its port by `launchSettings.json`.

The check: `init` generates the solution again by what it read, with that version of the template, and
compares it with the folder - "412 of 420 files of the template reproduced; these differ: ...". A file that
differs is a change of the team or a wrong reading; the report lists it so a person can tell which before
anything relies on the manifest. Files the team added are not counted, and `All.sln` is compared by its
projects. The check installs the template of that version from nuget.org.

| The folder | What `init` does |
|---|---|
| empty | writes nothing, and names `dotskit new -N <Company> -P <Product> -S <Service>` |
| no `src/services/*.All.sln` | writes nothing: not a solution of DotNetSolutionKit |
| a solution of 2.7 or later, no manifest | reads it, checks it, shows the manifest and the report, writes it after a yes |
| a solution of 2.0-2.6, which does not say its version | writes nothing, and asks for `--template-version 2.x.y`; the check then shows how well that version reproduces the solution |
| generated from the template's sources (version `source`) | writes nothing, and asks for the release it came from with `--template-version` |
| newer than the tool | writes nothing, and names `dotnet tool update -g SawKing.DotsKit.Tool` |
| of an earlier major version than the tool | writes nothing: the tool describes solutions of its own major version |
| a service of another version than the solution | writes nothing, names the service and its version, and asks for the version to describe the solution by with `--template-version`; with it, the check shows what differs |
| with a manifest that lists every service | writes nothing: "the manifest is current: 2.8.0, services Orders, Billing" |
| with a manifest, and a service under `src/services` it does not list | adds the service, read from its files, after the same check |
| services, but no `Common` | writes the manifest without the check and says so: the database and the flags read from `Common` are the template's defaults; `dotskit new` takes `Common` for removed by the team and does not add it back |
| uncommitted changes in git | runs: it writes only `.dotskit/` |

## Options

| Option | What it does |
|---|---|
| `--yes` | writes without asking, for CI |
| `--force` | resolves a conflict with the template's lines; the team's lines stay in git history |
| `--no-build` | does not build the solution after writing |
| `--allow-dirty` | runs on a working tree with uncommitted changes |
| `--template-version 2.x.y` | `init`: the version the solution was made by, where it does not say it |

## Exit codes

| Code | Meaning |
|---|---|
| `0` | written, and the solution builds; with `--no-build`, written |
| `1` | refused or failed, with the reason on one line; nothing was written because the answer was not yes; or written, and the build fails or was cancelled |
| `2` | written with conflicts: resolve them, then build |

## Limits of this version

- `dotskit new` works in a solution with a manifest: made by `dotskit`, or described by `dotskit init`.
- `dotskit new` needs a manifest of the tool's version: a solution of an earlier version is brought to it by
  `dotskit upgrade` first.
