export type GlobePin = { longitude: number; latitude: number; visitors: number };
export type GlobeSummary = { visitors: number; since: string | null; pins: GlobePin[] };
export type GlobeSnapshot = { live: GlobeSummary; historical: GlobeSummary };

// EventSource data is a network boundary. Keep malformed/reordered data out of the SVG.
export function parseSnapshot(data: string): GlobeSnapshot {
  const snapshot = JSON.parse(data) as GlobeSnapshot;
  for (const summary of [snapshot.live, snapshot.historical]) {
    if (!summary || !Number.isSafeInteger(summary.visitors) || summary.visitors < 0
      || !Array.isArray(summary.pins) || summary.pins.length > 66000
      || !(summary.since === null || typeof summary.since === 'string' && Number.isFinite(Date.parse(summary.since))))
      throw new Error('Invalid globe update');
    for (const pin of summary.pins) {
      if (!Number.isFinite(pin.longitude) || Math.abs(pin.longitude) > 180
        || !Number.isFinite(pin.latitude) || Math.abs(pin.latitude) > 90
        || !Number.isSafeInteger(pin.visitors) || pin.visitors < 1)
        throw new Error('Invalid globe pin');
    }
  }
  return snapshot;
}
