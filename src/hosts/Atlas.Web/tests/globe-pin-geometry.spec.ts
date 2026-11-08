import { expect, test } from '@playwright/test';
import { geoOrthographic } from 'd3-geo';
import {
  globeRadius,
  projectPin,
  preparePin,
  projectPreparedPin,
  isPinVisible,
} from '../src/lib/globe/pinGeometry';

test('pin anchors follow the same camera as the map at every latitude', () => {
  for (const angle of [-180, -45, 0, 90, 180]) {
    const projection = geoOrthographic()
      .translate([200, 200])
      .scale(globeRadius)
      .rotate([angle, -18]);
    for (const latitude of [-90, -60, 0, 18, 60, 90]) {
      const pin = { latitude, longitude: 37, visitors: 1 };
      const actual = projectPin(pin, angle).base;
      const expected = projection([pin.longitude, latitude])!;
      expect(actual[0]).toBeCloseTo(expected[0], 8);
      expect(actual[1]).toBeCloseTo(expected[1], 8);
    }
  }
});

test('raised pins move continuously through the center and equator', () => {
  for (const latitude of [0, 0.001, 18, -18]) {
    const pin = { latitude, longitude: 0, visitors: 1 };
    let previous = projectPin(pin, -180);
    for (let angle = -179.9; angle <= 180; angle += 0.1) {
      const current = projectPin(pin, angle);
      expect(
        Math.hypot(current.head[0] - previous.head[0], current.head[1] - previous.head[1]),
      ).toBeLessThan(0.4);
      previous = current;
    }
  }
});

test('far-side raised heads extend beyond the limb while the globe hides their roots', () => {
  const { base, head } = projectPin({ latitude: 0, longitude: 100, visitors: 1 }, 0, 23, 0);
  expect(base[2]).toBeLessThan(0);
  expect(head[2]).toBeLessThan(0);
  expect(Math.hypot(base[0] - 200, base[1] - 200)).toBeLessThan(globeRadius);
  expect(Math.hypot(head[0] - 200, head[1] - 200)).toBeGreaterThan(globeRadius);
  const hidden = projectPin({ latitude: 0, longitude: 150, visitors: 1 }, 0, 23, 0);
  expect(Math.hypot(hidden.head[0] - 200, hidden.head[1] - 200)).toBeLessThan(globeRadius);
});

test('cached geometry matches direct projection and culling retains horizon tips', () => {
  const pin = { latitude: 0, longitude: 100, visitors: 1 };
  const prepared = preparePin(pin);
  expect(projectPreparedPin(prepared, 40)).toEqual(projectPin(pin, 40));
  expect(isPinVisible(projectPin(pin, 0), 3.5)).toBe(true);
  expect(isPinVisible(projectPin({ ...pin, longitude: 180 }, 0), 3.5)).toBe(false);
});
