#addin Cake.Git

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var outputDirectory="Output";
var version=Argument<string>("version", "0.0.1");
var target = Argument<string>("target", "Default");
var release = Argument<bool>("release", true);
var nugetApiKey = Argument<string>("nugetApiKey", null);
var currentBranch = Argument<string>("currentBranch", GitBranchCurrent("./").FriendlyName);
var configuration=release?"Release":"Debug";

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(ctx =>
{
   // Executed BEFORE the first task.
   Information("Running tasks...");
});

Teardown(ctx =>
{
   // Executed AFTER the last task.
   Information("Finished running tasks.");
});

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("UpdateVersion").DoesForEach(GetFiles("**/Uragano.*.csproj"),(file)=>{
   XmlPoke(file,"/Project/PropertyGroup/Version",version);
   XmlPoke(file,"/Project/PropertyGroup/GeneratePackageOnBuild","false");
   XmlPoke(file,"/Project/PropertyGroup/Description","A simple, high performance RPC library.");
   XmlPoke(file,"/Project/PropertyGroup/PackageProjectUrl","https://github.com/1100100/Uragano");
   XmlPoke(file,"/Project/PropertyGroup/PackageTags","Uragano,RPC,DotNetty,Microservice,MessagePack,DynamicProxy,dotnetcore,service-discovery,polly,circuit-breaker,consul,zookeeper,dependency-injection");
   XmlPoke(file,"/Project/PropertyGroup/PackageIconUrl","https://raw.githubusercontent.com/1100100/Uragano/master/icon.png");
   XmlPoke(file,"/Project/PropertyGroup/Authors","Owen");
});

Task("Restore").Does(()=>{
   DotNetCoreRestore();
});

Task("Build").Does(()=>{
   DotNetCoreBuild("Uragano.sln",new DotNetCoreBuildSettings{
      Configuration=configuration
   });
});

Task("CleanPackage").Does(()=>{
   if(DirectoryExists(outputDirectory))
   {
      DeleteDirectory(outputDirectory,true);
   }
});

Task("Pack")
.IsDependentOn("CleanPackage")
.DoesForEach(GetFiles("**/Uragano.*.csproj"),(file)=>{
   DotNetCorePack(file.ToString(),new DotNetCorePackSettings{
      OutputDirectory=outputDirectory,
      Configuration=configuration
   });
});

Task("Push")
.IsDependentOn("Pack")
.Does(()=>{
   var nuGetPushSettings= new NuGetPushSettings {
      Source="https://www.nuget.org/api/v2/package",
      ApiKey=nugetApiKey
   };
   if(currentBranch=="master")
   {
      foreach (var package in GetFiles("Output/*.nupkg"))
      {
         NuGetPush(package,nuGetPushSettings);
      }
   }
   else
   {
      Information("Non-master build. Not publishing to NuGet. Current branch: " + currentBranch);
   }
});

Task("Default")
.IsDependentOn("UpdateVersion")
.IsDependentOn("Restore")
.IsDependentOn("Build")
.Does(() => {
   Information("Hello Cake!");
});



RunTarget(target);