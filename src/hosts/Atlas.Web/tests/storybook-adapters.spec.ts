import { expect, test } from '@playwright/test';
import { getResponse } from 'msw';
import { parentPathHandlers, type ExampleRoute } from '../storybook/Breadcrumb/parentPathHandlers';

// HTTP mocks implement the app's API contract, rather than an extra component prop path.
test('breadcrumb example supplies alternative parents and nearest-first selected ancestors', async () => {
  const routes: ExampleRoute[] = [
    {
      id: 'one',
      label: 'First path',
      items: [
        { id: 'root', title: 'Root' },
        { id: 'one', title: 'One' },
        { id: 'child', title: 'Child' },
      ],
    },
    {
      id: 'two',
      label: 'Other root path',
      items: [
        { id: 'other', title: 'Other root' },
        { id: 'two', title: 'Two' },
        { id: 'child', title: 'Child' },
      ],
    },
  ];
  let selected = '';
  const handlers = parentPathHandlers(
    () => routes,
    (label) => {
      selected = label;
    },
  );
  const parents = await getResponse(
    handlers,
    new Request('https://example.test/storybook-breadcrumb-api/nodes/child/parents'),
  );
  expect(await parents?.json()).toEqual([
    { id: 'one', title: 'One' },
    { id: 'two', title: 'Two' },
  ]);
  const ancestors = await getResponse(
    handlers,
    new Request(
      'https://example.test/storybook-breadcrumb-api/nodes/child/ancestors?firstParent=two',
    ),
  );
  expect(await ancestors?.json()).toEqual({
    items: [
      { id: 'two', title: 'Two' },
      { id: 'other', title: 'Other root' },
    ],
    hasMore: false,
  });
  expect(selected).toBe('Other root path');
});

test('Storybook breadcrumb mocks do not intercept the real application API', async () => {
  const handlers = parentPathHandlers(
    () => [],
    () => {},
  );
  expect(
    await getResponse(
      handlers,
      new Request('https://example.test/atlas-public-api/nodes/child/parents'),
    ),
  ).toBeUndefined();
});

test('Storybook breadcrumb picker uses production path-switching behavior', async ({ page }) => {
  await page.goto(
    'http://127.0.0.1:6006/iframe.html?id=components-breadcrumb--multiple-parents&viewMode=story',
  );
  await page.getByRole('button', { name: 'Choose another parent path' }).first().click();
  await page
    .getByRole('button', {
      name: 'Goal: Make essential services accessible to everyone',
      exact: true,
    })
    .click();
  await expect(page.getByRole('status')).toHaveText(
    'Current parent route: Goal: Make essential services accessible to everyone',
  );
  await expect(
    page.getByRole('button', {
      name: 'Idea: Make internet access free and available everywhere',
      exact: true,
    }),
  ).toBeVisible();
  await page.getByRole('button', { name: 'Different roots (3 paths)' }).click();
  await page.getByRole('button', { name: 'Choose another parent path' }).first().click();
  await page
    .getByRole('button', { name: 'Goal: Strengthen climate resilience', exact: true })
    .click();
  await expect(
    page.getByRole('button', {
      name: 'Question: How should we adapt to a changing climate?',
      exact: true,
    }),
  ).toBeVisible();
  await expect(
    page.getByRole('button', { name: 'Idea: Connect urban wildlife corridors', exact: true }),
  ).toBeVisible();
});

test('header and footer stories adapt normal links outside the application components', async ({
  page,
}) => {
  await page.goto('http://127.0.0.1:6006/iframe.html?id=layout-header--playground&viewMode=story');
  await page
    .getByRole('navigation', { name: 'Main navigation', exact: true })
    .getByRole('link', { name: 'Create', exact: true })
    .click();
  await expect(page).toHaveURL(/components-node-form--playground/);
  await page.goto('http://127.0.0.1:6006/iframe.html?id=layout-footer--playground&viewMode=story');
  await page.getByRole('link', { name: 'Create a contribution', exact: true }).click();
  await expect(page).toHaveURL(/components-node-form--playground/);
});

test('application breadcrumb still switches API-loaded parents without story props', async ({
  page,
}) => {
  const node = 'b1000000-0000-4000-8000-000000000001';
  const original = 'b1000000-0000-4000-8000-000000000002';
  const alternative = 'b1000000-0000-4000-8000-000000000003';
  await page.route('**/atlas-session-api/**', (route) => {
    const path = new URL(route.request().url()).pathname;
    if (path.endsWith('/votes'))
      return route.fulfill({
        json: { voteCount: 0, averageVote: null, currentParticipantVote: null },
      });
    if (path.endsWith('/auth/session')) return route.fulfill({ status: 401, json: {} });
    return route.fulfill({ json: [] });
  });
  await page.route('**/atlas-public-api/**', (route) => {
    const url = new URL(route.request().url());
    if (url.pathname.endsWith('/parents'))
      return route.fulfill({
        json: url.pathname.includes(`/nodes/${node}/`)
          ? [
              { id: original, title: 'Original parent' },
              { id: alternative, title: 'Alternative parent' },
            ]
          : [],
      });
    if (url.pathname.endsWith('/ancestors')) {
      expect(url.searchParams.get('firstParent')).toBe(alternative);
      return route.fulfill({
        json: { items: [{ id: alternative, title: 'Alternative parent' }], hasMore: false },
      });
    }
    if (url.pathname.endsWith('/comments'))
      return route.fulfill({ json: { items: [], next: null } });
    if (/\/nodes\/[^/]+$/.test(url.pathname))
      return route.fulfill({ json: { id: node, requestedTypeIds: [], status: 'Active' } });
    if (url.pathname.endsWith('/nodes'))
      return route.fulfill({
        json: {
          items: [
            {
              nodeId: node,
              voteCount: 0,
              averageVote: null,
              card: {
                title: 'Parent switching',
                typeName: 'Question',
                authorName: 'Author',
                createdAt: '2026-10-21T12:00:00Z',
                status: 'Active',
                shortDescription: 'Application breadcrumb regression.',
                commentCount: 0,
                communities: '',
                isHidden: false,
                breadcrumbSegment: {
                  items: [{ nodeId: original, label: 'Original parent' }],
                  continuationNodeId: null,
                  hasAlternativeRoutes: true,
                  cycleDetected: false,
                },
              },
            },
          ],
          totalCount: 1,
          page: 1,
          pageSize: 3,
          hasNextPage: false,
        },
      });
    return route.fulfill({ json: [] });
  });
  await page.goto(`http://127.0.0.1:4173/nodes?id=${node}`);
  const breadcrumb = page.getByRole('navigation', { name: 'Node breadcrumb' });
  await breadcrumb.getByRole('button', { name: 'Choose another parent path' }).click();
  await breadcrumb.getByRole('button', { name: 'Alternative parent', exact: true }).click();
  await expect(
    breadcrumb.getByRole('button', { name: 'Original parent', exact: true }),
  ).toHaveCount(0);
  await expect(
    breadcrumb.getByRole('button', { name: 'Alternative parent', exact: true }),
  ).toBeVisible();
});
