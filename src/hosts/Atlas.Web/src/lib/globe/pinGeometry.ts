import type { GlobePin } from './presence';

export type Vector = [number, number, number];
export const globeRadius = 176;
const radians = Math.PI / 180;
const palette = ['#b73539', '#d5a51c', '#28765c', '#356b9b', '#925493'];

export function preparePin(pin: GlobePin, length = 23, lean = 0.2) {
  const latitude = pin.latitude * radians;
  const longitude = pin.longitude * radians;
  const normal: Vector = [
    Math.cos(latitude) * Math.cos(longitude),
    Math.cos(latitude) * Math.sin(longitude),
    Math.sin(latitude),
  ];
  const east: Vector = [-Math.sin(longitude), Math.cos(longitude), 0];
  const north: Vector = [
    -Math.sin(latitude) * Math.cos(longitude),
    -Math.sin(latitude) * Math.sin(longitude),
    Math.cos(latitude),
  ];
  // Location-based variation stays fixed while rotating or refreshing snapshots.
  const seed = Math.sin(pin.longitude * 127.1 + pin.latitude * 311.7) * 43758.5453;
  const fraction = seed - Math.floor(seed);
  const bearing = fraction * Math.PI * 2;
  const direction = normal.map(
    (value, i) => value + lean * (Math.cos(bearing) * east[i] + Math.sin(bearing) * north[i]),
  );
  const magnitude = Math.hypot(...direction);
  return {
    base: normal.map((value) => value * globeRadius) as Vector,
    head: normal.map(
      (value, i) => value * globeRadius + (direction[i] / magnitude) * length,
    ) as Vector,
    color: palette[Math.floor(fraction * palette.length)],
    pin,
  };
}
export type PreparedPin = ReturnType<typeof preparePin>;
