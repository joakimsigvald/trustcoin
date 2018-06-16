using NUnit.Framework;

namespace Trustcoin.Story.Test
{
    [TestFixture]
    public class ExtensionsTests
    {
        [Test]
        public void MedianOfNothingIsUnknown()
        {
            Assert.That(new (float,float?)[0].Median(), Is.Null);
        }

        [TestCase(1, 1, 1)]
        [TestCase(2, 1, 1)]
        [TestCase(1, 2, 2)]
        public void TestSingleValue(float weight, float value, float expected)
        {
            var weightedValues = new[] { (weight, value) };
            var median = weightedValues.Median();
            Assert.That(median, Is.EqualTo(expected));
        }

        [TestCase(1, 1, 1, 1, 1)]
        [TestCase(2, 1, 2, 1, 1)]
        [TestCase(1, 2, 1, 2, 2)]
        [TestCase(1, 1, 1, 2, 1.5f)]
        [TestCase(1, 1, 2, 2, 2)]
        public void TestTwoValues(float w1, float v1, float w2, float v2, float expected)
        {
            var weightedValues = new[] { (w1, v1), (w2, v2) };
            var median = weightedValues.Median();
            Assert.That(median, Is.EqualTo(expected));
        }

        [TestCase(1, 3, 1, 2, 1, 1, 2)]
        [TestCase(2, 3, 1, 2, 1, 1, 2.5f)]
        [TestCase(3, 3, 1, 2, 1, 1, 3)]
        public void TestThreeValues(float w1, float v1, float w2, float v2, float w3, float v3, float expected)
        {
            var weightedValues = new[] { (w1, v1), (w2, v2), (w3, v3) };
            var median = weightedValues.Median();
            Assert.That(median, Is.EqualTo(expected));
        }
    }
}