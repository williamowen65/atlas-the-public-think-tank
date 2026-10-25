/** Explanatory counts only: these Unicode words are not SQL Server word-breaker tokens. */
export function words(text: string): string[] {
  return text.match(/[\p{L}\p{N}]+/gu) ?? [];
}
export function parts(text: string, query: string): { text: string; match: boolean }[] {
  const terms = new Set(words(query).map((word) => word.toLocaleLowerCase('en-US')));
  return (text.match(/[\p{L}\p{N}]+|[^\p{L}\p{N}]+/gu) ?? []).map((text) => ({
    text,
    match: terms.has(text.toLocaleLowerCase('en-US')),
  }));
}
export function matches(text: string, query: string): number {
  return parts(text, query).filter((part) => part.match).length;
}
export function passage(text: string, query: string): string {
  let offset = 0;
  for (const part of parts(text, query)) {
    if (part.match) break;
    offset += part.text.length;
  }
  const start = offset === text.length ? 0 : Math.max(0, offset - 65);
  const end = Math.min(text.length, start + 240);
  return `${start ? '…' : ''}${text.slice(start, end)}${end < text.length ? '…' : ''}`;
}
export type RankingInput = { kind: string; textRank: number; support: number };
/** Browser preview of weighted-v1. Actual global ordering and pagination run independently in SQL. */
export function experiment(input: RankingInput, votingPercent = 40) {
  const weight = Math.max(0, Math.min(100, votingPercent)) / 100;
  const text = Math.log1p(Math.max(0, input.textRank)) / Math.log(1001);
  const vote =
    input.kind === 'node'
      ? Math.max(0, Math.min(1, input.support / 10))
      : input.support / (5 + Math.abs(input.support));
  return {
    text,
    vote,
    textContribution: (1 - weight) * text,
    voteContribution: weight * vote,
    score: (1 - weight) * text + weight * vote,
  };
}

/** Use the active database stoplist rather than a guessed browser list. */
export function searchableTerms(query: string, stopWords: string[]): string {
  const ignored = new Set(stopWords.map((word) => word.toLocaleLowerCase('en-US')));
  return words(query)
    .filter((word) => !ignored.has(word.toLocaleLowerCase('en-US')))
    .join(' ');
}
