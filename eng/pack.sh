#!/usr/bin/env bash
set -euo pipefail

version=${1:-0.1.0-dev}
output=${2:-artifacts/packages}
if [[ ! "$version" =~ ^[0-9]+\.[0-9]+\.[0-9]+(-[0-9A-Za-z]+([.-][0-9A-Za-z]+)*)?$ ]]; then
    echo "invalid semantic version: $version" >&2
    exit 2
fi

root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
cd "$root"
output="$root/${output#./}"
case "$output" in
    "$root"/artifacts/*) ;;
    *)
        echo "package output must be under $root/artifacts" >&2
        exit 2
        ;;
esac

projects=(
    src/MonadicTypes/MonadicTypes.csproj
    src/MonadicTypes.Errors/MonadicTypes.Errors.csproj
    src/MonadicTypes.Async/MonadicTypes.Async.csproj
    src/MonadicTypes.Effects/MonadicTypes.Effects.csproj
    src/MonadicTypes.Diagnostics/MonadicTypes.Diagnostics.csproj
    src/MonadicTypes.AspNetCore/MonadicTypes.AspNetCore.csproj
    src/MonadicTypes.Generators/MonadicTypes.Generators.csproj
)

rm -rf "$output"
mkdir -p "$output"
for project in "${projects[@]}"; do
    dotnet pack "$project" -c Release --no-restore -o "$output" -p:Version="$version"
done

"$root/eng/verify-packages.sh" "$version" "$output"
