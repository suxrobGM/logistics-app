#!/usr/bin/env bun

/// <reference types="bun-types" />

const API_URL = "http://localhost:7000/swagger/v1/swagger.json";
const OUTPUT_PATH = "openapi.json";
const MAX_RETRIES = 3;
const RETRY_DELAY_MS = 2000;

async function fetchOpenApiSpec(): Promise<void> {
  let lastError: Error | undefined;

  for (let attempt = 1; attempt <= MAX_RETRIES; attempt++) {
    try {
      console.log(`Fetching OpenAPI spec from ${API_URL} (attempt ${attempt}/${MAX_RETRIES})...`);

      const response = await fetch(API_URL);

      if (!response.ok) {
        throw new Error(`HTTP ${response.status}: ${response.statusText}`);
      }

      const json = await response.json();
      const formatted = JSON.stringify(json, null, 2) + "\n";

      await Bun.write(OUTPUT_PATH, formatted);
      console.log(`Saved to ${OUTPUT_PATH} (${(formatted.length / 1024).toFixed(0)} KB)`);
      return;
    } catch (error) {
      lastError = error instanceof Error ? error : new Error(String(error));

      if (attempt < MAX_RETRIES) {
        console.log(`  Failed: ${lastError.message}. Retrying in ${RETRY_DELAY_MS / 1000}s...`);
        await Bun.sleep(RETRY_DELAY_MS);
      }
    }
  }

  console.error(`\nFailed to fetch OpenAPI spec after ${MAX_RETRIES} attempts.`);
  console.error(`Last error: ${lastError?.message}`);
  console.error(`\nMake sure the API is running at ${API_URL}`);
  process.exit(1);
}

fetchOpenApiSpec();
