import { expect, test, type Page } from '@playwright/test';
const parent = 'b1000000-0000-4000-8000-000000000001';
const created = 'b1000000-0000-4000-8000-000000000002';
const idea = 'a1000000-0000-4000-8000-000000000001';
const custom = 'a1000000-0000-4000-8000-000000000002';
const communityIds = ['e1000000-0000-4000-8000-000000000001', 'e1000000-0000-4000-8000-000000000002'];

async function setup(page: Page, initiallySignedIn = false) {
  let signedIn = initiallySignedIn; let failNode = false;
  const seedId = 'd1000000-0000-4000-8000-000000000001';
  let seedPresent = false;
  let composition: string | undefined;
  const writes: { path: string; body: Record<string, unknown> }[] = [];
  await page.route('**/atlas-public-api/**', route => {
    const url = new URL(route.request().url());
    if (url.pathname.endsWith('/catalog/node-types')) return route.fulfill({ json: [{ id: idea, name: 'Idea', pluralName: 'Ideas' }] });
    if (url.pathname.endsWith('/reactions')) return route.fulfill({ json: [] });
    if (url.pathname.endsWith('/communities'))
      return route.fulfill({ json: communityIds.map((id, i) => ({ id, name: `Community ${i + 1}`, status: 'Active' })) });
    if (url.pathname.endsWith('/description')) return route.fulfill({ json: seedPresent ? [{ id: seedId, kind: composition ? 'sectioned' : 'markdown', content: composition ? { composition } : { markdown: 'The full content' } }] : [] });
    if (url.pathname.endsWith('/comments')) return route.fulfill({ json: { items: [], next: null } });
    if (url.pathname.endsWith(`/nodes/${parent}`) || url.pathname.endsWith(`/nodes/${created}`)) return route.fulfill({ json: { id: parent, requestedTypeIds: [idea] } });
    const id = url.searchParams.get('NodeId') ?? parent;
    return route.fulfill({ json: { items: [{ nodeId: id, averageVote: null, voteCount: 0, card: { title: id === created ? 'Saved node' : 'Parent node', typeName: 'Idea', authorName: 'Sam', status: 'Active', createdAt: '2026-10-22T12:00:00Z', breadcrumb: '', childTypes: [{ typeId: idea, count: 0, label: '0 Ideas' }], communities: '', commentCount: 0, isHidden: false } }], totalCount: 1, page: 1, pageSize: 3, hasNextPage: false } });
  });
  await page.route('**/atlas-session-api/**', route => {
    if (route.request().method() === 'GET' && new URL(route.request().url()).pathname.endsWith('/moderation/queue')) return route.fulfill({ status: 403 });
    if (route.request().method() === 'GET' && /^\/atlas-session-api\/nodes\/[^/]+$/.test(new URL(route.request().url()).pathname)) return route.fulfill({ json: { hidden: false, requestedTypeIds: [] } });
    if (new URL(route.request().url()).pathname.endsWith('/votes')) return route.fulfill({ json: { voteCount: 0, averageVote: null, currentParticipantVote: null } });
    const request = route.request(); const path = new URL(request.url()).pathname;
    if (path.startsWith('/atlas-session-api/globe/')) return route.fulfill({ status: 503, json: {} });
    if (path.endsWith('/auth/session')) return signedIn ? route.fulfill({ json: { participantId: 'participant' } }) : route.fulfill({ status: 401, json: {} });
    if (path.endsWith('/auth/csrf')) return route.fulfill({ json: { token: 'csrf-fixture' } });
    if (request.method() === 'GET' && path.endsWith('/reactions')) return route.fulfill({ json: [] });
    expect(request.headers()['x-csrf-token']).toBe('csrf-fixture');
    const body = request.postDataJSON() ?? {}; writes.push({ path, body });
    if (path.endsWith('/auth/login')) { signedIn = true; return route.fulfill({ status: 204 }); }
    if (path.endsWith('/auth/logout')) { signedIn = false; return route.fulfill({ status: 204 }); }
    if (path.endsWith('/catalog/node-types')) return route.fulfill({ json: { id: custom, name: body.name } });
    if (failNode) return route.fulfill({ status: 400, json: {} });
    if (path.endsWith('/nodes')) { seedPresent = true; composition = typeof body.composition === 'string' ? body.composition : undefined; return route.fulfill({ status: 201, json: { id: created } }); }
    if (path.endsWith('/description/blocks') && request.method() === 'POST')
      return route.fulfill({ status: 201, json: { id: 'd1000000-0000-4000-8000-000000000002', kind: body.kind, content: { markdown: body.text } } });
    if (request.method() === 'DELETE') { seedPresent = false; return route.fulfill({ status: 204 }); }
    return route.fulfill({ status: 204 });
  });
  return { writes, fail: (value: boolean) => failNode = value };
}
async function login(page: Page) {
  const form = page.getByRole('form', { name: 'Sign in to Atlas' });
  await form.getByLabel('Email', { exact: true }).fill('sam@example.test');
  await form.getByLabel('Password', { exact: true }).fill('fixture-password');
  await form.getByRole('button', { name: 'Sign in', exact: true }).click();
}
async function fill(page: Page) {
  await page.getByLabel('Title', { exact: true }).fill('Created through the frontend');
  await page.getByRole('textbox', { name: 'Node content: editable rich-text document' }).fill('The full content');
}
for (const environment of ['storybook', 'spa']) {
  test(`${environment} root creation prompts sign-in and opens the saved node`, async ({ page }) => {
    const { writes } = await setup(page);
    await page.goto(environment === 'storybook' ? 'http://127.0.0.1:6006/iframe.html?id=components-node-form--playground&viewMode=story' : 'http://127.0.0.1:4173/new');
    await expect(page.getByRole('form', { name: 'Create root node' })).toHaveCount(0);
    if (environment === 'spa') await expect(page).toHaveURL(/\/login\?returnTo=/);
    await login(page);
    if (environment === 'spa') await expect(page).toHaveURL('http://127.0.0.1:4173/new');
    await fill(page);
    const form = page.getByRole('form', { name: 'Create root node' });
    const type = form.getByRole('combobox', { name: 'Node type' });
    const byline = form.locator('.byline');
    const editor = form.getByRole('textbox', { name: 'Node content: editable rich-text document' });
    const toolbar = form.getByRole('group', { name: 'Text formatting', exact: true });
    const left = (await type.boundingBox())!.x;
    for (const element of [byline, editor, toolbar]) {
      expect((await element.boundingBox())!.x).toBeCloseTo(left, 0);
    }
    await page.getByLabel('Requested contributions', { exact: true }).selectOption(idea);
    if (environment === 'spa') {
      const communities = page.getByRole('combobox', { name: 'Communities (optional)' });
      for (const id of communityIds) await communities.selectOption(id);
    }
    await page.getByRole('button', { name: 'Create node', exact: true }).click();
    await expect(page.getByRole('heading', { name: 'Saved node' })).toBeVisible();
    const save = writes.find(write => write.path.endsWith('/nodes'))!;
    expect(save.body).toMatchObject({ title: 'Created through the frontend', markdown: 'The full content', typeId: idea, parentId: null, requestedTypeIds: [idea] });
    expect(JSON.parse(String(save.body.composition))).toMatchObject({ version: 1, blocks: [{ kind: 'text', text: 'The full content' }] });
    expect(writes.some(write => write.path.includes('/description/blocks'))).toBe(false);
    expect(writes.filter(write => write.path.endsWith('/nodes'))).toHaveLength(1);
    expect(writes.filter(write => write.path.includes('/communities/')).map(write => write.path))
      .toEqual(environment === 'spa' ? communityIds.map(id => `/atlas-session-api/communities/${id}/nodes/${created}`) : []);
  });
}

