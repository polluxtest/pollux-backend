namespace Pollux.API
{
    using System.Reflection;

    public static class AssemblyPresentation
    {
        public static Assembly Assembly => Assembly.GetAssembly(typeof(AssemblyPresentation));
    }
}
