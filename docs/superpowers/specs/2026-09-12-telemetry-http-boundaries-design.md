# Telemetry and HTTP Boundary Deepening

## Problem

The library already supplies the intended production seams: typed railway
composition in the core, structured errors, explicit diagnostics projection,
ASP.NET Problem Details, error catalogs, and reflection-free OpenAPI metadata.
The release review identified two places where those seams should be made more
explicit and less vulnerable to policy drift:

1. Diagnostic messages are currently exported to `Activity` regardless of the
   transport visibility flag, which makes a safe production default unclear.
2. HTTP status identity is implemented across the ASP.NET boundary and OpenAPI
   path, so status/title/type consistency needs one internal source and public
   invariant coverage without adding a consumer-facing abstraction.

## User stories

1. As an application maintainer, I want telemetry recording to avoid exporting
   private diagnostic messages by default, so accidental backend disclosure is
   less likely.
2. As an application maintainer, I want an explicit opt-in for trusted internal
   telemetry that includes private diagnostics, so useful investigation detail
   remains available when policy permits it.
3. As an application maintainer, I want retained exception events and original
   stack traces preserved, so changing message disclosure does not weaken
   failure diagnosis or rethrow behavior.
4. As an ASP.NET consumer, I want category defaults and explicit 400–599 status
   overrides to produce deterministic status titles and problem-type identities,
   so runtime responses and generated documentation cannot silently diverge.
5. As a NativeAOT consumer, I want both changes to remain reflection-free and
   allocation-neutral on disabled and steady-state paths, so the release keeps
   its existing performance contract.

## Design

### Telemetry policy

Add a small public `ErrorTelemetryMessagePolicy` enum with three values:

- `PublicOnly`: include `error.message` only when `Error.IsMessagePublic` is
  true; this is the default policy.
- `Include`: include the diagnostic message regardless of transport visibility.
- `Omit`: do not add the diagnostic message tag.

Keep the existing three-argument `ErrorTelemetry.Record` overload for source and
binary compatibility. Its implementation delegates to a four-argument
overload using `PublicOnly`. The four-argument overload accepts the existing
activity-status policy plus the message policy.

Message policy changes only the domain `error.message` tag. Retained causes
continue to use the existing `Activity.AddException` path, preserving exception
type, message, and stack information. This keeps the stack-preservation
contract and avoids adding a second cause/event policy dimension.

The policy branch must execute only after the existing null/activity sampling
fast path. It must not allocate strings, closures, or policy objects. Invalid
enum values throw `ArgumentOutOfRangeException` only when the activity is
sampled, matching the existing validation behavior.

### HTTP identity module

Introduce an internal status-identity module inside
`MonadicTypes.AspNetCore`. It owns status-specific titles and problem-type URI
generation, retaining the bounded lazy URI cache and generic title for unknown
extension statuses. `ErrorProblemDetails` delegates to this module; no public
HTTP mapping interface or new package is introduced.

Category-to-status and category-specific problem identities remain explicit
because category defaults intentionally differ from explicit HTTP override
identities. The module must preserve:

- every accepted 400–599 explicit status;
- deterministic `urn:problem-type:http-{status}` identities;
- registered titles for known statuses and `HTTP error` for unknown ones;
- existing category defaults and custom numeric HTTP categories;
- no success or redirect statuses in the error adapter.

## Testing

- Add public `ErrorTelemetry` tests for default `PublicOnly`, public-message
  inclusion, private-message opt-in, omission, invalid policy, and unchanged
  retained-exception events.
- Add or retain allocation coverage for disabled/unsampled telemetry and the
  existing diagnostics benchmark family; no accepted baseline may be relaxed.
- Add public ASP.NET tests that enumerate all initialized `ErrorType` values and
  verify a non-empty status/title/type identity, while retaining the exhaustive
  explicit 400–599 and unknown-status tests.
- Run generated documentation verification because the new public enum and
  overload are compiler-documentation contracts.
- Run the full solution tests, formatting, package inspection, package
  consumption NativeAOT smoke, and the existing local benchmark checks.

## Out of scope

- No aggregate-error or reason-tree model.
- No public HTTP status registry or new mapper abstraction.
- No logging dependency, exporter integration, or automatic telemetry.
- No target-framework expansion.
- No SIMD, unsafe code, flyweight caching, or relaxed timing/allocation target.
