# Versioning And Releases

## Release Model

The repository uses one long-lived branch, `master`, and immutable annotated
Git tags. There are no release branches. Changes normally reach `master`
through a pull request. Authorized repository administrators retain an
emergency bypass, but every push still runs post-merge validation.

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

Keep `master` releasable. Publishing is separate from merging: merging to
`master` never creates a package or tag. Several changes may accumulate before
an operator starts a release.

Before releasing:

1. Confirm the latest `master` CI and CodeQL runs succeeded.
2. Confirm the intended version has not already been published or tagged.
3. Select the next version using the policy above. Never reuse a published
   version.

To release with the GitHub UI:

1. Open the repository's **Actions** page.
2. Select the **Release** workflow in the left sidebar.
3. Select **Run workflow** and choose `master`.
4. Enter a semantic version without a leading `v`, such as
   `0.1.0-preview.1`.
5. Select **Run workflow** and monitor every job until completion.

The same action can be started from an authenticated GitHub CLI:

```bash
gh workflow run release.yml --ref master -f version=0.1.0-preview.1
gh run list --workflow release.yml --limit 1
gh run watch <run-id> --exit-status
```

The version number is the only required release decision. The workflow first
publishes NativeAOT smoke applications on Windows, Linux, and macOS. It then
runs restore auditing, formatting, build, tests, package inspection, and
package-consumption NativeAOT validation. Only after those gates succeed does
it create the immutable annotated tag and publish to NuGet.org and GitHub
Packages. The GitHub Release is created after both registries accept the
packages. Creating the tag before registry publication prevents a partially
published version from existing without its source revision. Updating
`CHANGELOG.md` remains useful for curated notes but is not a mechanical release
requirement.

After a successful release, verify:

1. The workflow completed successfully.
2. The `v<version>` tag and corresponding GitHub Release exist.
3. Every expected `MonadicTypes.NET*` package appears on NuGet.org after its
   indexing delay.
4. A clean sample project can restore the version from NuGet.org.

NuGet.org temporarily displays newly uploaded packages under **Unlisted
Packages** while validation and indexing are in progress. This normally clears
within 15 minutes and is not the same as an owner deliberately unlisting a
published version. Check the package validation message and notification email;
investigate if indexing fails or remains incomplete for an hour.

If publication fails without a code or artifact change, rerun the same workflow
from the same tagged revision and version. Registry pushes are duplicate-safe,
and the workflow accepts an incomplete tag only when it resolves to the current
revision and has no GitHub Release. If code or package contents must change,
choose a new version. Never delete and reuse package versions or tags.

Run the same checks locally when investigating a release or package change:

```bash
dotnet restore MonadicTypes.slnx
dotnet build MonadicTypes.slnx -c Release --no-restore
dotnet test MonadicTypes.slnx -c Release --no-build --no-restore
eng/tools/linux-x64/mt-pack 0.1.0-preview.1
eng/tools/linux-x64/mt-test-packages 0.1.0-preview.1 linux-x64
```

The workflow performs formatting, restore auditing, build, tests, package
inspection, and package-based NativeAOT publication. It creates attested
`.nupkg` and `.snupkg` artifacts, a GitHub Release, and publishes the NuGet
packages to GitHub Packages. Timing benchmarks remain a deliberate
stable-machine check and do not run as a hosted release gate.

## GitHub Packages

Every release publishes automatically to:

```text
https://nuget.pkg.github.com/<package-owner>/index.json
```

The workflow uses its short-lived `GITHUB_TOKEN`; no publishing secret is
stored. GitHub Packages requires authentication for installation, including for
public packages. A consumer should create a classic personal access token with
`read:packages`, expose it only to its local or CI credential store, and add the
source:

```bash
dotnet nuget add source \
  --username <package-owner> \
  --password "$GITHUB_PACKAGES_TOKEN" \
  --store-password-in-clear-text \
  --name monadic-types-github \
  https://nuget.pkg.github.com/<package-owner>/index.json
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

PRs run locked audited restore, formatting, and affected-project build/tests on
Linux. A changed project causes all reverse dependants to build; repository-wide
compiler, package, analyzer, and engineering files invalidate the full graph.
PRs also run dependency and license review.

Relevant PRs run package creation and consumption before merge. After merge,
`master` runs the affected Windows and Linux graph plus cross-platform NativeAOT
where required. CodeQL runs on `master` and weekly. This avoids paying twice for
the same package validation while keeping PR feedback fast and making the
releasable branch the authoritative integration boundary.

The `master` ruleset requires a pull request and the PR validation checks.
Repository administrators have bypass permission for exceptional direct pushes;
using that bypass still triggers the complete post-push `master` validation.

## Dependency Security

NuGet dependencies are centrally pinned and lock files are committed. CI restore
uses locked mode, audits direct and transitive dependencies, and treats moderate
or higher advisories as build failures. A file-based C# verifier rejects
runtime-specific targets in shipping lock files before restore; explicit RID
restores suppress lock writing for shipping projects. Dependabot checks NuGet and GitHub Action
versions weekly. Packages that define the explicit FluentValidation and OpenAPI
compatibility matrix are excluded from bot updates; changing those pins requires
the compatibility, trimming, AOT, and benchmark review described in the
compatibility contract. Vulnerability auditing still covers ignored pins.
Dependency review rejects vulnerable additions and licenses that conflict with
the repository's intended commercial use.
