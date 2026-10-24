import { expect, test, type Page } from '@playwright/test';
const parent = 'b1000000-0000-4000-8000-000000000001';
const created = 'b1000000-0000-4000-8000-000000000002';
const idea = 'a1000000-0000-4000-8000-000000000001';
const custom = 'a1000000-0000-4000-8000-000000000002';
async function setup(page: Page, initiallySignedIn = false) {
  let signedIn = initiallySignedIn; let failNode = false;
  const writes: { path: string; body: Record<string, unknown> }[] = [];
  await page.route('**/atlas-public-api/**', route => {
    const url = new URL(route.request().url());
    if (url.pathname.endsWith('/catalog/node-types')) return route.fulfill({ json: [{ id: idea, name: 'Idea', pluralName: 'Ideas' }] });
    if (url.pathname.endsWith('/reactions') || url.pathname.endsWith('/description')) return route.fulfill({ json: [] });
    if (url.pathname.endsWith('/comments')) return route.fulfill({ json: { items: [], next: null } });
    if (url.pathname.endsWith(`/nodes/${parent}`) || url.pathname.endsWith(`/nodes/${created}`)) return route.fulfill({ json: { id: parent, requestedTypeIds: [idea] } });
    const id = url.searchParams.get('NodeId') ?? parent;
    return route.fulfill({ json: { items: [{ nodeId: id, averageVote: null, voteCount: 0, card: { title: id === created ? 'Saved node' : 'Parent node', shortDescription: 'Introduction', typeName: 'Idea', authorName: 'Sam', status: 'Active', createdAt: '2026-10-22T12:00:00Z', breadcrumb: '', childTypes: [{ typeId: idea, count: 0, label: '0 Ideas' }], communities: '', commentCount: 0, isHidden: false } }], totalCount: 1, page: 1, pageSize: 3, hasNextPage: false } });
  });
  await page.route('**/atlas-session-api/**', route => {
    if (new URL(route.request().url()).pathname.endsWith('/votes')) return route.fulfill({ json: { voteCount: 0, averageVote: null, currentParticipantVote: null } });
    const request = route.request(); const path = new URL(request.url()).pathname;
    if (path.endsWith('/auth/session')) return signedIn ? route.fulfill({ json: { participantId: 'participant' } }) : route.fulfill({ status: 401, json: {} });
    if (path.endsWith('/auth/csrf')) return route.fulfill({ json: { token: 'csrf-fixture' } });
    if (request.method() === 'GET' && path.endsWith('/reactions')) return route.fulfill({ json: [] });
    expect(request.headers()['x-csrf-token']).toBe('csrf-fixture');
    const body = request.postDataJSON() ?? {}; writes.push({ path, body });
    if (path.endsWith('/auth/login')) { signedIn = true; return route.fulfill({ status: 204 }); }
    if (path.endsWith('/auth/logout')) { signedIn = false; return route.fulfill({ status: 204 }); }
    if (path.endsWith('/catalog/node-types')) return route.fulfill({ json: { id: custom, name: body.name } });
    if (failNode) return route.fulfill({ status: 400, json: {} });
    return route.fulfill({ status: 201, json: { id: created } });
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
  await page.getByLabel('Short description', { exact: true }).fill('An introduction');
  await page.getByLabel('Content (Markdown)', { exact: true }).fill('The full content');
}
for (const environment of ['storybook', 'spa']) {
  test(`${environment} root creation prompts sign-in and opens the saved node`, async ({ page }) => {
    const { writes } = await setup(page);
    await page.goto(environment === 'storybook' ? 'http://127.0.0.1:6006/iframe.html?id=components-node-form--playground&viewMode=story' : 'http://127.0.0.1:4173/new');
    await expect(page.getByRole('form', { name: 'Create root node' })).toHaveCount(0);
    await login(page); await fill(page);
    await page.getByLabel('Requested contributions', { exact: true }).selectOption(idea);
    await page.getByRole('button', { name: 'Create node', exact: true }).click();
    await expect(page.getByRole('heading', { name: 'Idea: Saved node' })).toBeVisible();
    const save = writes.find(write => write.path.endsWith('/nodes'))!;
    expect(save.body).toEqual({ title: 'Created through the frontend', shortDescription: 'An introduction', markdown: 'The full content', typeId: idea, parentId: null, requestedTypeIds: [idea] });
    expect(writes.filter(write => write.path.endsWith('/nodes'))).toHaveLength(1);
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
  await expect(page.getByRole('heading', { name: 'Idea: Saved node' })).toBeVisible();
  expect(writes.find(write => write.path.endsWith('/nodes'))?.body.parentId).toBe(parent);
});

test('custom type creation is reused on a failed node retry and the draft remains intact', async ({ page }) => {
  const fixture = await setup(page, true); fixture.fail(true);
  await page.goto('http://127.0.0.1:6006/iframe.html?id=components-node-form--playground&viewMode=story');
  await fill(page); await page.getByLabel('Node type', { exact: true }).selectOption('new');
  await page.getByLabel('Type name', { exact: true }).fill('Evidence');
  await page.getByLabel('Automatically pluralize this type').uncheck();
  await page.getByRole('button', { name: 'Create node', exact: true }).click();
  await expect(page.getByRole('alert')).toContainText('Your draft has been kept');
  await expect(page.getByLabel('Title', { exact: true })).toHaveValue('Created through the frontend');
  fixture.fail(false);
  await page.getByRole('button', { name: 'Create node', exact: true }).click();
  await expect(page.getByRole('heading', { name: 'Idea: Saved node' })).toBeVisible();
  expect(fixture.writes.filter(write => write.path.endsWith('/catalog/node-types'))).toHaveLength(1);
  expect(fixture.writes.find(write => write.path.endsWith('/catalog/node-types'))?.body.autoPluralize).toBe(false);
  expect(fixture.writes.filter(write => write.path.endsWith('/nodes'))).toHaveLength(2);
});
