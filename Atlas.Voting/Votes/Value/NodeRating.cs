using System;
using System.Collections.Generic;
using System.Text;

namespace Atlas.Voting.Votes.Value
{
    public class NodeRating : IVoteValue
    {
        public int Value { get; set;  }

        public NodeRating(int value) {

            if (value > 0 && value < 11)
            {
                Value = value;
            }
            else { 
                throw new ArgumentOutOfRangeException("Node rating was out of range");
            }
        
        }
    }
}
