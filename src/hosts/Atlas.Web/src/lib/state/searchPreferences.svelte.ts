import { editorSession } from '../api/editorSession';
export const searchPreferences = $state({ votingWeight: 40, matchStrictness: 15, message: '' });
let storageKey = 'atlas-search-preferences:anonymous';
/** Preferences are private to this browser and partitioned by signed-in participant. */
export function watchSearchPreferences() {
  return editorSession.subscribe((owner) => {
    storageKey = `atlas-search-preferences:${owner ?? 'anonymous'}`;
    searchPreferences.votingWeight = 40;
    searchPreferences.matchStrictness = 15;
    searchPreferences.message = '';
    try {
      const saved = JSON.parse(localStorage.getItem(storageKey) ?? 'null');
      if (
        Number.isInteger(saved?.votingWeight) &&
        saved.votingWeight >= 0 &&
        saved.votingWeight <= 100
      )
        searchPreferences.votingWeight = saved.votingWeight;
      if (
        Number.isInteger(saved?.matchStrictness) &&
        saved.matchStrictness >= 0 &&
        saved.matchStrictness <= 100
      )
        searchPreferences.matchStrictness = saved.matchStrictness;
    } catch {
      /* Missing or unusable preferences use the default. */
    }
  });
}
export function saveVotingWeight(value: number) {
  if (!Number.isInteger(value) || value < 0 || value > 100) return;
  searchPreferences.votingWeight = value;
  persist();
}
export function saveMatchStrictness(value: number) {
  if (!Number.isInteger(value) || value < 0 || value > 100) return;
  searchPreferences.matchStrictness = value;
  persist();
}
function persist() {
  try {
    localStorage.setItem(
      storageKey,
      JSON.stringify({
        votingWeight: searchPreferences.votingWeight,
        matchStrictness: searchPreferences.matchStrictness,
      }),
    );
    searchPreferences.message = 'Saved in this browser.';
  } catch {
    searchPreferences.message = 'Applied for this session; browser storage is unavailable.';
  }
}
