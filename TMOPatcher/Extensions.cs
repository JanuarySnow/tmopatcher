using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Noggog;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace TMOPatcher
{
    public static class Extensions
    {
        public static bool HasKeyword(this IKeywordedGetter record, FormKey? formKey)
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

        public static bool HasAnyKeyword(this IKeywordedGetter record, FormKey?[] formKeys)
        {
            foreach (var formKey in formKeys)
            {
                if (HasKeyword(record, formKey))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool HasAnyKeyword(this IKeywordedGetter record, IReadOnlyList<FormKey> formKeys, [MaybeNullWhen(false)] out FormKey outKey)
        {
            foreach (var formKey in formKeys)
            {
                if (HasKeyword(record, formKey))
                {
                    outKey = formKey;
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
