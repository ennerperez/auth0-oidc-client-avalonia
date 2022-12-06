//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var buildDir = Directory("./build") + Directory(configuration);

// Define solutions.
var solutions = new Dictionary<string, string> {
     { "./Auth0Avalonia.sln", "Any" },
};

// Define AssemblyInfo source.
var assemblyInfoFile = ParseAssemblyInfo("./src/Auth0Avalonia/Properties/AssemblyInfo.cs");

// Define Nuspec source
var nuspecFile = new System.IO.FileInfo("src/Auth0Avalonia/Package.nuspec");

// Define version.
var ticks = DateTime.Now.ToString("ddHHmmss");
var assemblyVersion = assemblyInfoFile.AssemblyVersion.Replace(".*", "." + ticks.Substring(ticks.Length-8,8));
var version = EnvironmentVariable("APPVEYOR_BUILD_VERSION") ?? Argument("version", assemblyVersion);

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectory(buildDir);
});

Task("Restore")
    .IsDependentOn("Clean")
    .Does(() =>
{
    foreach (var solution in solutions)
    {
        NuGetRestore(solution.Key);
    }
});

Task("Build")
    .Does(context =>
{
	foreach (var solution in solutions)
	{
		DotNetBuild(solution.Key, new DotNetBuildSettings
        {
            Configuration = configuration,
        });
	}
});

Task("Package")
    .IsDependentOn("Build")
    .Does(context =>
{
	NuGetPack(nuspecFile.FullName, new NuGetPackSettings {
                Id = assemblyInfoFile.Title.Replace(" ", "."),
                Title = assemblyInfoFile.Title,
				Version = version,
				Authors = new[] { assemblyInfoFile.Company },
				Summary = assemblyInfoFile.Description,
				Copyright = assemblyInfoFile.Copyright,
				Properties = new Dictionary<string, string>() {{ "Configuration", configuration }},
                Verbosity = NuGetVerbosity.Detailed,
                OutputDirectory = buildDir
			});
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
	.IsDependentOn("Package");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);