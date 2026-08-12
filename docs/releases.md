# Versioning And Releases

## Release Model

The repository uses one long-lived branch, `master`, and immutable annotated
Git tags. There are no release branches. Direct pushes to `master` remain
possible while the project has one maintainer; CI reports failures but branch
protection does not currently prohibit a push.

A release tag has one of these forms:

```text
v0.1.0
v0.2.0-preview.1
v1.0.1
```

`Directory.Build.props` supplies `0.1.0-dev` to ordinary local builds. Release
artifacts always receive the exact version requested by the release workflow or
the version in a manually pushed tag.

## Semantic Versioning

Before `1.0.0`, the API is explicitly being exercised in real applications:

- increment the patch for compatible fixes and documentation;
- increment the minor version for compatible features and intentional breaking
  API changes; and
- use `-preview.N` when feedback may still change the package contract.

At and after `1.0.0`:

- patch releases contain backward-compatible fixes;
- minor releases contain backward-compatible functionality; and
- major releases may contain breaking public API or behavioral changes.

Package versions and tags are immutable. Never delete and reuse a published tag
or NuGet version. Correct a bad release with a new version; a severely broken
NuGet package may additionally be unlisted.

## Creating A Release

Keep `master` releasable. The shortest deliberate release procedure is:

1. Open GitHub Actions, select `Release`, choose `master`, and enter a semantic
   version such as `0.1.0-preview.1`.
2. Run the workflow.

The same action can be started from an authenticated GitHub CLI:

```bash
gh workflow run release.yml --ref master -f version=0.1.0-preview.1
```

The version number is the only required release decision. The workflow runs all
gates, creates and pushes the annotated tag, publishes to NuGet.org and GitHub
Packages, and then creates the GitHub Release with generated notes. Updating
`CHANGELOG.md` remains useful for curated notes but is not a mechanical release
requirement.

Run the same checks locally when investigating a release or package change:

```bash
dotnet restore MonadicTypes.slnx
dotnet build MonadicTypes.slnx -c Release --no-restore
dotnet test MonadicTypes.slnx -c Release --no-build --no-restore
./eng/pack.sh 0.1.0-preview.1
./eng/test-packages.sh 0.1.0-preview.1
```

The workflow performs formatting, restore auditing, build, tests, package
inspection, and package-based NativeAOT publication. It creates attested
`.nupkg` and `.snupkg` artifacts, a GitHub Release, and publishes the NuGet
packages to GitHub Packages. Timing benchmarks remain a deliberate
stable-machine check and do not run as a hosted release gate.

## GitHub Packages

Every release publishes automatically to:

```text
https://nuget.pkg.github.com/czernous/index.json
```

The workflow uses its short-lived `GITHUB_TOKEN`; no publishing secret is
stored. GitHub Packages requires authentication for installation, including for
public packages. A consumer should create a classic personal access token with
`read:packages`, expose it only to its local or CI credential store, and add the
source:

```bash
dotnet nuget add source \
  --username czernous \
  --password "$GITHUB_PACKAGES_TOKEN" \
  --store-password-in-clear-text \
  --name monadic-types-github \
  https://nuget.pkg.github.com/czernous/index.json
```

Do not commit the token. When both GitHub Packages and nuget.org are configured,
use NuGet package-source mapping so only `MonadicTypes.NET*` resolves from the
GitHub feed and all other packages resolve from nuget.org. This prevents source
confusion and avoids authentication failures affecting unrelated restores.

## NuGet Publishing

NuGet.org is the canonical public distribution channel and every release
publishes there. Before the first release, configure the repository variable:

- `NUGET_USER=<nuget.org profile name>`

On nuget.org, create a trusted-publishing policy for the GitHub owner that owns
this repository, repository `monadic-types-dotnet`, workflow `release.yml`, and
GitHub environment `release`.
This uses a short-lived OIDC credential and requires no stored API key. Confirm
ownership and availability of every `MonadicTypes.NET*` package ID before the
first release. If trusted publishing is not configured, the release fails rather
than silently creating a GitHub-only release.

## Pull Requests And Future Protection

PRs and direct pushes run the same affected-project graph. A changed project
causes its tests and all reverse dependants to build; repository-wide compiler,
package, analyzer, and engineering files invalidate the full graph. PRs also run
dependency review. CodeQL runs for PRs, `master`, and on a weekly schedule.

When direct pushes are eventually disabled, require the existing CI, CodeQL,
dependency-review, package, and NativeAOT checks in a repository ruleset. No
release branches or workflow redesign are needed.

## Dependency Security

NuGet dependencies are centrally pinned and lock files are committed. CI restore
uses locked mode, audits direct and transitive dependencies, and treats moderate
or higher advisories as build failures. Dependabot checks NuGet and GitHub Action
versions weekly. Packages that define the explicit FluentValidation and OpenAPI
compatibility matrix are excluded from bot updates; changing those pins requires
the compatibility, trimming, AOT, and benchmark review described in the
compatibility contract. Vulnerability auditing still covers ignored pins.
Dependency review rejects vulnerable additions and licenses that conflict with
the repository's intended commercial use.
