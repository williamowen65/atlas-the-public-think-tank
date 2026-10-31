import { test } from 'node:test';
import assert from 'node:assert/strict';
import { join } from 'node:path';
import { lanEnvironment } from './lanEnvironment.mjs';

test('missing origin fails instead of launching HTTP', () => {
  assert.throws(() => lanEnvironment({}, () => true), /ATLAS_LAN_ORIGIN/);
});
test('Windows certificate defaults are derived without mutating the parent environment', () => {
  const env = { LOCALAPPDATA: '/local', ATLAS_LAN_ORIGIN: 'https://192.168.50.55:5173' };
  const actual = lanEnvironment(env, () => true);
  assert.equal(actual.ATLAS_LAN_CERT, join('/local', 'Atlas', 'dev-certs', 'atlas.pem'));
  assert.equal(actual.ATLAS_LAN_KEY, join('/local', 'Atlas', 'dev-certs', 'atlas-key.pem'));
  assert.equal(env.ATLAS_LAN_CERT, undefined);
});
test('explicit paths work without Windows defaults', () => {
  const env = { ATLAS_LAN_ORIGIN: 'https://192.168.50.55:5173', ATLAS_LAN_CERT: '/cert', ATLAS_LAN_KEY: '/key' };
  assert.deepEqual(lanEnvironment(env, () => true), env);
});
test('missing certificate files stop startup with actionable guidance', () => {
  assert.throws(() => lanEnvironment({ LOCALAPPDATA: '/local', ATLAS_LAN_ORIGIN: 'https://192.168.50.55:5173' }, () => false), /ATLAS_LAN_CERT/);
});
