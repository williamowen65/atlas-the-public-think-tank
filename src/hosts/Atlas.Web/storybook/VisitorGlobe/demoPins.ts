import { geoContains, geoBounds, type GeoPermissibleObjects } from 'd3-geo';
import type { GlobePin } from '../../src/lib/globe/presence';
export type DemoDistribution = 'worldwide' | 'uniform' | 'clustered';
// Illustrative regions, not population weights or account data.
const regions: [number, number][] = [
  [-122, 47],
  [-118, 34],
  [-97, 33],
  [-87, 42],
  [-74, 41],
  [-99, 19],
  [-74, 5],
  [-47, -24],
  [-58, -35],
  [0, 51],
  [2, 49],
  [13, 52],
  [29, 41],
  [37, 56],
  [-7, 34],
  [3, 7],
  [31, 30],
  [37, -1],
  [28, -26],
  [55, 25],
  [73, 19],
  [77, 29],
  [90, 24],
  [101, 14],
  [107, -6],
  [121, 31],
  [116, 40],
  [127, 38],
  [140, 36],
  [151, -34],
];
const clusters: [number, number][] = [
  [-122, 47],
  [2, 49],
  [140, 36],
];
export function demoPins(
  count: number,
  distribution: DemoDistribution,
  land?: GeoPermissibleObjects,
): GlobePin[] {
  if (distribution === 'worldwide' && !land) return [];
  let seed = 1729;
  const random = () => {
    seed = (Math.imul(seed, 1664525) + 1013904223) >>> 0;
    return seed / 4294967296;
  };
  const colors = ['#b73539', '#d5a51c', '#28765c', '#356b9b', '#925493'];
  const total = Math.max(0, Math.min(10000, Math.floor(Number.isFinite(count) ? count : 0)));
  const features: GeoPermissibleObjects[] =
    land && 'features' in land && Array.isArray(land.features) ? land.features : land ? [land] : [];
  const indexed = features.map((feature) => ({ feature, bounds: geoBounds(feature) }));
  const onLand = (longitude: number, latitude: number) =>
    indexed.some(
      ({ feature, bounds: [[west, south], [east, north]] }) =>
        latitude >= south &&
        latitude <= north &&
        (west <= east
          ? longitude >= west && longitude <= east
          : longitude >= west || longitude <= east) &&
        geoContains(feature, [longitude, latitude]),
    );
  const pins: GlobePin[] = [];
  for (let attempts = 0; pins.length < total && attempts < total * 80; attempts++) {
    let longitude: number, latitude: number;
    if (distribution === 'clustered') {
      const center = clusters[pins.length % clusters.length];
      longitude = center[0] + (random() - 0.5) * 1.8;
      latitude = center[1] + (random() - 0.5) * 1.8;
    } else if (distribution === 'uniform' || random() < 0.2) {
      longitude = random() * 360 - 180;
      latitude = (Math.asin(random() * 2 - 1) * 180) / Math.PI;
    } else {
      const center = regions[Math.floor(random() * regions.length)];
      longitude = center[0] + (random() - 0.5) * 20;
      latitude = center[1] + (random() - 0.5) * 16;
    }
    longitude = Math.round(longitude);
    latitude = Math.round(latitude);
    if (distribution === 'worldwide' && !onLand(longitude, latitude)) continue;
    pins.push({
      longitude,
      latitude,
      visitors: 1,
      color: colors[Math.floor(random() * colors.length)],
    });
  }
  return pins;
}
