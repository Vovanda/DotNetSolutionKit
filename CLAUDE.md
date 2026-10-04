# DotNetSolutionKit

A `dotnet new` template. The template itself is `template/`; everything else in the repository (the docs,
the site, the workflows at the root) is about it and is not packed.

How changes and releases are made: [CONTRIBUTING.md](CONTRIBUTING.md). In short:

- work on a branch and open a pull request into `master`; never push to `master`;
- merge only when the pull request's checks are green and so is the CI of what it generates: the Samples
  `preview` branch, regenerated from the pull request's branch (`regenerate.yml` with `ref`);
- a change in a generated solution's behaviour comes with a test;
- a release is the version in `version.json` and a tag `v<version>`; it is raised by hand, there is no
  check for it.

Before a commit:

- the repository is public: no names of clients or of their products in code, comments, docs or commits;
- what a text (a doc, a comment, a commit message) says about the code is checked against the code;
- a document in `docs/` has a Russian twin `<name>.ru.md`; a change in one goes into the other, and the
  site lists the twin in `site/config.js`.

The generated solutions are checked from outside too: DotNetSolutionKit.Samples regenerates its branches
from each release, and `nightly` from `master`. A red CI there is a bug of the template.
