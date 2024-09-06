namespace Pollux.Application.Mappers
{
    using System.Reflection;

    /// <summary>
    /// Get this assembly for further mapping
    /// </summary>
    public static class AssemblyApplication
    {
        public static Assembly Assembly => Assembly.GetAssembly(typeof(AssemblyApplication));
    }
}
