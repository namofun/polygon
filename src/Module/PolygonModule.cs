using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Polygon.Packaging;
using Polygon.Storages;
using SatelliteSite;
using SatelliteSite.IdentityModule.Entities;
using System.IO;

[assembly: RoleDefinition(10, "Judgehost", "judgehost", "(Internal/System) Judgehost")]
[assembly: RoleDefinition(11, "Problem", "prob", "Problem Provider")]

[assembly: ConfigurationInteger(1, "Judging", "process_limit", 64, "Maximum number of processes that the submission is allowed to start (including shell and possibly interpreters).")]
[assembly: ConfigurationInteger(2, "Judging", "script_timelimit", 30, "Maximum seconds available for compile/compare scripts. This is a safeguard against malicious code and buggy scripts, so a reasonable but large amount should do.")]
[assembly: ConfigurationInteger(3, "Judging", "script_memory_limit", 30, "Maximum memory usage (in kB) by compile/compare scripts. This is a safeguard against malicious code and buggy script, so a reasonable but large amount should do.")]
[assembly: ConfigurationInteger(4, "Judging", "script_filesize_limit", 30, "Maximum filesize (in kB) compile/compare scripts may write. Submission will fail with compiler-error when trying to write more, so this should be greater than any *intermediate or final* result written by compilers.")]
[assembly: ConfigurationString(5, "Judging", "timelimit_overshoot", "1s|10%", "Time that submissions are kept running beyond timelimit before being killed. Specify as \"Xs\" for X seconds, \"Y%\" as percentage, or a combination of both separated by one of \"+|&\" for the sum, maximum, or minimum of both.")]
[assembly: ConfigurationInteger(6, "Judging", "output_storage_limit", 60000, "Maximum size of error/system output stored in the database (in bytes); use \"-1\" to disable any limits.")]
[assembly: ConfigurationInteger(7, "Judging", "diskspace_error", 1048576, "Minimum free disk space (in kB) on judgehosts.")]
[assembly: ConfigurationInteger(8, "Judging", "update_judging_seconds", 0, "Post updates to a judging every X seconds. Set to 0 to update after each judging_run.")]

namespace SatelliteSite.PolygonModule
{
    public class PolygonModule<TUser, TRole, TContext> : AbstractModule
        where TUser : User, new()
        where TRole : Role, IRoleWithProblem, new()
        where TContext : IdentityDbContext<TUser, TRole, int>
    {
        public override string Area => "Polygon";

        public override void Initialize()
        {
        }

        public override void RegisterEndpoints(IEndpointBuilder endpoints)
        {
            endpoints.MapControllers();

            endpoints.MapApiDocument(
                name: "domjudge",
                title: "Polygon Module",
                description: "DOMjudge judgehost compatible API",
                version: "v7.2.0");
        }

        public override void RegisterServices(IServiceCollection services)
        {
            services.ConfigureSwaggerGen(options => options.OperationFilter<SwaggerFixFilter>());
            services.AddDbModelSupplier<TContext, PolygonEntityConfiguration<TUser, TRole, TContext>>();
            services.AddPolygonStorage<PolygonFacade<TUser, TRole, TContext>>();
            services.AddPolygonPackaging();

            services.AddPolygonFileDirectory().Configure<IWebHostEnvironment>((options, environment) =>
            {
                options.JudgingDirectory = Path.Combine(environment.ContentRootPath, "Runs");
                options.ProblemDirectory = Path.Combine(environment.ContentRootPath, "Problems");
            });
        }
    }
}
