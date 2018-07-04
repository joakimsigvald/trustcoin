namespace Trustcoin.Story.Types
{
    public struct ConfidenceValue
    {
        public ConfidenceValue(float confidence, float value)
        {
            Confidence = confidence;
            Value = value;
        }

        public float Confidence { get; set; }
        public float Value { get; set; }
    }
}