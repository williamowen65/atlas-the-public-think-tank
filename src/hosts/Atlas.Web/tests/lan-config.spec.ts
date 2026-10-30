import { expect, test } from '@playwright/test';
import { lanOrigin } from '../dev/lanHttps';

const certificates = { ATLAS_LAN_CERT: 'local-cert.pem', ATLAS_LAN_KEY: 'local-key.pem' };
test('LAN mode is disabled unless explicitly configured', () => {
  expect(lanOrigin({})).toBeUndefined();
});
for (const origin of ['https://192.168.1.25:5173', 'https://10.0.0.2:5173', 'https://172.16.0.2:5173']) {
  test(`accepts exact private HTTPS origin ${origin}`, () => {
    expect(lanOrigin({ ...certificates, ATLAS_LAN_ORIGIN: origin })?.origin).toBe(origin);
  });
}
for (const origin of ['http://192.168.1.25:5173', 'https://example.com', 'https://8.8.8.8', 'https://192.168.1.25/path', 'https://user@192.168.1.25', 'https://192.168.1.25?query=1']) {
  test(`rejects unsupported LAN origin ${origin}`, () => {
    expect(() => lanOrigin({ ...certificates, ATLAS_LAN_ORIGIN: origin })).toThrow();
  });
}
test('LAN mode requires both certificate and key paths', () => {
  expect(() => lanOrigin({ ATLAS_LAN_ORIGIN: 'https://192.168.1.25:5173' })).toThrow();
});
