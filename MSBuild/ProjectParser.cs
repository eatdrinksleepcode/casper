using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Casper.IO;

namespace Casper {
    public static class ProjectParser {
        public static IEnumerable<Tuple<string, IFile>> ExtractProjectsFromSolution(IFile file) {
            return from line in file.ReadAllLines()
                let match = crackProjectLine.Match(line)
                where match.Success
                let name = match.Groups["PROJECTNAME"].Value.Trim()
                let projectFile = file.Directory.File(match.Groups["RELATIVEPATH"].Value.Trim()
                    .Replace('\\', Path.DirectorySeparatorChar))
                where name != "Solution Items"
                // TODO: also filter out based on project type (GUID)
                where projectFile.Exists()
                select Tuple.Create(name, projectFile);
        }

        // From MSBuild SolutionParser
        private static readonly Regex crackProjectLine = new Regex
        (
            "^"                                             // Beginning of line
            + "Project\\(\"(?<PROJECTTYPEGUID>.*)\"\\)"
            + "\\s*=\\s*"                                    // Any amount of whitespace plus "=" plus any amount of whitespace
            + "\"(?<PROJECTNAME>.*)\""
            + "\\s*,\\s*"                                   // Any amount of whitespace plus "," plus any amount of whitespace
            + "\"(?<RELATIVEPATH>.*)\""
            + "\\s*,\\s*"                                   // Any amount of whitespace plus "," plus any amount of whitespace
            + "\"(?<PROJECTGUID>.*)\""
            + "$"                                           // End-of-line
        );
    }
}