using Casper.IO;
using NUnit.Framework;

namespace Casper {
	[TestFixture]
	public class SolutionTests {
		private readonly IFileSystem fileSystem = RealFileSystem.Instance;
		private IDirectory projectRootPath;
		private IDirectory libraryProjectPath;
		private IDirectory libraryUnitTestsProjectPath;

		[SetUp]
		public void SetUp() {
			projectRootPath = fileSystem.Directory(System.IO.Path.GetRandomFileName());
			libraryProjectPath = projectRootPath.Directory("Library");
			libraryUnitTestsProjectPath = projectRootPath.Directory("Library.UnitTests");

			System.IO.Directory.CreateDirectory(projectRootPath.Path);
			System.IO.Directory.CreateDirectory(libraryProjectPath.Path);
			System.IO.Directory.CreateDirectory(libraryUnitTestsProjectPath.Path);
		}

		[TearDown]
		public void TearDown() {
			projectRootPath.Delete();
		}
		
		[Test]
		public void ConfigureSolution() {
			var slnFile = projectRootPath.File("Test.sln");
			slnFile.WriteAllText(@"
Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio 2013
VisualStudioVersion = 12.0.31101.0
MinimumVisualStudioVersion = 10.0.40219.1
Project(""{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}"") = ""My.Library"", ""Library\My.Library.csproj"", ""{1027A646-4C10-44B9-939F-639D49D7CBF7}""
EndProject
Project(""{2150E333-8FDC-42A3-9474-1A3956D46DE8}"") = ""Extra Files"", ""Extras"", ""{42CCF79A-CE5F-4FB8-ABD4-F7E747823504}""
	ProjectSection(SolutionItems) = preProject
		Extras\AssemblyInfo.cs = Extras\AssemblyInfo.cs
		Extras\packages.config = Extras\packages.config
	EndProjectSection
EndProject
Project(""{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}"") = ""My.Library.UnitTests"", ""Library.UnitTests\My.Library.UnitTests.csproj"", ""{3D7D7E4E-5F2B-44DD-9AB8-B52468ECFC41}""
EndProject
");

			projectRootPath.Directory("Library").File("My.Library.csproj").WriteAllText(
@"<?xml version=""1.0"" encoding=""utf-8""?>
<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
</Project>
");

			projectRootPath.Directory("Library.UnitTests").File("My.Library.UnitTests.csproj").WriteAllText(
@"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <ItemGroup>
    <ProjectReference Include=""..\Library\My.Library.csproj"">
      <Project>{1027A646-4C10-44B9-939F-639D49D7CBF7}</Project>
      <Name>My.Library</Name>
    </ProjectReference>
  </ItemGroup>
</Project>"
);

			var rootProject = new TestProject(fileSystem, projectRootPath);
			var libraryProject = new TestProject(rootProject, projectRootPath.Directory("Library"), "My.Library");
			var libraryTestProject = new TestProject(rootProject, projectRootPath.Directory("Library.UnitTests"), "My.Library.UnitTests");
			rootProject.ConfigureFromSolution(slnFile.Path);

			Assert.That(rootProject.Projects.Count, Is.EqualTo(2));
			Assert.That(libraryTestProject.Tasks["Compile"].DependsOn, Contains.Item(libraryProject.Tasks["Compile"]));
			CollectionAssert.IsEmpty(libraryTestProject.Tasks["Clean"].DependsOn);
		}
	}
}
