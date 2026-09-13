using Atlas.ConsoleApp.Storage;
using Atlas.Voting;
using Atlas.Voting.Data;
using Atlas.Voting.Target;
using Atlas.Voting.Votes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Atlas.Console.Storage
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

        /// <summary>Reads participant persistence records from JSON.</summary>
        private List<StoredParticipant> ReadStoredParticipants()
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

            return JsonSerializer.Deserialize<List<StoredParticipant>>(
                       json,
                       _jsonOptions)
                   ?? [];
        }

        /// <summary>Writes participant persistence records to JSON.</summary>
        private void WriteStoredParticipants(
            List<StoredParticipant> participants)
        {
            var directory = Path.GetDirectoryName(_filePath);

            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(participants, _jsonOptions);
            File.WriteAllText(_filePath, json);
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
