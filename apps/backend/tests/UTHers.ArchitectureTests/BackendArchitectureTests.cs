using System.Xml.Linq;

namespace UTHers.ArchitectureTests;

public sealed class BackendArchitectureTests
{
    private static readonly IReadOnlyDictionary<string, string[]> ExpectedReferences =
        new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            ["UTHers.Api"] = ["UTHers.Application", "UTHers.Contracts", "UTHers.Infrastructure"],
            ["UTHers.Application"] = ["UTHers.Domain"],
            ["UTHers.Contracts"] = [],
            ["UTHers.Domain"] = [],
            ["UTHers.Infrastructure"] = ["UTHers.Application", "UTHers.Domain"]
        };

    [Fact]
    public void ProductionProjectsHaveRequiredCompilerSettings()
    {
        foreach (var projectName in ExpectedReferences.Keys)
        {
            var project = LoadProject(projectName);

            Assert.Equal("net10.0", GetProperty(project, "TargetFramework"));
            Assert.Equal("enable", GetProperty(project, "Nullable"));
            Assert.Equal("enable", GetProperty(project, "ImplicitUsings"));
        }
    }

    [Fact]
    public void ProductionProjectReferencesMatchExpectedGraph()
    {
        foreach (var (projectName, expectedReferences) in ExpectedReferences)
        {
            var actualReferences = GetProjectReferences(LoadProject(projectName));

            Assert.Equal(
                expectedReferences.OrderBy(reference => reference, StringComparer.Ordinal),
                actualReferences.OrderBy(reference => reference, StringComparer.Ordinal));
        }
    }

    [Fact]
    public void ProductionProjectReferencesDoNotContainCycles()
    {
        var graph = ExpectedReferences.Keys.ToDictionary(
            projectName => projectName,
            projectName => GetProjectReferences(LoadProject(projectName)),
            StringComparer.Ordinal);

        var visited = new HashSet<string>(StringComparer.Ordinal);
        var visiting = new HashSet<string>(StringComparer.Ordinal);

        foreach (var projectName in graph.Keys)
        {
            Assert.False(HasCycle(projectName, graph, visiting, visited), $"Cycle detected from {projectName}.");
        }
    }

    [Fact]
    public void PublicResponseContractsDoNotExposeSensitiveCredentialFields()
    {
        var forbiddenTerms = new[]
        {
            "Authorization",
            "Cookie",
            "Credential",
            "Password",
            "Session",
            "Token"
        };

        var responseFiles = Directory.GetFiles(FindContractsRoot(), "*Response.cs", SearchOption.AllDirectories);
        Assert.NotEmpty(responseFiles);

        foreach (var responseFile in responseFiles)
        {
            var responseSource = File.ReadAllText(responseFile);

            foreach (var forbiddenTerm in forbiddenTerms)
            {
                Assert.DoesNotContain(forbiddenTerm, responseSource, StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    [Fact]
    public void IdentityContractsDoNotUseUniversityIdentityFields()
    {
        var forbiddenTerms = new[]
        {
            "MSSV",
            "StudentId",
            "UniversityUsername"
        };

        var identityRoot = Path.Combine(FindContractsRoot(), "Identity");
        var identityFiles = Directory.GetFiles(identityRoot, "*.cs", SearchOption.AllDirectories);
        Assert.NotEmpty(identityFiles);

        foreach (var identityFile in identityFiles)
        {
            var identitySource = File.ReadAllText(identityFile);

            foreach (var forbiddenTerm in forbiddenTerms)
            {
                Assert.DoesNotContain(forbiddenTerm, identitySource, StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    private static XDocument LoadProject(string projectName)
    {
        var projectPath = Path.Combine(FindBackendRoot(), "src", projectName, $"{projectName}.csproj");
        Assert.True(File.Exists(projectPath), $"Expected production project was not found: {projectPath}");

        return XDocument.Load(projectPath);
    }

    private static string GetProperty(XDocument project, string propertyName)
    {
        var values = project
            .Descendants()
            .Where(element => element.Name.LocalName == propertyName)
            .Select(element => element.Value.Trim())
            .ToArray();

        return Assert.Single(values);
    }

    private static string[] GetProjectReferences(XDocument project)
    {
        return project
            .Descendants()
            .Where(element => element.Name.LocalName == "ProjectReference")
            .Select(element => element.Attribute("Include")?.Value)
            .Where(include => !string.IsNullOrWhiteSpace(include))
            .Select(include => Path.GetFileNameWithoutExtension(include!.Replace('\\', '/')))
            .ToArray();
    }

    private static bool HasCycle(
        string projectName,
        IReadOnlyDictionary<string, string[]> graph,
        ISet<string> visiting,
        ISet<string> visited)
    {
        if (visited.Contains(projectName))
        {
            return false;
        }

        if (!visiting.Add(projectName))
        {
            return true;
        }

        foreach (var dependency in graph[projectName])
        {
            if (HasCycle(dependency, graph, visiting, visited))
            {
                return true;
            }
        }

        visiting.Remove(projectName);
        visited.Add(projectName);
        return false;
    }

    private static string FindBackendRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "UTHers.sln")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("Could not locate the backend root containing UTHers.sln.");
    }

    private static string FindContractsRoot()
    {
        return Path.Combine(FindBackendRoot(), "src", "UTHers.Contracts");
    }
}
