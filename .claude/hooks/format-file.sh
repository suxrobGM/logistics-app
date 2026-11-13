#!/usr/bin/env bash
# PostToolUse(Edit|Write): format the edited file the way `bun run format` would.
# Angular: Biome for ts/json/css, Prettier for templates. C#: dotnet format on the nearest csproj.

INPUT=$(cat)
FILE_PATH=$(echo "$INPUT" | node -e "let d='';process.stdin.on('data',c=>d+=c).on('end',()=>{try{console.log(JSON.parse(d).tool_input.file_path||'')}catch{console.log('')}})")

[[ -z "$FILE_PATH" || ! -f "$FILE_PATH" ]] && exit 0

EXT="${FILE_PATH##*.}"
EXT="${EXT,,}"
ANGULAR_DIR="private/src/Client/Logistics.Angular"
NORMALIZED="${FILE_PATH//\\//}"

case "$EXT" in
  ts|json|css|html)
    [[ "$NORMALIZED" != *"$ANGULAR_DIR"* ]] && exit 0
    ANGULAR_ROOT="${NORMALIZED%%$ANGULAR_DIR*}${ANGULAR_DIR}"
    if [[ "$EXT" == "html" ]]; then
      (cd "$ANGULAR_ROOT" && bun run prettier --write "$NORMALIZED" >/dev/null 2>&1)
    else
      (cd "$ANGULAR_ROOT" && bun run biome check --write "$NORMALIZED" >/dev/null 2>&1)
    fi
    ;;
  cs)
    DIR=$(dirname "$NORMALIZED")
    while [[ -n "$DIR" && "$DIR" != "/" && "$DIR" != "." ]]; do
      CSPROJ=$(find "$DIR" -maxdepth 1 -name '*.csproj' -print -quit 2>/dev/null)
      if [[ -n "$CSPROJ" ]]; then
        dotnet format "$CSPROJ" --include "$NORMALIZED" --verbosity quiet >/dev/null 2>&1
        break
      fi
      DIR=$(dirname "$DIR")
    done
    ;;
esac

exit 0
