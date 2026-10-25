export type Entry = {
  key: string; kind: 'node' | 'comment'; title?: string; shortDescription?: string; blocks?: string[];
  body?: string; sameTextAs?: string | null; votes: number[]; at: string;
};
export type Phase = { name: string; votes: Record<string, number[]>; expectedOrder: string[]; sameScoresAs?: string };
export type Scenario = { id: string; name: string; description: string; query: string; entries: Entry[]; phases: Phase[] };
export type Evidence = { id: string; nodeId: string; kind: string; textRank: number; support: number; multiplier: number; finalScore: number; tieDate: string };
export type Row = { position: number; page: number; key: string; searchText: string; votes: number[]; average: number | null; netScore: number; evidence: Evidence };
export type Report = { policy: string; corpus: string; phases: { name: string; rows: Row[]; checks: { description: string; passed: boolean }[] }[] };
