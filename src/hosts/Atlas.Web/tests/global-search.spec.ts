import { expect, test, type Page } from '@playwright/test';
const first = 'b1000000-0000-4000-8000-000000000001';
const found = 'b1000000-0000-4000-8000-000000000002';
const other = 'b1000000-0000-4000-8000-000000000003';
const entry = (id: string, title: string) => ({
  nodeId: id,
  voteCount: 12,
  averageVote: 7.5,
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
    if (url.pathname.endsWith('/reactions')) return route.fulfill({ json: [] });
    if (url.pathname.endsWith('/catalog/node-types')) return route.fulfill({ json: [] });
    if (url.pathname.endsWith('/comments'))
      return route.fulfill({ json: { items: [], next: null } });
    if (/\/nodes\/[^/]+$/.test(url.pathname))
      return route.fulfill({
        json: { id: url.pathname.split('/').at(-1), requestedTypeIds: [], status: 'Active' },
      });
    if (url.pathname.endsWith('/nodes') || url.pathname.endsWith('/search')) {
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
  await page.setViewportSize({ width: 320, height: 740 });
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
  await expect(
    results.getByRole('link', { name: 'Question: Salmon research', exact: true }),
  ).toBeVisible();
  await expect(results.getByRole('spinbutton')).toHaveCount(0);
  await expect(results.getByRole('navigation')).toHaveCount(0);
  await expect(results).toContainText('4 Comments');
  await expect(results).toContainText('2 Ideas');
  expect(await results.evaluate((element) => element.scrollWidth <= element.clientWidth)).toBe(
    true,
  );
  await expect(
    results.getByLabel('Importance: 7.5 out of 10, 12 votes', { exact: true }).first(),
  ).toBeVisible();
  // An outside click dismisses the whole search without clearing the accepted query.
  // Click the page gutter below the overlay; the node heading is covered on narrow screens.
  await page.mouse.click(5, 730);
  await expect(results).toHaveCount(0);
  await page.getByRole('button', { name: 'Toggle search', exact: true }).click();
  await expect(input).toHaveValue('salmon votes:1..5');
  await expect(
    results.getByRole('link', { name: 'Question: Salmon research', exact: true }),
  ).toBeVisible();

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
  await expect(results.getByRole('button', { name: 'Load more results' })).toHaveCount(0);
  await results.evaluate((element) => {
    element.scrollTop = element.scrollHeight;
  });
  await expect(
    results.getByRole('link', { name: 'Question: More salmon evidence', exact: true }),
  ).toBeVisible();
  await results.getByRole('link', { name: 'Question: Salmon research', exact: true }).click();
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
      .getByRole('link', { name: 'Question: Salmon research', exact: true }),
  ).toBeVisible();
  await expect(
    feed.getByRole('heading', { name: 'Question: Original node', exact: true }),
  ).toBeVisible();
  expect(browseReads()).toBe(before);
  await page.getByRole('button', { name: 'Clear search and filters', exact: true }).click();
  await expect(page.getByRole('region', { name: 'Search results', exact: true })).toHaveCount(0);
});

test('unavailable full-text search shows a clear error and allows retry', async ({ page }) => {
  await mock(page);
  let unavailable = true;
  await page.route('**/atlas-public-api/search?**', (route) => {
    const url = new URL(route.request().url());
    if (unavailable && url.searchParams.has('SearchText'))
      return route.fulfill({
        status: 503,
        json: {
          status: 503,
          code: 'search_unavailable',
          title: 'Private backend diagnostic must not be shown',
        },
      });
    return route.fallback();
  });
  await page.goto('http://127.0.0.1:4173');
  await page.getByRole('button', { name: 'Toggle search', exact: true }).click();
  await page.getByRole('textbox', { name: 'Search nodes and filters' }).fill('salmon');
  await page.getByRole('button', { name: 'Search', exact: true }).click();
  const results = page.getByRole('region', { name: 'Search results', exact: true });
  await expect(results.getByRole('alert')).toHaveText(
    'Text search is temporarily unavailable. Please try again later.',
  );
  await expect(results.getByText('No results found.', { exact: true })).toHaveCount(0);
  unavailable = false;
  await results.getByRole('button', { name: 'Retry search', exact: true }).click();
  await expect(results.getByRole('alert')).toHaveCount(0);
  await expect(
    page.getByRole('region', { name: 'Search results', exact: true }).getByRole('link').first(),
  ).toBeVisible();
});

