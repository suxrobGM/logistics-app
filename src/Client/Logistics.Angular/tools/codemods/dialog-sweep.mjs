/**
 * dialog-sweep.mjs — S7 of the PrimeNG -> spartan migration. Dialogs and the collapsible fieldset.
 *
 *   node tools/codemods/dialog-sweep.mjs --census   # report only; writes .dialog-refusals.json
 *   node tools/codemods/dialog-sweep.mjs --apply    # rewrite templates + TS imports, then prettier
 *   node tools/codemods/dialog-sweep.mjs --check    # exit 1 if any <p-dialog>/<p-fieldset> remain
 *
 *   <p-dialog>   -> <ui-dialog>       46 sites / 42 files
 *   <p-fieldset> -> <ui-collapsible>   5 sites /  2 files
 *
 * See lib/html.mjs for the codemod contract (parse to analyse, span-splice to mutate, refuse rather
 * than guess, idempotent). Modelled on cosmetic-sweep.mjs.
 *
 * =============================================================================================
 * THE #1 RISK IS A DIALOG THAT LOSES ITS WIDTH, OR ITS [(visible)].
 * =============================================================================================
 * Both failures are quiet. `[style]="{ width: '650px' }"` is the ONLY thing that sizes a p-dialog, and
 * 45 of the 46 carry one; drop it in translation and the dialog still opens, still works, and renders
 * at Helm's default 28rem — a 650px form silently squeezed into 448px, on a page nobody re-opens
 * during review. Drop `[(visible)]` and the dialog simply never opens: no error, no type failure,
 * just a button that does nothing.
 *
 * So `conserve()` re-parses the OUTPUT of every file and checks three multisets against the input:
 *
 *   open   — every `visible` expression on a p-dialog must reappear as an `open` expression on a
 *            ui-dialog, byte for byte.
 *   width  — every width string parsed out of a `[style]` must reappear as a `width` attribute.
 *   header — same, for the title.
 *
 * Multisets, not counts: two dialogs in one file that swapped their widths would conserve the count
 * and still be wrong (accident-detail and dvir-detail each hold two). Comparing sorted values catches
 * that. A file that fails conservation is written back UNCHANGED and reported as a refusal.
 *
 * =============================================================================================
 * WHAT [style] IS ALLOWED TO BE
 * =============================================================================================
 * Across all 46 sites `[style]` is WIDTH AND NOTHING ELSE — no height, no position, no colour:
 *
 *     { width: '450px' }                       ×14 (and 500/600/650/400/480/460/550px, 28/32/36/40rem)
 *     { width: '80vw', maxWidth: '1200px' }    ×1   (attach-load-dialog)
 *     { width: '90vw', maxWidth: '1200px' }    ×1   (trip-wizard-loads)
 *
 * STYLE_SHAPE matches exactly those two forms and NOTHING else. A `[style]` carrying anything the
 * regex does not recognise is a REFUSAL, not a best guess — silently dropping half a style object is
 * precisely the invisible damage this contract exists to prevent.
 *
 * =============================================================================================
 * WHAT IS **NOT** REWRITTEN, AND WHY THAT IS THE WHOLE POINT
 * =============================================================================================
 * `header`, `[modal]`, `[closable]`, `[draggable]`, `[resizable]`, `[breakpoints]` pass through
 * UNTOUCHED — ui-dialog takes the same names with the same p-dialog DEFAULTS (`draggable` and
 * `resizable` both default TRUE; see dialog.ts). That is deliberate: every one of the 10 `[draggable]`
 * bindings in the repo says `"false"` — they are opting OUT of a default-on affordance — so a sweep
 * that "helpfully" dropped them, or a ui-dialog that defaulted them to false, would strip dragging
 * from the 36 dialogs that never mention it. The rename is a rename.
 *
 * Likewise `p-fieldset` -> `ui-collapsible` is a PURE tag rename: `legend`, `[toggleable]` and
 * `[collapsed]` keep their names and their semantics.
 */

