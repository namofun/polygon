using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;

namespace SatelliteSite
{
    public class HostModule : AbstractModule
    {
        public override string Area => "Host";

        public override void Initialize()
        {
        }

        public override void RegisterEndpoints(IEndpointBuilder endpoints)
        {
            endpoints.MapControllers();

            endpoints.WithErrorHandler("Host", "Default")
                .MapStatusCode("/{**slug}");
        }
    }
}
