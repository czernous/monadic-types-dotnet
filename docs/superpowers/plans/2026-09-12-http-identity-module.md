# HTTP Identity Module Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Centralize status-specific Problem Details identity generation internally while preserving all existing ASP.NET behavior, allocation characteristics, and public seams.

**Architecture:** Extract status title and HTTP problem-type URI policy from `ErrorProblemDetails` into one internal `HttpStatusIdentity` module. Category-to-status and category-specific identities remain in the existing error policy because they intentionally differ from explicit HTTP identities. No public mapper or status-registry abstraction is added.

**Tech Stack:** C#/.NET 10, ASP.NET Core `ProblemDetails`, OpenAPI metadata, xUnit, NativeAOT smoke applications.

**Spec:** `docs/superpowers/specs/2026-09-12-telemetry-http-boundaries-design.md`

## Global Constraints

- Accept every explicit HTTP status from 400 through 599 and reject all other ranges.
- Preserve registered titles, generic `HTTP error` fallback, and `urn:problem-type:http-{status}` identities.
- Preserve category defaults, custom numeric HTTP categories, visibility, trace IDs, and endpoint headers.
- Keep status-specific URI generation lazy and bounded; do not add request-path allocation.
- Keep the public ASP.NET/OpenAPI interfaces unchanged and reflection-free.

---

### Task 1: Add invariant coverage at the public ASP.NET seam

**Files:**
- Modify: `tests/MonadicTypes.AspNetCore.Tests/ErrorProblemDetailsTests.cs`

**Interfaces:**
- Consumes: `ErrorProblemDetails.CreateExample`, `GetStatusCode`, and `ErrorType`.
- Produces: exhaustive category identity assertions that fail if a category loses a mapping.

- [ ] **Step 1: Write the invariant test**

Add a public-seam test that enumerates every initialized built-in category except `Custom`, constructs an initialized error, and asserts a status from 400 through 599 plus non-empty title and type. Keep the existing exhaustive 400–599 and unknown-status tests unchanged.

- [ ] **Step 2: Run the targeted test**

```powershell
dotnet test tests\MonadicTypes.AspNetCore.Tests\MonadicTypes.AspNetCore.Tests.csproj -c Release --no-restore --filter FullyQualifiedName~ErrorProblemDetailsTests
```

Expected: the invariant may pass immediately because current behavior is already covered; record that fact and do not weaken the test to inspect private implementation.

### Task 2: Extract the internal status identity implementation

**Files:**
- Create: `src/MonadicTypes.AspNetCore/HttpStatusIdentity.cs`
- Modify: `src/MonadicTypes.AspNetCore/ErrorProblemDetails.cs`

**Interfaces:**
- Consumes: validated status codes from `ErrorProblemDetails`.
- Produces: internal title and URI lookup used by explicit HTTP Problem Details.

- [ ] **Step 1: Move status-specific policy without changing output**

Move the known status-title switch and bounded lazy URI cache into the internal module. Keep an explicit type initializer so URI cache initialization remains cold-path-only. Return the existing generic `HTTP error` title for unregistered statuses.

- [ ] **Step 2: Delegate from `ErrorProblemDetails`**

Replace only the private status-title and URI calls in `CreateCore`. Leave category status mapping, category titles/URIs, visibility, trace handling, validation, and typed-result construction in their existing module.

- [ ] **Step 3: Run the targeted ASP.NET tests**

Run the command from Task 1. Expected: all existing and new tests pass with identical status, title, type, detail, code, and trace outputs.

### Task 3: Verify AOT, package, and performance contracts

**Files:**
- Modify: `docs/package-readmes/MonadicTypes.AspNetCore.md` only if internal ownership clarification improves consumer guidance.
- Regenerate: generated API outputs only if XML comments changed.

- [ ] **Step 1: Run full solution tests and formatting**

```powershell
dotnet test MonadicTypes.slnx -c Release --no-restore
dotnet format whitespace MonadicTypes.slnx --verify-no-changes --no-restore
```

- [ ] **Step 2: Run documentation verification**

```powershell
dotnet run --project eng\MonadicTypes.Docs.Tool\MonadicTypes.Docs.Tool.csproj -c Release --no-restore verify .
```

- [ ] **Step 3: Run package inspection and package NativeAOT consumption**

```powershell
eng\tools\win-x64\mt-pack.exe 0.3.0-preview.2
eng\tools\win-x64\mt-test-packages.exe 0.3.0-preview.2 win-x64
```

- [ ] **Step 4: Run the existing ASP.NET benchmark families**

Confirm status override and catalog metadata paths preserve established allocation/timing baselines. Do not relax or replace an accepted baseline.

- [ ] **Step 5: Commit the slice**

```powershell
git add src/MonadicTypes.AspNetCore/HttpStatusIdentity.cs src/MonadicTypes.AspNetCore/ErrorProblemDetails.cs tests/MonadicTypes.AspNetCore.Tests/ErrorProblemDetailsTests.cs
git commit -m "refactor: centralize HTTP problem identity"
```
