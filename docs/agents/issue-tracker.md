# Issue tracker

Use GitHub issues and pull requests in
[czernous/monadic-types-dotnet](https://github.com/czernous/monadic-types-dotnet).
The default branch is `master`. Use `gh` from the clone and confirm the remote
before external writes.

## Assessment

```powershell
gh issue list --state open --limit 100 --json number,title,labels,url
gh issue view <number> --comments
gh pr view <number>
gh pr diff <number>
gh pr checks <number>
```

Issues and PRs share a number space; resolve the object before acting. Read the
discussion and compare the report with current source, documented contracts,
and the reported package version. Distinguish reproduced defects from guidance
gaps, enhancements, and unresolved design decisions.

A buildable issue needs a minimal example or evidence, expected behavior, scope,
dependencies on unresolved decisions, and verifiable acceptance criteria. PRs
are implementation/review artifacts by default, not automatically approved
feature requests. Link the relevant issue and describe the final change and
verification in the PR body.

## Labels and readiness

Preserve existing labels: `bug` for verified defects, `enhancement` for agreed
additions, `documentation` for guidance, and `question` for unresolved questions.
Retain dependency automation's labels. These classify work; they do not by
themselves establish readiness or authorize implementation.

Where no readiness label exists, record unresolved decisions and acceptance
criteria in the issue or local handoff. Do not equate `help wanted` with agent
readiness or `good first issue` with human ownership. Use `duplicate`, `invalid`,
or `wontfix` only with evidence and authority to change the issue's disposition.

No repository-installed triage skill or dedicated triage state-label mapping is
currently configured. Session-provided skills can assist assessment without
being installed here. Configure a dedicated triage-label file when the repository
adopts that workflow; preserve existing labels when mapping roles.

## Writes and completion

Follow the user's established scope for external changes. Inspection authorizes
reading, not comments, relabelling, closing, merging, or publishing. Skill
instructions do not expand that authority. Send comments or other messages only
when explicitly authorized.

For multiline issue or PR text, use a structured argument or an exact UTF-8
temporary file with `--body-file`; do not interpolate prose into shell code.

Close an issue when its acceptance criteria are met and the change is integrated,
or an authorized disposition resolves it. A local patch alone is not an integrated
fix. Record verification and remaining follow-up without implying unrun checks
passed.
