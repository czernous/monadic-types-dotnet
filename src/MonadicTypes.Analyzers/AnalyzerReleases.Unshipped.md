; Unshipped analyzer release

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|------
MT0001 | MonadicTypes.Usage | Warning | Reports `Option<T> == null` and `Option<T> != null` comparisons.
MT0002 | MonadicTypes.Usage | Warning | Reports `Map`/`MapError` projections that produce nested `Result` or `Option` values.
