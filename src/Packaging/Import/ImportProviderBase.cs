using Microsoft.Extensions.Logging;
using Polygon.Entities;
using Polygon.Storages;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Polygon.Packaging
{
    public abstract class ImportProviderBase : IImportProvider
    {
        public IPolygonFacade Facade { get; }
        private ILogger Logger { get; }
        public StringBuilder LogBuffer { get; }

        public ImportProviderBase(IPolygonFacade facade, ILogger logger)
        {
            Facade = facade;
            Logger = logger;
            LogBuffer = new StringBuilder();
        }

        protected void Log(string log)
        {
            Logger.LogInformation(log);
            LogBuffer.AppendLine(log);
        }

        protected Task<ImportContext> CreateAsync(Problem problem)
        {
            return ImportContext.CreateAsync(Facade, Log, problem);
        }

        public static string TryGetPackageName(string orig)
        {
            var name = orig.ToUpper().EndsWith(".ZIP") ? orig[0..^4] : orig;
            if (string.IsNullOrEmpty(name))
                name = "UNTITLED";
            return name;
        }

        public abstract Task<List<Problem>> ImportAsync(Stream stream, string streamFileName, string username);
    }
}
