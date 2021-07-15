using System.Reflection;

namespace Pollux.API
{
    public static class AssemblyPresentation
    {
        public static Assembly Assembly => Assembly.GetAssembly(typeof(AssemblyPresentation));
    }
}
