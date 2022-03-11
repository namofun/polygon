using MediatR;
using Microsoft.Extensions.Diagnostics;
using Microsoft.Extensions.Logging;
using SatelliteSite.Services;
using Xylab.Polygon.Storages;

namespace Xylab.Polygon.Judgement
{
    public partial class DOMjudgeLikeHandlers
    {
        public IPolygonFacade Facade { get; }

        public ITelemetryClient Telemetry { get; }

        public IMediator Mediator { get; }

        public IAuditlogger Auditlogger { get; }

        public ILogger<DOMjudgeLikeHandlers> Logger { get; }

        public DOMjudgeLikeHandlers(
            IPolygonFacade facade,
            ITelemetryClient telemetry,
            IMediator mediator,
            IAuditlogger auditlogger,
            ILogger<DOMjudgeLikeHandlers> logger)
        {
            Facade = facade;
            Telemetry = telemetry;
            Mediator = mediator;
            Auditlogger = auditlogger;
            Logger = logger;
        }
    }
}
