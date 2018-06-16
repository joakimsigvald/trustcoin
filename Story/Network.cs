using System;
using System.Collections.Generic;

namespace Trustcoin.Story
{
    public class Network
    {
        private static int _nextId = 1;
        private readonly List<Person> _nodes = new List<Person>();

        public void AddNode(string name)
        {
            _nodes.Add(new PrivatePerson(_nextId++, name));
        }

        public IEnumerable<Person> Nodes => _nodes;

        public Person Get(string name)
            => _nodes.Find(n => n.Name == name);
    }
}
