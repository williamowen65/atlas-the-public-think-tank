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

                    _ => throw new InvalidOperationException(
                        "Unsupported vote target type.")
                },

                Value = vote.Value.Value,
                CreatedAt = vote.CreatedAt,
                UpdatedAt = vote.UpdatedAt
            };
        }

        public Vote? GetById(VoteId id)
        {
            throw new NotImplementedException();
        }

        public Vote? GetByParticipantAndTarget(ParticipantId participantId, VoteTarget voteTarget)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<Vote> GetTargetVotes(VoteTarget target)
        {
            throw new NotImplementedException();
        }

        public void Save(Vote vote)
        {
            throw new NotImplementedException();
        }
    }
}
