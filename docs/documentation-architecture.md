# Documentation Architecture

Documentation has two different responsibilities and must not duplicate either
one by hand:

- Compiler XML is the canonical public API contract.
- Reviewed Markdown templates own concepts, usage guidance, trade-offs, and
  application examples.

Generated reference material must never become the source for XML comments, and
conceptual prose must not attempt to maintain a second exhaustive member list.

## Generated Outputs

A repository-owned `MonadicTypes.Docs.Tool` will consume Release assemblies and
their compiler XML files without loading assemblies through runtime reflection.
It will use `System.Reflection.Metadata` for signatures and XML parsing for
documentation, then deterministically emit:

- one API reference page per package;
- a machine-readable navigation and search manifest;
- API inventory blocks embedded between generated markers in the root README;
- API inventory blocks embedded in each NuGet package README;
- a report for undocumented, unresolved, or orphaned members and links.

Output must contain no timestamps, absolute paths, machine names, generated
temporary names, or environment-specific ordering. The tool will support:

- `generate`, which updates committed outputs;
- `verify`, which generates in memory and fails when committed output differs;
- `manifest`, which emits the static-site input without rewriting prose.

`verify` belongs in pull-request and release validation. Package creation must
depend on successful verification so NuGet cannot publish stale member lists.

## Manual Outputs

These remain reviewed Markdown:

- project orientation and package selection;
- pipeline composition and application examples;
- behavioral explanations that span several members;
- performance policy and accepted benchmark evidence;
- compatibility, release, dependency, and contribution policy.

Templates may include generated regions, but generators must preserve all text
outside explicit markers. Examples should eventually become compilable example
projects or extracted compile-smoke snippets instead of unchecked code fences.

## Site Layer

The documentation site is deferred until generated reference output and manual
templates pass the same verification gate. Bun can then provide the static HTML,
TypeScript, CSS, development server, and production bundling layer. It must
consume generated Markdown and the navigation/search manifest rather than parse
.NET binaries itself.

The site build will be deterministic and deploy only static output to GitHub
Pages. It must add no runtime dependency to any NuGet package and must not be a
prerequisite for local library builds, tests, NativeAOT checks, or packaging.

## Delivery Order

1. Complete the current API and XML documentation audit.
2. Implement metadata/XML extraction and deterministic member identities.
3. Generate package reference pages and README inventory regions.
4. Add `verify` to CI and package validation.
5. Convert representative code fences to compile-smoke examples.
6. Build the Bun static site and GitHub Pages workflow.

The site is presentation. The generated API model and verification gate are the
durable documentation infrastructure.
