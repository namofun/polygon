using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Polygon;
using Polygon.Packaging;
using SatelliteSite;

[assembly: RoleDefinition(10, "Judgehost", "judgehost", "(Internal/System) Judgehost")]
[assembly: RoleDefinition(11, "ProblemCreator", "prob", "Problem Provider")]

[assembly: ConfigurationInteger(1, "Judging", "process_limit", 64, "Maximum number of processes that the submission is allowed to start (including shell and possibly interpreters).")]
[assembly: ConfigurationInteger(2, "Judging", "script_timelimit", 30, "Maximum seconds available for compile/compare scripts. This is a safeguard against malicious code and buggy scripts, so a reasonable but large amount should do.")]
[assembly: ConfigurationInteger(3, "Judging", "script_memory_limit", 2097152, "Maximum memory usage (in kB) by compile/compare scripts. This is a safeguard against malicious code and buggy script, so a reasonable but large amount should do.")]
[assembly: ConfigurationInteger(4, "Judging", "script_filesize_limit", 540672, "Maximum filesize (in kB) compile/compare scripts may write. Submission will fail with compiler-error when trying to write more, so this should be greater than any *intermediate or final* result written by compilers.")]
[assembly: ConfigurationString(5, "Judging", "timelimit_overshoot", "1s|10%", "Time that submissions are kept running beyond timelimit before being killed. Specify as \"Xs\" for X seconds, \"Y%\" as percentage, or a combination of both separated by one of \"+|&\" for the sum, maximum, or minimum of both.")]
[assembly: ConfigurationInteger(6, "Judging", "output_storage_limit", 60000, "Maximum size of error/system output stored in the database (in bytes); use \"-1\" to disable any limits.")]
[assembly: ConfigurationInteger(7, "Judging", "diskspace_error", 1048576, "Minimum free disk space (in kB) on judgehosts.")]
[assembly: ConfigurationInteger(8, "Judging", "update_judging_seconds", 0, "Post updates to a judging every X seconds. Set to 0 to update after each judging_run.")]

namespace SatelliteSite.PolygonModule
{
    public class PolygonModule<TRole> : AbstractModule, IAuthorizationPolicyRegistry
        where TRole : class, IServiceRole, new()
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