import fs from "node:fs";
import path from "node:path";
import {
  allAttrs,
  lineOf,
  parse,
  removeAttr,
  renameAttr,
  renameTag,
  Splicer,
  visitElements,
} from "./lib/html.mjs";
import {
  listFiles,
  parseMode,
  readText,
  relative,
  runPrettier,
  WORKSPACE_ROOT,
  writeText,
} from "./lib/io.mjs";
import { addImport, addToComponentImports, removeImportSpecifier } from "./lib/ts.mjs";

const mode = parseMode();
const REFUSALS_OUT = path.join(WORKSPACE_ROOT, "tools/codemods/.dialog-refusals.json");

const COMPONENTS = {
  "p-dialog": { tag: "ui-dialog", symbol: "UiDialog", path: "feedback/dialog/dialog" },
  "p-fieldset": {
    tag: "ui-collapsible",
    symbol: "UiCollapsible",
    path: "layout/collapsible/collapsible",
  },
};

const PRIMENG_MODULES = [
  { module: "primeng/dialog", specifiers: ["DialogModule", "Dialog"], needed: /<p-dialog\b/ },
  {
    module: "primeng/fieldset",
    specifiers: ["FieldsetModule", "Fieldset"],
    needed: /<p-fieldset\b/,
  },
];

/** `visible` -> `open` and friends. Anything absent from here (and from PASSTHROUGH) is a refusal. */
const RENAME = {
  "p-dialog": {
    visible: "open", // covers [(visible)], [visible] and (visibleChange) -> openChange
    visibleChange: "openChange",
    onHide: "closed",
    onShow: "opened",
  },
  "p-fieldset": {},
};

/** Same name, same meaning, same default on ui-dialog. Left byte-for-byte alone. */
const PASSTHROUGH = {
  "p-dialog": new Set(["header", "modal", "closable", "draggable", "resizable", "breakpoints"]),
  "p-fieldset": new Set(["legend", "toggleable", "collapsed"]),
};

/** The only two `[style]` shapes in the repo. Everything else REFUSES. */
const STYLE_SHAPE = /^\{\s*width:\s*'([^']+)'\s*(?:,\s*maxWidth:\s*'([^']+)'\s*)?\}$/;

const refusals = [];
const refuse = (file, line, reason, snippet) =>
  refusals.push({ file: relative(file), line, reason, snippet });

const stats = new Map(); // tag -> { seen, done, refused }
const bump = (tag, key) => {
  const s = stats.get(tag) ?? { seen: 0, done: 0, refused: 0 };
  s[key]++;
  stats.set(tag, s);
};

/**
 * Everything a dialog must not lose in translation, keyed by what it becomes. Collected from the
 * INPUT (p-* tags) and from the OUTPUT (ui-* tags) and compared as sorted multisets.
 */
function invariants(nodes, forTag) {
  const found = { open: [], width: [], maxWidth: [], header: [] };
  const dialogTags = forTag === "before" ? ["p-dialog"] : ["ui-dialog"];

  visitElements(nodes, (el) => {
    const name = el.name ?? el.tagName;
    if (!dialogTags.includes(name)) return;

    for (const attr of allAttrs(el)) {
      if (attr.kind === "reference") continue;
      const value = attr.value ?? "";

      if (name === "p-dialog") {
        if (attr.name === "visible") found.open.push(value);
        if (attr.name === "header") found.header.push(value);
        if (attr.name === "style") {
          const m = STYLE_SHAPE.exec(value.trim());
          if (m) {
            found.width.push(m[1]);
            if (m[2]) found.maxWidth.push(m[2]);
          }
        }
      } else {
        if (attr.name === "open") found.open.push(value);
        if (attr.name === "header") found.header.push(value);
        if (attr.name === "width") found.width.push(value);
        if (attr.name === "maxWidth") found.maxWidth.push(value);
      }
    }
  });

  for (const key of Object.keys(found)) found[key].sort();
  return found;
}

const sameMultiset = (a, b) => a.length === b.length && a.every((v, i) => v === b[i]);

