using System;
using System.Collections.Generic;
using System.Text;

namespace Atlas.Voting.Value
{
    public class NodeImportanceRating : IVoteValue
    {
        public int Value { get;}

        public NodeImportanceRating(int value) {

            if (0 <= value && value <= 10)
            {
                Value = value;
            }
            else
            {
                throw new ArgumentOutOfRangeException(
                        nameof(value),
                        value,
                        "Node importance rating must be between 0 and 10.");
            }
        }


    }
}
