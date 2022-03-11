using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Xylab.Polygon.Packaging.Tests
{
    [TestClass]
    public class ResourceDictionaryTests
    {
        [TestMethod]
        public void TestlibExistence()
        {
            var stream = typeof(CodeforcesImportProvider).Assembly
                .GetManifestResourceStream("Xylab.Polygon.Packaging.Resources.testlib.h");
            Assert.IsNotNull(stream);
            stream.Dispose();
        }
    }
}
