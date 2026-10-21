import type { NodeCardData } from '../../src/lib/components/NodeCard.svelte';

// Titles, IDs and first paragraphs follow DemoData.SeedNode/SeedBlock.
// Extra display metadata, discussion and aggregates are illustrative fixtures.
const question = 'b1000000-0000-4000-8000-000000000001';
const issue = 'b1000000-0000-4000-8000-000000000002';
const idea = 'b1000000-0000-4000-8000-000000000004';
const defaults = { createdAt: '2026-09-28T15:20:00Z', status: 'Active' as const };
export const nodeFixtures: Record<string, { node: NodeCardData; average: number | null; totalVotes: number; comments: string[] }> = {
  question: {
    node: { ...defaults, id: question, title: 'What ideas could help make the world better?', typeName: 'Question', authorName: 'Demo participant 01',
      description: 'Useful ideas can come from anyone, and a small change in one place may inspire progress somewhere else. People of every age and background notice different needs and possibilities. Even an idea too big for one person to carry out can grow when others can see it, develop it, and connect it with ideas from other areas of life.\n\nWhere would you begin? Explore a connected issue, contribute an idea, or help identify evidence that would move the conversation forward. This example invites us to connect broad questions with practical work in our communities.',
      communities: ['The Public Think Tank'], breadcrumb: [], requestedTypes: ['Ideas', 'Issues', 'Goals'], commentCount: 2,
      reactions: [{ label: 'Thought-provoking', count: 12 }], related: [{ id: issue, title: 'Homelessness', typeName: 'Issue' }, { id: idea, title: 'Learn from solutions that already work', typeName: 'Idea' }] },
    average: 7.4, totalVotes: 21, comments: ['Which local problems could benefit from people sharing what they know?', 'Connecting the question to specific examples makes it easier to contribute.'],
  },
  issue: {
    node: { ...defaults, id: issue, title: 'Homelessness', typeName: 'Issue', authorName: 'Demo participant 02',
      description: "Homelessness affects people's safety, health, and ability to build a stable life. Listening to people affected and understanding the causes can help communities address the issue in ways that meet people's needs.\n\nHousing, health care, employment, and support networks intersect here. A useful next step is to understand how these factors differ across communities and what people with lived experience identify as the most urgent needs.\n\nConnect evidence and proposed responses so readers can follow the reasoning behind an idea, consider its limits, and see what remains uncertain.",
      communities: ['Community wellbeing'], breadcrumb: [{ id: question, title: 'What ideas could help make the world better?' }], requestedTypes: ['Questions', 'Evidence', 'Ideas', 'Solutions'], commentCount: 1,
      reactions: [{ label: 'Important context', count: 8 }], related: [{ id: idea, title: 'Learn from solutions that already work', typeName: 'Idea' }, { id: question, title: 'What ideas could help make the world better?', typeName: 'Question' }] },
    average: 8.1, totalVotes: 34, comments: ['It would help to hear directly from people who have experienced housing insecurity.'],
  },
  idea: {
    node: { ...defaults, id: idea, title: 'Learn from solutions that already work', typeName: 'Idea', authorName: 'Demo participant 04',
      description: 'People are already finding effective ways to address difficult problems. Studying what worked, for whom, and under what conditions could help others adapt promising approaches without assuming that one solution fits everywhere.\n\nStart with a clear account of the original context. What resources were required? Who participated? What changed, and how was that change measured?\n\nSharing both successes and limitations helps another community decide what to adapt. Evidence and thoughtful questions can turn an inspiring example into something others can evaluate and build on.',
      communities: ['The Public Think Tank'], breadcrumb: [{ id: question, title: 'What ideas could help make the world better?' }], requestedTypes: ['Evidence', 'Questions'], commentCount: 0,
      reactions: [], related: [{ id: question, title: 'What ideas could help make the world better?', typeName: 'Question' }] },
    average: null, totalVotes: 0, comments: [],
  },
};
export const fixtureOptions = Object.keys(nodeFixtures);
