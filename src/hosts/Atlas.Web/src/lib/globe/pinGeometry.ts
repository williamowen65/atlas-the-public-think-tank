import type { GlobePin } from './presence';

type Vector = [number, number, number];
export const globeRadius = 176;
const radians = Math.PI / 180;
const palette = ['#b73539', '#d5a51c', '#28765c', '#356b9b', '#925493'];

/** Orthographic camera shared with the map's [angle, -18] D3 rotation. */
export function projectVector([x, y, z]: Vector, angle: number): Vector {
  const a = angle * radians;
  const tilt = 18 * radians;
  const forward = x * Math.cos(a) - y * Math.sin(a);
  return [
    200 + x * Math.sin(a) + y * Math.cos(a),
    200 - z * Math.cos(tilt) + forward * Math.sin(tilt),
    forward * Math.cos(tilt) + z * Math.sin(tilt),
  ];
}

export function projectPin(pin: GlobePin, angle: number, length = 23, lean = 0.2) {
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
  const base = projectVector(normal.map((value) => value * globeRadius) as Vector, angle);
  const head = projectVector(
    normal.map((value, i) => value * globeRadius + (direction[i] / magnitude) * length) as Vector,
    angle,
  );
  const segments: { from: Vector; to: Vector; front: boolean }[] = [];
  if (base[2] >= 0 === head[2] >= 0) {
    segments.push({ from: base, to: head, front: head[2] >= 0 });
  } else {
    const t = -base[2] / (head[2] - base[2]);
    const crossing = base.map((value, i) => value + t * (head[i] - value)) as Vector;
    segments.push(
      { from: base, to: crossing, front: base[2] >= 0 },
      { from: crossing, to: head, front: head[2] >= 0 },
    );
  }
  return { base, head, segments, color: palette[Math.floor(fraction * palette.length)] };
}
