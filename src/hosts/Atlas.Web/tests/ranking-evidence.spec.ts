import { expect, test } from '@playwright/test';
import { experiment, matches, words, parts, passage } from '../storybook/SearchRanking/evidence';

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
