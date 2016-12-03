using System;
using System.IO;
using System.Reflection;

namespace AirBreather
{
    // I use embedded resources whenever I reasonably can...
    // so making them just a tiny bit easier to use helps.
    public static class ResourceUtility
    {
        public static Stream OpenEmbeddedResourceFile(string resourceKey) => OpenEmbeddedResourceFile(resourceKey, Assembly.GetCallingAssembly());

        public static Stream OpenEmbeddedResourceFile(string resourceKey, Assembly assembly)
        {
            assembly.ValidateNotNull(nameof(assembly));
            resourceKey.ValidateNotBlank(nameof(resourceKey));

            // Assumes the assembly's default namespace is equal to its name,
            // which is ridiculous in the extremely general case, but fine for me.
            return assembly.GetManifestResourceStream(String.Join(".", assembly.GetName().Name, resourceKey));
        }
    }
}
