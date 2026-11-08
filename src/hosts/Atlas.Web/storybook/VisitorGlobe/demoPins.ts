import type { GlobePin } from '../../src/lib/globe/presence';

// Synthetic fixtures for appearance exploration; worldwide includes oceans, clustered uses city anchors.
const clusters: [number, number][] = [
  [-122, 47],
  [2, 49],
  [140, 36],
];

export function demoPins(count: number, distribution: 'worldwide' | 'clustered'): GlobePin[] {
  const centers = clusters;
  let seed = 1729;
  const random = () => {
    seed = (Math.imul(seed, 1664525) + 1013904223) >>> 0;
    return seed / 4294967296;
  };
  const colors = ['#b73539', '#d5a51c', '#28765c', '#356b9b', '#925493'];
  const total = Math.max(0, Math.min(10000, Math.floor(Number.isFinite(count) ? count : 0)));
  return Array.from({ length: total }, (_, index) => {
    const [longitude, latitude] = centers[(index * 17) % centers.length];
    const color = colors[Math.floor(random() * colors.length)];
    if (distribution === 'worldwide')
      return {
        longitude: random() * 360 - 180,
        latitude: (Math.asin(random() * 2 - 1) * 180) / Math.PI,
        visitors: 1,
        color,
      };
    const spread = distribution === 'clustered' ? 1.8 : 4;
    return {
      longitude: Math.round((longitude + (random() - 0.5) * spread) * 100) / 100,
      latitude: Math.round((latitude + (random() - 0.5) * spread) * 100) / 100,
      visitors: 1,
      color,
    };
  });
}