            endpoints.WithErrorHandler("Polygon", "Editor")
                .MapFallbackNotFound("/polygon/{probid}/{**slug}")
                .MapStatusCode("/polygon/{probid:problem}/{**slug}");
        }

        public override void RegisterServices(IServiceCollection services)
        {
            services.AddScoped<IExportProvider, KattisExportProvider>();
            services.AddScoped<IStatementProvider, MarkdownStatementProvider>();
            services.AddScoped<IStatementWriter, MarkdownStatementWriter>();

            services.AddMediatRAssembly(typeof(Polygon.Judgement.DOMjudgeLikeHandlers).Assembly);

            services.AddImportProvider<KattisImportProvider>("kattis", "Kattis Package");
            // services.AddImportProvider<XmlImportProvider>("xysxml", "XiaoYang's XML");
            services.AddImportProvider<FpsImportProvider>("hustoj", "HUSTOJ FPS XML");
            services.AddImportProvider<CodeforcesImportProvider>("cfplyg", "CodeForces Polygon (Linux)");
            services.AddImportProvider<DataImportProvider>("data", "Data (.in and .out/.ans)");

            new TRole().Configure(services);
            services.EnsureScoped<Polygon.Storages.IPolygonFacade>();

            services.PostConfigure<PolygonOptions>(o => o.FinalizeSettings());

            services.AddScoped<IPolygonFeature, AccessorFeature>();
            services.ConfigureRouting(options =>
            {
                options.ConstraintMap.Add("problem", typeof(RequirePolygonFeatureConstraint));
            });
        }

        public override void RegisterMenu(IMenuContributor menus)
        {
            menus.Submenu(MenuNameDefaults.DashboardConfigurations, menu =>
            {
                menu.HasEntry(400)
                    .HasTitle(string.Empty, "Problems")
                    .HasLink("Dashboard", "Problems", "List")
                    .RequireRoles("Administrator,ProblemCreator");

                menu.HasEntry(401)
                    .HasTitle(string.Empty, "Executables")
                    .HasLink("Dashboard", "Executables", "List")
                    .RequireRoles("Administrator");

                menu.HasEntry(402)
                    .HasTitle(string.Empty, "Judgehosts")
                    .HasLink("Dashboard", "Judgehosts", "List")
                    .RequireRoles("Administrator");

                menu.HasEntry(403)
                    .HasTitle(string.Empty, "Internal Errors")
                    .HasLink("Dashboard", "InternalErrors", "List")
                    .RequireRoles("Administrator");

                menu.HasEntry(404)
                    .HasTitle(string.Empty, "Languages")
                    .HasLink("Dashboard", "Languages", "List")
                    .RequireRoles("Administrator");

                menu.HasEntry(405)
                    .HasTitle(string.Empty, "Submissions")
                    .HasLink("Dashboard", "Problems", "Status")
                    .RequireRoles("Administrator");
            });

            menus.Submenu(MenuNameDefaults.DashboardDocuments, menu =>
            {
                menu.HasEntry(150)
                    .HasTitle(string.Empty, "DOMjudge Judgehost API")
                    .HasLink("/api/doc/domjudge");
            });

            menus.Menu(MenuNameDefaults.DashboardNavbar, menu =>
            {
                menu.HasSubmenu(100, menu =>
                {
                    menu.HasLink("#")
                        .HasTitle("fas fa-gavel", "Judgings")
                        .RequireRoles("Administrator")
                        .HasBadge("judgehosts", BootstrapColor.warning)
                        .HasBadge("internalerrors", BootstrapColor.danger)
                        .ActiveWhenController("Judgehosts,InternalErrors");

                    menu.HasEntry(0)
                        .HasIdentifier("menu_judgehosts")
                        .HasLink("Dashboard", "Judgehosts", "List")
                        .HasTitle("fas fa-server fa-fw", "judgehosts")
                        .HasBadge("judgehosts", BootstrapColor.warning);

                    menu.HasEntry(1)
                        .HasIdentifier("menu_internal_error")
                        .HasLink("Dashboard", "InternalErrors", "List")
                        .HasTitle("fas fa-bolt fa-fw", "internal error")
                        .HasBadge("internalerrors", BootstrapColor.warning);
                });

                menu.HasEntry(300)
                    .HasTitle("fas fa-book-open", "Problems")
                    .HasLink("Dashboard", "Problems", "List")
                    .ActiveWhenController("Problems")
                    .RequireRoles("Administrator,ProblemCreator");
            });

            menus.Menu(ResourceDictionary.MenuNavbar, menu =>
            {
                menu.HasEntry(1)
                    .HasTitle("fas fa-lightbulb", "Description")
                    .HasLink("Polygon", "Description", "Preview")
                    .ActiveWhenController("Description");

                menu.HasEntry(2)
                    .HasTitle("fas fa-compass", "Testcases")
                    .HasLink("Polygon", "Testcases", "Testcases")
                    .ActiveWhenController("Testcases");

                menu.HasEntry(3)
                    .HasTitle("fas fa-file-code", "Submissions")
                    .HasLink("Polygon", "Submissions", "List")
                    .ActiveWhenController("Submissions");

                menu.HasEntry(4)
                    .HasTitle("fas fa-arrow-right", "Catalog")
                    .HasLink("Dashboard", "Problems", "List")
                    .RequireRoles("Administrator,ProblemCreator");
            });

            menus.Component(ResourceDictionary.ComponentProblemOverview);
        }

        public void RegisterPolicies(IAuthorizationPolicyContainer container)
        {
            container.AddPolicy2("HasDashboard", b => b.AcceptRole("ProblemCreator"));
        }
    }
}
