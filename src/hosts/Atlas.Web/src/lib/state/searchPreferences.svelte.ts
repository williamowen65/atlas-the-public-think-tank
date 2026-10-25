import { editorSession } from '../api/editorSession';
export const searchPreferences = $state({ votingWeight: 40, message: '' });
let storageKey = 'atlas-search-preferences:anonymous';
/** Preferences are private to this browser and partitioned by signed-in participant. */
export function watchSearchPreferences() {
  return editorSession.subscribe((owner) => {
    storageKey = `atlas-search-preferences:${owner ?? 'anonymous'}`;
    searchPreferences.votingWeight = 40;
    searchPreferences.message = '';
    try {
      const saved = JSON.parse(localStorage.getItem(storageKey) ?? 'null');
      if (
        Number.isInteger(saved?.votingWeight) &&
        saved.votingWeight >= 0 &&
        saved.votingWeight <= 100
      )
        searchPreferences.votingWeight = saved.votingWeight;
    } catch {
      /* Missing or unusable preferences use the default. */
    }
  });
}
export function saveVotingWeight(value: number) {
  if (!Number.isInteger(value) || value < 0 || value > 100) return;
  searchPreferences.votingWeight = value;
  try {
    localStorage.setItem(storageKey, JSON.stringify({ votingWeight: value }));
    searchPreferences.message = 'Saved in this browser.';
  } catch {
    searchPreferences.message = 'Applied for this session; browser storage is unavailable.';
  }
}
