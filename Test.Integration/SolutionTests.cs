using Casper.IO;
using NUnit.Framework;

namespace Casper {
	[TestFixture]
	public class SolutionTests {
		readonly IFileSystem fileSystem = RealFileSystem.Instance;
		IDirectory projectRootPath;
		IDirectory libraryProjectPath;
		IDirectory libraryUnitTestsProjectPath;

		[SetUp]
		public void SetUp() {
			projectRootPath = fileSystem.MakeTemporaryDirectory();
			libraryProjectPath = projectRootPath.Directory("Library");
			libraryUnitTestsProjectPath = projectRootPath.Directory("Library.UnitTests");

			System.IO.Directory.CreateDirectory(projectRootPath.FullPath);
			System.IO.Directory.CreateDirectory(libraryProjectPath.FullPath);
			System.IO.Directory.CreateDirectory(libraryUnitTestsProjectPath.FullPath);
		}

		[TearDown]
		public void TearDown() {
			projectRootPath.Delete();
		}
		
		[Test]
		public void ConfigureAllProjectsWithDependencies() {
			var slnFile = projectRootPath.File("Test.sln");
			slnFile.WriteAllText(@"
Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio 2013
VisualStudioVersion = 12.0.31101.0
MinimumVisualStudioVersion = 10.0.40219.1
Project(""{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}"") = ""My.Library"", ""Library\My.Library.csproj"", ""{1027A646-4C10-44B9-939F-639D49D7CBF7}""
EndProject
Project(""{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}"") = ""My.Library.UnitTests"", ""Library.UnitTests\My.Library.UnitTests.csproj"", ""{3D7D7E4E-5F2B-44DD-9AB8-B52468ECFC41}""
EndProject
");

			projectRootPath.File("Library/My.Library.csproj").WriteAllText(
@"<?xml version=""1.0"" encoding=""utf-8""?>
<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
</Project>
");

			projectRootPath.File("Library.UnitTests/My.Library.UnitTests.csproj").WriteAllText(
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
			rootProject.ConfigureFromSolution(slnFile.FullPath);

			Assert.That(rootProject.Projects.Count, Is.EqualTo(2));
			var libraryProject = rootProject.Projects["My.Library"];
			var libraryTestProject = rootProject.Projects["My.Library.UnitTests"];
			Assert.That(libraryTestProject.Tasks["Compile"].DependsOn, Contains.Item(libraryProject.Tasks["Compile"]));
			CollectionAssert.IsEmpty(libraryTestProject.Tasks["Clean"].DependsOn);
		}

		[Test]
		public void ConfigureProjectFromSolutionAlreadyIncluded() {
			var slnFile = projectRootPath.File("Test.sln");
			slnFile.WriteAllText(@"
Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio 2013
VisualStudioVersion = 12.0.31101.0
MinimumVisualStudioVersion = 10.0.40219.1
Project(""{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}"") = ""My.Library"", ""Library\My.Library.csproj"", ""{1027A646-4C10-44B9-939F-639D49D7CBF7}""
EndProject
");

			projectRootPath.File("Library/My.Library.csproj").WriteAllText(
@"<?xml version=""1.0"" encoding=""utf-8""?>
<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
</Project>
");

			var rootProject = new TestProject(fileSystem, projectRootPath);
			var libraryProject = new TestProject(rootProject, projectRootPath.Directory("Library"), "My.Library");
			rootProject.ConfigureFromSolution(slnFile.FullPath);
			libraryProject.AddTask("Task1", new TestTask());

			Assert.That(rootProject.Projects.Count, Is.EqualTo(1));
			Assert.That(libraryProject.Tasks, Has.Count.EqualTo(3));
		}

		[Test]
		public void ConfigureSolutionWithSolutionDirectory() {
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
");

			projectRootPath.File("Library/My.Library.csproj").WriteAllText(
@"<?xml version=""1.0"" encoding=""utf-8""?>
<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
</Project>
");

			var rootProject = new TestProject(fileSystem, projectRootPath);
			rootProject.ConfigureFromSolution(slnFile.FullPath);

			Assert.That(rootProject.Projects.Count, Is.EqualTo(1));
		}

		[Test]
		public void ConfigureProjectWithOtherProjectFileInDirectory() {
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

			projectRootPath.File("Library/My.Library.csproj").WriteAllText(
@"<?xml version=""1.0"" encoding=""utf-8""?>
<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
	<Target Name=""Compile""/>
	<Target Name=""Clean""/>
</Project>
");

			projectRootPath.File("Library/My.Library.other.csproj").WriteAllText("");

			var rootProject = new TestProject(fileSystem, projectRootPath);
			rootProject.ConfigureFromSolution(slnFile.FullPath);

			Assert.That(rootProject.Projects.Count, Is.EqualTo(1));
			Assert.DoesNotThrow(() => rootProject.ExecuteTasks("My.Library:Compile"));
			Assert.DoesNotThrow(() => rootProject.ExecuteTasks("My.Library:Clean"));
		}
		
		[Test]
		public void ConfigureProjectWithCustomizedBuild() {
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

			projectRootPath.File("Library/My.Library.csproj").WriteAllText(
				@"<?xml version=""1.0"" encoding=""utf-8""?>
<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
	<Target Name=""Compile""/>
	<Target Name=""Clean""/>
</Project>
");

			projectRootPath.File("Library/build.casper").WriteAllText(@"
task PreCompile
");

			var rootProject = new TestProject(fileSystem, projectRootPath);
			rootProject.ConfigureWith((p, loader) => {
				p.ConfigureFromSolution(slnFile.FullPath, loader);				
			});
			rootProject.ConfigureAll(new BooProjectLoader(fileSystem, "build.casper"));

			Assert.That(rootProject.Projects.Count, Is.EqualTo(1));
			Assert.DoesNotThrow(() => rootProject.ExecuteTasks("My.Library:PreCompile"));
			Assert.DoesNotThrow(() => rootProject.ExecuteTasks("My.Library:Compile"));
			Assert.DoesNotThrow(() => rootProject.ExecuteTasks("My.Library:Clean"));
		}

	}
}
