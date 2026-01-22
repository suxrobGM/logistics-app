/**
 * Calculates estimated reading time for content.
 * @param content HTML or plain text content
 * @param wordsPerMinute Reading speed (default: 200 words per minute)
 * @returns Formatted read time string (e.g., "5 min read")
 */
export function getReadTime(content: string | undefined | null, wordsPerMinute = 200): string {
  if (!content) return "3 min read";

  const plainText = content.replace(/<[^>]*>/g, "");
  const wordCount = plainText.split(/\s+/).filter(Boolean).length;
  const minutes = Math.max(1, Math.ceil(wordCount / wordsPerMinute));

  return `${minutes} min read`;
}
