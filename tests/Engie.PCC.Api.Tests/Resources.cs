using System.IO;
using System.Reflection;

namespace Engie.PCC.Api.Tests
{
    public static class Resources
    {
        public static string ReadResourceFile(string name)
        {
            string resourceName = $"Engie.PCC.Api.Tests.{name}";
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
