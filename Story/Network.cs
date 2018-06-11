using System;
using System.Collections.Generic;

namespace Trustcoin.Story
{
    public class Network
    {
        private static int _nextId = 1;
        private readonly List<Node> _nodes = new List<Node>();

        public void AddNode(string name)
        {
            _nodes.Add(new PrivateNode(_nextId++, name));
        }

        public IEnumerable<Node> Nodes => _nodes;

        public Node Get(string name)
            => _nodes.Find(n => n.Name == name);
    }
}
