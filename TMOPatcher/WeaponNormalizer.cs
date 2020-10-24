using Mutagen.Bethesda;
using Mutagen.Bethesda.FormKeys.SkyrimSE;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using System;
using System.Linq;
using static TMOPatcher.Helpers;

namespace TMOPatcher
{
    public class WeaponNormalizer
    {
        public Statics Statics { get; }
        public SynthesisState<ISkyrimMod, ISkyrimModGetter> State { get; }

        public WeaponNormalizer(Statics statics, SynthesisState<ISkyrimMod, ISkyrimModGetter> state) 
        {
            Statics = statics;
            State = state;
        }

        public void RunPatch()
        {
            var loadOrder = State.LoadOrder.PriorityOrder
                .OnlyEnabled()
                .Where(modGetter => !Statics.ExcludedMods.Contains(modGetter.ModKey));

            foreach (var record in loadOrder.WinningOverrides<IWeaponGetter>().Where(weapon => ShouldPatchWeapon(weapon)))
            {
                var baseWeapon = GetBaseWeapon(record);
                if (baseWeapon == null) continue;
                if (baseWeapon.FormKey == record.FormKey) continue;

                var weapon = State.PatchMod.Weapons.GetOrAddAsOverride(record);

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
            var excludedWeaponTypes = new FormKey?[] { Skyrim.Keyword.WeapTypeStaff, Skyrim.Keyword.WeapTypeBow };
            if (weapon.HasAnyKeyword(excludedWeaponTypes)) return false;

            if (weapon.Template.FormKey != null) return false;

            if (weapon.Data == null) return false;

            if (weapon.Data.Flags.HasFlag(WeaponData.Flag.NonPlayable)) return false;

            if (weapon.Name == null) return false;

            return true;
        }

        private IWeaponGetter? GetBaseWeapon(IWeaponGetter weapon)
        {
            if (!weapon.HasAnyKeyword(Statics.WeaponMaterials, out var material))
            {
                Log(weapon, "Couldn't determine the weapon material");
                return null;
            }

            if (!weapon.HasAnyKeyword(Statics.WeaponTypes, out var type))
            {
                Log(weapon, "Couldn't determine the weapon type");
                return null;
            }

            if (!Statics.BaseWeapons.TryGetValue(material, out var weaponTypes))
            {
                Log(weapon, $"Material({material}) is not valid");
                return null;
            }

            if (!weaponTypes.TryGetValue(type, out var baseWeapon))
            {
                Log(weapon, $"WeaponType({type}) is not valid");
                return null;
            }

            return Statics.BaseWeapons[material][type];
        }
    }
}
