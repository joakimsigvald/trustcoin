using System;
using System.Collections.Generic;
using System.Linq;

namespace Trustcoin.Story
{
    public class PrivateNode : Node
    {
        private readonly Dictionary<Node, NodeData> _peers = new Dictionary<Node, NodeData>();

        public PrivateNode(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; }
        public string Name { get; }

        public void Like(Node node)
        {
            var nodeData = Get(node);
            if (nodeData.Like)
                return;

            nodeData.StartLiking();
            foreach (var peer in GetPeers(except: node))
                peer.ILike(this, node);
        }

        public NodeData Get(Node node)
            => _peers.TryGetValue(node, out var data)
                ? data
                : _peers[node] = CreateNodeData(node);

        public void ILike(Node source, Node target)
        {
            var sourceData = Get(source);
            var targetData = Get(target);
            var relation = sourceData.Get(target);
            targetData.Money += 1 - relation.Strength;
            relation.Strengthen();
        }

        public override string ToString()
            => $"{Name}: Reputation={EstimateReputation(this)}, Money={EstimateMoney(this)}";

        public override bool Equals(object obj)
            => Id == (obj as Node)?.Id;

        public override int GetHashCode()
            => Id;

        private float? EstimateReputation(Node target)
            => Estimate(target, nd => nd.Trust);

        private float? EstimateMoney(Node target)
            => Estimate(target, nd => nd.Money);

        private float? Estimate(Node target, Func<NodeData, float> getValue)
            => GetPeers(except: target).Select(p => GetWeightedValue(p, target, getValue))
                .ToList()
                .Median();

        private IEnumerable<Node> GetPeers(params Node[] except)
            => _peers.Keys.Except(except);

        private NodeData CreateNodeData(Node node)
        {
            var nodeData = new NodeData();
            nodeData.Initialize(EstimateReputation(node), EstimateMoney(node));
            return nodeData;
        }

        private (float, float) GetWeightedValue(Node peer, Node target, Func<NodeData, float> getValue)
            => (Get(peer).Trust, getValue(peer.Get(target)));
    }
}