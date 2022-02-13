using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Linq;

namespace Polygon.Storages
{
    public static class PostgresTimeDiff
    {
        public static double ExtractEpochFromAge(DateTimeOffset end, DateTimeOffset start)
            => (end - start).TotalSeconds;

        private static SqlFunctionExpression Builtin(
            string name,
            Type clrType,
            RelationalTypeMapping dbType,
            params SqlExpression[] arguments)
            => new(
                functionName: name,
                arguments: arguments,
                nullable: false,
                argumentsPropagateNullability: new[] { false, false },
                type: clrType,
                typeMapping: dbType);

        public static void ConfigureModel(ModelBuilder builder)
        {
            // EXTRACT(EPOCH FROM INTERVAL '5 days 3 hours')
            builder.HasDbFunction(new Func<DateTimeOffset, DateTimeOffset, double>(ExtractEpochFromAge).Method, func =>
            {
                func.HasParameter("end")
                    .HasStoreType("timestamp with time zone");

                func.HasParameter("start")
                    .HasStoreType("timestamp with time zone");

                func.HasStoreType("double precision");

                func.HasTranslation(exp =>
                    Builtin("EXTRACT", typeof(double), new DoubleTypeMapping("double precision"),
                        Builtin("EPOCH FROM AGE", typeof(TimeSpan), new TimeSpanTypeMapping("INTERVAL"),
                            exp.ToArray())));
            });
        }
    }
}
