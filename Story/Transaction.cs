using System;
using System.Collections.Generic;
using System.Linq;

namespace Trustcoin.Story
{
    public class Transaction<TContent> : Transaction
    {
        public Transaction(TContent content) => Content = content;

        public TContent Content { get; }

        public Transaction<TContent> Sign(int id)
            => new Transaction<TContent>(Content)
            {
                Id = Id,
                Signatures = Signatures.Append(id).ToList()
            };
    }

    public class Transaction
    {
        protected List<int> Signatures { get; set; } = new List<int>();

        public Guid Id { get; protected set; } = Guid.NewGuid();

        public bool IsSignedBy(int id)
            => Signatures.Contains(id);
    }
}