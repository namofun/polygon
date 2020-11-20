using MediatR;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;
using Polygon.Storages;
using SatelliteSite.Services;

namespace Polygon.Judgement
{
    public partial class DOMjudgeLikeHandlers
    {
        public IPolygonFacade Facade { get; }

        public TelemetryClient Telemetry { get; }

        public IMediator Mediator { get; }

        public IAuditlogger Auditlogger { get; }

        public ILogger<DOMjudgeLikeHandlers> Logger { get; }

        public DOMjudgeLikeHandlers(
            IPolygonFacade facade,
            TelemetryClient telemetry,
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
