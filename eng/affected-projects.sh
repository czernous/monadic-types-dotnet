#!/usr/bin/env bash
set -euo pipefail

if [[ $# -lt 1 || $# -gt 2 ]]; then
    echo "usage: $0 <base-revision> [head-revision]" >&2
    exit 2
fi

base=$1
head=${2:-HEAD}
root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
cd "$root"

projects=()
while IFS= read -r project; do
    projects+=("${project#./}")
done < <(find src tests benchmarks \
    -type d \( -name bin -o -name obj -o -name artifacts \) -prune -o \
    -name '*.csproj' -type f ! -path 'tests/MonadicTypes.PackageSmoke/*' -print | sort)

affected=()
contains() {
    local candidate=$1
    local item
    for item in "${affected[@]:-}"; do
        [[ "$item" == "$candidate" ]] && return 0
    done
    return 1
}

add_affected() {
    contains "$1" || affected+=("$1")
}

changed_files=()
while IFS= read -r changed; do
    [[ -n "$changed" ]] && changed_files+=("${changed//\\//}")
done < <(git diff --name-only --diff-filter=ACMRTUXB "$base" "$head")

affects_all=false
for changed in "${changed_files[@]:-}"; do
    case "$changed" in
        .editorconfig|.gitattributes|.github/*|BannedSymbols.txt|Directory.Build.props|Directory.Build.targets|Directory.Packages.props|global.json|MonadicTypes.slnx|src/Directory.Build.props|docs/package-readme.md|eng/*)
            affects_all=true
            break
            ;;
    esac
done

if [[ "$affects_all" == true ]]; then
    for project in "${projects[@]}"; do
        add_affected "$project"
    done
else
    for changed in "${changed_files[@]:-}"; do
        best=""
        for project in "${projects[@]}"; do
            directory=${project%/*}
            if [[ "$changed" == "$directory/"* && ${#directory} -gt ${#best} ]]; then
                best=$directory
                matched_project=$project
            fi
        done
        [[ -n "$best" ]] && add_affected "$matched_project"
    done
fi

closure_changed=true
while [[ "$closure_changed" == true ]]; do
    closure_changed=false
    for project in "${projects[@]}"; do
        contains "$project" && continue
        project_directory=${project%/*}
        while IFS= read -r reference; do
            [[ -z "$reference" ]] && continue
            reference=${reference//\\//}
            reference_directory=$(cd "$project_directory/$(dirname "$reference")" && pwd)
            referenced_project="${reference_directory#"$root"/}/$(basename "$reference")"
            if contains "$referenced_project"; then
                add_affected "$project"
                closure_changed=true
                break
            fi
        done < <(sed -n 's/.*ProjectReference Include="\([^"]*\)".*/\1/p' "$project")
    done
done

printf '%s\n' "${affected[@]:-}" | sed '/^$/d' | sort
