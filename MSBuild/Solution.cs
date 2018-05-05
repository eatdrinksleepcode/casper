using Casper;

public static class Solution {
    
    public static void ConfigureFromSolution(this ProjectBase rootProject, string solutionFilePath) {
        new SolutionConfigurator(rootProject, solutionFilePath).Configure();
    }
}
