using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using System.Collections.Generic;
using System.Linq;

namespace TMOPatcher
{
    public static class Extensions
    {
        public static bool hasKeyword(this IArmorGetter record, FormKey? formKey)
        {
            return HasKeyword(record, formKey);
        }

        public static bool hasKeyword(this IWeaponGetter record, FormKey? formKey)
        {
            return HasKeyword(record, formKey);
        }

        public static bool hasAnyKeyword(this IWeaponGetter record, FormKey?[] formKeys)
        {
            return HasAnyKeyword(record, formKeys);
        }

        public static bool hasAnyKeyword(this IArmorGetter record, FormKey?[] formKeys)
        {
            return HasAnyKeyword(record, formKeys);
        }

        public static bool hasAnyKeyword(this IArmorGetter record, IReadOnlyList<FormKey> formKeys, out FormKey? outKey)
        {
            return HasAnyKeyword(record, formKeys, out outKey);
        }

        public static bool hasAnyKeyword(this IWeaponGetter record, IReadOnlyList<FormKey> formKeys, out FormKey? outKey)
        {
            return HasAnyKeyword(record, formKeys, out outKey);
        }

        public static bool HasKeyword(dynamic record, FormKey? formKey)
        {
            foreach (var kwda in record.Keywords ?? Enumerable.Empty<IFormLink<IKeywordGetter>>())
            {
                if (kwda?.FormKey == formKey)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool HasAnyKeyword(dynamic record, FormKey?[] formKeys)
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

        public static bool HasAnyKeyword(dynamic record, IReadOnlyList<FormKey> formKeys, out FormKey? outKey)
        {
            foreach (var formKey in formKeys)
            {
                if (HasKeyword(record, formKey))
                {
                    outKey = formKey;
                    return true;
                }
            }

            outKey = null;
            return false;
        }

        public static T AddReturn<T>(this IList<T> list, T item)
        {
            list.Add(item);
            return item;
        }
    }
}
