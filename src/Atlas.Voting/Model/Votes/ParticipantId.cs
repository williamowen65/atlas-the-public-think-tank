using System;
using System.Collections.Generic;
using System.Text;

namespace Atlas.Voting.Votes
{
    public class ParticipantId
    {
        public Guid Id { get; }

        public ParticipantId(Guid participantId) {

            if (participantId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Parcipant ID cannot be empty.",
                    nameof(participantId));
            }

            Id = participantId;
        }
    }
}
