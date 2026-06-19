var target = Argument("target", "Default");

const string ContainerName = "rankingcandidatos-postgres";
const string PostgresImage = "postgres:16-alpine";
const string PostgresPort = "5432";
const string PostgresPassword = "postgres";
const string PostgresDatabase = "rankingcandidatos";
const string PostgresVolume = "rankingcandidatos-postgres-data";

var backendDir = MakeAbsolute(Directory("."));
var rootDir = MakeAbsolute(Directory(".."));
var apiProject = backendDir.CombineWithFilePath("RankingCandidatos.Api/RankingCandidatos.Api.csproj");
var frontendDir = rootDir.Combine("frontend");
var npmCommand = IsRunningOnWindows() ? "npm.cmd" : "npm";
var dotnetEfArgs = $"tool run dotnet-ef -- database update --project \"{apiProject}\"";

bool CommandSucceeds(string command, string arguments)
{
    var exitCode = StartProcess(command, new ProcessSettings
    {
        Arguments = arguments,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        Silent = true,
        WorkingDirectory = rootDir
    });

    return exitCode == 0;
}

void RunOptional(string command, string arguments)
{
    StartProcess(command, new ProcessSettings
    {
        Arguments = arguments,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        Silent = true,
        WorkingDirectory = rootDir
    });
}

void StartPostgres()
{
    if (CommandSucceeds("docker", $"container inspect {ContainerName}"))
    {
        StartProcess("docker", $"start {ContainerName}");
        return;
    }

    StartProcess("docker",
        "run " +
        $"--name {ContainerName} " +
        "--detach " +
        $"--publish {PostgresPort}:5432 " +
        $"--env POSTGRES_PASSWORD={PostgresPassword} " +
        $"--env POSTGRES_DB={PostgresDatabase} " +
        $"--volume {PostgresVolume}:/var/lib/postgresql/data " +
        PostgresImage);
}

void ShowPostgresInfo()
{
    Information("");
    Information("Infraestrutura pronta.");
    Information($"- Banco Postgres: 127.0.0.1:{PostgresPort}");
    Information($"- Database: {PostgresDatabase}");
    Information("- Usuario: postgres");
    Information($"- Senha: {PostgresPassword}");
}

Task("Default")
    .Does(() =>
{
    Information("Targets disponíveis:");
    Information("- DevUp");
    Information("- DevUpApps");
    Information("- DbMigrate");
    Information("- DevDown");
    Information("- DevReset");
    Information("- DbLogs");
    Information("- RunApi");
    Information("- RunFrontend");
    Information("- Build");
});

Task("DevUp")
    .Does(() =>
{
    StartPostgres();
    ShowPostgresInfo();
});

Task("DbMigrate")
    .IsDependentOn("DevUp")
    .Does(() =>
{
    Information("");
    Information("Aplicando migrations do banco...");
    StartProcess("dotnet", new ProcessSettings
    {
        Arguments = dotnetEfArgs,
        WorkingDirectory = rootDir
    });
});

Task("DevUpApps")
    .IsDependentOn("DevUp")
    .Does(() =>
{
    Information("");
    Information("Iniciando API e frontend em processos separados...");

    StartProcess(npmCommand, new ProcessSettings
    {
        Arguments = "install",
        WorkingDirectory = frontendDir
    });

    StartAndReturnProcess("dotnet", new ProcessSettings
    {
        Arguments = $"run --project \"{apiProject}\"",
        WorkingDirectory = rootDir
    });

    StartAndReturnProcess(npmCommand, new ProcessSettings
    {
        Arguments = "run dev",
        WorkingDirectory = frontendDir
    });
});

Task("DevDown")
    .Does(() =>
{
    RunOptional("docker", $"rm --force {ContainerName}");
    Information("Infraestrutura local parada.");
});

Task("DevReset")
    .Does(() =>
{
    RunOptional("docker", $"rm --force {ContainerName}");
    RunOptional("docker", $"volume rm {PostgresVolume}");
    StartPostgres();
    Information("Banco local resetado.");
});

Task("DbLogs")
    .Does(() =>
{
    StartProcess("docker", $"logs {ContainerName}");
});

Task("RunApi")
    .Does(() =>
{
    StartProcess("dotnet", $"run --project \"{apiProject}\"");
});

Task("RunFrontend")
    .Does(() =>
{
    StartProcess(npmCommand, new ProcessSettings
    {
        Arguments = "install",
        WorkingDirectory = frontendDir
    });

    StartProcess(npmCommand, new ProcessSettings
    {
        Arguments = "run dev",
        WorkingDirectory = frontendDir
    });
});

Task("Build")
    .Does(() =>
{
    StartProcess("dotnet", $"build \"{apiProject}\"");

    StartProcess(npmCommand, new ProcessSettings
    {
        Arguments = "install",
        WorkingDirectory = frontendDir
    });

    StartProcess(npmCommand, new ProcessSettings
    {
        Arguments = "run build",
        WorkingDirectory = frontendDir
    });
});

RunTarget(target);
