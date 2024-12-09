using Bureau.Google.Calendar.UI.Constants;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Reflection;

namespace Bureau.UI.Web.Components.Helpers
{
    public static class ModuleAssemblies
    {
        public static Assembly[] GetAssemblies()
        {

            //List<Assembly> allAssemblies = new List<Assembly>();

            //string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            //var files = Directory.GetFiles(path, "*.dll");

            //foreach (string dll in files.Where(x => Path.GetFileName(x).StartsWith("ModularBlazor")))
            //{
            //    allAssemblies.Add(Assembly.LoadFile(dll));
            //}
            //var returnAssemblies = allAssemblies
            //    .Where(w => w.GetTypes().Any(a => a.GetInterfaces().Contains(typeof(IModule))));

            //return returnAssemblies.ToList();
            Assembly[] assemblies = new Assembly[]
            {
                typeof(BureauGoogleCalendarUIUris).Assembly
            };

            return assemblies;

        }
    }
}
