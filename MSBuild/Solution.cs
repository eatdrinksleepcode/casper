using Casper;

public static class Solution {
    
    public static void ConfigureFromSolution(this ProjectBase rootProject, string solutionFilePath, IProjectLoader loader = null) {
        new SolutionConfigurator(rootProject, solutionFilePath, loader).Configure();
    }
}
