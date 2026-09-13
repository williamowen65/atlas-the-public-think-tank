using System;
using System.Collections.Generic;
using System.Text;

namespace Atlas.Voting.Target
{
    public class VoteTarget
    {
        public Guid Id { get;}
        public VoteTarget(Guid id)
        { 
          Id = id;  
        }
    }
}
