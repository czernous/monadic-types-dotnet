using MonadicTypes;
using MonadicTypes.Collections;
using MonadicTypes.Linq;

IReadOnlyList<int> values = new[] { 1, 2, 3 };
Result<int[], string> traversed = values.TraverseToArray(
    static value => Result<int, string>.Ok(value + 1));
Result<int, string> queried = traversed
    .Select(static items => items[0])
    .SelectMany(
        static value => Result<int, string>.Ok(value + 1),
        static (left, right) => left + right);

return queried.Match(
    static value => value is 5 ? 0 : 1,
    static _ => 1);
