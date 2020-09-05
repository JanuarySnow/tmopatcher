using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using System;
using System.Linq;
using static TMOPatcher.Helpers;

namespace TMOPatcher
{
    public class WeaponNormalizer
    {
        public Statics Statics { get; set; }

        public WeaponNormalizer(Statics statics) 
        {
            Statics = statics;
        }

        public SynthesisState<ISkyrimMod, ISkyrimModGetter>? State { get; set; }

        public void RunPatch(SynthesisState<ISkyrimMod, ISkyrimModGetter> state)
        {
            State = state;

            ModKey[] excludedMods = { "Skyrim.esm", "Update.esm", "Dawnguard.esm", "HearthFires.esm", "Dragonborn.esm", "Unofficial Skyrim Special Edition Patch.esp" };
            var loadOrder = state.LoadOrder.PriorityOrder.Where(modGetter => !excludedMods.Contains(modGetter.ModKey));

            foreach (var record in loadOrder.WinningOverrides<IWeaponGetter>().Where(weapon => ShouldPatchWeapon(weapon)))
            {
                var baseWeapon = GetBaseWeapon(record);
                if (baseWeapon == null) continue;
                if (baseWeapon.FormKey == record.FormKey) continue;

                var weapon = state.PatchMod.Weapons.GetOrAddAsOverride(record);

                if (baseWeapon.BasicStats != null && weapon.BasicStats != null)
                {
                    weapon.BasicStats.Damage = baseWeapon.BasicStats.Damage;
                    weapon.BasicStats.Value = baseWeapon.BasicStats.Value;
                    weapon.BasicStats.Weight = baseWeapon.BasicStats.Weight;
                }

                if (baseWeapon.Data != null && weapon.Data != null)
                {
                    weapon.Data.Reach = baseWeapon.Data.Reach;
                    weapon.Data.Speed = baseWeapon.Data.Speed;
                }

                if (baseWeapon.Critical != null && weapon.Critical != null)
                {
                    weapon.Critical.Damage = baseWeapon.Critical.Damage;
                }

                weapon.DetectionSoundLevel = baseWeapon.DetectionSoundLevel;
            }
        }

        private bool ShouldPatchWeapon(IWeaponGetter weapon)
        {
            var excludedWeaponTypes = new FormKey?[] { Statics.Keywords["WeapTypeStaff"], Statics.Keywords["WeapTypeBow"] };
            if (weapon.hasAnyKeyword(excludedWeaponTypes)) return false;

            if (weapon.Template.FormKey != null) return false;

            if (weapon.Data == null) return false;

            if (weapon.Data.Flags.HasFlag(WeaponData.Flag.NonPlayable)) return false;

            if (weapon.Name == null) return false;

            return true;
        }

        private IWeaponGetter? GetBaseWeapon(IWeaponGetter weapon)
        {
            FormKey? material;
            FormKey? type;

            if (!weapon.hasAnyKeyword(Statics.WeaponMaterials, out material) || material == null)
            {
                Log(weapon, "Couldn't determine the weapon material");
                return null;
            }

            if (!weapon.hasAnyKeyword(Statics.WeaponTypes, out type) || type == null)
            {
                Log(weapon, "Couldn't determine the weapon type");
                return null;
            }

            if (!Statics.BaseWeapons.TryGetValue((FormKey)material, out var weaponTypes))
            {
                Log(weapon, $"Material({material}) is not valid");
                return null;
            }

            if (!weaponTypes.TryGetValue((FormKey)type, out var baseWeapon))
            {
                Log(weapon, $"WeaponType({type}) is not valid");
            }

            return Statics.BaseWeapons[(FormKey)material][(FormKey)type];
        }
    }
}
