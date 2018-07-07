using System;
namespace Trustcoin.Story
{
    public class Transaction<TContent>
    {
        public Transaction(int signature, TContent content) 
        {
            Signature = signature;
            Content = content;
        }

        public TContent Content { get; }
        private int Signature { get; }

        public Guid Id { get; } = Guid.NewGuid();

        public bool IsSignedBy(int id)
            => Signature == id;
    }
}