test('mixed pages retain comment identity and open an off-page nested thread', async ({ page }) => {
  await mock(page);
  const root = 'c1000000-0000-4000-8000-000000000001';
  const parent = 'c1000000-0000-4000-8000-000000000002';
  const target = 'c1000000-0000-4000-8000-000000000003';
  const author = 'd1000000-0000-4000-8000-000000000001';
  const at = '2026-10-21T12:00:00Z';
  const comment = {
    nodeId: first,
    card: null,
    averageVote: null,
    voteCount: 0,
    comment: {
      id: target,
      nodeTitle: 'Original node',
      authorId: author,
      authorName: 'Comment writer',
      createdAt: at,
      excerpt: 'The matching salmon passage.',
      score: -3,
    },
  };
  await page.route('**/atlas-public-api/**', (route) => {
    const url = new URL(route.request().url());
    if (url.pathname.endsWith('/search')) {
      const secondPage = url.searchParams.get('Page') === '2';
      return route.fulfill({
        json: {
          items: secondPage
            ? [entry(other, 'Later node')]
            : [entry(found, 'Salmon research'), comment],
          totalCount: 3,
          page: secondPage ? 2 : 1,
          pageSize: 2,
          hasNextPage: !secondPage,
        },
      });
    }
    if (url.pathname.endsWith(`/comments/${target}/path`))
      return route.fulfill({
        json: {
          items: [
            {
              id: root,
              authorId: author,
              status: 'Removed',
              body: null,
              createdAt: at,
              parentId: null,
            },
            {
              id: parent,
              authorId: author,
              status: 'Active',
              body: 'Ancestor reply',
              createdAt: at,
              parentId: root,
            },
            {
              id: target,
              authorId: author,
              status: 'Active',
              body: 'The matching salmon passage.',
              createdAt: at,
              parentId: parent,
            },
          ],
        },
      });
    return route.fallback();
  });
  await page.goto('http://127.0.0.1:4173');
  await page.getByRole('button', { name: 'Toggle search', exact: true }).click();
  await page.getByRole('textbox', { name: 'Search nodes and filters' }).fill('salmon');
  await page.getByRole('button', { name: 'Search', exact: true }).click();
  const results = page.getByRole('region', { name: 'Search results', exact: true });
  await expect(results.getByText('Comment writer', { exact: false })).toBeVisible();
  await expect(results.getByText('The matching salmon passage.')).toBeVisible();
  await expect.poll(async () => (await results.locator('.comment-result').boundingBox())?.width ?? 0).toBeGreaterThan(200);
  await expect(results.getByLabel('Comment score: -3', { exact: true })).toBeVisible();
  const columns = await results.locator('.result-content').evaluateAll(elements => elements.map(element => element.getBoundingClientRect().left));
  expect(Math.abs(columns[0] - columns[1])).toBeLessThanOrEqual(1);
  await results.evaluate((el) => {
    el.scrollTop = el.scrollHeight;
  });
  await expect(results.getByRole('link', { name: 'Question: Later node' })).toBeVisible();
  await expect(results.getByRole('link', { name: 'Comment on Original node' })).toHaveCount(1);
  await results.getByRole('link', { name: 'Comment on Original node' }).click();
  await expect(page).toHaveURL(new RegExp(`comment=${target}`));
  const selected = page.locator(`[data-comment-id="${target}"]`);
  await expect(selected).toHaveClass(/search-target/);
  await expect(selected).toBeFocused();
  await expect(selected).toBeInViewport();
  await expect(selected).toContainText('The matching salmon passage.');
  await expect(page.locator(`[data-comment-id="${parent}"]`)).toContainText('Ancestor reply');
});
