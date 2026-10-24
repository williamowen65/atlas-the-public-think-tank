import { expect, test, type Page } from '@playwright/test';
const first = 'b1000000-0000-4000-8000-000000000001';
const found = 'b1000000-0000-4000-8000-000000000002';
const other = 'b1000000-0000-4000-8000-000000000003';
const entry = (id: string, title: string) => ({
  nodeId: id,
  voteCount: 0,
  averageVote: null,
  card: {
    title,
    typeName: 'Question',
    shortDescription: 'A compact result introduction.',
    authorName: 'Author',
    createdAt: '2026-10-21T12:00:00Z',
    status: 'Active',
    communities: '',
    commentCount: 4,
    childTypes: [{ typeId: 'idea', label: '2 Ideas', count: 2 }],
    isHidden: false,
  },
});
async function mock(page: Page) {
  const requests: URL[] = [];
  await page.route('**/atlas-session-api/**', (route) =>
    new URL(route.request().url()).pathname.endsWith('/votes')
      ? route.fulfill({ json: { voteCount: 0, averageVote: null, currentParticipantVote: null } })
      : route.fulfill({ status: 401, json: {} }),
  );
  await page.route('**/atlas-public-api/**', (route) => {
    const url = new URL(route.request().url());
    requests.push(url);
    if (url.pathname.endsWith('/catalog/node-types')) return route.fulfill({ json: [] });
    if (url.pathname.endsWith('/comments'))
      return route.fulfill({ json: { items: [], next: null } });
    if (/\/nodes\/[^/]+$/.test(url.pathname))
      return route.fulfill({
        json: { id: url.pathname.split('/').at(-1), requestedTypeIds: [], status: 'Active' },
      });
    if (url.pathname.endsWith('/nodes')) {
      const search = url.searchParams.get('SearchText');
      const pageNumber = Number(url.searchParams.get('Page'));
      const id = url.searchParams.get('NodeId') ?? first;
      return route.fulfill({
        json: {
          items: search
            ? [
                entry(
                  pageNumber === 1 ? found : other,
                  pageNumber === 1 ? 'Salmon research' : 'More salmon evidence',
                ),
              ]
            : [entry(id, id === first ? 'Original node' : 'Salmon research')],
          totalCount: search ? 2 : 1,
          page: pageNumber,
          pageSize: 3,
          hasNextPage: !!search && pageNumber === 1,
        },
      });
    }
    return route.fulfill({ json: [] });
  });
  return requests;
}

test('search from a node shows compact filtered results, pages them and opens a result', async ({
  page,
}) => {
  const requests = await mock(page);
  await page.goto(`http://127.0.0.1:4173/nodes?id=${first}`);
  await expect(
    page.getByRole('heading', { name: 'Question: Original node', exact: true }),
  ).toBeVisible();
  await page.getByRole('button', { name: 'Toggle search', exact: true }).click();
  const input = page.getByRole('textbox', { name: 'Search nodes and filters' });
  await input.fill('salmon votes:1..5');
  await page.getByRole('button', { name: 'Search', exact: true }).click();
  const results = page.getByRole('region', { name: 'Search results', exact: true });
  await expect(results.getByRole('link', { name: 'Salmon research', exact: true })).toBeVisible();
  await expect(results.getByRole('spinbutton')).toHaveCount(0);
  await expect(results.getByRole('navigation')).toHaveCount(0);
  await expect(results).toContainText('4 comments');
  await expect(results).toContainText('2 Ideas');
  await expect(
    page.getByRole('heading', { name: 'Question: Original node', exact: true }),
  ).toBeVisible();
  expect(
    requests.some(
      (url) =>
        url.searchParams.get('SearchText') === 'salmon' &&
        url.searchParams.get('MinimumVoteCount') === '1' &&
        url.searchParams.get('MaximumVoteCount') === '5',
    ),
  ).toBe(true);
  await results.getByRole('button', { name: 'Load more results' }).click();
  await expect(
    results.getByRole('link', { name: 'More salmon evidence', exact: true }),
  ).toBeVisible();
  await results.getByRole('link', { name: 'Salmon research', exact: true }).click();
  await expect(page).toHaveURL(new RegExp(`id=${found}`));
  await expect(results).toHaveCount(0);
  await expect(
    page.getByRole('heading', { name: 'Question: Salmon research', exact: true }),
  ).toBeVisible();
});

test('header searches leave the browsing feed intact and clear removes the dropdown', async ({
  page,
}) => {
  const requests = await mock(page);
  await page.goto('http://127.0.0.1:4173');
  const feed = page.getByRole('region', { name: 'Node feed', exact: true });
  await expect(
    feed.getByRole('heading', { name: 'Question: Original node', exact: true }),
  ).toBeVisible();
  const browseReads = () =>
    requests.filter((url) => url.pathname.endsWith('/nodes') && !url.searchParams.has('SearchText'))
      .length;
  const before = browseReads();
  await page.getByRole('button', { name: 'Toggle search', exact: true }).click();
  await page.getByRole('textbox', { name: 'Search nodes and filters' }).fill('salmon');
  await page.getByRole('button', { name: 'Search', exact: true }).click();
  await expect(
    page
      .getByRole('region', { name: 'Search results', exact: true })
      .getByRole('link', { name: 'Salmon research', exact: true }),
  ).toBeVisible();
  await expect(
    feed.getByRole('heading', { name: 'Question: Original node', exact: true }),
  ).toBeVisible();
  expect(browseReads()).toBe(before);
  await page.getByRole('button', { name: 'Clear search and filters', exact: true }).click();
  await expect(page.getByRole('region', { name: 'Search results', exact: true })).toHaveCount(0);
});