/** Rewrite one template. Returns the new source, or null when nothing changed. */
function sweepHtml(file, src) {
  const nodes = parse(src, file);
  const splicer = new Splicer(src, relative(file));
  const symbols = new Set();
  let touched = 0;

  visitElements(nodes, (el) => {
    const tag = el.name ?? el.tagName;
    const spec = COMPONENTS[tag];
    if (!spec) return;

    bump(tag, "seen");
    const line = lineOf(src, el.startSourceSpan.start.offset);
    const rename = RENAME[tag];
    const passthrough = PASSTHROUGH[tag];

    // Plan every edit for this element BEFORE touching the splicer, so an unknown attribute leaves
    // the element completely untouched rather than half-converted.
    const edits = [];
    for (const attr of allAttrs(el)) {
      if (attr.kind === "reference") continue; // #ref on the tag survives a rename unchanged

      if (tag === "p-dialog" && attr.name === "style") {
        const m = STYLE_SHAPE.exec((attr.value ?? "").trim());
        if (!m) {
          bump(tag, "refused");
          refuse(file, line, `[style] is not a width-only object literal`, attr.value ?? "");
          return;
        }
        const [, width, maxWidth] = m;
        const replacement = `width="${width}"` + (maxWidth ? ` maxWidth="${maxWidth}"` : "");
        edits.push(() =>
          splicer.replace(
            attr.sourceSpan.start.offset,
            attr.sourceSpan.end.offset,
            replacement,
            `[style] -> width`,
          ),
        );
        continue;
      }

      if (rename[attr.name]) {
        // Two-way bindings arrive collapsed (see html.mjs#twoWay), so renaming the key of a
        // `[(visible)]` rewrites both halves through the one keySpan. `[visible]` + `(visibleChange)`
        // written separately are two attrs and each hits its own entry in RENAME.
        const to = rename[attr.name];
        edits.push(() => renameAttr(splicer, attr, to));
        continue;
      }

      if (passthrough.has(attr.name)) continue;

      bump(tag, "refused");
      refuse(file, line, `<${tag}> has an attribute this codemod does not map: ${attr.name}`, "");
      return;
    }

    for (const edit of edits) edit();
    renameTag(splicer, el, spec.tag);
    symbols.add(spec.symbol);
    touched++;
    bump(tag, "done");
  });

  if (touched === 0) return null;

  const out = splicer.apply();

  // Conservation: a dialog that came out the other side without its width or its open-binding is the
  // failure mode this whole script exists to prevent. Compare before/after and refuse the FILE.
  const before = invariants(nodes, "before");
  const after = invariants(parse(out, file), "after");
  for (const key of ["open", "width", "maxWidth", "header"]) {
    if (!sameMultiset(before[key], after[key])) {
      refuse(
        file,
        0,
        `conservation failed for "${key}": [${before[key]}] -> [${after[key]}] — file left unchanged`,
        "",
      );
      return null;
    }
  }

  return { out, symbols };
}

/* ------------------------------------------------------------------------------------------------
 * TS wiring — identical in shape to cosmetic-sweep.mjs.
 * ---------------------------------------------------------------------------------------------- */

function ownerComponent(templateFile) {
  const ts = templateFile.replace(/\.html$/, ".ts");
  let src;
  try {
    src = readText(ts);
  } catch {
    return null;
  }
  const base = path.basename(templateFile);
  if (!new RegExp(`templateUrl:\\s*["'\`]\\./${base.replace(/\./g, "\\.")}["'\`]`).test(src)) {
    return null;
  }
  return { file: ts, src };
}

/** `@logistics/shared/ui` from an app; a relative path from inside the shared lib itself (no cycle). */
function importSpecifier(tsFile, componentPath) {
  const rel = relative(tsFile);
  if (!rel.startsWith("projects/shared/")) return "@logistics/shared/ui";
  const from = path.dirname(path.join(WORKSPACE_ROOT, rel));
  const to = path.join(WORKSPACE_ROOT, "projects/shared/src/lib/ui", componentPath);
  let spec = path.relative(from, to).split(path.sep).join("/");
  if (!spec.startsWith(".")) spec = `./${spec}`;
  return spec;
}

function stripFromComponentImports(src, remove) {
  return src.replace(/(\bimports:\s*\[)([^\]]*)\]/, (_m, head, body) => {
    const kept = body
      .split(",")
      .map((s) => s.trim())
      .filter(Boolean)
      .filter((n) => !remove.has(n));
    return `${head}${kept.join(", ")}]`;
  });
}

