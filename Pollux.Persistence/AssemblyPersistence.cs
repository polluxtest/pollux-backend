namespace Pollux.Persistence
{
    using System.Reflection;

    public static class AssemblyPersistence
    {
        public static Assembly Assembly => Assembly.GetAssembly(typeof(AssemblyPersistence));
    }
}
