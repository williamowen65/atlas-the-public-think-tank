export type GlobePin = { longitude: number; latitude: number; visitors: number; color?: string };
export type GlobeSummary = { visitors: number; since: string | null; pins: GlobePin[] };
export type GlobeSnapshot = {
  live: GlobeSummary;
  historical: GlobeSummary;
  capturedAt: string;
  canShare: boolean;
  hasLocation: boolean;
  onlineWindowMinutes: number;
  pinColor?: string | null;
};

/** Validate the short JSON snapshot without trusting network data in the renderer. */
export function parseSnapshot(data: unknown): GlobeSnapshot {
  const snapshot = (typeof data === 'string' ? JSON.parse(data) : data) as GlobeSnapshot;
  if (
    !snapshot ||
    typeof snapshot.canShare !== 'boolean' ||
    typeof snapshot.hasLocation !== 'boolean' ||
    (snapshot.pinColor != null && !/^#[0-9a-fA-F]{6}$/.test(snapshot.pinColor)) ||
    !Number.isFinite(Date.parse(snapshot.capturedAt)) ||
    !Number.isSafeInteger(snapshot.onlineWindowMinutes) ||
    snapshot.onlineWindowMinutes < 1
  )
    throw new Error('Invalid globe snapshot');
  for (const summary of [snapshot.live, snapshot.historical]) {
    if (
      !summary ||
      !Number.isSafeInteger(summary.visitors) ||
      summary.visitors < 0 ||
      !Array.isArray(summary.pins) ||
      summary.pins.length > 66000 ||
      !(
        summary.since === null ||
        (typeof summary.since === 'string' && Number.isFinite(Date.parse(summary.since)))
      )
    )
      throw new Error('Invalid globe summary');
    for (const pin of summary.pins) {
      if (
        !Number.isFinite(pin.longitude) ||
        Math.abs(pin.longitude) > 180 ||
        !Number.isFinite(pin.latitude) ||
        Math.abs(pin.latitude) > 90 ||
        !Number.isSafeInteger(pin.visitors) ||
        pin.visitors < 1 ||
        (pin.color !== undefined && !/^#[0-9a-fA-F]{6}$/.test(pin.color))
      )
        throw new Error('Invalid globe pin');
    }
  }
  return snapshot;
}
