import type { NodeType } from '../api/nodePage';
import type { DiscoveryFilters } from '../api/discovery';

const uuid = /^[0-9a-f]{8}-[0-9a-f-]{27,}$/i;
/**
 * Parse shared search syntax for both the filter editor and global results; does not fetch data.
 * @param value Search text plus optional field tokens such as votes:1..5.
 * @param types Loaded catalog used to resolve type names or IDs.
 * @returns Free text, API constraints and validation messages; callers reject invalid queries.
 */
export function parseDiscoveryQuery(value: string, types: NodeType[]) {
  const filters: DiscoveryFilters = {};
  const words: string[] = [];
  const errors: string[] = [];
  for (const part of value.match(/"[^"]*"|\S+/g) ?? []) {
    const match = /^(type|author|community|votes|average|from|through|archived):(.+)$/i.exec(part);
    if (!match) {
      words.push(part);
      continue;
    }
    const [, rawKey, rawValue] = match;
    const key = rawKey.toLowerCase();
    const v = rawValue.replace(/^"|"$/g, '');
    if (key === 'type') {
      const found = types.find(
        (t) => t.id.toLowerCase() === v.toLowerCase() || t.name.toLowerCase() === v.toLowerCase(),
      );
      if (found) filters.typeId = found.id;
      else errors.push('Unknown node type: ' + v);
    } else if (key === 'author' || key === 'community') {
      if (uuid.test(v)) {
        if (key === 'author') filters.authorId = v;
        else filters.communityId = v;
      } else errors.push(key + ' requires an ID');
    } else if (key === 'votes' || key === 'average') {
      const m = /^(\d+(?:\.\d+)?)?\.\.(\d+(?:\.\d+)?)?$/.exec(v);
      if (!m || (!m[1] && !m[2])) {
        errors.push('Use ' + key + ':minimum..maximum');
        continue;
      }
      const low = m[1] ? Number(m[1]) : undefined,
        high = m[2] ? Number(m[2]) : undefined;
      if (
        (low !== undefined && high !== undefined && low > high) ||
        (key === 'votes' && [low, high].some((n) => n !== undefined && !Number.isInteger(n))) ||
        (key === 'average' && [low, high].some((n) => n !== undefined && n > 10))
      ) {
        errors.push('Invalid ' + key + ' range');
        continue;
      }
      if (key === 'votes') {
        filters.minVotes = low;
        filters.maxVotes = high;
      } else {
        filters.minAverage = low;
        filters.maxAverage = high;
      }
    } else if (key === 'from' || key === 'through') {
      if (/^\d{4}-\d{2}-\d{2}$/.test(v) && !Number.isNaN(Date.parse(v))) {
        if (key === 'from') filters.from = v;
        else filters.through = v;
      } else errors.push('Invalid date: ' + v);
    } else if (key === 'archived') {
      if (v === 'true') filters.archived = true;
      else if (v !== 'false') errors.push('Use archived:true or archived:false');
    }
  }
  return { text: words.join(' ').trim(), filters, errors };
}
