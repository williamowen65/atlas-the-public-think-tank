using System;
using System.Collections.Generic;
using System.Text;

namespace Atlas.Voting.Votes
{
    /// <summary>Identifies a voteId within the Voting boundary.</summary>
    public readonly record struct VoteId(Guid Value)
    {
        public static VoteId New() {
            return new VoteId(Guid.NewGuid());
        }
    }
}

