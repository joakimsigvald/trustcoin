using System.Collections.Generic;

namespace Trustcoin.Story
{
    public class NodeData
    {
        public const float DefaultTrust = 0.2f;
        public const float DefaultMoney = 0f;

        public bool Like { get; set; }
        private readonly Dictionary<Node, RelationData> _relations = new Dictionary<Node, RelationData>();
        private float? _trust;
        private float? _money;

        public float Trust
        {
            get => _trust ?? DefaultTrust;
            private set => _trust = value;
        }

        public float Money
        {
            get => _money ?? DefaultMoney;
            set => _money = value;
        }

        public void Initialize(float? trust, float? money)
        {
            _trust = trust;
            _money = money;
        }

        public void StartLiking()
        {
            Like = true;
            IncreaseTrust();
            Money++;
        }

        private void IncreaseTrust()
        {
            Trust += 0.1f * (0.99f - Trust);
        }

        public RelationData Get(Node node)
            => _relations.TryGetValue(node, out var nd)
                ? nd
                : _relations[node] = new RelationData();

        public (float, float) WeightedTrust(float trust)
            => (1, trust);
    }
}