test('child creation keeps parent and selected type through sign-in', async ({ page }) => {
  const { writes } = await setup(page);
  await page.goto(`http://127.0.0.1:6006/iframe.html?id=compositions-node-page--playground&viewMode=story&args=nodeId:${parent}`);
  await page.getByRole('tab', { name: '0 Ideas', exact: true }).click();
  await page.getByRole('button', { name: 'Add Idea', exact: true }).click();
  await expect(page.getByRole('form', { name: 'Create child node' })).toHaveCount(0);
  await login(page);
  await expect(page.getByLabel('Node type', { exact: true })).toHaveValue(idea);
  await expect(page.getByRole('form', { name: 'Create child node' }).getByRole('navigation')).toContainText('Parent node');
  await fill(page); await page.getByRole('button', { name: 'Create node', exact: true }).click();
  await expect(page.getByRole('heading', { name: 'Saved node' })).toBeVisible();
  expect(writes.find(write => write.path.endsWith('/nodes'))?.body.parentId).toBe(parent);
});

test('custom type creation is reused on a failed node retry and the draft remains intact', async ({ page }) => {
  const fixture = await setup(page, true); fixture.fail(true);
  await page.goto('http://127.0.0.1:6006/iframe.html?id=components-node-form--playground&viewMode=story');
  await fill(page); await page.getByLabel('Node type', { exact: true }).selectOption('new');
  await page.getByLabel('Type name', { exact: true }).fill('Evidence');
  await page.getByRole('checkbox', { name: 'Automatically pluralize this type', exact: true }).uncheck();
  await page.getByRole('button', { name: 'Create node', exact: true }).click();
  await expect(page.getByRole('alert')).toContainText('Your draft has been kept');
  await expect(page.getByLabel('Title', { exact: true })).toHaveValue('Created through the frontend');
  fixture.fail(false);
  await page.getByRole('button', { name: 'Create node', exact: true }).click();
  await expect(page.getByRole('heading', { name: 'Saved node' })).toBeVisible();
  expect(fixture.writes.filter(write => write.path.endsWith('/catalog/node-types'))).toHaveLength(1);
  expect(fixture.writes.find(write => write.path.endsWith('/catalog/node-types'))?.body.autoPluralize).toBe(false);
  expect(fixture.writes.filter(write => write.path.endsWith('/nodes'))).toHaveLength(2);
});

