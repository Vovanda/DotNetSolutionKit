# Working with AI agents

In most tasks there is no weighty reason to refuse AI tools. With them or without them, the one doing the
work answers for the quality and the purpose of the result: that responsibility cannot be passed to a tool,
and should not be.

The template was written with an AI agent on the Claude Opus model. There is no attribution or co-authorship
of the model in the commits or in the files: the model is not the author of the project's decisions. It is
used as a tool, a high-level compiler and generator: the author makes a decision, the model turns it into
code, the author checks the result and answers for it. Neither an operating system nor a compiler that
generates optimal code is named as a co-author, and the model is in the same role here.

The template can generate a solution that is also set up for work with AI agents. Its rules and skills are
written to keep the agent as close as possible to the task it was given, but they do not guarantee that the
agent writes everything right and does not damage the architecture. Do not take the agent's decisions on
trust: check what can be checked formally with tests, and the rest by eye. The template works without
agents as well.

## What the solution gets

A generated solution gets rules for the agent and skills. `--Agent` chooses their format:

| `--Agent` | Rules | Skills | Plan of long work |
|---|---|---|---|
| `claude`, the default | `CLAUDE.md` | `.claude/skills/` | `.claude/session-context/` |
| `opencode` | `AGENTS.md` | `.opencode/skills/` | `.opencode/session-context/` |
| `none` | - | - | - |

The content is the same in both formats; the names of the files and folders differ.

The rules hold the layers of the solution, what lives where and how to check a change. The skills, one per
kind of work:

| Skill | Work |
|---|---|
| `scaffold-service` | a new service |
| `add-entity` | an entity or an aggregate |
| `add-repository` | a repository |
| `add-specification` | a query condition and search |
| `add-domain-event` | a domain event and its handlers |
| `add-service-class` | an application service |
| `add-controller` | a controller |
| `add-tests` | the tests of a service |
| `ef-migration` | a migration of a service |
| `pick-pattern` | choosing a pattern before the code |
| `self-review` | a check of your own commits |
| `code-review` | a review of someone else's pull request |
| `plan-and-iterate` | the plan of long work |

The skills are the author's own. Their rules grew out of real cases on products built on the template and
were corrected by them, so many rules say which mistake they prevent.

## How to use them

The agent reads the rules file itself. A skill is called by its name or picked up when the task matches its
description. A skill goes step by step and points to the documentation of the mechanism, so the agent does
the work the way this site describes it.

Run long work with `plan-and-iterate`: the agent keeps the plan in a file and does not lose it between
sessions.
