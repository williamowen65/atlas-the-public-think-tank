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

test('worldwide demo fixtures are dispersed, repeatable and independently colored', () => {
  const pins = demoPins(1000, 'worldwide');
  expect(pins).toEqual(demoPins(1000, 'worldwide'));
  expect(new Set(pins.map((pin) => Math.floor(pin.longitude / 30))).size).toBe(12);
  expect(new Set(pins.map((pin) => Math.floor(pin.latitude / 15))).size).toBeGreaterThan(9);
  expect(
    new Set(pins.filter((pin) => pin.longitude > 0 && pin.latitude > 0).map((pin) => pin.color))
      .size,
  ).toBe(5);
});
