# DotNetSolutionKit

A `dotnet new` template. The template itself is `template/`; everything else in the repository (the docs,
the site, the workflows at the root) is about it and is not packed.

Branches, pull requests and releases: [CONTRIBUTING.md](CONTRIBUTING.md). A version is raised only by a
release, when the author says so.

Before a commit:

- the repository is public: no names of clients or of their products in code, comments, docs or commits;
- what a text (a doc, a comment, a commit message) says about the code is checked against the code;
- a document in `docs/` has a Russian twin `<name>.ru.md`; a change in one goes into the other, and the
  site lists the twin in `site/config.js`.
