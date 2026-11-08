import { expect, test } from '@playwright/test';
import { preparePin, globeRadius } from '../src/lib/globe/pinGeometry';
test('fixed pin geometry is deterministic, anchored on the sphere and outward', () => {
  for (const latitude of [-90, -45, 0, 18, 45, 90]) {
    const pin = { latitude, longitude: 37, visitors: 1 };
    const prepared = preparePin(pin);
    expect(prepared).toEqual(preparePin(pin));
    expect(Math.hypot(...prepared.base)).toBeCloseTo(globeRadius, 8);
    const direction = prepared.head.map((value, i) => value - prepared.base[i]);
    expect(Math.hypot(...direction)).toBeCloseTo(23, 8);
    expect(direction.reduce((sum, value, i) => sum + value * prepared.base[i], 0)).toBeGreaterThan(
      0,
    );
  }
});