function wireTs(templateFile, finalHtml, symbols) {
  const owner = ownerComponent(templateFile);
  if (!owner) {
    refuse(templateFile, 0, `cannot resolve the owning component — ui imports NOT added`, "");
    return null;
  }
  let src = owner.src;

  for (const symbol of [...symbols].sort()) {
    const componentPath = Object.values(COMPONENTS).find((c) => c.symbol === symbol).path;
    const added = addToComponentImports(src, symbol);
    if (!added.changed && added.reason !== "already in imports") {
      refuse(owner.file, 0, `could not add ${symbol} to @Component imports: ${added.reason}`, "");
      return null;
    }
    const imported = addImport(added.src, symbol, importSpecifier(owner.file, componentPath));
    if (!imported.changed && imported.reason !== "already imported") {
      refuse(owner.file, 0, `could not import ${symbol}: ${imported.reason}`, "");
      return null;
    }
    src = imported.src;
  }

  // Drop each primeng module whose tag is gone from the FINAL html. A refusal still standing in the
  // file means the tag is still there, the regex still matches, and the module correctly stays.
  const remove = new Set();
  for (const { module, specifiers, needed } of PRIMENG_MODULES) {
    if (needed.test(finalHtml)) continue;
    for (const s of specifiers) {
      const before = src;
      src = removeImportSpecifier(src, module, s).src;
      if (src !== before) remove.add(s);
    }
  }
  if (remove.size > 0) src = stripFromComponentImports(src, remove);

  return { file: owner.file, src };
}

/* ------------------------------------------------------------------------------------------------
 * Drive
 * ---------------------------------------------------------------------------------------------- */

const touchedHtml = [];
const touchedTs = new Map();

for (const file of listFiles({ dirs: ["projects"], ext: [".html"] })) {
  const src = readText(file);
  if (!/<p-(dialog|fieldset)\b/.test(src)) continue;

  const swept = sweepHtml(file, src);
  if (!swept) continue;

  const wired = wireTs(file, swept.out, swept.symbols);
  if (!wired) continue; // TS could not be wired -> leave the html alone too, or the tag is undeclared

  if (mode === "apply") {
    writeText(file, swept.out);
    touchedTs.set(wired.file, wired.src);
  }
  touchedHtml.push(file);
}

if (mode === "apply") {
  for (const [file, src] of touchedTs) writeText(file, src);
  runPrettier([...touchedHtml, ...touchedTs.keys()]);
}

const line = "-".repeat(72);
console.log(`\n${line}\ndialog-sweep — ${mode}\n${line}`);
console.log(
  `  ${"tag".padEnd(14)}${"->".padEnd(3)}${"component".padEnd(18)}${"seen".padStart(6)}${"done".padStart(7)}${"REFUSED".padStart(9)}`,
);
for (const [tag, spec] of Object.entries(COMPONENTS)) {
  const s = stats.get(tag) ?? { seen: 0, done: 0, refused: 0 };
  console.log(
    `  ${tag.padEnd(14)}${"->".padEnd(3)}${spec.tag.padEnd(18)}${String(s.seen).padStart(6)}${String(s.done).padStart(7)}${String(s.refused).padStart(9)}`,
  );
}
console.log(`\n  html files: ${touchedHtml.length}   ts files: ${touchedTs.size}`);

console.log(`\n${line}\nREFUSALS (${refusals.length})\n${line}`);
for (const r of refusals) {
  console.log(`  ${r.file}:${r.line}  ${r.reason}${r.snippet ? `\n      ${r.snippet}` : ""}`);
}
fs.writeFileSync(REFUSALS_OUT, `${JSON.stringify(refusals, null, 2)}\n`, "utf8");
console.log(`\nrefusal list -> ${relative(REFUSALS_OUT)}`);

if (mode === "check") {
  const left = listFiles({ dirs: ["projects"], ext: [".html"] }).filter((f) =>
    /<p-(dialog|fieldset)\b/.test(readText(f)),
  );
  if (left.length > 0) {
    console.error(
      `\ncheck FAILED — <p-dialog>/<p-fieldset> still present in ${left.length} file(s):`,
    );
    for (const f of left) console.error(`  ${relative(f)}`);
    process.exit(1);
  }
  console.log("\ncheck OK — no <p-dialog> / <p-fieldset> left.");
}
