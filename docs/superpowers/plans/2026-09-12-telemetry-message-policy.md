# Telemetry Message Policy Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make diagnostic-message telemetry secure by default while preserving retained exception events, stack traces, and existing disabled-path performance.

**Architecture:** Add one small public policy enum and preserve the existing three-argument `ErrorTelemetry.Record` seam. The existing overload delegates to a new four-argument overload using `PublicOnly`; only the domain `error.message` tag changes by policy. Cause events remain on the existing `Activity.AddException` path.

**Tech Stack:** C#/.NET 10, `System.Diagnostics.Activity`, xUnit, generated XML/API documentation, BenchmarkDotNet consumer benchmarks.

**Spec:** `docs/superpowers/specs/2026-09-12-telemetry-http-boundaries-design.md`

## Global Constraints

- Keep `PublicOnly` as the default policy.
- Preserve the existing three-argument overload and its binary-compatible signature.
- Preserve cause event/type/message/stack recording.
- Do not allocate on absent/unsampled activity paths or steady-state policy branches.
- Keep telemetry optional, explicit, reflection-free, trimming-safe, and NativeAOT-compatible.
- Do not add exporter, logging, DI, or aggregate-error dependencies.

---

### Task 1: Lock the policy contract with public tests

**Files:**
- Modify: `tests/MonadicTypes.Diagnostics.Tests/ErrorTelemetryTests.cs`

**Interfaces:**
- Consumes: existing `ErrorTelemetry.Record(Activity?, Error?, ErrorActivityStatusPolicy)`.
- Produces: failing assertions for `ErrorTelemetryMessagePolicy` and the four-argument overload.

- [ ] **Step 1: Write the failing tests**

Add public `Activity` seam tests for default `PublicOnly`, public-message inclusion, private-message opt-in with `Include`, `Omit`, invalid enum values, and unchanged retained-exception events. Use the existing sampled-activity fixture pattern and assert tags/events, never private helpers.

- [ ] **Step 2: Run the targeted test to verify it fails**

```powershell
dotnet test tests\MonadicTypes.Diagnostics.Tests\MonadicTypes.Diagnostics.Tests.csproj -c Release --no-restore --filter FullyQualifiedName~ErrorTelemetryTests
```

Expected: compilation fails because the enum and four-argument overload do not yet exist.

### Task 2: Implement the minimal policy seam

**Files:**
- Create: `src/MonadicTypes.Diagnostics/ErrorTelemetryMessagePolicy.cs`
- Modify: `src/MonadicTypes.Diagnostics/ErrorTelemetry.cs`

**Interfaces:**
- Consumes: `Error.IsMessagePublic`, `ErrorActivityStatusPolicy`, existing `Activity` projection.
- Produces: public `ErrorTelemetryMessagePolicy` and four-argument `Record` overload.

- [ ] **Step 1: Add the enum**

Create a documented `byte` enum with exactly `PublicOnly`, `Include`, and `Omit`. Include XML examples so the generated API reference remains complete.

- [ ] **Step 2: Preserve the existing overload**

Keep the current three-argument signature and delegate to the four-argument overload with `ErrorTelemetryMessagePolicy.PublicOnly`.

- [ ] **Step 3: Add the policy overload**

Retain the existing null/activity sampling fast path before policy validation. Add `error.message` only for `Include`, or for `PublicOnly` when `error.IsMessagePublic` is true. Keep the existing cause/event and status-policy logic unchanged. Throw `ArgumentOutOfRangeException` for invalid message policies only after the sampled activity and non-null error checks.

- [ ] **Step 4: Run the targeted tests**

Run the command from Task 1. Expected: all telemetry tests pass.

### Task 3: Update generated contracts and verify performance

**Files:**
- Modify: `src/MonadicTypes.Diagnostics/ErrorTelemetry.cs` XML comments as needed.
- Modify: `docs/package-readmes/MonadicTypes.Diagnostics.md`.
- Modify: `README.md`.
- Regenerate: `docs/api-reference.md` and API inventory regions.

- [ ] **Step 1: Document the secure default and cause behavior**

State that `PublicOnly` is the default, `Include` is an explicit trusted-backend choice, `Omit` removes only the domain message tag, and retained causes remain recorded through the existing exception event.

- [ ] **Step 2: Regenerate and verify documentation**

```powershell
dotnet run --project eng\MonadicTypes.Docs.Tool\MonadicTypes.Docs.Tool.csproj -c Release --no-restore generate .
dotnet run --project eng\MonadicTypes.Docs.Tool\MonadicTypes.Docs.Tool.csproj -c Release --no-restore verify .
```

- [ ] **Step 3: Verify allocation behavior**

Run the existing diagnostics benchmark family and confirm disabled, unsampled, and steady-state recording retain established baselines. Do not update a baseline to accept a regression.

- [ ] **Step 4: Commit the slice**

```powershell
git add src/MonadicTypes.Diagnostics/ErrorTelemetry.cs src/MonadicTypes.Diagnostics/ErrorTelemetryMessagePolicy.cs tests/MonadicTypes.Diagnostics.Tests/ErrorTelemetryTests.cs README.md docs/package-readmes/MonadicTypes.Diagnostics.md docs/api-reference.md docs/api-manifest.json
git commit -m "feat: add secure telemetry message policy"
```
