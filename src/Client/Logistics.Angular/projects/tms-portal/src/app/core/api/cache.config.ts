interface CacheRule {
  pattern: RegExp;
  ttl: number; // milliseconds
}

/**
 * Predefined cache rules for different API endpoints.
 */
const cacheRules: CacheRule[] = [
  // Reference/static data - longer TTL
  { pattern: /\/settings/, ttl: 30 * 60 * 1000 }, // 30 min
  { pattern: /\/roles/, ttl: 30 * 60 * 1000 }, // 30 min

  // Frequently accessed lists - moderate TTL
  { pattern: /\/employees/, ttl: 5 * 60 * 1000 }, // 5 min
  { pattern: /\/customers/, ttl: 5 * 60 * 1000 }, // 5 min
  { pattern: /\/trucks/, ttl: 5 * 60 * 1000 }, // 5 min
  { pattern: /\/drivers/, ttl: 5 * 60 * 1000 }, // 5 min

  // Default for other GET requests
  { pattern: /.*/, ttl: 2 * 60 * 1000 }, // 2 min default
];

export function getTtlForUrl(url: string): number | null {
  for (const rule of cacheRules) {
    if (rule.pattern.test(url)) {
      return rule.ttl;
    }
  }
  return null;
}
