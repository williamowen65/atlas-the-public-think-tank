import { commentRequest } from './comments';

/** Community details, counts and membership for the requesting session. */
export interface Community {
  /** Stable identifier of this record. */
  id: string;
  /** Human-readable display name. */
  name: string;
  /** Description supplied for this record. */
  description: string;
  /** Participant ID of the community owner. */
  ownerParticipantId: string;

  /** Backend lifecycle status, interpreted by the caller for presentation. */
  status: string;
  /** API creation timestamp. */
  createdAt: string;
  /** Number of members reported by the API. */
  memberCount: number;
  /** Number of associated nodes reported by the API. */
  nodeCount: number;
  /** Whether the requesting participant belongs to this community. */
  isMember: boolean;
}

/** Community ownership quota and rolling-window availability. */
export interface CreationAllowance {
  /** Maximum communities this participant may own. */
  maximumOwned: number;
  /** Current number of communities owned. */
  owned: number;

  /** Rolling-window quota information, or null if none is supplied. */
  rolling: {
    /** Creations remaining in the rolling window. */
    remaining: number;
    /** Timestamp when another creation slot becomes available, when supplied. */
    nextSlotAt: string | null;
  } | null;
}
/** Read public communities; optional path is a caller-built suffix such as a query string. */
export const publicCommunities = (path = '') =>
  commentRequest<Community[]>('/atlas-public-api', 'communities' + path);
/** Read one public community by ID. */
export const communityDetail = (id: string) =>
  commentRequest<Community>('/atlas-public-api', 'communities/' + encodeURIComponent(id));
/** Read communities associated with the signed-in participant. */
export const myCommunities = () =>
  commentRequest<Community[]>('/atlas-session-api', 'communities/mine');
/** Read the signed-in participant’s community creation quota. */
export const creationAllowance = () =>
  commentRequest<CreationAllowance>('/atlas-session-api', 'communities/creation-allowance');
/**
 * Send a community mutation through session/CSRF transport; path is appended after
 * communities.
 */
export const communityAction = (path: string, method: 'POST' | 'PUT' | 'DELETE', body?: unknown) =>
  commentRequest('/atlas-session-api', 'communities' + path, method, body);
