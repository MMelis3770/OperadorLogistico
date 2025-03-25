using System.Reflection;

namespace SeidorSmallWebApi.Extensions;

public static class ExtensionsHelper
{
    public static List<Assembly> GetModulePathAssemblies(string path)
    {
        var libraries = Directory.GetFiles(path, "*.dll", SearchOption.AllDirectories);        

        var assemblies = new List<Assembly>();
        foreach (string library in libraries)
        {
            try
            {
                var assembly = Assembly.LoadFrom(library);
                assemblies.Add(assembly);
            }
            catch
            {
                // Ignore errors when loading assemblies, make sure web application does not crash
                continue;
            }
        }

        return assemblies;
    }

    public static List<Assembly> GetExecutablePathAssemblies(string modulePath)
    {
        string executingPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.RelativeSearchPath ?? "");
        var allLibraries = Directory.GetFiles(executingPath, "*.dll", SearchOption.AllDirectories);
        var modulePathLibraries = Directory.GetFiles(modulePath, "*.dll", SearchOption.AllDirectories);

        // Do not add references from module path again
        var libraries = allLibraries.Except(modulePathLibraries);

        var assemblies = new List<Assembly>();
        foreach (string library in libraries)
        {
            try
            {
                var assembly = Assembly.LoadFrom(library);
                assemblies.Add(assembly);
            }
            catch
            {
                // Ignore errors when loading assemblies, make sure web application does not crash
                continue;
            }
        }

        return assemblies;
    }
}