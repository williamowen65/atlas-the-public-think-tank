using System;
using System.Collections.Generic;
using System.Text;

namespace Atlas.Voting.Votes
{
    public readonly record struct VoteTarget(Guid id)
    {
        public static VoteTarget New()
        {
            return new VoteTarget(Guid.NewGuid());
        }
    }
}
