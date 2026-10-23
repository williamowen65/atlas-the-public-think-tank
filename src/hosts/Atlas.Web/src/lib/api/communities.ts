import { commentRequest } from './comments';

export interface Community {
  id: string; name: string; description: string; ownerParticipantId: string;
  status: string; createdAt: string; memberCount: number; nodeCount: number; isMember: boolean;
}
export interface CreationAllowance {
  maximumOwned: number; owned: number;
  rolling: { remaining: number; nextSlotAt: string | null } | null;
}
export const publicCommunities = (path = '') =>
  commentRequest<Community[]>('/atlas-public-api', 'communities' + path);
export const communityDetail = (id: string) =>
  commentRequest<Community>('/atlas-public-api', 'communities/' + encodeURIComponent(id));
export const myCommunities = () =>
  commentRequest<Community[]>('/atlas-session-api', 'communities/mine');
export const creationAllowance = () =>
  commentRequest<CreationAllowance>('/atlas-session-api', 'communities/creation-allowance');
export const communityAction = (path: string, method: 'POST' | 'PUT' | 'DELETE', body?: unknown) =>
  commentRequest('/atlas-session-api', 'communities' + path, method, body);
