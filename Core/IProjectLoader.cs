namespace Casper {
    public interface IProjectLoader {
        ProjectBase LoadProject(string scriptPath, ProjectBase parent);
        ProjectBase LoadProject(string scriptPath, ProjectBase parent, string name);
    }
}
