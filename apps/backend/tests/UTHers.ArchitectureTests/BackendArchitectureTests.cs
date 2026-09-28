using System.Xml.Linq;
using UTHers.Application.Integrations.Portal;
using UTHers.Contracts.UniversityConnections.Common;
using UTHers.Contracts.UniversityConnections.Portal;

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
    public void ConnectPortalRequestContainsExactlyApprovedInputs()
    {
        var propertyNames = typeof(ConnectPortalRequest)
            .GetProperties()
            .Select(property => property.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            new[] { "CaptchaToken", "Password", "Username" },
            propertyNames);
    }

    [Fact]
    public void CaptchaTokenIsAnInputOnly()
    {
        Assert.Contains(
            typeof(ConnectPortalRequest).GetProperties(),
            property => property.Name == "CaptchaToken");
        Assert.DoesNotContain(
            GetPublicResponseContractProperties(),
            property => property.Name.Contains("CaptchaToken", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void PublicResponseContractsDoNotExposeSensitiveCredentialFields()
    {
        var forbiddenTerms = new[]
        {
            "Authorization",
            "CaptchaToken",
            "ChallengeToken",
            "Cookie",
            "Credential",
            "Jwt",
            "Password",
            "RefreshToken",
            "SessionId",
            "Token"
        };

        var responseProperties = GetPublicResponseContractProperties();
        Assert.NotEmpty(responseProperties);

        foreach (var responseProperty in responseProperties)
        {
            foreach (var forbiddenTerm in forbiddenTerms)
            {
                Assert.DoesNotContain(forbiddenTerm, responseProperty.Name, StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    [Fact]
    public void UniversityConnectionResponseContainsOnlyConnectionState()
    {
        var property = Assert.Single(typeof(UniversityConnectionResponse).GetProperties());

        Assert.Equal("IsConnected", property.Name);
        Assert.Equal(typeof(bool), property.PropertyType);
    }

    [Fact]
    public void ErrorResponseHasNoDeclarationOrProductionConsumer()
    {
        var productionRoot = Path.Combine(FindBackendRoot(), "src");
        var productionSources = Directory
            .GetFiles(productionRoot, "*.cs", SearchOption.AllDirectories)
            .Where(path => !IsGeneratedSource(path))
            .ToArray();

        Assert.DoesNotContain(
            productionSources,
            path => File.ReadAllText(path).Contains("ErrorResponse", StringComparison.Ordinal));
        Assert.False(File.Exists(Path.Combine(FindContractsRoot(), "Common", "Errors", "ErrorResponse.cs")));
    }

    [Fact]
    public void PublicAndApplicationContractsDoNotExposeInfrastructureTypes()
    {
        var contractAssemblies = new[]
        {
            typeof(ConnectPortalRequest).Assembly,
            typeof(IPortalClient).Assembly
        };
        var exposedTypes = contractAssemblies
            .SelectMany(assembly => assembly.GetExportedTypes())
            .SelectMany(GetPublicBoundaryTypes)
            .Distinct()
            .ToArray();

        Assert.DoesNotContain(
            exposedTypes,
            type => type.Namespace?.StartsWith("UTHers.Infrastructure", StringComparison.Ordinal) == true);
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

    private static Type[] GetPublicResponseContractTypes() =>
        typeof(ConnectPortalRequest).Assembly
            .GetExportedTypes()
            .Where(type => type.Name.EndsWith("Response", StringComparison.Ordinal))
            .ToArray();

    private static System.Reflection.PropertyInfo[] GetPublicResponseContractProperties() =>
        GetPublicResponseContractTypes()
            .SelectMany(type => type.GetProperties())
            .ToArray();

    private static IEnumerable<Type> GetPublicBoundaryTypes(Type type)
    {
        foreach (var boundaryType in ExpandType(type))
        {
            yield return boundaryType;
        }

        foreach (var property in type.GetProperties())
        {
            foreach (var boundaryType in ExpandType(property.PropertyType))
            {
                yield return boundaryType;
            }
        }

        foreach (var constructor in type.GetConstructors())
        {
            foreach (var parameter in constructor.GetParameters())
            {
                foreach (var boundaryType in ExpandType(parameter.ParameterType))
                {
                    yield return boundaryType;
                }
            }
        }

        foreach (var method in type.GetMethods())
        {
            foreach (var boundaryType in ExpandType(method.ReturnType))
            {
                yield return boundaryType;
            }

            foreach (var parameter in method.GetParameters())
            {
                foreach (var boundaryType in ExpandType(parameter.ParameterType))
                {
                    yield return boundaryType;
                }
            }
        }
    }

    private static IEnumerable<Type> ExpandType(Type type)
    {
        yield return type;

        if (type.HasElementType && type.GetElementType() is { } elementType)
        {
            foreach (var expandedType in ExpandType(elementType))
            {
                yield return expandedType;
            }
        }

        foreach (var genericArgument in type.GetGenericArguments())
        {
            foreach (var expandedType in ExpandType(genericArgument))
            {
                yield return expandedType;
            }
        }
    }

    private static bool IsGeneratedSource(string path) =>
        path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase) ||
        path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase);
}
