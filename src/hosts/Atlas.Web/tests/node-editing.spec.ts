import { expect, test, type Page } from '@playwright/test';
const node = 'b1000000-0000-4000-8000-000000000001';
const author = 'c1000000-0000-4000-8000-000000000001';
const block = 'd1000000-0000-4000-8000-000000000001';
const type = 'a1000000-0000-4000-8000-000000000001';
async function setup(page: Page, participant: string | null = author) {
  let title = 'Editable node',
    intro = 'Original introduction',
    markdown = 'Original Markdown';
  let fail = false;
  const writes: { path: string; body: Record<string, unknown> }[] = [];
  await page.route('**/atlas-public-api/**', (route) => {
    const path = new URL(route.request().url()).pathname;
    if (path.endsWith('/catalog/node-types'))
      return route.fulfill({ json: [{ id: type, name: 'Idea' }] });
    if (path.endsWith('/reactions')) return route.fulfill({ json: [] });
    if (path.endsWith('/description'))
      return route.fulfill({ json: [{ id: block, kind: 'markdown', content: { markdown } }] });
    if (path.endsWith('/comments')) return route.fulfill({ json: { items: [], next: null } });
    if (path.endsWith(`/nodes/${node}`))
      return route.fulfill({
        json: {
          id: node,
          authorId: author,
          title,
          shortDescription: intro,
          requestedTypeIds: [],
          status: 'Active',
        },
      });
    return route.fulfill({
      json: {
        items: [
          {
            nodeId: node,
            averageVote: null,
            voteCount: 0,
            card: {
              title,
              shortDescription: intro,
              typeName: 'Idea',
              authorName: 'Sam',
              authorId: author,
              status: 'Active',
              createdAt: '2026-10-22T12:00:00Z',
              breadcrumb: '',
              childTypes: [],
              communities: '',
              commentCount: 0,
              isHidden: false,
            },
          },
        ],
        totalCount: 1,
        page: 1,
        pageSize: 3,
        hasNextPage: false,
      },
    });
  });
  await page.route('**/atlas-session-api/**', (route) => {
    const request = route.request(),
      path = new URL(request.url()).pathname;
    if (path.endsWith('/auth/session'))
      return participant
        ? route.fulfill({ json: { participantId: participant } })
        : route.fulfill({ status: 401, json: {} });
    if (path.endsWith('/auth/csrf')) return route.fulfill({ json: { token: 'editor-csrf' } });
    if (path.endsWith('/votes'))
      return route.fulfill({
        json: { voteCount: 0, averageVote: null, currentParticipantVote: null },
      });
    if (request.method() === 'GET') return route.fulfill({ json: [] });
    expect(request.method()).toBe('PUT');
    expect(request.headers()['x-csrf-token']).toBe('editor-csrf');
    const body = request.postDataJSON();
    writes.push({ path, body });
    if (fail) return route.fulfill({ status: 403, json: {} });
    if (path.endsWith('/short-description')) intro = body.text;
    if (path.endsWith('/title')) title = body.text;
    if (path.endsWith(`/blocks/${block}`)) {
      markdown = body.text;
      return route.fulfill({ json: { id: block, kind: 'markdown', content: { markdown } } });
    }
    return route.fulfill({ status: 204 });
  });
  return {
    writes,
    fail: (value: boolean) => {
      fail = value;
    },
  };
}
const story = `http://127.0.0.1:6006/iframe.html?id=compositions-node-page--playground&viewMode=story&args=nodeId:${node}`;
for (const environment of ['storybook', 'spa']) {
  test(`${environment}: owner edits introduction and Markdown repeatedly without replacing the block`, async ({
    page,
  }) => {
    const fixture = await setup(page);
    await page.goto(environment === 'storybook' ? story : `http://127.0.0.1:4173/nodes?id=${node}`);
    await page.getByRole('button', { name: 'Edit your node', exact: true }).click();
    const editor = page.getByRole('dialog', { name: 'Edit node', exact: true });
    await editor.getByLabel('Short description', { exact: true }).fill('Updated introduction');
    await editor.getByRole('button', { name: 'Save short description', exact: true }).click();
    await expect(editor.getByRole('status')).toContainText('Saved.');
    await editor.getByLabel('Markdown', { exact: true }).fill('Testing testing');
    fixture.fail(true);
    await editor.getByRole('button', { name: 'Save block 1', exact: true }).click();
    await expect(editor.getByRole('alert')).toContainText('Unsaved edits have been kept');
    await expect(editor.getByLabel('Markdown', { exact: true })).toHaveValue('Testing testing');
    fixture.fail(false);
    await editor.getByRole('button', { name: 'Save block 1', exact: true }).click();
    await expect(editor.getByRole('status')).toContainText('Saved.');
    await editor.getByLabel('Markdown', { exact: true }).fill('Revised again');
    await editor.getByRole('button', { name: 'Save block 1', exact: true }).click();
    await expect(editor.getByRole('status')).toContainText('Saved.');
    await editor.getByRole('button', { name: 'Close editor', exact: true }).click();
    await expect(page.getByText('Updated introduction', { exact: true })).toBeVisible();
    await expect(page.getByText('Revised again', { exact: true })).toBeVisible();
    expect(fixture.writes.filter((w) => w.path.endsWith(`/blocks/${block}`))).toHaveLength(3);
  });
}
for (const participant of [null, 'another-participant']) {
  test(`edit control is hidden for ${participant ?? 'anonymous users'}`, async ({ page }) => {
    const fixture = await setup(page, participant);
    await page.goto(story);
    await expect(page.getByRole('heading', { name: 'Idea: Editable node' })).toBeVisible();
    await expect(page.getByRole('button', { name: 'Edit your node', exact: true })).toHaveCount(0);
    expect(fixture.writes).toHaveLength(0);
  });
}
test('private revision survives reopening without publishing and restores only on request', async ({
  page,
}) => {
  const fixture = await setup(page);
  await page.goto(story);
  await page.getByRole('button', { name: 'Edit your node', exact: true }).click();
  const editor = page.getByRole('dialog', { name: 'Edit node', exact: true });
  await editor.getByLabel('Markdown', { exact: true }).fill('Private revision');
  await editor.getByRole('button', { name: 'Save private revision draft', exact: true }).click();
  await expect(editor.getByRole('status')).toContainText('saved privately');
  expect(fixture.writes).toHaveLength(0);
  page.on('dialog', (dialog) => dialog.accept());
  await editor.getByRole('button', { name: 'Close editor', exact: true }).click();
  await page.getByRole('button', { name: 'Edit your node', exact: true }).click();
  await expect(editor.getByLabel('Markdown', { exact: true })).toHaveValue('Original Markdown');
  await editor.getByRole('button', { name: 'Restore private revision draft', exact: true }).click();
  await expect(editor.getByLabel('Markdown', { exact: true })).toHaveValue('Private revision');
  expect(fixture.writes).toHaveLength(0);
});

test('owned cards expose the same editor from the discovery feed', async ({ page }) => {
  await setup(page);
  await page.goto(
    'http://127.0.0.1:6006/iframe.html?id=compositions-feed--playground&viewMode=story',
  );
  await page.getByRole('button', { name: 'Edit your node', exact: true }).first().click();
  await expect(
    page
      .getByRole('dialog', { name: 'Edit node', exact: true })
      .getByLabel('Markdown', { exact: true }),
  ).toHaveValue('Original Markdown');
});

test('node edit proxy rejects foreign origins and unrequested block operations', async ({
  request,
}) => {
  const base = 'http://127.0.0.1:6007/atlas-session-api';
  for (const suffix of [
    `nodes/${node}/title`,
    `nodes/${node}/short-description`,
    `nodes/${node}/description/blocks/${block}`,
  ]) {
    const result = await request.put(`${base}/${suffix}`, {
      headers: { Origin: 'https://example.test' },
      data: { text: 'Untrusted change' },
    });
    expect(result.status()).toBe(403);
  }
  const remove = await request.delete(`${base}/nodes/${node}/description/blocks/${block}`, {
    headers: { Origin: 'http://127.0.0.1:6007' },
  });
  expect(remove.status()).toBe(405);
});
