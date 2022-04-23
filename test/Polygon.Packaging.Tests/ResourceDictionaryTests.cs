using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Xylab.Polygon.Packaging.Tests
{
    [TestClass]
    public class ResourceDictionaryTests
    {
        [TestMethod]
        public void TestlibExistence()
        {
            var stream = CodeforcesImportProvider.GetTestlib();
            Assert.IsNotNull(stream);
            stream.Dispose();
        }
    }
}
