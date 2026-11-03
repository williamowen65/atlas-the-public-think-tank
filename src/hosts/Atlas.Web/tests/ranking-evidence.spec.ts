import { expect, test } from '@playwright/test';
import {
  experiment,
  matches,
  words,
  parts,
  passage,
  searchableTerms,
  coverage,
} from '../storybook/SearchRanking/evidence';

test('literal evidence counts whole words, casing and Unicode without inventing stemming', () => {
  expect(words('Café, testing! test-tested.')).toHaveLength(4);
  expect(matches('TESTING testing test retesting', 'testing')).toBe(2);
  expect(matches('test', 'testing')).toBe(0);
  expect(parts('<script>TESTING</script>', 'testing').filter((p) => p.match)).toEqual([
    { text: 'TESTING', match: true },
  ]);
  const text = `${'prefix '.repeat(100)}testing ${'suffix '.repeat(100)}`;
  expect(passage(text, 'testing')).toContain('testing');
  expect(passage(text, 'testing').length).toBeLessThanOrEqual(242);
});
test('experimental balance permits rated nodes to compete with short unvoted comments', () => {
  const comment = { kind: 'comment', textRank: 608, support: 0 };
  const node = { kind: 'node', textRank: 269, support: 7 };
  expect(experiment(node).score).toBeGreaterThan(experiment(comment).score);
  expect(experiment(node, 0).score).toBeLessThan(experiment(comment, 0).score);
  expect(experiment(node).textContribution + experiment(node).voteContribution).toBeCloseTo(
    experiment(node).score,
  );
  expect(experiment({ ...node, support: 8 }).score).toBeGreaterThan(experiment(node).score);
  expect(experiment({ ...comment, support: -1 }).score).toBeLessThan(experiment(comment).score);
  expect(experiment({ ...comment, support: 1 }).score).toBeGreaterThan(experiment(comment).score);
  expect(experiment({ kind: 'node', textRank: 0, support: 0 }).score).toBe(0);
});

test('active SQL stopwords are excluded without guessing or dropping meaningful substrings', () => {
  const query = searchableTerms('OTTERS and the islands', ['and', 'the']);
  expect(query).toBe('OTTERS islands');
  expect(matches('And the otters visit islands', query)).toBe(2);
  expect(searchableTerms('and the', ['and', 'the'])).toBe('');
  expect(searchableTerms('candy and islands', [])).toBe('candy and islands');
});

test('preview uses authoritative phrase-aware SQL text signal when supplied', () => {
  const direct = experiment({ kind: 'node', textRank: 191, support: 7, textSignal: 0.85 }, 40);
  expect(direct.textContribution).toBeCloseTo(0.51);
  expect(direct.score).toBeCloseTo(0.79);
});

test('literal query coverage counts distinct terms rather than repeated hits', () => {
  expect(coverage('housing housing housing', 'housing retention services')).toBe('1 / 3');
  expect(coverage('retention services housing', 'housing retention services')).toBe('3 / 3');
});
