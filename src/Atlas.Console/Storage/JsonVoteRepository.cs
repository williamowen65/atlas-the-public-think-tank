using Atlas.ConsoleApp.Storage;
using Atlas.Voting;
using Atlas.Voting.Data;
using Atlas.Voting.Target;
using Atlas.Voting.Votes;
using System.Text.Json;

namespace Atlas.ConsoleApp.Storage
{
    public sealed class JsonVoteRepository : IVoteRepository
    {

        private readonly string _filePath;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true
        };

        /// <summary>Initializes the JSON adapter and ensures its backing file is available.</summary>
        public JsonVoteRepository(string filePath)
        {
            _filePath = filePath;
        }

        /// <summary>Reads Vote persistence records from JSON.</summary>
        private List<StoredVote> ReadStoredVotes()
        {
            if (!File.Exists(_filePath))
            {
                return [];
            }

            var json = File.ReadAllText(_filePath);

            if (string.IsNullOrWhiteSpace(json))
            {
                return [];
            }

            return JsonSerializer.Deserialize<List<StoredVote>>(
                       json,
                       _jsonOptions)
                   ?? [];
        }

        /// <summary>Writes Vote persistence records to JSON.</summary>
        private void WriteStoredVotes(
            List<StoredVote> votes)
        {
            var directory = Path.GetDirectoryName(_filePath);

            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(votes, _jsonOptions);
            File.WriteAllText(_filePath, json);
        }

        private static StoredVote ToStorage(Vote vote)
        {
            return new StoredVote
            {
                Id = vote.Id.Value,
                ParticipantId = vote.ParticipantId.Id,
                TargetId = vote.Target.Id,

                TargetType = vote.Target switch
                {
                    NodeVoteTarget => "Node",
                    NodeReactionVoteTarget => "NodeReaction",

                    _ => throw new InvalidOperationException(
                        "Unsupported vote target type.")
                },

                Value = vote.Value.Value,
                CreatedAt = vote.CreatedAt,
                UpdatedAt = vote.UpdatedAt
            };
        }

        private static Vote ToDomain(StoredVote storedVote)
        {
            VoteTarget target = storedVote.TargetType switch
            {
                "Node" => new NodeVoteTarget(
                    storedVote.TargetId),
                "NodeReaction" => new NodeReactionVoteTarget(
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

        public void Save(Vote vote)
        {
            ArgumentNullException.ThrowIfNull(vote);

            var storedVotes = ReadStoredVotes();

            var existingIndex = storedVotes.FindIndex(
                storedVote => storedVote.Id == vote.Id.Value);

            var replacement = ToStorage(vote);

            if (existingIndex >= 0)
            {
                storedVotes[existingIndex] = replacement;
            }
            else
            {
                storedVotes.Add(replacement);
            }

            WriteStoredVotes(storedVotes);
        }


        /// <summary>Physically removes a vote from current JSON persistence.</summary>
        public void Delete(VoteId id)
        {
            var storedVotes = ReadStoredVotes();

            var removedCount = storedVotes.RemoveAll(
                storedVote => storedVote.Id == id.Value);

            if (removedCount > 0)
            {
                WriteStoredVotes(storedVotes);
            }
        }

        public Vote? GetById(VoteId id)
        {
            return ReadStoredVotes()
                .Select(ToDomain)
                .SingleOrDefault(
                vote => vote.Id == id);
        }

        public IReadOnlyCollection<Vote> GetTargetVotes(
        VoteTarget target)
        {
            return ReadStoredVotes()
                .Select(ToDomain)
                .Where(vote =>
                    vote.Target.Id == target.Id &&
                    vote.Target.GetType() == target.GetType())
                .ToArray();
        }

        public Vote? GetByParticipantAndTarget(
            ParticipantId participantId,
            VoteTarget voteTarget)
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
