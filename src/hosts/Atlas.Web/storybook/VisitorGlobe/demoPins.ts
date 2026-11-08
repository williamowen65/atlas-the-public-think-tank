import type { GlobePin } from '../../src/lib/globe/presence';

// Fictional, city-inspired positions for appearance exploration; never account/location data.
const cities: [number, number][] = [
  [-123, 49],
  [-122, 38],
  [-118, 34],
  [-74, 41],
  [-99, 19],
  [-79, 44],
  [-157, 21],
  [-74, 5],
  [-77, -12],
  [-71, -33],
  [-58, -35],
  [-47, -24],
  [-66, 11],
  [-22, 64],
  [-4, 40],
  [0, 51],
  [2, 49],
  [13, 53],
  [18, 59],
  [29, 41],
  [37, 56],
  [-7, 34],
  [-17, 15],
  [3, 6],
  [31, 30],
  [37, -1],
  [18, -34],
  [28, -26],
  [48, -19],
  [55, 25],
  [73, 19],
  [77, 29],
  [85, 28],
  [101, 14],
  [104, 1],
  [107, -6],
  [121, 15],
  [121, 25],
  [127, 38],
  [140, 36],
  [151, -34],
  [145, -38],
  [175, -37],
];
const clusters: [number, number][] = [
  [-122, 47],
  [2, 49],
  [140, 36],
];

export function demoPins(count: number, distribution: 'worldwide' | 'clustered'): GlobePin[] {
  const centers = distribution === 'clustered' ? clusters : cities;
  let seed = 1729;
  const random = () => {
    seed = (Math.imul(seed, 1664525) + 1013904223) >>> 0;
    return seed / 4294967296;
  };
  const total = Math.max(0, Math.min(1000, Math.floor(Number.isFinite(count) ? count : 0)));
  return Array.from({ length: total }, (_, index) => {
    const [longitude, latitude] = centers[(index * 17) % centers.length];
    const spread = distribution === 'clustered' ? 1.8 : 4;
    return {
      longitude: Math.round((longitude + (random() - 0.5) * spread) * 100) / 100,
      latitude: Math.round((latitude + (random() - 0.5) * spread) * 100) / 100,
      visitors: 1,
    };
  });
}
