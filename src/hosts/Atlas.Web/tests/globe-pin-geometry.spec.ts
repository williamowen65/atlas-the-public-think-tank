import { expect, test } from '@playwright/test';
import { demoPins } from '../storybook/VisitorGlobe/demoPins';
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

test('saved pin colors reach the GPU geometry without location-based replacement', () => {
  expect(preparePin({ latitude: 0, longitude: 0, visitors: 1, color: '#2468ac' }).color).toBe(
    '#2468ac',
  );
});

test('uniform demo fixtures are dispersed, repeatable and independently colored', () => {
  const pins = demoPins(1000, 'uniform');
  expect(pins).toEqual(demoPins(1000, 'uniform'));
  expect(new Set(pins.map((pin) => Math.floor(((pin.longitude + 180) % 360) / 30))).size).toBe(12);
  expect(new Set(pins.map((pin) => Math.floor(pin.latitude / 15))).size).toBeGreaterThan(9);
  expect(
    new Set(pins.filter((pin) => pin.longitude > 0 && pin.latitude > 0).map((pin) => pin.color))
      .size,
  ).toBe(5);
});

test('globe picking inverts the map camera and rejects the area outside Earth', async () => {
  const { pickLocation } = await import('../src/lib/globe/pickLocation');
  const { geoOrthographic } = await import('d3-geo');
  expect(pickLocation(200, 200, 400, -25)).toEqual({ longitude: 25, latitude: 18 });
  expect(pickLocation(0, 0, 400, 0)).toBeNull();
  for (const [longitude, latitude, angle] of [
    [-122, 48, 120],
    [37, -10, -37],
    [80, 45, -60],
  ]) {
    const point = geoOrthographic().translate([200, 200]).scale(176).rotate([angle, -18])([
      longitude,
      latitude,
    ])!;
    expect(
      pickLocation(((point[0] + 20) / 440) * 400, ((point[1] + 20) / 440) * 400, 400, angle),
    ).toEqual({ longitude, latitude });
  }
});

test('realistic fixtures use land and remain dispersed', async () => {
  const { readFileSync } = await import('node:fs');
  const { geoContains } = await import('d3-geo');
  const land = JSON.parse(
    readFileSync(new URL('../static/globe/land.geojson', import.meta.url), 'utf8'),
  );
  const pins = demoPins(500, 'worldwide', land);
  expect(pins).toHaveLength(500);
  expect(pins.every((pin) => geoContains(land, [pin.longitude, pin.latitude]))).toBe(true);
  expect(
    new Set(pins.map((pin) => Math.floor(((pin.longitude + 180) % 360) / 30))).size,
  ).toBeGreaterThan(7);
});
