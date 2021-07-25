namespace Casper {
    public interface IProjectLoader {
        ProjectBase LoadProject(string projectPath, ProjectBase parent);
        ProjectBase LoadProject(string projectPath, ProjectBase parent, string name);
        ProjectBase LoadProject(string projectPath);
    }
}
