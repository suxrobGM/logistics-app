/**
 * io.mjs — filesystem helpers shared by the tools.
 *
 * LINE ENDINGS ARE LOAD-BEARING. This repo is Windows with `core.autocrlf=true` and a `.gitattributes`
 * of `* text=auto eol=lf`, so files sit CRLF in the working tree but LF in the index. A tool that reads
 * a file and writes it back verbatim-but-CRLF produces a whole-file phantom diff: every line shows as
 * changed and the one line that actually changed is invisible. So: `readText` normalizes to LF,
 * `writeText` writes LF. Never call fs.readFileSync/writeFileSync directly.
 */

import { execFileSync } from "node:child_process";
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

/** Angular workspace root (…/src/Client/Logistics.Angular) — this file lives at tools/lib/. */
export const WORKSPACE_ROOT = path.resolve(fileURLToPath(new URL(".", import.meta.url)), "../..");

/** Read a file as text with line endings normalized to LF. */
export function readText(file) {
  return fs.readFileSync(file, "utf8").replace(/\r\n/g, "\n");
}

/** Write text with LF endings. Returns true if the file actually changed on disk (keeps tools idempotent). */
export function writeText(file, contents) {
  const normalized = contents.replace(/\r\n/g, "\n").replace(/\r/g, "\n");
  let previous = null;
  try {
    previous = fs.readFileSync(file, "utf8");
  } catch {
    // new file
  }
  if (previous !== null && previous.replace(/\r\n/g, "\n") === normalized) {
    return false;
  }
  fs.mkdirSync(path.dirname(file), { recursive: true });
  fs.writeFileSync(file, normalized, "utf8");
  return true;
}

/**
 * List source files via git, so we can never walk node_modules/ or dist/.
 *
 * `--cached --others --exclude-standard` = tracked files PLUS untracked-but-not-ignored ones. The
 * `--others` half is load-bearing: a plain `git ls-files` sees only TRACKED files, so every file a
 * change CREATES is invisible to every checker until it is committed — and the gates then run green
 * over precisely the newest, least-reviewed code. A checker that cannot see the thing it checks
 * confirms only its own blind spots.
 *
 * @param {object} [options]
 * @param {string[]} [options.dirs]  workspace-relative dirs to scan (default: ['projects'])
 * @param {string[]} [options.ext]   extensions to keep, e.g. ['.html', '.ts'] (default: all)
 * @returns {string[]} absolute paths, POSIX separators
 */
export function listFiles({ dirs = ["projects"], ext = null } = {}) {
  const out = execFileSync(
    "git",
    ["ls-files", "-z", "--cached", "--others", "--exclude-standard", "--", ...dirs],
    { cwd: WORKSPACE_ROOT, encoding: "utf8", maxBuffer: 64 * 1024 * 1024 },
  );

  // Dedupe: a path staged with `git add -N` can surface in both --cached and --others.
  const rel = [...new Set(out.split("\0").filter(Boolean))];
  const keep = ext
    ? rel.filter((f) => ext.some((e) => f.toLowerCase().endsWith(e.toLowerCase())))
    : rel;

  return (
    keep
      .map((f) => path.join(WORKSPACE_ROOT, f).split(path.sep).join("/"))
      // git ls-files lists index entries; a file can be staged-deleted but still listed.
      .filter((f) => fs.existsSync(f))
  );
}

/** Make an absolute path printable: relative to the workspace root, POSIX separators. */
export function relative(file) {
  return path.relative(WORKSPACE_ROOT, file).split(path.sep).join("/");
}

/** 1-indexed line number of a character offset. */
export function lineOf(src, offset) {
  return src.slice(0, offset).split("\n").length;
}
