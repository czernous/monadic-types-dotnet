#!/usr/bin/env bash
set -euo pipefail

if [[ $# -ne 2 ]]; then
    echo "usage: $0 <version> <package-directory>" >&2
    exit 2
fi

version=$1
package_directory=$2
expected_ids=(
    MonadicTypes.NET
    MonadicTypes.NET.Errors
    MonadicTypes.NET.Async
    MonadicTypes.NET.Effects
    MonadicTypes.NET.Diagnostics
    MonadicTypes.NET.AspNetCore
    MonadicTypes.NET.Generators
)

package_count=$(find "$package_directory" -maxdepth 1 -name '*.nupkg' -type f | wc -l | tr -d ' ')
symbol_count=$(find "$package_directory" -maxdepth 1 -name '*.snupkg' -type f | wc -l | tr -d ' ')
expected_symbol_count=$((${#expected_ids[@]} - 1))
if [[ "$package_count" -ne ${#expected_ids[@]} || "$symbol_count" -ne "$expected_symbol_count" ]]; then
    echo "expected ${#expected_ids[@]} packages and $expected_symbol_count symbol packages; found $package_count and $symbol_count" >&2
    exit 1
fi

for id in "${expected_ids[@]}"; do
    package="$package_directory/$id.$version.nupkg"
    [[ -f "$package" ]] || { echo "missing package: $package" >&2; exit 1; }
    entries=$(unzip -Z1 "$package")
    for required in README.md LICENSE NOTICE; do
        grep -Fxq "$required" <<< "$entries" || { echo "$id does not contain $required" >&2; exit 1; }
    done

    if [[ "$id" == MonadicTypes.NET.Generators ]]; then
        grep -Fxq 'analyzers/dotnet/cs/MonadicTypes.Generators.dll' <<< "$entries" || {
            echo "generator assembly is not under analyzers/dotnet/cs" >&2
            exit 1
        }
        grep -Fxq 'analyzers/dotnet/cs/MonadicTypes.Generators.pdb' <<< "$entries" || {
            echo "generator package does not contain its portable PDB" >&2
            exit 1
        }
        if grep -q '^lib/' <<< "$entries"; then
            echo "generator package exposes an unintended runtime library" >&2
            exit 1
        fi
    else
        assembly=${id/MonadicTypes.NET/MonadicTypes}.dll
        grep -Fxq "lib/net10.0/$assembly" <<< "$entries" || {
            echo "$id does not contain lib/net10.0/$assembly" >&2
            exit 1
        }
    fi
done

echo "verified ${#expected_ids[@]} packages and symbol packages for $version"
