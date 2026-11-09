/** Inverse of the orthographic camera used by the WebGL shaders. */
export function pickLocation(x: number, y: number, size: number, angle: number) {
  if (size <= 0) return null;
  const screenX = (x / size - 0.5) * 440;
  const screenY = (0.5 - y / size) * 440;
  const square = 176 ** 2 - screenX ** 2 - screenY ** 2;
  if (square < 0) return null;
  const depth = Math.sqrt(square);
  const tilt = (18 * Math.PI) / 180;
  const yaw = (angle * Math.PI) / 180;
  const forward = depth * Math.cos(tilt) - screenY * Math.sin(tilt);
  const north = screenY * Math.cos(tilt) + depth * Math.sin(tilt);
  const worldX = forward * Math.cos(yaw) + screenX * Math.sin(yaw);
  const worldY = -forward * Math.sin(yaw) + screenX * Math.cos(yaw);
  return {
    longitude: Math.round((Math.atan2(worldY, worldX) * 180) / Math.PI),
    latitude: Math.round((Math.asin(Math.max(-1, Math.min(1, north / 176))) * 180) / Math.PI),
  };
}
