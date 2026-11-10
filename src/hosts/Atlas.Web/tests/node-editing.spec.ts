import { expect, test, type Page } from '@playwright/test';

const node = 'b1000000-0000-4000-8000-000000000001';
const author = 'c1000000-0000-4000-8000-000000000001';
const originalBlock = 'd1000000-0000-4000-8000-000000000001';
const addedBlock = 'd1000000-0000-4000-8000-000000000002';
const type = 'a1000000-0000-4000-8000-000000000001';
const story = `http://127.0.0.1:6006/iframe.html?id=compositions-node-page--playground&viewMode=story&args=nodeId:${node}`;

async function setup(page: Page, participant: string | null = author) {
  let title = 'Editable node';
  let shortDescription = 'Original introduction';
  let fail = false;
  let blocks = [{ id: originalBlock, kind: 'markdown', content: { markdown: 'Original Markdown' } }] as {
    id: string; kind: string;
    content: { markdown?: string; url?: string; altText?: string; caption?: string };
  }[];
  const writes: { path: string; method: string; body: Record<string, unknown> }[] = [];
  await page.route('**/atlas-public-api/**', (route) => {
    const path = new URL(route.request().url()).pathname;
    if (path.endsWith('/catalog/node-types'))
      return route.fulfill({ json: [{ id: type, name: 'Idea' }] });
    if (path.endsWith('/reactions')) return route.fulfill({ json: [] });
    if (path.endsWith('/description')) return route.fulfill({ json: blocks });
    if (path.endsWith('/comments')) return route.fulfill({ json: { items: [], next: null } });
    if (path.endsWith(`/nodes/${node}`))
      return route.fulfill({ json: {
        id: node, authorId: author, title, shortDescription, requestedTypeIds: [], status: 'Active',
      } });
    return route.fulfill({ json: {
      items: [{
        nodeId: node, averageVote: null, voteCount: 0,
        card: {
          title, shortDescription, typeName: 'Idea', authorName: 'Sam', authorId: author,
          status: 'Active', createdAt: '2026-10-22T12:00:00Z',
          breadcrumb: '', childTypes: [], communities: '', commentCount: 0, isHidden: false,
        },
      }],
      totalCount: 1, page: 1, pageSize: 3, hasNextPage: false,
    } });
  });
  await page.route('**/atlas-session-api/**', (route) => {
    const request = route.request();
    const path = new URL(request.url()).pathname;
    const method = request.method();
    if (path.startsWith('/atlas-session-api/globe/'))
      return route.fulfill({ status: 503, json: {} });
    if (path.endsWith('/auth/session'))
      return participant
        ? route.fulfill({ json: { participantId: participant } })
        : route.fulfill({ status: 401, json: {} });
    if (path.endsWith('/auth/csrf')) return route.fulfill({ json: { token: 'editor-csrf' } });
    if (path.endsWith('/moderation/queue')) return route.fulfill({ status: 403 });
    if (path.endsWith('/votes')) return route.fulfill({ json: {
      voteCount: 0, averageVote: null, currentParticipantVote: null,
    } });
    if (method === 'GET' && path.endsWith('/description'))
      return route.fulfill({ json: blocks });
    if (method === 'GET') return route.fulfill({ json: [] });
    expect(request.headers()['x-csrf-token']).toBe('editor-csrf');
    const body = (request.postDataJSON() ?? {}) as Record<string, unknown>;
    writes.push({ path, method, body });
    if (fail) return route.fulfill({ status: 403, json: {} });
    if (path.endsWith('/short-description')) shortDescription = String(body.text);
    if (path.endsWith('/title')) title = String(body.text);
    if (path.endsWith('/description/blocks') && method === 'POST') {
      const saved = {
        id: addedBlock, kind: String(body.kind),
        content: body.kind === 'image'
          ? { url: String(body.url), altText: String(body.text), caption: String(body.caption ?? '') }
          : { markdown: String(body.text) },
      };
      const index = typeof body.index === 'number' ? body.index : blocks.length;
      blocks.splice(index, 0, saved);
      return route.fulfill({ json: saved });
    }
    const blockId = path.match(/\/description\/blocks\/([0-9a-f-]{36})/i)?.[1];
    if (blockId && method === 'DELETE') blocks = blocks.filter((block) => block.id !== blockId);
    if (blockId && method === 'PUT' && path.endsWith('/position')) {
      const index = blocks.findIndex((block) => block.id === blockId);
      if (index >= 0) blocks.splice(Number(body.index), 0, blocks.splice(index, 1)[0]);
    } else if (blockId && method === 'PUT') {
      const block = blocks.find((value) => value.id === blockId);
      if (block) block.content = body.kind === 'image'
        ? { url: String(body.url), altText: String(body.text), caption: String(body.caption ?? '') }
        : { markdown: String(body.text) };
      return route.fulfill({ json: block });
    }
    return route.fulfill({ status: 204 });
  });
  return { writes, fail: (value: boolean) => fail = value, getBlocks: () => blocks };
}

