# Repository guidance

MonadicTypes.NET is an experimental .NET library for the maintainer's applications.
Keep its API small and driven by demonstrated application needs. Preserve explicit
package boundaries, NativeAOT compatibility, and measured performance contracts.

## Before changing code

- Read [development policy](docs/development.md) for toolchain, formatting,
  analyzers, lockfiles, and native tooling requirements.
- Inspect the working tree and preserve unrelated changes. Read the relevant
  issue, source, and tests before treating a reported behavior as a defect.
- Read [domain guidance](docs/agents/domain.md) for contracts and context pointers.
- For dependency changes, read [dependency policy](docs/dependency-policy.md).
  Update dependencies and portable lockfiles together; keep CI locked restores.
- For public API or documentation changes, read
  [documentation architecture](docs/documentation-architecture.md). Compiler XML
  owns API contracts. Regenerate API outputs; do not edit generated regions by hand.
- For packaging or publishing work, read [release policy](docs/releases.md).

## Verification

Use the commands in the development policy. For behavior changes, reproduce the
reported case and run the relevant tests. Add regression coverage for the public
behavior, including natural consumer syntax when overload inference is involved.
Verify affected builds with analyzers enabled. Public API changes also require
documentation regeneration and verification; packaging, NativeAOT, and performance
changes require their corresponding documented checks.

Changes to native tooling source require regeneration of the affected committed
Windows and Linux executables as described in the development policy. Report any
host limitation explicitly. Do not claim completion from source tests alone when
required generated outputs or platform checks remain outstanding.

In the handoff, state what changed, which checks ran and their results, and any
remaining limitations. Keep issue completion tied to its acceptance criteria.

Existing allocation and timing targets are acceptance constraints. Record a
failed candidate and investigate its cause; do not relax a target or replace a
baseline to accept the change. New APIs need comparable controls and documented
targets before their measurements are accepted. Disclose cold initialization,
owned-output allocations, and unverified platforms separately from hot-path
claims. Release readiness requires the established gates, not just passing tests.

## Agent skills

Use the narrowest applicable skill available in the current session and read its
instructions before applying it. The names below refer to optional installed
skills, not repository scripts. If unavailable, follow the repository policies
directly and disclose any material capability gap. Agents may recommend skills
to install when they would materially help: name the skill and its source,
explain the task-specific benefit and any overlap with existing skills, and
request installation approval unless already authorized. A recommendation does
not itself authorize installation. Explicit user instructions take precedence.

| Task | Skill | Boundary |
| --- | --- | --- |
| Assess an incoming issue | `matt-skills-curated:triage` | Validate the report and scope before prescribing a fix. |
| Diagnose broken behavior or CI | `matt-skills-curated:diagnosing-bugs` | Establish the cause before implementation. |
| Clarify an unresolved API decision | `matt-skills-curated:grill-me` | Use when a consequential decision needs user input, not for routine implementation choices. |
| Record agreed terminology or architecture | `matt-skills-curated:domain-modeling` | Capture settled contracts and decisions. |
| Implement an agreed change | `matt-skills-curated:implement` | Use a concrete issue or specification with acceptance criteria. |
| Add behavior or fix a regression | `matt-skills-curated:tdd` | Exercise public behavior; avoid tests that only mirror implementation. |
| Review a diff or PR | `matt-skills-curated:code-review` | Review scope, contracts, and verification evidence. |
| Change agent instructions | `matt-skills-curated:writing-for-agents` | Keep shared instructions short and link task-specific policy. |
| Change tracker/workflow conventions | `matt-skills-curated:setup-engineering-workflows` | Inspect existing conventions before modifying them. |

Avoid stacking equivalent skills for the same purpose unless the task needs both.
Delegation is optional; this guide does not require subagents.

### Issue tracker

Use GitHub issues and pull requests for `czernous/monadic-types-dotnet`.
See [issue-tracker conventions](docs/agents/issue-tracker.md).

### Domain docs

Use one shared domain context across the solution's feature packages.
See [domain documentation rules](docs/agents/domain.md).

Maintain this file as the canonical repository agent entry point; do not add a
second parallel `CLAUDE.md` instruction file.
