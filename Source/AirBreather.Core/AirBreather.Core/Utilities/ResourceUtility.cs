using System;
using System.IO;
using System.Reflection;

namespace AirBreather.Core.Utilities
{
    // I use embedded resources whenever I reasonably can...
    // so making them just a tiny bit easier to use helps.
    public static class ResourceUtility
    {
        public static Stream OpenEmbeddedResourceFile(string resourceKey)
        {
            return OpenEmbeddedResourceFile(resourceKey, Assembly.GetCallingAssembly());
        }

        public static Stream OpenEmbeddedResourceFile(string resourceKey, Assembly assembly)
        {
            // Assumes the assembly's default namespace is equal to its name,
            // which is ridiculous in the extremely general case, but fine for me.
            return assembly.GetManifestResourceStream(String.Join(".", assembly.GetName().Name, resourceKey));
        }
    }
}
