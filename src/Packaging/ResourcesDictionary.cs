using System;
using System.IO;
using System.Reflection;

namespace Polygon.Packaging
{
    public class ResourcesDictionary
    {
        public static Stream Read(string name)
        {
            name = "Polygon.Packaging.Resources." + name;
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
            if (stream == null) throw new InvalidOperationException($"EmbeddedResource {name} not found.");
            return stream;
        }

        public static Stream GetTestlib() => Read("testlib.h");
        public static Stream GetOlymp() => Read("olymp.sty");
        public static Stream GetContestTexBegin() => Read("contest.tex");
    }
}
