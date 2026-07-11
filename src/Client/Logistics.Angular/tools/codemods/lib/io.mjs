/**
 * io.mjs — filesystem + process helpers shared by every codemod and check.
 *
 * LINE ENDINGS ARE LOAD-BEARING.
 * This repo is Windows with `core.autocrlf=true` and a `.gitattributes` of `* text=auto eol=lf`.
 * Files therefore sit CRLF in the working tree but LF in the index. A codemod that reads a file
 * and writes it back verbatim-but-CRLF produces a whole-file phantom diff: every line shows as
 * changed, and the one line that actually changed is invisible. The reviewability of the diff is
 * one of our detectors, so we protect it here rather than in each codemod:
 *
 *   readText  : normalizes \r\n -> \n  (callers only ever see LF)
 *   writeText : strips \r  and writes LF
 *
 * Never use fs.readFileSync/writeFileSync directly in a codemod. Use these.
 */

import { execFileSync } from "node:child_process";
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

/** Angular workspace root (…/src/Client/Logistics.Angular) — this file lives at tools/codemods/lib/. */
export const WORKSPACE_ROOT = path.resolve(
  fileURLToPath(new URL(".", import.meta.url)),
  "../../..",
);

/** Read a file as text with line endings normalized to LF. */
export function readText(file) {
  return fs.readFileSync(file, "utf8").replace(/\r\n/g, "\n");
}

/**
 * Write text with LF endings only. Returns true if the file actually changed on disk
 * (so callers can report accurate "N files changed" counts and stay idempotent).
 */
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
 * List tracked files via `git ls-files`, so we can never walk node_modules/ or dist/.
 *
 * @param {object} [options]
 * @param {string[]} [options.dirs]  repo-relative-to-workspace dirs to scan (default: ['projects'])
 * @param {string[]} [options.ext]   extensions to keep, e.g. ['.html', '.ts'] (default: all)
 * @returns {string[]} absolute paths, POSIX separators
 */
export function listFiles({ dirs = ["projects"], ext = null } = {}) {
  const out = execFileSync("git", ["ls-files", "-z", "--", ...dirs], {
    cwd: WORKSPACE_ROOT,
    encoding: "utf8",
    maxBuffer: 64 * 1024 * 1024,
  });

  const rel = out.split("\0").filter(Boolean);
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

/**
 * Run prettier over the given files.
 *
 * Codemods MUST call this on every .html they touch. lint-staged excludes .html, so if the codemod
 * does not format it, nothing else will, and the span-splice will leave attribute soup behind.
 * No-op on an empty list. Batched to stay under the Windows command-line length limit.
 */
export function runPrettier(files) {
  const targets = [...new Set(files)].filter(Boolean);
  if (targets.length === 0) return;

  const BATCH = 40;
  for (let i = 0; i < targets.length; i += BATCH) {
    const batch = targets.slice(i, i + BATCH).map((f) => relative(f));
    execFileSync("bunx", ["prettier", "--write", "--log-level", "warn", ...batch], {
      cwd: WORKSPACE_ROOT,
      encoding: "utf8",
      stdio: ["ignore", "inherit", "inherit"],
      shell: process.platform === "win32",
    });
  }
}

/**
 * Parse the codemod CLI contract. See the header of html.mjs for the full contract.
 * Exactly one of --census / --apply / --check must be given.
 */
export function parseMode(argv = process.argv.slice(2)) {
  const modes = ["census", "apply", "check"].filter((m) => argv.includes(`--${m}`));
  if (modes.length !== 1) {
    console.error("Usage: <codemod> --census | --apply | --check");
    process.exit(2);
  }
  return modes[0];
}
