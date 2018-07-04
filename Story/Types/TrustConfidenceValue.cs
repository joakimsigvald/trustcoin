namespace Trustcoin.Story.Types
{
    public struct TrustConfidenceValue
    {
        public TrustConfidenceValue(float trust, ConfidenceValue confidenceValue)
        {
            Trust = trust;
            Confidence = confidenceValue.Confidence;
            Value = confidenceValue.Value;
        }

        public float Trust { get; set; }
        public float Confidence { get; set; }
        public float Value { get; set; }
    }
}