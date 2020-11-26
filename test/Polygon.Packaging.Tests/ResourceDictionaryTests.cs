using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Polygon.Packaging.Tests
{
    [TestClass]
    public class ResourceDictionaryTests
    {
        [TestMethod]
        public void TestlibExistence()
        {
            var stream = typeof(CodeforcesImportProvider).Assembly
                .GetManifestResourceStream("Polygon.Packaging.Resources.testlib.h");
            Assert.IsNotNull(stream);
            stream.Dispose();
        }
    }
}
