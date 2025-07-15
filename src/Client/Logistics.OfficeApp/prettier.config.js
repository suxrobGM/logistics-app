/** @type {import("prettier").Config} */
const config = {
  printWidth: 100, // max 100 chars in line, code is easy to read
  useTabs: false, // use spaces instead of tabs
  tabWidth: 2, // "visual width" of of the "tab"
  trailingComma: "es5", // add trailing commas in objects, arrays, etc.
  semi: true, // add ; when needed
  singleQuote: false, // use double quotes
  bracketSpacing: false, // import {some} ... instead of import { some } ...
  arrowParens: "always", // braces even for single param in arrow functions (a) => { }
  jsxSingleQuote: false, // "" for react props, like in html
  bracketSameLine: false, // pretty JSX
  endOfLine: "lf", // 'lf' for linux, 'crlf' for windows, we need to use 'lf' for git
  plugins: ["@trivago/prettier-plugin-sort-imports"],
  importOrderParserPlugins: ["typescript", "decorators"],
  importOrder: ["^(angular/(.*)$)|^(angular$)", "<THIRD_PARTY_MODULES>", "^@/(.*)$", "^[./]"],
  importOrderSeparation: false,
  importOrderSortSpecifiers: true,
  importOrderSideEffects: false,
  overrides: [
    {
      files: "*.html",
      options: {
        parser: "angular",
      },
    },
  ],
};

export default config;
