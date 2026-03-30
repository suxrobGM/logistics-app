/**
 * @see https://prettier.io/docs/configuration
 * @type {import("prettier").Config}
 */
const config = {
  printWidth: 100, // max 100 chars in line, code is easy to read
  useTabs: false, // use spaces instead of tabs
  tabWidth: 2, // "visual width" of the "tab"
  trailingComma: "all", // add trailing commas in objects, arrays, etc.
  semi: true, // add ; when needed
  singleQuote: false, // use double quotes
  bracketSpacing: true, // import { some }
  arrowParens: "always", // braces even for single param in arrow functions (a) => { }
  jsxSingleQuote: false, // "" for react props, like in html
  bracketSameLine: false, // pretty JSX
  endOfLine: "lf", // 'lf' for linux, 'crlf' for windows, we need to use 'lf' for git,
  plugins: ["@ianvs/prettier-plugin-sort-imports", "prettier-plugin-tailwindcss"],
  importOrder: ["<BUILTIN_MODULES>", "<THIRD_PARTY_MODULES>", "^@/(.*)$", "^[./]"],
  importOrderParserPlugins: ["typescript", "jsx", "decorators-legacy"],
  importOrderTypeScriptVersion: "5.0.0",
  overrides: [
    {
      files: ["*.jsx", "*.tsx"],
      options: {
        importOrder: [
          "<BUILTIN_MODULES>",
          "^(react/(.*)$)|^(react$)",
          "^(angular/(.*)$)|^(angular$)",
          "<THIRD_PARTY_MODULES>",
          "^@/(.*)$",
          "^[./]",
        ],
      },
    },
    {
      files: "*.html",
      options: {
        parser: "angular",
      },
    },
  ],
};

export default config;
