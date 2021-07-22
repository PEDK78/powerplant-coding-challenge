#load build/paths.cake
#addin nuget:?package=Cake.Coverlet&version=2.5.4
#tool nuget:?package=ReportGenerator&version=4.8.12
#tool nuget:?package=GitVersion.CommandLine&version=5.6.10
#tool nuget:?package=NuGet.CommandLine&version=5.9.1

var target = Argument("target", "Package");

var configuration = Argument("configuration", "Release");
var framework = Argument("framework", "net5.0");

var codeCoverageReportPath = Argument<FilePath>("CodeCoverageReportPath", @"coverage.zip");
var mainProjectPath = Argument<FilePath>("ProjectPath", @"./src/Engie.PCC.Api/Engie.PCC.Api.csproj");
var packageOutputPath = Argument<DirectoryPath>("PackageOutputPath", @"packages");


var publishDirectory = @"./publish/";
var codeCoverageDirectory = @"./code-coverage/";
var packageVersion = "0.0.1";
var cuberturaFileName = "results";
var jsonFilePath = codeCoverageDirectory + File("code-coverage.json");


//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    if (!DirectoryExists(codeCoverageDirectory))
    {
        CreateDirectory(codeCoverageDirectory);
    }

    //CleanDirectories($"./src/**/bin/{configuration}");
    CleanDirectories(Paths.SourceDirectory + "/**/bin");
	CleanDirectories("./**/TestResults");
    CleanDirectory(publishDirectory);
    CleanDirectory(packageOutputPath);
    CleanDirectory(codeCoverageDirectory);

    if(FileExists("coverage.zip"))
    {
        DeleteFile("coverage.zip");
    }	
});

Task("Restore")
    .Does(() =>
{
    DotNetCoreRestore(Paths.SolutionFile.ToString());
});

Task("Version")
    .Does(() => {
        var version = GitVersion(new GitVersionSettings{ OutputType = GitVersionOutput.Json });
        Information($"Calculated semantic version: {version.SemVer}");
        
        packageVersion = version.NuGetVersionV2;
        Information($"Corresponding package version: {packageVersion}");

        if(!BuildSystem.IsLocalBuild)
        {
            GitVersion(new GitVersionSettings{
                UpdateAssemblyInfo = true,
                OutputType = GitVersionOutput.BuildServer
            });
        }

        var propsFile = "./Directory.Build.props";
        XmlPoke(propsFile, "//Version", packageVersion);
    });

Task("Build")
	.IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .IsDependentOn("Version")
    .Does(() =>
{
    DotNetCoreBuild(Paths.SolutionFile.ToString(), new DotNetCoreBuildSettings
    {
        Configuration = configuration,
    });
});

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
{
    var testSettings = new DotNetCoreTestSettings {
        Configuration = configuration,
        NoRestore = true,
        NoBuild = true,
        Logger = "trx"
    };
	
    var coverletSettings = new CoverletSettings {
        CollectCoverage = true,
        CoverletOutputFormat = CoverletOutputFormat.cobertura,
        CoverletOutputDirectory = codeCoverageDirectory,
        CoverletOutputName = cuberturaFileName
    };

    var projects = GetFiles(Paths.TestsDirectory + "/**/*.csproj");
    var firstProject = projects.First();
    var lastProject = projects.Last();

	foreach(var project in projects)
	{
        Information("Running tests in {0}", project.GetFilenameWithoutExtension());

        testSettings.VSTestReportPath = File("./TestResults/" + project.GetFilenameWithoutExtension() + ".trx");

        if(project != firstProject)
        {
            coverletSettings.MergeWithFile = jsonFilePath;
        }
	
        if(project == lastProject)
        {
           coverletSettings.CoverletOutputFormat  = CoverletOutputFormat.cobertura; 
        }

		DotNetCoreTest(project.FullPath, testSettings, coverletSettings);
	}
});

Task("CodeCoverageReport")
    .IsDependentOn("Test")
    .WithCriteria(() => BuildSystem.IsLocalBuild)
    .Does(() =>
{
    var cuberturaFileExtension = ".cobertura.xml";
    var resultFile = File(cuberturaFileName + cuberturaFileExtension);
    var coverageFilePath = File(codeCoverageDirectory + cuberturaFileName + cuberturaFileExtension);
        
    var reportSettings = new ReportGeneratorSettings
    {
        ReportTypes = new [] { ReportGeneratorReportType.HtmlInline_AzurePipelines_Dark }
    };

    ReportGenerator(coverageFilePath, Directory(codeCoverageDirectory), reportSettings);
    Zip(codeCoverageDirectory, codeCoverageReportPath);
});

Task("Package")
    .IsDependentOn("CodeCoverageReport")
    .Does(() =>
{
    var publishSettings = new DotNetCorePublishSettings
    {
        Framework = framework,
        Configuration = configuration,
        OutputDirectory = publishDirectory
    };
    DotNetCorePublish(mainProjectPath.ToString(), publishSettings);

    var propsFile = "./Directory.Build.props";
    var title = XmlPeek(propsFile, "//AssemblyTitle");
    var authors = XmlPeek(propsFile, "//Authors");
    var description = XmlPeek(propsFile, "//Description");

    var nuGetPackSettings   = new NuGetPackSettings 
    {
        Id                      = "PEDK-Engie-CPP",
        Version                 = packageVersion,
        Title                   = title,
        Authors                 = new[] {authors},
        //Owners                  = new[] {""},
        Description             = description,
        Summary                 = "",
        Copyright               = "",
        //ReleaseNotes            = new [] {"Bug fixes", "Issue fixes", "Typos"},
        //Tags                    = new [] {"Cake", "Script", "Build"},
        RequireLicenseAcceptance = false,
        Symbols                 = false,
        NoPackageAnalysis       = true,
        Files                   = new [] {new NuSpecContent {Source = "**", Target = "App"}, },
        BasePath                = publishDirectory,
        OutputDirectory         = packageOutputPath
    };

     NuGetPack(nuGetPackSettings);
});

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);