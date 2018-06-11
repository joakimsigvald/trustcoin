namespace Trustcoin.Story
{
    public class RelationData
    {
        public float Strength { get; set; }

        public void Strengthen()
        {
            Strength += 0.1f * (0.99f - Strength);
        }
    }
}