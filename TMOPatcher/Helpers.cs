using System;
using System.Collections.Generic;
using System.Text;

namespace TMOPatcher
{
    public static class Helpers
    {
        public static void Log(dynamic record, string message)
        {
            Console.WriteLine($"{record.Name}({record.FormKey}): {message}");
        }
    }
}
