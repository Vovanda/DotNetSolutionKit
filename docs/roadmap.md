# Roadmap

Releases done, the step in progress and the planned steps, in order. The axis counts steps, not dates; a longer bar is a larger step, and bars side by side can go in parallel.

```mermaid
gantt
    dateFormat X
    axisFormat %s
    tickInterval 1second

    section Done
    v1.0 the template before 2026                         :done, s1, 0, 3000ms
    v2.0 layered services, shared host, flags             :done, s2, after s1, 3000ms
    v2.1 deployment, API gateway, CI, correlation         :done, s3, after s2, 2000ms
    v2.2 idempotent commands                              :done, s4, after s3, 500ms
    v2.3 surviving a secret store outage                  :done, s5, after s4, 500ms
    v2.4 tokens in cookies, CSRF                          :done, s6, after s5, 500ms
    v2.5 audit journal                                    :done, s7, after s6, 1000ms
    v2.6 NUnit or xUnit                                   :done, s8, after s7, 1000ms

    section Now
    v2.7 site, samples, nuget.org, major check, gateway (#5), Vault, settings reloaded, SQL Server :done, s9, after s8, 4000ms
    shipped workflows run in the template's CI; work in dev, master releases :done, s10, after s8, 1000ms
    gateway Swagger like a product's, open bugs closed :done, s11, after s10, 2000ms
    rules and skills for an AI agent, Claude Code or OpenCode (--Agent) :done, s12, after s11, 1000ms
    a marketplace on the template, end to end; what it finds is fixed :active, s13, after s12, 2000ms
    a solution's coverage on demand, a report per service (#65) :done, s14, after s9, 1000ms
    a page on what the template gives: principles, economics, price :done, s16, after s14, 500ms
    the map of what a solution gets knows every flag of 2.7 :done, s17, after s16, 300ms
    tests carry one set of attributes on NUnit and xUnit, no #if :done, s18, after s17, 500ms
    the database is chosen in one place, DatabaseProvider :done, s19, after s18, 500ms
    Samples full-alt: SQL Server, xUnit, Vault, Kubernetes, audit :done, s20, after s19, 300ms

    section Next
    3.0 on .NET 10 in master, with the path from 2.x; 2.x stays on .NET 8 :s15, after s9, 3000ms
```

What each release brought: [version.json](https://github.com/sawking-tech/DotNetSolutionKit/blob/master/version.json).
Work goes into `dev`, and a release reaches `master` about once a week ([CONTRIBUTING](https://github.com/sawking-tech/DotNetSolutionKit/blob/master/CONTRIBUTING.md)).
Everything in Now goes into 2.x, on .NET 8, before 3.0, which is planned for the week after: support for .NET 8
ends on 10 November 2026, and from 3.0 on the 2.x branch takes fixes only.

## Reducing lock-in

The defaults are the author's preferences; a team with its own standard should be able to replace them.

| Technology | Where the template depends on it now |
|---|---|
| PostgreSQL | none: `--Database mssql` generates the solution on SQL Server, each of these behind the same seam, see [persistence](architecture/persistence.md#sql-server) |
| Infisical | none: `--Vault` reads the same way from HashiCorp Vault, through the same `ISecretStore` port, see [secrets](features/secrets.md#hashicorp-vault) |
| GitHub CI | only the generated workflow; the checks are scripts any CI can call |