test('create explains missing fields before sending a request', async ({ page }) => {
  const { writes } = await setup(page, true);
  await page.goto('http://127.0.0.1:4173/new');
  const create = page.getByRole('button', { name: 'Create node', exact: true });
  await create.click();
  await expect(page.getByRole('alert')).toHaveText('Enter a node title.');
  expect(writes.some(write => write.path.endsWith('/nodes'))).toBe(false);
  await fill(page);
  await create.click();
  await expect(page.getByRole('heading', { name: 'Saved node' })).toBeVisible();
  expect(writes.filter(write => write.path.endsWith('/nodes'))).toHaveLength(1);
});

test('a photo missing alt text is flagged before publishing', async ({ page }) => {
  const { writes } = await setup(page, true);
  await page.route('https://example.test/photo.jpg', route => route.abort());
  await page.goto('http://127.0.0.1:4173/new');
  await fill(page);
  await page.getByRole('button', { name: 'Add section', exact: true }).click();
  await page.getByRole('button', { name: 'Add this layout' }).click();
  await page.getByRole('button', { name: /Add block · Section 2/ }).click();
  await page.getByRole('button', { name: 'Image', exact: true }).click();
  const sidebar = page.getByRole('complementary', { name: 'Editor controls' });
  await expect(sidebar.getByLabel('Image URL')).toBeVisible();
  await sidebar.getByRole('button', { name: 'Collapse editor controls' }).click();
  await expect(sidebar).toBeHidden();
  await page.getByText('Add image details', { exact: true }).click();
  await expect(sidebar.getByLabel('Image URL')).toBeVisible();
  await sidebar.getByLabel('Image URL').fill('https://example.test/photo.jpg');
  await expect(page.getByRole('button', { name: 'Edit image', exact: true })
    .getByText('Alternative text is required.', { exact: true })).toBeVisible();
  await expect(page.getByLabel('Alternative text (required)')).toHaveAttribute('aria-invalid', 'true');
  await page.getByLabel('Alternative text (required)').fill('Trees along a walkway');
  await expect(page.getByText('Alternative text is required.', { exact: true })).toHaveCount(0);
  expect(writes.some(write => write.path.endsWith('/nodes'))).toBe(false);
});

test('editor controls fit the available viewport and close on an outside click', async ({ page }) => {
  await setup(page, true);
  await page.setViewportSize({ width: 1280, height: 800 });
  await page.goto('http://127.0.0.1:4173/new');
  const toggle = page.getByRole('button', { name: 'Editor settings' });
  const sidebar = page.getByRole('complementary', { name: 'Editor controls' });
  await toggle.click();
  await expect(sidebar).toBeVisible();
  await expect.poll(async () => {
    const header = (await page.getByRole('banner').boundingBox())!;
    const panel = (await sidebar.boundingBox())!;
    return Math.abs(panel.y - Math.max(0, header.y + header.height));
  }).toBeLessThan(1);
  await sidebar.getByRole('heading').click();
  await expect(sidebar).toBeVisible();
  await page.getByRole('textbox', { name: 'Title', exact: true }).click();
  await expect(sidebar).toBeHidden();
  await expect(page.getByRole('textbox', { name: 'Title', exact: true })).toBeFocused();

  await page.setViewportSize({ width: 390, height: 740 });
  await toggle.click();
  await expect(sidebar).toBeVisible();
  const mobile = (await sidebar.boundingBox())!;
  expect(mobile.y).toBeCloseTo(0);
  expect(mobile.height).toBeCloseTo(740);
});
