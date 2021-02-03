using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using System;
using System.Collections.Generic;
using System.Text;

namespace TMOPatcher
{
    public static class Helpers
    {
        public static void Log(IMajorRecordCommonGetter record, string message)
        {
            Console.WriteLine($"{record.EditorID}({record.FormKey}): {message}");
        }
    }
}
