namespace Pollux.Application.Mappers
{
    using System.Reflection;

    public static class AssemblyApplication
    {
        public static Assembly Assembly => Assembly.GetAssembly(typeof(AssemblyApplication));
    }
}
