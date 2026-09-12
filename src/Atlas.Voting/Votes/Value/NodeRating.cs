using System;
using System.Collections.Generic;
using System.Text;

namespace Atlas.Voting.Votes.Value
{
    public class NodeRating : IVoteValue
    {
        public int Value { get;}

        public NodeRating(int value) {

            if (0 <= value && value <= 10)
            {
                Value = value;
            }
            else
            {
                throw new ArgumentOutOfRangeException("Node rating was out of range");
            }
        }


    }
}
