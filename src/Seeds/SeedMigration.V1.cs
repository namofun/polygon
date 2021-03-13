using Microsoft.EntityFrameworkCore.Migrations;

namespace Polygon.Storages
{
    public static class SeedMigrationV1
    {
        public static void Up(MigrationBuilder migrationBuilder)
        {
            var executables = SeedResource.GetSeedExecutables();
            var executableValues = new object[executables.Count, 6];
            for (int i = 0; i < executables.Count; i++)
            {
                executableValues[i, 0] = executables[i].Id;
                executableValues[i, 1] = executables[i].Description;
                executableValues[i, 2] = executables[i].Md5sum;
                executableValues[i, 3] = executables[i].Type;
                executableValues[i, 4] = executables[i].ZipFile;
                executableValues[i, 5] = executables[i].ZipSize;
            }

            var languages = SeedResource.GetSeedLanguages();
            var languageValues = new object[languages.Count, 7];
            for (int i = 0; i < languages.Count; i++)
            {
                languageValues[i, 0] = languages[i].Id;
                languageValues[i, 1] = languages[i].AllowJudge;
                languageValues[i, 2] = languages[i].AllowSubmit;
                languageValues[i, 3] = languages[i].CompileScript;
                languageValues[i, 4] = languages[i].FileExtension;
                languageValues[i, 5] = languages[i].Name;
                languageValues[i, 6] = languages[i].TimeFactor;
            }

            migrationBuilder.InsertData(
                table: "PolygonExecutables",
                columns: new[] { "ExecId", "Description", "Md5sum", "Type", "ZipFile", "ZipSize" },
                values: executableValues);

            migrationBuilder.InsertData(
                table: "PolygonLanguages",
                columns: new[] { "LangId", "AllowJudge", "AllowSubmit", "CompileScript", "FileExtension", "Name", "TimeFactor" },
                values: languageValues);
        }
    }
}
