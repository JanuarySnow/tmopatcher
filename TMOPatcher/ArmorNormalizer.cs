using Mutagen.Bethesda;
using Mutagen.Bethesda.FormKeys.SkyrimSE;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using System;
using System.Linq;
using static TMOPatcher.Helpers;

namespace TMOPatcher
{
    public class ArmorNormalizer
    {
        public Statics Statics { get; }
        public IPatcherState<ISkyrimMod, ISkyrimModGetter> State { get; }

        public ArmorNormalizer(Statics statics, IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            Statics = statics;
            State = state;
        }

        public void RunPatch()
        {
            var loadOrder = State.LoadOrder.PriorityOrder
                .OnlyEnabled()
                .Where(modGetter => !Statics.ExcludedMods.Contains(modGetter.ModKey));

            foreach (var record in loadOrder.WinningOverrides<IArmorGetter>().Where(armor => ShouldPatchArmor(armor)))
            {
                var baseArmor = GetBaseArmor(record);
                if (baseArmor == null) continue;
                if (baseArmor.FormKey == record.FormKey) continue;

                var armor = State.PatchMod.Armors.GetOrAddAsOverride(record);

                armor.ArmorRating = baseArmor.ArmorRating;
                armor.Value = baseArmor.Value;
                armor.Weight = baseArmor.Weight;
            }
        }

        private bool ShouldPatchArmor(IArmorGetter armor)
        {
            var excludedArmorTypes = new FormKey?[] { Skyrim.Keyword.ArmorClothing, Skyrim.Keyword.ArmorJewelry };
            if (armor.HasAnyKeyword(excludedArmorTypes)) return false;

            if (!armor.TemplateArmor.IsNull) return false;

            if (armor.BodyTemplate?.Flags.HasFlag(BodyTemplate.Flag.NonPlayable) == true) return false;

            if (armor.Name == null) return false;

            return true;
        }

        private IArmorGetter? GetBaseArmor(IArmorGetter armor)
        {
            FormKey type;

            if (armor.BodyTemplate == null)
            {
                Log(armor, "Armor did not have a BodyTemplate");
                return null;
            }

            if (armor.BodyTemplate.ArmorType == ArmorType.HeavyArmor)
                type = Skyrim.Keyword.ArmorHeavy;
            else if (armor.BodyTemplate.ArmorType == ArmorType.LightArmor) 
                type = Skyrim.Keyword.ArmorLight;
            else
            {
                Log(armor, "Couldn't determine if the armor was heavy or light.");
                return null;
            }

            if (!armor.HasAnyKeyword(Statics.ArmorMaterials, out var material))
            {
                Log(armor, "Couldn't determine the armor material");
                return null;
            }

            if (!armor.HasAnyKeyword(Statics.ArmorSlots, out var slot))
            {
                Log(armor, "Couldn't determine the armor slot");
                return null;
            }

            if (!Statics.BaseArmors.TryGetValue(type, out var materials))
            {
                Log(armor, "Armor did not have a valid armor type");
                return null;
            }

            if (!materials.TryGetValue(material, out var slots))
            {
                Log(armor, $"Material({material}) is not valid for this ArmorType({type})");
                return null;
            }

            if (!slots.TryGetValue(slot, out var baseArmor))
            {
                Log(armor, $"ArmorSlot({slot}): No valid armor slot (Helmet, Cuirass, etc) was found");
                return null;
            }

            return baseArmor;
        }
    }
}
