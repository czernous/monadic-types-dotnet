#!/usr/bin/env bash
set -euo pipefail

if [[ $# -lt 1 || $# -gt 2 ]]; then
    echo "usage: $0 <version> [runtime-identifier]" >&2
    exit 2
fi

version=$1
rid=${2:-}
root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
# Git for Windows otherwise rewrites HTTPS arguments as local MSYS paths.
export MSYS2_ARG_CONV_EXCL="https://*"
project="$root/tests/MonadicTypes.PackageSmoke/MonadicTypes.PackageSmoke.csproj"
packages="$root/artifacts/packages"
output="$root/artifacts/package-smoke${rid:+/$rid}"

restore_args=(
    "$project"
    -p:PackageVersionToTest="$version"
    --source "$packages"
    --source https://api.nuget.org/v3/index.json
)
publish_args=(
    "$project"
    -c Release
    --no-restore
    -p:PackageVersionToTest="$version"
    -o "$output"
)
if [[ -n "$rid" ]]; then
    restore_args+=(-r "$rid")
    publish_args+=(-r "$rid")
fi

dotnet restore "${restore_args[@]}"
dotnet publish "${publish_args[@]}"
