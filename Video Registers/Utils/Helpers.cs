using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Video_Registers.Utils
{
    public class Helpers
    {
        public static string GetVersionApp()
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            return version.ToString();
        }
    }
}
