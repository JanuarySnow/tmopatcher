using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Noggog;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Mutagen.Bethesda.Plugins.Aspects;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using System.Linq;

namespace TMOPatcher
{
    public static class Extensions
    {
        public static bool HasKeyword(this IKeywordedGetter<IKeywordGetter> record, FormKey? formKey)
        {
            foreach (var kwda in record.Keywords.EmptyIfNull())
            {
                if (kwda?.FormKey == formKey)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool HasAnyKeyword(this IKeywordedGetter<IKeywordGetter> record, HashSet<IFormLinkGetter<IKeywordGetter>> formKeys)
        {
            foreach (var kwda in record.Keywords.EmptyIfNull())
            {
                if (formKeys.Contains(kwda))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool TryHasAnyKeyword(this IKeywordedGetter<IKeywordGetter> record, HashSet<IFormLinkGetter<IKeywordGetter>> formKeys, [MaybeNullWhen(false)] out IFormLinkGetter<IKeywordGetter> outKey)
        {
            foreach (var kwda in record.Keywords.EmptyIfNull())
            {
                if (formKeys.Contains(kwda))
                {
                    outKey = kwda;
                    return true;
                }
            }

            outKey = default;
            return false;
        }

        public static T AddReturn<T>(this IList<T> list, T item)
        {
            list.Add(item);
            return item;
        }
    }
}
