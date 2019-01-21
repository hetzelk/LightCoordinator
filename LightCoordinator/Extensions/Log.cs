using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightCoordinator.Extensions
{
    public static class Log
    {
        public static void Write(string data)
        {
            //TODO: check the programs log level
            if (true)
            {
                Console.WriteLine(data);
            }
        }

        public static void Error(string data)
        {
            //TODO: check the programs log level
            if (true)
            {
                Console.WriteLine("Error: " + data);
            }
        }
    }
}
