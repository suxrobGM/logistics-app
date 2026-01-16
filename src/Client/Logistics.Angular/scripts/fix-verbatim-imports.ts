#!/usr/bin/env bun

/**
 * Post-processing script for ng-openapi-gen output
 * Converts regular imports to `import type` for verbatimModuleSyntax compatibility
 */
/// <reference types="bun-types" />
import { readdir } from "fs/promises";
import { join, relative } from "path";

const GENERATED_DIR = "projects/shared/src/lib/api/generated";

// Imports that are used at runtime (not type-only)
const RUNTIME_IMPORTS = new Set([
  // Angular
  "Injectable",
  "HttpClient",
  "HttpContext",
  "HttpResponse",
  "HttpParams",
  "HttpHeaders",
  "HttpRequest",
  // RxJS
  "Observable",
  "filter",
  "map",
  "firstValueFrom",
  // Local runtime utilities
  "RequestBuilder",
  "ApiConfiguration",
]);

// Files that should not be processed (they define the types themselves)
const SKIP_FILES = new Set(["strict-http-response.ts"]);

/**
 * Recursively get all .ts files in a directory
 */
async function getTypeScriptFiles(dir: string): Promise<string[]> {
  const files: string[] = [];
  const entries = await readdir(dir, { withFileTypes: true });

  for (const entry of entries) {
    const fullPath = join(dir, entry.name);
    if (entry.isDirectory()) {
      files.push(...(await getTypeScriptFiles(fullPath)));
    } else if (entry.name.endsWith(".ts")) {
      files.push(fullPath);
    }
  }

  return files;
}

/**
 * Check if an import is type-only based on its usage in the file
 */
function isTypeOnlyImport(
  importName: string,
  fileContent: string,
  importStatement: string,
): boolean {
  // If it's a known runtime import, don't convert
  if (RUNTIME_IMPORTS.has(importName)) {
    return false;
  }

  // Remove the import statement itself to avoid false positives
  const contentWithoutImport = fileContent.replace(importStatement, "");

  // Check for runtime usage patterns
  const runtimePatterns = [
    // instanceof checks
    new RegExp(`instanceof\\s+${importName}\\b`),
    // Function calls
    new RegExp(`${importName}\\s*\\(`),
    // new keyword
    new RegExp(`new\\s+${importName}\\b`),
    // Property access (could be static methods/properties)
    new RegExp(`${importName}\\.`),
    // Used as a value (assigned to variable)
    new RegExp(`=\\s*${importName}\\b(?!\\s*[<>])`),
  ];

  for (const pattern of runtimePatterns) {
    if (pattern.test(contentWithoutImport)) {
      return false;
    }
  }

  return true;
}

/**
 * Process a single file and fix its imports
 */
async function processFile(filePath: string): Promise<boolean> {
  const file = Bun.file(filePath);
  const content = await file.text();

  // Match import statements
  const importRegex = /^import\s+\{([^}]+)\}\s+from\s+['"]([^'"]+)['"];?$/gm;

  let modified = content;
  let hasChanges = false;

  // Find all imports and check if they should be type-only
  const imports = [...content.matchAll(importRegex)];

  for (const match of imports) {
    const [fullMatch, importList, importPath] = match;

    // Skip if already a type import
    if (fullMatch.includes("import type")) {
      continue;
    }

    // Parse imported names
    const importNames = importList.split(",").map((name) => name.trim());

    // Check each import
    const typeOnlyImports: string[] = [];
    const runtimeImports: string[] = [];

    for (const name of importNames) {
      // Handle aliased imports like "foo as bar"
      const baseName = name.includes(" as ") ? name.split(" as ")[0].trim() : name;

      if (isTypeOnlyImport(baseName, content, fullMatch)) {
        typeOnlyImports.push(name);
      } else {
        runtimeImports.push(name);
      }
    }

    // If all imports are type-only, convert the entire import
    if (typeOnlyImports.length > 0 && runtimeImports.length === 0) {
      const newImport = `import type { ${typeOnlyImports.join(", ")} } from '${importPath}';`;
      modified = modified.replace(fullMatch, newImport);
      hasChanges = true;
    }
    // If mixed, split into two imports
    else if (typeOnlyImports.length > 0 && runtimeImports.length > 0) {
      const typeImport = `import type { ${typeOnlyImports.join(", ")} } from '${importPath}';`;
      const runtimeImport = `import { ${runtimeImports.join(", ")} } from '${importPath}';`;
      modified = modified.replace(fullMatch, `${runtimeImport}\n${typeImport}`);
      hasChanges = true;
    }
  }

  if (hasChanges) {
    await Bun.write(filePath, modified);
    return true;
  }

  return false;
}

async function main(): Promise<void> {
  console.log("Fixing imports for verbatimModuleSyntax compatibility...\n");

  const files = await getTypeScriptFiles(GENERATED_DIR);
  let fixedCount = 0;

  for (const file of files) {
    const fileName = file.split(/[/\\]/).pop();

    // Skip files that have runtime code
    if (fileName && SKIP_FILES.has(fileName)) {
      continue;
    }

    try {
      const wasFixed = await processFile(file);
      if (wasFixed) {
        console.log(`  Fixed: ${relative(GENERATED_DIR, file)}`);
        fixedCount++;
      }
    } catch (error) {
      console.error(`  Error processing ${file}:`, error instanceof Error ? error.message : error);
    }
  }

  console.log(`\nDone! Fixed ${fixedCount} file(s).`);
}

main().catch(console.error);
