using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Polygon.Packaging.Tests
{
    [TestClass]
    public class ResourceDictionaryTests
    {
        [TestMethod]
        [DataRow("testlib.h")]
        [DataRow("olymp.sty")]
        [DataRow("contest.tex")]
        public void TestExistence(string fileName)
        {
            ResourcesDictionary.Read(fileName).Dispose();
        }

        [TestMethod]
        [DataRow("what.the.fuck")]
        public void TestNonExistence(string fileName)
        {
            Assert.ThrowsException<InvalidOperationException>(() => ResourcesDictionary.Read(fileName));
        }
    }
}
