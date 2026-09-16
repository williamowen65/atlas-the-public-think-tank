using Atlas.Voting;
using Atlas.Voting.Data;
using Atlas.Voting.Target;
using Atlas.Voting.Votes;
using System.Collections.Concurrent;
using System.Text.Json;

namespace Atlas.ConsoleApp.Storage
{
    /// <summary>
    /// Persists current votes in one JSON file for the Console prototype.
    /// </summary>
    public sealed class JsonVoteRepository : IVoteRepository
    {
        private static readonly ConcurrentDictionary<string, object>
            FileGates = new(
                OperatingSystem.IsWindows()
                    ? StringComparer.OrdinalIgnoreCase
                    : StringComparer.Ordinal);

        private readonly string _filePath;
        private readonly object _fileGate;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true
        };

        /// <summary>
        /// Initializes the JSON adapter and shares one synchronization gate
        /// with every repository instance addressing the same local file.
        /// </summary>
        public JsonVoteRepository(string filePath)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(
                filePath);

            _filePath = Path.GetFullPath(filePath);
            _fileGate = FileGates.GetOrAdd(
                _filePath,
                _ => new object());
        }

        /// <summary>
        /// Atomically reads, creates or changes, and rewrites the one current
        /// participant-target vote for this JSON file.
        /// </summary>
        public Vote SetCurrentVote(
            VoteTarget target,
            ParticipantId participantId,
            int voteValue)
        {
            lock (_fileGate)
            {
                var storedVotes =
                    ReadStoredVotes();

                var targetType =
                    GetTargetType(target);

                var existingIndex =
                    storedVotes.FindIndex(storedVote =>
                        storedVote.ParticipantId == participantId.Id &&
                        storedVote.TargetId == target.Id &&
                        storedVote.TargetType == targetType);

                Vote currentVote;

                if (existingIndex < 0)
                {
                    currentVote = new Vote(
                        target,
                        participantId,
                        voteValue);

                    storedVotes.Add(
                        ToStorage(currentVote));
                }
                else
                {
                    currentVote =
                        ToDomain(storedVotes[existingIndex]);

                    currentVote.ChangeValue(
                        voteValue,
                        NextChangedAt(currentVote));

                    storedVotes[existingIndex] =
                        ToStorage(currentVote);
                }

                WriteStoredVotes(
                    storedVotes);

                return currentVote;
            }
        }

        private List<StoredVote> ReadStoredVotes()
        {
            if (!File.Exists(_filePath))
            {
                return [];
            }

            var json = File.ReadAllText(
                _filePath);

            if (string.IsNullOrWhiteSpace(json))
            {
                return [];
            }

            return JsonSerializer.Deserialize<List<StoredVote>>(
                       json,
                       _jsonOptions)
                   ?? [];
        }

        private void WriteStoredVotes(
            List<StoredVote> votes)
        {
            var directory =
                Path.GetDirectoryName(_filePath);

            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(
                    directory);
            }

            var json =
                JsonSerializer.Serialize(
                    votes,
                    _jsonOptions);

            File.WriteAllText(
                _filePath,
                json);
        }

        private static StoredVote ToStorage(
            Vote vote)
        {
            return new StoredVote
            {
                Id = vote.Id.Value,
                ParticipantId = vote.ParticipantId.Id,
                TargetId = vote.Target.Id,
                TargetType = GetTargetType(vote.Target),
                Value = vote.Value.Value,
                CreatedAt = vote.CreatedAt,
                UpdatedAt = vote.UpdatedAt
            };
        }

        private static Vote ToDomain(
            StoredVote storedVote)
        {
            VoteTarget target = storedVote.TargetType switch
            {
                "Node" => new NodeVoteTarget(
                    storedVote.TargetId),

                _ => throw new InvalidDataException(
                    $"Unsupported stored vote target type " +
                    $"'{storedVote.TargetType}'.")
            };

            return Vote.Reconstitute(
                new VoteId(storedVote.Id),
                target,
                new ParticipantId(storedVote.ParticipantId),
                storedVote.Value,
                storedVote.CreatedAt,
                storedVote.UpdatedAt);
        }

        private static string GetTargetType(
            VoteTarget target)
        {
            return target switch
            {
                NodeVoteTarget => "Node",

                _ => throw new InvalidOperationException(
                    "Unsupported vote target type.")
            };
        }

        private static DateTimeOffset NextChangedAt(
            Vote existingVote)
        {
            var changedAt = DateTimeOffset.UtcNow;

            return changedAt > existingVote.UpdatedAt
                ? changedAt
                : existingVote.UpdatedAt.AddTicks(1);
        }

        public void Save(Vote vote)
        {
            ArgumentNullException.ThrowIfNull(vote);

            lock (_fileGate)
            {
                var storedVotes =
                    ReadStoredVotes();

                var existingIndex =
                    storedVotes.FindIndex(
                        storedVote =>
                            storedVote.Id == vote.Id.Value);

                var replacement =
                    ToStorage(vote);

                if (existingIndex >= 0)
                {
                    storedVotes[existingIndex] =
                        replacement;
                }
                else
                {
                    storedVotes.Add(
                        replacement);
                }

                WriteStoredVotes(
                    storedVotes);
            }
        }

        public void Delete(VoteId id)
        {
            lock (_fileGate)
            {
                var storedVotes =
                    ReadStoredVotes();

                var removedCount =
                    storedVotes.RemoveAll(
                        storedVote =>
                            storedVote.Id == id.Value);

                if (removedCount > 0)
                {
                    WriteStoredVotes(
                        storedVotes);
                }
            }
        }

        public Vote? GetById(VoteId id)
        {
            lock (_fileGate)
            {
                return ReadStoredVotes()
                    .Select(ToDomain)
                    .SingleOrDefault(
                        vote => vote.Id == id);
            }
        }

        public IReadOnlyCollection<Vote> GetTargetVotes(
            VoteTarget target)
        {
            lock (_fileGate)
            {
                return ReadStoredVotes()
                    .Select(ToDomain)
                    .Where(vote =>
                        vote.Target.Id == target.Id &&
                        vote.Target.GetType() == target.GetType())
                    .ToArray();
            }
        }

        public Vote? GetByParticipantAndTarget(
            ParticipantId participantId,
            VoteTarget voteTarget)
        {
            lock (_fileGate)
            {
                return ReadStoredVotes()
                    .Select(ToDomain)
                    .SingleOrDefault(vote =>
                        vote.ParticipantId.Id == participantId.Id &&
                        vote.Target.Id == voteTarget.Id &&
                        vote.Target.GetType() == voteTarget.GetType());
            }
        }
    }
}