for (const environment of ['storybook', 'spa']) {
  test(`${environment}: owner adds and reorders an image block, keeping existing block identity`, async ({ page }) => {
    const fixture = await setup(page);
    await page.goto(environment === 'storybook' ? story : `http://127.0.0.1:4173/nodes?id=${node}`);
    await page.getByRole('button', { name: 'Edit your node', exact: true }).click();
    const editor = page.getByRole('dialog', { name: 'Edit node', exact: true });
    await editor.getByRole('button', { name: '+ Image' }).click();
    await editor.getByLabel('Image URL (HTTP or HTTPS)').fill('https://example.org/city.jpg');
    await editor.getByLabel('Alternative text (required)').fill('A pedestrian square with trees');
    await editor.getByRole('button', { name: 'Move block 2 up' }).click();
    await editor.getByRole('button', { name: 'Publish changes' }).click();
    await expect(editor.getByRole('status')).toContainText('Changes published');
    expect(fixture.getBlocks().map((block) => block.id)).toEqual([addedBlock, originalBlock]);
    expect(fixture.writes.some((w) => w.method === 'POST' && w.path.endsWith('/description/blocks'))).toBe(true);
    expect(fixture.writes.some((w) => w.method === 'PUT' && w.path.endsWith('/position'))).toBe(true);
    await editor.getByRole('button', { name: 'Close editor' }).click();
    await expect(page.getByAltText('A pedestrian square with trees')).toBeVisible();
  });
}

test('image blocks require valid HTTP(S) URL and alternative text before publishing', async ({ page }) => {
  const fixture = await setup(page);
  await page.goto(story);
  await page.getByRole('button', { name: 'Edit your node', exact: true }).click();
  const editor = page.getByRole('dialog', { name: 'Edit node', exact: true });
  await editor.getByRole('button', { name: '+ Image' }).click();
  await editor.getByRole('button', { name: 'Publish changes' }).click();
  await expect(editor.getByRole('alert')).toContainText('HTTP(S)');
  await editor.getByLabel('Image URL (HTTP or HTTPS)').fill('https://example.org/city.jpg');
  await editor.getByRole('button', { name: 'Publish changes' }).click();
  await expect(editor.getByRole('alert')).toContainText('screen readers');
  expect(fixture.writes).toHaveLength(0);
});

test('failed changes retain the editor contents for a retry', async ({ page }) => {
  const fixture = await setup(page);
  await page.goto(story);
  await page.getByRole('button', { name: 'Edit your node', exact: true }).click();
  const editor = page.getByRole('dialog', { name: 'Edit node', exact: true });
  await editor.getByLabel('Text (Markdown)').fill('Unsaved edit');
  fixture.fail(true);
  await editor.getByRole('button', { name: 'Publish changes' }).click();
  await expect(editor.getByRole('alert')).toContainText('Some changes may already be public');
  await expect(editor.getByLabel('Text (Markdown)')).toHaveValue('Unsaved edit');
});

test('private revisions remain unpublished and can be restored', async ({ page }) => {
  const fixture = await setup(page);
  await page.goto(story);
  await page.getByRole('button', { name: 'Edit your node', exact: true }).click();
  const editor = page.getByRole('dialog', { name: 'Edit node', exact: true });
  await editor.getByLabel('Text (Markdown)').fill('Private revision');
  await editor.getByRole('button', { name: 'Save private revision', exact: true }).click();
  expect(fixture.writes).toHaveLength(0);
  page.on('dialog', (dialog) => dialog.accept());
  await editor.getByRole('button', { name: 'Close editor' }).click();
  await page.getByRole('button', { name: 'Edit your node', exact: true }).click();
  await expect(editor.getByLabel('Text (Markdown)')).toHaveValue('Original Markdown');
  await editor.getByRole('button', { name: 'Restore private revision' }).click();
  await expect(editor.getByLabel('Text (Markdown)')).toHaveValue('Private revision');
});

for (const participant of [null, 'another-participant']) {
  test(`edit control is hidden for ${participant ?? 'anonymous users'}`, async ({ page }) => {
    const fixture = await setup(page, participant);
    await page.goto(story);
    await expect(page.getByRole('heading', { name: 'Idea: Editable node' })).toBeVisible();
    await expect(page.getByRole('button', { name: 'Edit your node', exact: true })).toHaveCount(0);
    expect(fixture.writes).toHaveLength(0);
  });
}

test('node edit bridge rejects foreign-origin block mutations', async ({ request }) => {
  const base = 'http://127.0.0.1:6007/atlas-session-api';
  for (const [method, suffix] of [
    ['PUT', `nodes/${node}/title`],
    ['PUT', `nodes/${node}/description/blocks/${originalBlock}/position`],
    ['DELETE', `nodes/${node}/description/blocks/${originalBlock}`],
  ]) {
    const response = await request.fetch(`${base}/${suffix}`, {
      method, headers: { Origin: 'https://example.test' }, data: { index: 0 },
    });
    expect(response.status()).toBe(403);
  }
});
