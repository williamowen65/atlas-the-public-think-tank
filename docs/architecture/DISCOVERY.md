# Discovery boundary

Discovery is Atlas's read-oriented boundary for finding content. It owns the
query language, filtering rules, and ranking policy used by consumer-facing
content lists. It does not own Nodes, descriptions, votes, Communities, or
their persistence.

## Why Discovery instead of Feed

`Feed` describes one presentation. `Discovery` describes the broader user
capability: browsing a ranked feed, narrowing it with filters, and searching.
A future UI can therefore expose Feed and Search experiences without inventing
separate ranking and filtering rules.

## Current query

| Input | Behavior |
|---|---|
| Search text | Case-insensitive match against title or searchable content |
| Community ID | Includes only Nodes associated with that Community |
| Reaction IDs | Includes only Nodes containing every selected active reaction |
| Vote count range | Inclusive minimum and/or maximum number of current votes |
| Average vote range | Inclusive minimum and/or maximum average from 0–10; unrated Nodes do not match an average range |
| Created-date range | Inclusive start and/or end calendar date; full creation timestamps remain preserved |
| Include archived | Off by default; intended for administrative workflows |

All filters compose and are applied before ranking. Results contain Node identities and ranking
metadata, not mutable Graph aggregates.

## Ranking policy

The first MVP ranks primarily by the average 0–10 Node vote. A small logarithmic
vote-count factor provides stable confidence ordering without allowing raw
popularity to overwhelm rating quality. Search title matches receive a small
intent boost. Remaining ties use vote count, last update, and title so output is
deterministic.

This formula is deliberately isolated in `DiscoveryService`; it can evolve
without changing Graph or the console consumer.

## Boundary flow

1. The host adapter projects Graph, Content, Voting, and Communities into
   `DiscoveryCandidate` read models.
2. `DiscoveryService` filters, searches, and ranks those candidates.
3. Consumers receive `RankedDiscoveryItem` references.
4. The console resolves a selected Node by ID only after the user opens it.

`INodeRepository` no longer exposes an unrestricted `GetAll()` operation.
Graph offers only identity lookup, persistence, and purpose-specific graph
queries for children, authorship, and parent selection. The JSON adapter also
implements a host-side `IDiscoveryNodeReader` that supplies the current
projection input without making bulk browsing part of Graph's domain contract.

## Console verification

Choose **Discover nodes** from the main menu. The view always shows ranked
Discovery results. Use `S` for text, `C` for Community, `R` for one or more
reactions, `V` for vote-count and average-vote ranges, `D` for the created-date
range, `X` to clear all filters, a result number to open it, or `0` to return.

The Discovery table shows the Created date as `YYYY-MM-DD`. `UpdatedAt` remains
a separate fact used for deterministic ranking ties and Node details. Discovery
does not combine creation and modification into an ambiguous synthetic date.
