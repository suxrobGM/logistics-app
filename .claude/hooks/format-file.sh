#!/usr/bin/env bash
# Auto-formatter hook for Claude Code (PostToolUse: Edit|Write)
# - TypeScript, HTML, SCSS, CSS, JSON → Prettier (Angular project only)
# - C# → dotnet format (nearest .csproj)

INPUT=$(cat)
FILE_PATH=$(echo "$INPUT" | node -e "process.stdin.resume();let d='';process.stdin.on('data',c=>d+=c);process.stdin.on('end',()=>{try{console.log(JSON.parse(d).tool_input.file_path||'')}catch{console.log('')}})")

# Skip if no file path or file doesn't exist
[[ -z "$FILE_PATH" || ! -f "$FILE_PATH" ]] && exit 0

EXT="${FILE_PATH##*.}"
EXT="${EXT,,}" # lowercase

ANGULAR_DIR="private/src/Client/Logistics.Angular"

case "$EXT" in
  ts|html|scss|css|json)
    # Only format files inside the Angular project
    if [[ "$FILE_PATH" == *"$ANGULAR_DIR"* ]]; then
      ANGULAR_ROOT="${FILE_PATH%%$ANGULAR_DIR*}${ANGULAR_DIR}"
      (cd "$ANGULAR_ROOT" && bun run prettier --write "$FILE_PATH" 2>/dev/null) && echo "Formatted: $FILE_PATH"
    fi
    ;;
  cs)
    # Walk up to find nearest .csproj
    DIR=$(dirname "$FILE_PATH")
    while [[ -n "$DIR" && "$DIR" != "/" ]]; do
      CSPROJ=$(find "$DIR" -maxdepth 1 -name '*.csproj' -print -quit 2>/dev/null)
      if [[ -n "$CSPROJ" ]]; then
        dotnet format "$CSPROJ" --include "$FILE_PATH" --verbosity quiet 2>/dev/null && echo "Formatted: $FILE_PATH"
        break
      fi
      DIR=$(dirname "$DIR")
    done
    ;;
esac

exit 0
