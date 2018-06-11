using System.Collections.Generic;

namespace Trustcoin.Story
{
    public interface Node
    {
        int Id { get; }
        string Name { get; }
        void Like(Node node);
        void ILike(Node source, Node target);
        NodeData Get(Node node);
    }
}