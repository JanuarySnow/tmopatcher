using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using Newtonsoft.Json.Linq;
using Noggog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mutagen.Bethesda.FormKeys.SkyrimSE;

namespace TMOPatcher
{
    public class RecipeTemplate
    {
        public List<Item> Items = new List<Item>();
        public FormKey? Perk;
        public FormKey Bench;
    }

    public class Item
    {
        public FormKey Record;
        public int Count;
    }


    public class Statics
    {
        public static readonly ICollection<ModKey> ExcludedMods = new ModKey[] { Constants.Skyrim, Constants.Update, Constants.Dawnguard, Constants.HearthFires, Constants.Dragonborn, "Unofficial Skyrim Special Edition Patch.esp" }.ToHashSet();

        public IReadOnlyList<FormKey> ArmorMaterials { get; set; }
        public IReadOnlyList<FormKey> ArmorSlots { get; set; }

        public IReadOnlyList<FormKey> WeaponMaterials { get; set; }
        public IReadOnlyList<FormKey> WeaponTypes { get; set; }


        public Dictionary<FormKey, Dictionary<FormKey, Dictionary<FormKey, IArmorGetter?>>> BaseArmors;
        public Dictionary<FormKey, Dictionary<FormKey, IWeaponGetter?>> BaseWeapons;
        public Dictionary<FormKey, Dictionary<string, Dictionary<FormKey, RecipeTemplate>>> RecipeTemplates = new Dictionary<FormKey, Dictionary<string, Dictionary<FormKey, RecipeTemplate>>>();

        public Dictionary<string, FormKey> CraftingSupplies;
        public Dictionary<string, FormKey> Keywords = new Dictionary<string, FormKey>();
        public Dictionary<string, FormKey> Perks = new Dictionary<string, FormKey>();

        public Dictionary<string, Dictionary<string, Dictionary<FormKey, IConstructibleObjectGetter>>> Recipes = new Dictionary<string, Dictionary<string, Dictionary<FormKey, IConstructibleObjectGetter>>>()
        {
            ["armors"] = new Dictionary<string, Dictionary<FormKey, IConstructibleObjectGetter>>()
            {
                ["breakdown"] = new Dictionary<FormKey, IConstructibleObjectGetter>() { },
                ["creation"] = new Dictionary<FormKey, IConstructibleObjectGetter>() { },
                ["tempering"] = new Dictionary<FormKey, IConstructibleObjectGetter>() { }
            },

            ["weapons"] = new Dictionary<string, Dictionary<FormKey, IConstructibleObjectGetter>>()
            {
                ["breakdown"] = new Dictionary<FormKey, IConstructibleObjectGetter>() { },
                ["creation"] = new Dictionary<FormKey, IConstructibleObjectGetter>() { },
                ["tempering"] = new Dictionary<FormKey, IConstructibleObjectGetter>() { }
            },
        };
        public SynthesisState<ISkyrimMod, ISkyrimModGetter> State { get; set; }
        private ILinkCache LinkCache { get; }

        public Statics(SynthesisState<ISkyrimMod, ISkyrimModGetter> state)
        {
            State = state;

            ArmorSlots = new FormKey[]
            {
                Skyrim.Keyword.ArmorBoots,
                Skyrim.Keyword.ArmorCuirass,
                Skyrim.Keyword.ArmorGauntlets,
                Skyrim.Keyword.ArmorHelmet,
                Skyrim.Keyword.ArmorShield,
            };

            ArmorMaterials = new FormKey[]
            {
                Update.Keyword.ArmorMaterialBearStormcloak,
                Update.Keyword.ArmorMaterialBlades,
                Skyrim.Keyword.ArmorMaterialDaedric,
                Skyrim.Keyword.ArmorMaterialDragonplate,
                Skyrim.Keyword.ArmorMaterialDragonscale,
                Skyrim.Keyword.ArmorMaterialDwarven,
                Skyrim.Keyword.ArmorMaterialEbony,
                Skyrim.Keyword.ArmorMaterialElven,
                Skyrim.Keyword.ArmorMaterialElvenGilded,
                Skyrim.Keyword.ArmorMaterialLeather,
                Update.Keyword.ArmorMaterialForsworn,
                Update.Keyword.ArmorMaterialMS02Forsworn,
                Skyrim.Keyword.ArmorMaterialGlass,
                Skyrim.Keyword.ArmorMaterialHide,
                Skyrim.Keyword.ArmorMaterialImperialHeavy,
                Skyrim.Keyword.ArmorMaterialImperialLight,
                Skyrim.Keyword.ArmorMaterialImperialStudded,
                Skyrim.Keyword.ArmorMaterialIron,
                Skyrim.Keyword.ArmorMaterialIronBanded,
                Skyrim.Keyword.ArmorMaterialLeather,
                Skyrim.Keyword.ArmorMaterialOrcish,
                Update.Keyword.ArmorMaterialPenitus,
                Skyrim.Keyword.ArmorMaterialScaled,
                Skyrim.Keyword.ArmorMaterialSteel,
                Skyrim.Keyword.ArmorMaterialSteelPlate,
                Skyrim.Keyword.ArmorMaterialStormcloak,
                Skyrim.Keyword.ArmorMaterialStudded,
                Update.Keyword.ArmorMaterialThievesGuild,
                Update.Keyword.ArmorMaterialThievesGuildLeader,
                Skyrim.Keyword.ArmorNightingale,
                Dawnguard.Keyword.DLC1ArmorMaterialDawnguard,
                Dawnguard.Keyword.DLC1ArmorMaterialFalmerHardened,
                Dawnguard.Keyword.DLC1ArmorMaterialHunter,
                Dawnguard.Keyword.DLC1ArmorMaterialVampire,
                Dawnguard.Keyword.DLC1ArmorMaterielFalmerHeavy,
                Dawnguard.Keyword.DLC1ArmorMaterielFalmerHeavyOriginal,
                Dragonborn.Keyword.DLC2ArmorMaterialBonemoldHeavy,
                Dragonborn.Keyword.DLC2ArmorMaterialBonemoldLight,
                Dragonborn.Keyword.DLC2ArmorMaterialChitinHeavy,
                Dragonborn.Keyword.DLC2ArmorMaterialChitinLight,
                Dragonborn.Keyword.DLC2ArmorMaterialMoragTong,
                Dragonborn.Keyword.DLC2ArmorMaterialNordicHeavy,
                Dragonborn.Keyword.DLC2ArmorMaterialNordicLight,
                Dragonborn.Keyword.DLC2ArmorMaterialStalhrimHeavy,
                Dragonborn.Keyword.DLC2ArmorMaterialStalhrimLight,
            };

            WeaponMaterials = new FormKey[]
            {
                Dawnguard.Keyword.DLC1WeapMaterialDragonbone,
                Dragonborn.Keyword.DLC2WeaponMaterialNordic,
                Dragonborn.Keyword.DLC2WeaponMaterialStalhrim,
                Skyrim.Keyword.WeapMaterialDaedric,
                Skyrim.Keyword.WeapMaterialDraugr,
                Skyrim.Keyword.WeapMaterialDraugrHoned,
                Skyrim.Keyword.WeapMaterialDwarven,
                Skyrim.Keyword.WeapMaterialEbony,
                Skyrim.Keyword.WeapMaterialElven,
                Skyrim.Keyword.WeapMaterialFalmer,
                Skyrim.Keyword.WeapMaterialFalmerHoned,
                Skyrim.Keyword.WeapMaterialGlass,
                Skyrim.Keyword.WeapMaterialImperial,
                Skyrim.Keyword.WeapMaterialIron,
                Skyrim.Keyword.WeapMaterialOrcish,
                Skyrim.Keyword.WeapMaterialSilver,
                Skyrim.Keyword.WeapMaterialSteel,
                Skyrim.Keyword.WeapMaterialWood,
            };

            WeaponTypes = new FormKey[] {
                Skyrim.Keyword.WeapTypeBattleaxe,
                Skyrim.Keyword.WeapTypeBow,
                Skyrim.Keyword.WeapTypeDagger,
                Skyrim.Keyword.WeapTypeGreatsword,
                Skyrim.Keyword.WeapTypeMace,
                Skyrim.Keyword.WeapTypeStaff,
                Skyrim.Keyword.WeapTypeSword,
                Skyrim.Keyword.WeapTypeWarAxe,
                Skyrim.Keyword.WeapTypeWarhammer,
            };

            CraftingSupplies = new Dictionary<string, FormKey>()
            {
                { nameof(Skyrim.Ingestible.Ale),                  Skyrim.Ingestible.Ale },
                { nameof(Skyrim.MiscItem.BearCavePelt),           Skyrim.MiscItem.BearCavePelt },
                { nameof(Skyrim.Ingredient.BoneMeal),             Skyrim.Ingredient.BoneMeal },
                { nameof(Skyrim.MiscItem.Coal01),                 Skyrim.MiscItem.Coal01 },
                { nameof(Skyrim.MiscItem.Charcoal),               Skyrim.MiscItem.Charcoal },
                { nameof(Skyrim.MiscItem.ChaurusChitin),          Skyrim.MiscItem.ChaurusChitin },
                { nameof(Skyrim.Ingredient.DaedraHeart),          Skyrim.Ingredient.DaedraHeart },
                { nameof(Skyrim.Ingredient.deathBell),            Skyrim.Ingredient.deathBell },
                { nameof(Dragonborn.MiscItem.DLC2ChitinPlate),    Dragonborn.MiscItem.DLC2ChitinPlate },
                { nameof(Dragonborn.Ingredient.DLC2NetchJelly),   Dragonborn.Ingredient.DLC2NetchJelly },
                { nameof(Dragonborn.MiscItem.DLC2NetchLeather),   Dragonborn.MiscItem.DLC2NetchLeather },
                { nameof(Dragonborn.MiscItem.DLC2OreStalhrim),    Dragonborn.MiscItem.DLC2OreStalhrim },
                { nameof(Skyrim.MiscItem.DeerHide),               Skyrim.MiscItem.DeerHide },
                { nameof(Skyrim.MiscItem.DragonBone),             Skyrim.MiscItem.DragonBone },
                { nameof(Skyrim.MiscItem.DragonScales),           Skyrim.MiscItem.DragonScales },
                { nameof(Skyrim.Ingredient.FireSalts),            Skyrim.Ingredient.FireSalts },
                { nameof(Skyrim.MiscItem.Firewood01),             Skyrim.MiscItem.Firewood01 },
                { nameof(Skyrim.Ingredient.FrostSalts),           Skyrim.Ingredient.FrostSalts },
                { nameof(Skyrim.MiscItem.GoatHide),               Skyrim.MiscItem.GoatHide },
                { nameof(Skyrim.MiscItem.IngotCorundum),          Skyrim.MiscItem.IngotCorundum },
                { nameof(Skyrim.MiscItem.IngotDwarven),           Skyrim.MiscItem.IngotDwarven },
                { nameof(Skyrim.MiscItem.IngotEbony),             Skyrim.MiscItem.IngotEbony },
                { nameof(Skyrim.MiscItem.IngotGold),              Skyrim.MiscItem.IngotGold },
                { nameof(Skyrim.MiscItem.IngotIMoonstone),        Skyrim.MiscItem.IngotIMoonstone },
                { nameof(Skyrim.MiscItem.IngotIron),              Skyrim.MiscItem.IngotIron },
                { nameof(Skyrim.MiscItem.IngotMalachite),         Skyrim.MiscItem.IngotMalachite },
                { nameof(Skyrim.MiscItem.IngotOrichalcum),        Skyrim.MiscItem.IngotOrichalcum },
                { nameof(Skyrim.MiscItem.IngotQuicksilver),       Skyrim.MiscItem.IngotQuicksilver },
                { nameof(Skyrim.MiscItem.ingotSilver),            Skyrim.MiscItem.ingotSilver },
                { nameof(Skyrim.MiscItem.IngotSteel),             Skyrim.MiscItem.IngotSteel },
                { nameof(Skyrim.MiscItem.Leather01),              Skyrim.MiscItem.Leather01 },
                { nameof(Skyrim.MiscItem.LeatherStrips),          Skyrim.MiscItem.LeatherStrips },
                { nameof(Skyrim.Ingredient.PowderedMammothTusk),  Skyrim.Ingredient.PowderedMammothTusk },
                { nameof(Skyrim.MiscItem.SabreCatPelt),           Skyrim.MiscItem.SabreCatPelt },
                { nameof(Skyrim.Ingredient.SaltPile),             Skyrim.Ingredient.SaltPile },
                { nameof(Skyrim.SoulGem.SoulGemPetty),            Skyrim.SoulGem.SoulGemPetty },
                { nameof(Skyrim.Ingredient.VoidSalts),            Skyrim.Ingredient.VoidSalts },
                { nameof(Skyrim.MiscItem.WolfPelt),               Skyrim.MiscItem.WolfPelt },
 
                // Nord Hero Support 
                { nameof(Skyrim.Weapon.DraugrBattleAxeHoned),     Skyrim.Weapon.DraugrBattleAxeHoned },
                { nameof(Skyrim.Weapon.DraugrBowSupple),          Skyrim.Weapon.DraugrBowSupple },
                { nameof(Skyrim.Weapon.DraugrGreatswordHoned),    Skyrim.Weapon.DraugrGreatswordHoned },
                { nameof(Skyrim.Weapon.DraugrSwordHoned),         Skyrim.Weapon.DraugrSwordHoned },
                { nameof(Skyrim.Weapon.DraugrWarAxeHoned),        Skyrim.Weapon.DraugrWarAxeHoned },
            };

            var templates = new List<JObject>();
            var files = Directory.GetFiles("data");
            LinkCache = State.LoadOrder.ToImmutableLinkCache();

            foreach (var file in files)
            {
                TextReader textReader = File.OpenText(file);
                templates.Add(JObject.Parse(textReader.ReadToEnd()));
            }

            foreach (var kwda in state.LoadOrder.PriorityOrder.WinningOverrides<IKeywordGetter>())
            {
                if (kwda.EditorID == null) continue;

                Keywords[kwda.EditorID] = kwda.FormKey;
            }

            foreach (var perk in state.LoadOrder.PriorityOrder.WinningOverrides<IPerkGetter>())
            {
                if (perk.EditorID == null) continue;

                Perks[perk.EditorID] = perk.FormKey;
            }

            foreach (var cobj in state.LoadOrder.PriorityOrder.WinningOverrides<IConstructibleObjectGetter>())
            {
                if (cobj.CreatedObject.FormKey == null) continue;

                if (LinkCache.TryLookup<IArmorGetter>((FormKey)cobj.CreatedObject.FormKey, out var armor))
                {
                    CacheRecipe(cobj, armor, "armors");
                }
                else if (LinkCache.TryLookup<IWeaponGetter>((FormKey)cobj.CreatedObject.FormKey, out var weapon))
                {
                    CacheRecipe(cobj, weapon, "weapons");
                }
                else
                {
                    if (cobj.Items == null) continue;
                    if (LinkCache.TryLookup<IArmorGetter>(cobj.Items[0].Item.Item.FormKey, out armor) && armor.FormKey != cobj.CreatedObject.FormKey)
                    {
                        CacheRecipe(cobj, armor, "armors");
                    }
                    else if (LinkCache.TryLookup<IWeaponGetter>(cobj.Items[0].Item.Item.FormKey, out weapon) && weapon.FormKey != cobj.CreatedObject.FormKey)
                    {
                        CacheRecipe(cobj, weapon, "weapons");
                    }
                }
            }

            foreach (var template in templates)
            {
                var material = template["material"];

                if (material == null) continue;

                var key = new FormKey(material![0]!.ToString(), Convert.ToUInt32(material[1]!.ToString(), 16));
                RecipeTemplates[key] = new Dictionary<string, Dictionary<FormKey, RecipeTemplate>>();
                RecipeTemplates[key]["creation"] = new Dictionary<FormKey, RecipeTemplate>();
                RecipeTemplates[key]["tempering"] = new Dictionary<FormKey, RecipeTemplate>();
                RecipeTemplates[key]["breakdown"] = new Dictionary<FormKey, RecipeTemplate>();

                foreach (var slot in template["creation"].EmptyIfNull())
                {
                    ParseSlot(slot!, key, "creation");
                }

                foreach (var slot in template["tempering"].EmptyIfNull())
                {
                    ParseSlot(slot!, key, "tempering");
                }

                foreach (var slot in template["breakdown"].EmptyIfNull())
                {
                    ParseSlot(slot!, key, "breakdown");
                }
            }

            BaseArmors = new Dictionary<FormKey, Dictionary<FormKey, Dictionary<FormKey, IArmorGetter?>>>()
            {
                [Skyrim.Keyword.ArmorHeavy] = new Dictionary<FormKey, Dictionary<FormKey, IArmorGetter?>>()
                {
                    [Update.Keyword.ArmorMaterialBlades] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Skyrim.Keyword.ArmorBoots,      LookupArmor(Skyrim.Armor.ArmorBladesBoots) },
                        { Skyrim.Keyword.ArmorCuirass,    LookupArmor(Skyrim.Armor.ArmorBladesCuirass) },
                        { Skyrim.Keyword.ArmorGauntlets,  LookupArmor(Skyrim.Armor.ArmorBladesGauntlets) },
                        { Skyrim.Keyword.ArmorHelmet,     LookupArmor(Skyrim.Armor.ArmorBladesHelmet) },
                        { Skyrim.Keyword.ArmorShield,     LookupArmor(Skyrim.Armor.ArmorBladesShield) },
                    },

                    [Skyrim.Keyword.ArmorMaterialDaedric] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Skyrim.Keyword.ArmorBoots,      LookupArmor(Skyrim.Armor.ArmorDaedricBoots) },
                        { Skyrim.Keyword.ArmorCuirass,    LookupArmor(Skyrim.Armor.ArmorDaedricCuirass) },
                        { Skyrim.Keyword.ArmorGauntlets,  LookupArmor(Skyrim.Armor.ArmorDaedricGauntlets) },
                        { Skyrim.Keyword.ArmorHelmet,     LookupArmor(Skyrim.Armor.ArmorDaedricHelmet) },
                        { Skyrim.Keyword.ArmorShield,     LookupArmor(Skyrim.Armor.ArmorDaedricShield) }
                    },

                    [Skyrim.Keyword.ArmorMaterialDragonplate] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Skyrim.Keyword.ArmorBoots,      LookupArmor(Skyrim.Armor.ArmorDragonplateBoots) },
                        { Skyrim.Keyword.ArmorCuirass,    LookupArmor(Skyrim.Armor.ArmorDragonplateCuirass) },
                        { Skyrim.Keyword.ArmorGauntlets,  LookupArmor(Skyrim.Armor.ArmorDragonplateGauntlets) },
                        { Skyrim.Keyword.ArmorHelmet,     LookupArmor(Skyrim.Armor.ArmorDragonplateHelmet) },
                        { Skyrim.Keyword.ArmorShield,     LookupArmor(Skyrim.Armor.ArmorDragonplateShield) }
                    },

                    [Skyrim.Keyword.ArmorMaterialDwarven] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Skyrim.Keyword.ArmorBoots,      LookupArmor(Skyrim.Armor.ArmorDwarvenBoots) },
                        { Skyrim.Keyword.ArmorCuirass,    LookupArmor(Skyrim.Armor.ArmorDwarvenCuirass) },
                        { Skyrim.Keyword.ArmorGauntlets,  LookupArmor(Skyrim.Armor.ArmorDwarvenGauntlets) },
                        { Skyrim.Keyword.ArmorHelmet,     LookupArmor(Skyrim.Armor.ArmorDwarvenHelmet) },
                        { Skyrim.Keyword.ArmorShield,     LookupArmor(Skyrim.Armor.ArmorDwarvenShield) }
                    },

                    [Skyrim.Keyword.ArmorMaterialEbony] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Skyrim.Keyword.ArmorBoots,      LookupArmor(Skyrim.Armor.ArmorEbonyBoots) },
                        { Skyrim.Keyword.ArmorCuirass,    LookupArmor(Skyrim.Armor.ArmorEbonyCuirass) },
                        { Skyrim.Keyword.ArmorGauntlets,  LookupArmor(Skyrim.Armor.ArmorEbonyGauntlets) },
                        { Skyrim.Keyword.ArmorHelmet,     LookupArmor(Skyrim.Armor.ArmorEbonyHelmet) },
                        { Skyrim.Keyword.ArmorShield,     LookupArmor(Skyrim.Armor.ArmorEbonyShield) }
                    },

                    [Update.Keyword.ArmorMaterialFalmer] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Skyrim.Keyword.ArmorBoots,      LookupArmor(Skyrim.Armor.ArmorFalmerBoots) },
                        { Skyrim.Keyword.ArmorCuirass,    LookupArmor(Skyrim.Armor.ArmorFalmerCuirass) },
                        { Skyrim.Keyword.ArmorGauntlets,  LookupArmor(Skyrim.Armor.ArmorFalmerGauntlets) },
                        { Skyrim.Keyword.ArmorHelmet,     LookupArmor(Skyrim.Armor.ArmorFAlmerHelmet) },
                        { Skyrim.Keyword.ArmorShield,     LookupArmor(Skyrim.Armor.ArmorFalmerShield) }
                    },

                    [Skyrim.Keyword.ArmorMaterialImperialHeavy] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Skyrim.Keyword.ArmorBoots,      LookupArmor(Skyrim.Armor.ArmorImperialBoots) },
                        { Skyrim.Keyword.ArmorCuirass,    LookupArmor(Skyrim.Armor.ArmorImperialCuirass) },
                        { Skyrim.Keyword.ArmorGauntlets,  LookupArmor(Skyrim.Armor.ArmorImperialGauntlets) },
                        { Skyrim.Keyword.ArmorHelmet,     LookupArmor(Skyrim.Armor.ArmorImperialHelmet) },
                        { Skyrim.Keyword.ArmorShield,     LookupArmor(Skyrim.Armor.ArmorImperialShield) }
                    },

                    [Skyrim.Keyword.ArmorMaterialIron] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Skyrim.Keyword.ArmorBoots,      LookupArmor(Skyrim.Armor.ArmorIronBoots) },
                        { Skyrim.Keyword.ArmorCuirass,    LookupArmor(Skyrim.Armor.ArmorIronCuirass) },
                        { Skyrim.Keyword.ArmorGauntlets,  LookupArmor(Skyrim.Armor.ArmorIronGauntlets) },
                        { Skyrim.Keyword.ArmorHelmet,     LookupArmor(Skyrim.Armor.ArmorIronHelmet) },
                        { Skyrim.Keyword.ArmorShield,     LookupArmor(Skyrim.Armor.ArmorIronShield) }
                    },

                    [Skyrim.Keyword.ArmorMaterialIronBanded] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Skyrim.Keyword.ArmorBoots,      null                                                 },
                        { Skyrim.Keyword.ArmorCuirass,    LookupArmor(Skyrim.Armor.ArmorIronBandedCuirass) },
                        { Skyrim.Keyword.ArmorGauntlets,  null                                                 },
                        { Skyrim.Keyword.ArmorHelmet,     null                                                 },
                        { Skyrim.Keyword.ArmorShield,     LookupArmor(Skyrim.Armor.ArmorIronBandedShield) }
                    },

                    [Skyrim.Keyword.ArmorMaterialOrcish] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Skyrim.Keyword.ArmorBoots,      LookupArmor(Skyrim.Armor.ArmorOrcishBoots) },
                        { Skyrim.Keyword.ArmorCuirass,    LookupArmor(Skyrim.Armor.ArmorOrcishCuirass) },
                        { Skyrim.Keyword.ArmorGauntlets,  LookupArmor(Skyrim.Armor.ArmorOrcishGauntlets) },
                        { Skyrim.Keyword.ArmorHelmet,     LookupArmor(Skyrim.Armor.ArmorOrcishHelmet) },
                        { Skyrim.Keyword.ArmorShield,     LookupArmor(Skyrim.Armor.ArmorOrcishShield) }
                    },

                    [Skyrim.Keyword.ArmorMaterialSteel] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Skyrim.Keyword.ArmorBoots,      LookupArmor(Skyrim.Armor.ArmorSteelBootsA) },
                        { Skyrim.Keyword.ArmorCuirass,    LookupArmor(Skyrim.Armor.ArmorSteelCuirassA) },
                        { Skyrim.Keyword.ArmorGauntlets,  LookupArmor(Skyrim.Armor.ArmorSteelGauntletsA) },
                        { Skyrim.Keyword.ArmorHelmet,     LookupArmor(Skyrim.Armor.ArmorSteelHelmetA) },
                        { Skyrim.Keyword.ArmorShield,     LookupArmor(Skyrim.Armor.ArmorSteelShield) }
                    },

                    [Skyrim.Keyword.ArmorMaterialSteelPlate] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Skyrim.Keyword.ArmorBoots,      LookupArmor(Skyrim.Armor.ArmorSteelPlateBoots) },
                        { Skyrim.Keyword.ArmorCuirass,    LookupArmor(Skyrim.Armor.ArmorSteelPlateCuirass) },
                        { Skyrim.Keyword.ArmorGauntlets,  LookupArmor(Skyrim.Armor.ArmorSteelPlateGauntlets) },
                        { Skyrim.Keyword.ArmorHelmet,     LookupArmor(Skyrim.Armor.ArmorSteelPlateHelmet) },
                        { Skyrim.Keyword.ArmorShield,     null                                                 }
                    },

                    [Dawnguard.Keyword.DLC1ArmorMaterialDawnguard] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Skyrim.Keyword.ArmorBoots,      LookupArmor(Dawnguard.Armor.DLC1ArmorDawnguardBootsHeavy) },
                        { Skyrim.Keyword.ArmorCuirass,    LookupArmor(Dawnguard.Armor.DLC1ArmorDawnguardCuirassHeavy1) },
                        { Skyrim.Keyword.ArmorGauntlets,  LookupArmor(Dawnguard.Armor.DLC1ArmorDawnguardGauntletsHeavy) },
                        { Skyrim.Keyword.ArmorHelmet,     LookupArmor(Dawnguard.Armor.DLC1ArmorDawnguardHelmetHeavy) },
                        { Skyrim.Keyword.ArmorShield,     LookupArmor(Dawnguard.Armor.DLC1ArmorDawnguardShield) }
                    },

                    [Dawnguard.Keyword.DLC1ArmorMaterialFalmerHardened] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Skyrim.Keyword.ArmorBoots,      LookupArmor(Dawnguard.Armor.DLC1ArmorFalmerHardenedBoots) },
                        { Skyrim.Keyword.ArmorCuirass,    LookupArmor(Dawnguard.Armor.DLC1ArmorFalmerHardenedCuirass) },
                        { Skyrim.Keyword.ArmorGauntlets,  LookupArmor(Dawnguard.Armor.DLC1ArmorFalmerHardenedGauntlets) },
                        { Skyrim.Keyword.ArmorHelmet,     LookupArmor(Dawnguard.Armor.DLC1ArmorFAlmerHardenedHelmet) },
                        { Skyrim.Keyword.ArmorShield,     null }
                    },

                    [Dawnguard.Keyword.DLC1ArmorMaterialHunter] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Skyrim.Keyword.ArmorBoots,      LookupArmor(Dawnguard.Armor.DLC1ArmorHunterBoots) },
                        { Skyrim.Keyword.ArmorCuirass,    LookupArmor(Dawnguard.Armor.DLC1ArmorHunterCuirass) },
                        { Skyrim.Keyword.ArmorGauntlets,  LookupArmor(Dawnguard.Armor.DLC1ArmorHunterGauntlets) },
                        { Skyrim.Keyword.ArmorHelmet,     null },
                        { Skyrim.Keyword.ArmorShield,     null }
                    },

                    [Dawnguard.Keyword.DLC1ArmorMaterielFalmerHeavy] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Skyrim.Keyword.ArmorBoots,      LookupArmor(Dawnguard.Armor.DLC1ArmorFalmerHeavyBoots) },
                        { Skyrim.Keyword.ArmorCuirass,    LookupArmor(Dawnguard.Armor.DLC1ArmorFalmerHeavyCuirass) },
                        { Skyrim.Keyword.ArmorGauntlets,  LookupArmor(Dawnguard.Armor.DLC1ArmorFalmerHeavyGauntlets) },
                        { Skyrim.Keyword.ArmorHelmet,     LookupArmor(Dawnguard.Armor.DLC1ArmorFAlmerHeavyHelmet) },
                        { Skyrim.Keyword.ArmorShield,     null                                                 }
                    },

                    [Dragonborn.Keyword.DLC2ArmorMaterialBonemoldHeavy] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Skyrim.Keyword.ArmorBoots,      LookupArmor(Dragonborn.Armor.DLC2ArmorBonemoldBoots) },
                        { Skyrim.Keyword.ArmorCuirass,    LookupArmor(Dragonborn.Armor.DLC2ArmorBonemoldCuirassVariant02) },
                        { Skyrim.Keyword.ArmorGauntlets,  LookupArmor(Dragonborn.Armor.DLC2ArmorBonemoldGauntlets) },
                        { Skyrim.Keyword.ArmorHelmet,     LookupArmor(Dragonborn.Armor.DLC2ArmorBonemoldHelmet) },
                        { Skyrim.Keyword.ArmorShield,     LookupArmor(Dragonborn.Armor.DLC2ArmorBonemoldShield) }
                    },

                    [Dragonborn.Keyword.DLC2ArmorMaterialChitinHeavy] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Skyrim.Keyword.ArmorBoots,      LookupArmor(Dragonborn.Armor.DLC2ArmorChitinHeavyBoots) },
                        { Skyrim.Keyword.ArmorCuirass,    LookupArmor(Dragonborn.Armor.DLC2ArmorChitinHeavyCuirass) },
                        { Skyrim.Keyword.ArmorGauntlets,  LookupArmor(Dragonborn.Armor.DLC2ArmorChitinHeavyGauntlets) },
                        { Skyrim.Keyword.ArmorHelmet,     LookupArmor(Dragonborn.Armor.DLC2ArmorChitinHeavyHelmet) },
                        { Skyrim.Keyword.ArmorShield,     null                                                 }
                    },

                    [Dragonborn.Keyword.DLC2ArmorMaterialNordicHeavy] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Skyrim.Keyword.ArmorBoots,      LookupArmor(Dragonborn.Armor.DLC2ArmorNordicHeavyBoots) },
                        { Skyrim.Keyword.ArmorCuirass,    LookupArmor(Dragonborn.Armor.DLC2ArmorNordicHeavyCuirass) },
                        { Skyrim.Keyword.ArmorGauntlets,  LookupArmor(Dragonborn.Armor.DLC2ArmorNordicHeavyGauntlets) },
                        { Skyrim.Keyword.ArmorHelmet,     LookupArmor(Dragonborn.Armor.DLC2ArmorNordicHeavyHelmet) },
                        { Skyrim.Keyword.ArmorShield,     LookupArmor(Dragonborn.Armor.DLC2ArmorNordicShield) }
                    },

                    [Dragonborn.Keyword.DLC2ArmorMaterialStalhrimHeavy] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Skyrim.Keyword.ArmorBoots,      LookupArmor(Dragonborn.Armor.DLC2ArmorStalhrimHeavyBoots) },
                        { Skyrim.Keyword.ArmorCuirass,    LookupArmor(Dragonborn.Armor.DLC2ArmorStalhrimHeavyCuirass) },
                        { Skyrim.Keyword.ArmorGauntlets,  LookupArmor(Dragonborn.Armor.DLC2ArmorStalhrimHeavyGauntlets) },
                        { Skyrim.Keyword.ArmorHelmet,     LookupArmor(Dragonborn.Armor.DLC2ArmorStalhrimHeavyHelmet) },
                        { Skyrim.Keyword.ArmorShield,     null                                                 }
                    }
                },

                [Skyrim.Keyword.ArmorLight] = new Dictionary<FormKey, Dictionary<FormKey, IArmorGetter?>>()
                {
                    [Update.Keyword.ArmorMaterialBearStormcloak] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Skyrim.Keyword.ArmorBoots,      LookupArmor(Skyrim.Armor.ArmorStormcloakBearBoots) },
                        { Skyrim.Keyword.ArmorCuirass,    LookupArmor(Skyrim.Armor.ArmorStormcloakBearCuirass) },
                        { Skyrim.Keyword.ArmorGauntlets,  LookupArmor(Skyrim.Armor.ArmorStormcloakBearGauntlets) },
                        { Skyrim.Keyword.ArmorHelmet,     LookupArmor(Skyrim.Armor.ArmorStormcloakBearHelmet) },
                        { Skyrim.Keyword.ArmorShield,     null                                                 }
                    },

                    [Skyrim.Keyword.ArmorMaterialDragonscale] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Skyrim.Keyword.ArmorBoots,      LookupArmor(Skyrim.Armor.ArmorDragonscaleBoots) },
                        { Skyrim.Keyword.ArmorCuirass,    LookupArmor(Skyrim.Armor.ArmorDragonscaleCuirass) },
                        { Skyrim.Keyword.ArmorGauntlets,  LookupArmor(Skyrim.Armor.ArmorDragonscaleGauntlets) },
                        { Skyrim.Keyword.ArmorHelmet,     LookupArmor(Skyrim.Armor.ArmorDragonscaleHelmet) },
                        { Skyrim.Keyword.ArmorShield,     LookupArmor(Skyrim.Armor.ArmorDragonscaleShield) }
                    },

                    [Skyrim.Keyword.ArmorMaterialElven] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Skyrim.Keyword.ArmorBoots,      LookupArmor(Skyrim.Armor.ArmorElvenBoots) },
                        { Skyrim.Keyword.ArmorCuirass,    LookupArmor(Skyrim.Armor.ArmorElvenCuirass) },
                        { Skyrim.Keyword.ArmorGauntlets,  LookupArmor(Skyrim.Armor.ArmorElvenGauntlets) },
                        { Skyrim.Keyword.ArmorHelmet,     LookupArmor(Skyrim.Armor.ArmorElvenHelmet) },
                        { Skyrim.Keyword.ArmorShield,     LookupArmor(Skyrim.Armor.ArmorElvenShield) }
                    },

                    [Skyrim.Keyword.ArmorMaterialElvenGilded] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Skyrim.Keyword.ArmorBoots,      null },
                        { Skyrim.Keyword.ArmorCuirass,    LookupArmor(Skyrim.Armor.ArmorElvenGildedCuirass) },
                        { Skyrim.Keyword.ArmorGauntlets,  null },
                        { Skyrim.Keyword.ArmorHelmet,     null },
                        { Skyrim.Keyword.ArmorShield,     null }
                    },

                    [Update.Keyword.ArmorMaterialForsworn] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Skyrim.Keyword.ArmorBoots,      LookupArmor(Skyrim.Armor.ForswornBoots) },
                        { Skyrim.Keyword.ArmorCuirass,    LookupArmor(Skyrim.Armor.ForswornCuirass) },
                        { Skyrim.Keyword.ArmorGauntlets,  LookupArmor(Skyrim.Armor.ForswornGauntlets) },
                        { Skyrim.Keyword.ArmorHelmet,     LookupArmor(Skyrim.Armor.ForswornHelmet) },
                        { Skyrim.Keyword.ArmorShield,     null                                                 }
                    },

                    [Update.Keyword.ArmorMaterialMS02Forsworn] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Skyrim.Keyword.ArmorBoots,      LookupArmor(Skyrim.Armor.MS02ForswornBoots) },
                        { Skyrim.Keyword.ArmorCuirass,    LookupArmor(Skyrim.Armor.MS02ForswornArmor) },
                        { Skyrim.Keyword.ArmorGauntlets,  LookupArmor(Skyrim.Armor.MS02ForswornGauntlets) },
                        { Skyrim.Keyword.ArmorHelmet,     LookupArmor(Skyrim.Armor.MS02ForswornHelmet) },
                        { Skyrim.Keyword.ArmorShield,     null                                                 }
                    },

                    [Skyrim.Keyword.ArmorMaterialGlass] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Skyrim.Keyword.ArmorBoots,      LookupArmor(Skyrim.Armor.ArmorGlassBoots) },
                        { Skyrim.Keyword.ArmorCuirass,    LookupArmor(Skyrim.Armor.ArmorGlassCuirass) },
                        { Skyrim.Keyword.ArmorGauntlets,  LookupArmor(Skyrim.Armor.ArmorGlassGauntlets) },
                        { Skyrim.Keyword.ArmorHelmet,     LookupArmor(Skyrim.Armor.ArmorGlassHelmet) },
                        { Skyrim.Keyword.ArmorShield,     LookupArmor(Skyrim.Armor.ArmorGlassShield) }
                    },

                    [Skyrim.Keyword.ArmorMaterialHide] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Skyrim.Keyword.ArmorBoots,      LookupArmor(Skyrim.Armor.ArmorHideBoots) },
                        { Skyrim.Keyword.ArmorCuirass,    LookupArmor(Skyrim.Armor.ArmorHideCuirass) },
                        { Skyrim.Keyword.ArmorGauntlets,  LookupArmor(Skyrim.Armor.ArmorHideGauntlets) },
                        { Skyrim.Keyword.ArmorHelmet,     LookupArmor(Skyrim.Armor.ArmorHideHelmet) },
                        { Skyrim.Keyword.ArmorShield,     LookupArmor(Skyrim.Armor.ArmorHideShield) }
                    },

                    [Skyrim.Keyword.ArmorMaterialImperialLight] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Skyrim.Keyword.ArmorBoots,      LookupArmor(Skyrim.Armor.ArmorImperialLightBoots) },
                        { Skyrim.Keyword.ArmorCuirass,    LookupArmor(Skyrim.Armor.ArmorImperialLightCuirass) },
                        { Skyrim.Keyword.ArmorGauntlets,  LookupArmor(Skyrim.Armor.ArmorImperialLightGauntlets) },
                        { Skyrim.Keyword.ArmorHelmet,     LookupArmor(Skyrim.Armor.ArmorImperialLightHelmet) },
                        { Skyrim.Keyword.ArmorShield,     LookupArmor(Skyrim.Armor.ArmorImperialLightShield) }
                    },

                    [Skyrim.Keyword.ArmorMaterialImperialStudded] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Skyrim.Keyword.ArmorBoots,      null },
                        { Skyrim.Keyword.ArmorCuirass,    LookupArmor(Skyrim.Armor.ArmorImperialStuddedCuirass) },
                        { Skyrim.Keyword.ArmorGauntlets,  null },
                        { Skyrim.Keyword.ArmorHelmet,     LookupArmor(Skyrim.Armor.ArmorImperialHelmet) },
                        { Skyrim.Keyword.ArmorShield,     null }
                    },

                    [Skyrim.Keyword.ArmorMaterialLeather] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Skyrim.Keyword.ArmorBoots,      LookupArmor(Skyrim.Armor.ArmorLeatherBoots) },
                        { Skyrim.Keyword.ArmorCuirass,    LookupArmor(Skyrim.Armor.ArmorLeatherCuirass) },
                        { Skyrim.Keyword.ArmorGauntlets,  LookupArmor(Skyrim.Armor.ArmorLeatherGauntlets) },
                        { Skyrim.Keyword.ArmorHelmet,     LookupArmor(Skyrim.Armor.ArmorLeatherHelmet) },
                        { Skyrim.Keyword.ArmorShield,     null }
                    },

                    [Update.Keyword.ArmorMaterialPenitus] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Skyrim.Keyword.ArmorBoots,      LookupArmor(Skyrim.Armor.ArmorPenitusBoots) },
                        { Skyrim.Keyword.ArmorCuirass,    LookupArmor(Skyrim.Armor.ArmorPenitusCuirass) },
                        { Skyrim.Keyword.ArmorGauntlets,  LookupArmor(Skyrim.Armor.ArmorPenitusGauntlets) },
                        { Skyrim.Keyword.ArmorHelmet,     LookupArmor(Skyrim.Armor.ArmorPenitusHelmet) },
                        { Skyrim.Keyword.ArmorShield,     null }
                    },

                    [Skyrim.Keyword.ArmorMaterialScaled] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Skyrim.Keyword.ArmorBoots,      LookupArmor(Skyrim.Armor.ArmorScaledBoots) },
                        { Skyrim.Keyword.ArmorCuirass,    LookupArmor(Skyrim.Armor.ArmorScaledCuirass) },
                        { Skyrim.Keyword.ArmorGauntlets,  LookupArmor(Skyrim.Armor.ArmorScaledGauntlets) },
                        { Skyrim.Keyword.ArmorHelmet,     LookupArmor(Skyrim.Armor.ArmorScaledHelmet) },
                        { Skyrim.Keyword.ArmorShield,     null }
                    },

                    [Skyrim.Keyword.ArmorMaterialStormcloak] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Skyrim.Keyword.ArmorBoots,      LookupArmor(Skyrim.Armor.ArmorStormcloakBoots) },
                        { Skyrim.Keyword.ArmorCuirass,    LookupArmor(Skyrim.Armor.ArmorStormcloakCuirass) },
                        { Skyrim.Keyword.ArmorGauntlets,  LookupArmor(Skyrim.Armor.ArmorStormcloakGauntlets) },
                        { Skyrim.Keyword.ArmorHelmet,     LookupArmor(Skyrim.Armor.ArmorStormcloakHelmetFull) },
                        { Skyrim.Keyword.ArmorShield,     null }
                    },

                    [Skyrim.Keyword.ArmorMaterialStudded] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Skyrim.Keyword.ArmorBoots,      null },
                        { Skyrim.Keyword.ArmorCuirass,    LookupArmor(Skyrim.Armor.ArmorStuddedCuirass) },
                        { Skyrim.Keyword.ArmorGauntlets,  null },
                        { Skyrim.Keyword.ArmorHelmet,     null },
                        { Skyrim.Keyword.ArmorShield,     null }
                    },

                    [Update.Keyword.ArmorMaterialThievesGuild] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Skyrim.Keyword.ArmorBoots,      LookupArmor(Skyrim.Armor.ArmorThievesGuildBootsPlayer) },
                        { Skyrim.Keyword.ArmorCuirass,    LookupArmor(Skyrim.Armor.ArmorThievesGuildCuirassPlayer) },
                        { Skyrim.Keyword.ArmorGauntlets,  LookupArmor(Skyrim.Armor.ArmorThievesGuildGauntletsPlayer) },
                        { Skyrim.Keyword.ArmorHelmet,     LookupArmor(Skyrim.Armor.ArmorThievesGuildHelmetPlayer) },
                        { Skyrim.Keyword.ArmorShield,     null }
                    },

                    [Update.Keyword.ArmorMaterialThievesGuildLeader] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Skyrim.Keyword.ArmorBoots,      LookupArmor(Skyrim.Armor.ArmorThievesGuildLeaderBoots) },
                        { Skyrim.Keyword.ArmorCuirass,    LookupArmor(Skyrim.Armor.ArmorThievesGuildLeaderCuirass) },
                        { Skyrim.Keyword.ArmorGauntlets,  LookupArmor(Skyrim.Armor.ArmorThievesGuildLeaderGauntlets) },
                        { Skyrim.Keyword.ArmorHelmet,     LookupArmor(Skyrim.Armor.ArmorThievesGuildLeaderHelmet) },
                        { Skyrim.Keyword.ArmorShield,     null }
                    },

                    [Skyrim.Keyword.ArmorNightingale] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Skyrim.Keyword.ArmorBoots,      LookupArmor(Skyrim.Armor.ArmorNightingaleBoots) },
                        { Skyrim.Keyword.ArmorCuirass,    LookupArmor(Skyrim.Armor.ArmorNightingaleCuirass) },
                        { Skyrim.Keyword.ArmorGauntlets,  LookupArmor(Skyrim.Armor.ArmorNightingaleGauntlets) },
                        { Skyrim.Keyword.ArmorHelmet,     LookupArmor(Skyrim.Armor.ArmorNightingaleHelmet) },
                        { Skyrim.Keyword.ArmorShield,     null }
                    },

                    [Dawnguard.Keyword.DLC1ArmorMaterialDawnguard] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Skyrim.Keyword.ArmorBoots,      LookupArmor(Dawnguard.Armor.DLC1ArmorDawnguardBootsLight) },
                        { Skyrim.Keyword.ArmorCuirass,    LookupArmor(Dawnguard.Armor.DLC1ArmorDawnguardCuirassLight1) },
                        { Skyrim.Keyword.ArmorGauntlets,  LookupArmor(Dawnguard.Armor.DLC1ArmorDawnguardGauntletsLight) },
                        { Skyrim.Keyword.ArmorHelmet,     LookupArmor(Dawnguard.Armor.DLC1ArmorDawnguardHelmetLight) },
                        { Skyrim.Keyword.ArmorShield,     LookupArmor(Dawnguard.Armor.DLC1DawnguardRuneShield) }
                    },

                    [Dawnguard.Keyword.DLC1ArmorMaterialVampire] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Skyrim.Keyword.ArmorBoots,      LookupArmor(Dawnguard.Armor.DLC1ArmorVampireBoots) },
                        { Skyrim.Keyword.ArmorCuirass,    LookupArmor(Dawnguard.Armor.DLC1ArmorVampireArmorGray)  },
                        { Skyrim.Keyword.ArmorGauntlets,  LookupArmor(Dawnguard.Armor.DLC1ArmorVampireGauntlets) },
                        { Skyrim.Keyword.ArmorHelmet,     null },
                        { Skyrim.Keyword.ArmorShield,     null }
                    },

                    // Yes... this is really light armor.  WTF Bethesda 
                    [Dawnguard.Keyword.DLC1ArmorMaterielFalmerHeavyOriginal] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Skyrim.Keyword.ArmorBoots,      LookupArmor(Skyrim.Armor.ArmorFalmerBoots) },
                        { Skyrim.Keyword.ArmorCuirass,    LookupArmor(Skyrim.Armor.ArmorFalmerCuirass) },
                        { Skyrim.Keyword.ArmorGauntlets,  LookupArmor(Skyrim.Armor.ArmorFalmerGauntlets) },
                        { Skyrim.Keyword.ArmorHelmet,     LookupArmor(Skyrim.Armor.ArmorFAlmerHelmet) },
                        { Skyrim.Keyword.ArmorShield,     null                                                 }
                    },

                    [Dragonborn.Keyword.DLC2ArmorMaterialBonemoldLight] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Skyrim.Keyword.ArmorBoots,      null                                                 },
                        { Skyrim.Keyword.ArmorCuirass,    null                                                 },
                        { Skyrim.Keyword.ArmorGauntlets,  null                                                 },
                        { Skyrim.Keyword.ArmorHelmet,     null                                                 },
                        { Skyrim.Keyword.ArmorShield,     null                                                 }
                    },

                    [Dragonborn.Keyword.DLC2ArmorMaterialChitinLight] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Skyrim.Keyword.ArmorBoots,      LookupArmor(Dragonborn.Armor.DLC2ArmorChitinLightBoots) },
                        { Skyrim.Keyword.ArmorCuirass,    LookupArmor(Dragonborn.Armor.DLC2ArmorChitinLightCuirass) },
                        { Skyrim.Keyword.ArmorGauntlets,  LookupArmor(Dragonborn.Armor.DLC2ArmorChitinLightGauntlets) },
                        { Skyrim.Keyword.ArmorHelmet,     LookupArmor(Dragonborn.Armor.DLC2ArmorChitinLightHelmet) },
                        { Skyrim.Keyword.ArmorShield,     LookupArmor(Dragonborn.Armor.DLC2ArmorChitinShield) }
                    },

                    [Dragonborn.Keyword.DLC2ArmorMaterialMoragTong] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Skyrim.Keyword.ArmorBoots,      LookupArmor(Dragonborn.Armor.DLC2MoragTongBoots) },
                        { Skyrim.Keyword.ArmorCuirass,    LookupArmor(Dragonborn.Armor.DLC2MoragTongCuirass) },
                        { Skyrim.Keyword.ArmorGauntlets,  LookupArmor(Dragonborn.Armor.DLC2MoragTongGauntlets) },
                        { Skyrim.Keyword.ArmorHelmet,     LookupArmor(Dragonborn.Armor.DLC2MoragTongHood) },
                        { Skyrim.Keyword.ArmorShield,     null }
                    },

                    [Dragonborn.Keyword.DLC2ArmorMaterialStalhrimLight] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Skyrim.Keyword.ArmorBoots,      LookupArmor(Dragonborn.Armor.DLC2ArmorStalhrimLightBoots) },
                        { Skyrim.Keyword.ArmorCuirass,    LookupArmor(Dragonborn.Armor.DLC2ArmorStalhrimLightCuirass) },
                        { Skyrim.Keyword.ArmorGauntlets,  LookupArmor(Dragonborn.Armor.DLC2ArmorStalhrimLightGauntlets) },
                        { Skyrim.Keyword.ArmorHelmet,     LookupArmor(Dragonborn.Armor.DLC2ArmorStalhrimLightHelmet) },
                        { Skyrim.Keyword.ArmorShield,     LookupArmor(Dragonborn.Armor.DLC2ArmorStalhrimShield) }
                    }
                }
            };

            BaseWeapons = new Dictionary<FormKey, Dictionary<FormKey, IWeaponGetter?>>()
            {
                [Dawnguard.Keyword.DLC1WeapMaterialDragonbone] = new Dictionary<FormKey, IWeaponGetter?>()
                {
                    { Skyrim.Keyword.WeapTypeBattleaxe,  LookupWeapon(Dawnguard.Weapon.DLC1DragonboneBattleaxe) },
                    { Skyrim.Keyword.WeapTypeDagger,     LookupWeapon(Dawnguard.Weapon.DLC1DragonboneDagger) },
                    { Skyrim.Keyword.WeapTypeGreatsword, LookupWeapon(Dawnguard.Weapon.DLC1DragonboneGreatsword) },
                    { Skyrim.Keyword.WeapTypeMace,       LookupWeapon(Dawnguard.Weapon.DLC1DragonboneMace) },
                    { Skyrim.Keyword.WeapTypeSword,      LookupWeapon(Dawnguard.Weapon.DLC1DragonboneSword) },
                    { Skyrim.Keyword.WeapTypeWarAxe,     LookupWeapon(Dawnguard.Weapon.DLC1DragonboneWarAxe) },
                    { Skyrim.Keyword.WeapTypeWarhammer,  LookupWeapon(Dawnguard.Weapon.DLC1DragonboneWarhammer) }
                },

                [Dragonborn.Keyword.DLC2WeaponMaterialNordic] = new Dictionary<FormKey, IWeaponGetter?>()
                {
                    { Skyrim.Keyword.WeapTypeBattleaxe,  LookupWeapon(Dragonborn.Weapon.DLC2NordicBattleaxe) },
                    { Skyrim.Keyword.WeapTypeDagger,     LookupWeapon(Dragonborn.Weapon.DLC2NordicDagger) },
                    { Skyrim.Keyword.WeapTypeGreatsword, LookupWeapon(Dragonborn.Weapon.DLC2NordicGreatsword) },
                    { Skyrim.Keyword.WeapTypeMace,       LookupWeapon(Dragonborn.Weapon.DLC2NordicMace) },
                    { Skyrim.Keyword.WeapTypeSword,      LookupWeapon(Dragonborn.Weapon.DLC2NordicSword) },
                    { Skyrim.Keyword.WeapTypeWarAxe,     LookupWeapon(Dragonborn.Weapon.DLC2NordicWarAxe) },
                    { Skyrim.Keyword.WeapTypeWarhammer,  LookupWeapon(Dragonborn.Weapon.DLC2NordicWarhammer) }
                },

                [Dragonborn.Keyword.DLC2WeaponMaterialStalhrim] = new Dictionary<FormKey, IWeaponGetter?>()
                {
                    { Skyrim.Keyword.WeapTypeBattleaxe,  LookupWeapon(Dragonborn.Weapon.DLC2StalhrimBattleaxe) },
                    { Skyrim.Keyword.WeapTypeDagger,     LookupWeapon(Dragonborn.Weapon.DLC2StalhrimDagger) },
                    { Skyrim.Keyword.WeapTypeGreatsword, LookupWeapon(Dragonborn.Weapon.DLC2StalhrimGreatsword) },
                    { Skyrim.Keyword.WeapTypeMace,       LookupWeapon(Dragonborn.Weapon.DLC2StalhrimMace) },
                    { Skyrim.Keyword.WeapTypeSword,      LookupWeapon(Dragonborn.Weapon.DLC2StalhrimSword) },
                    { Skyrim.Keyword.WeapTypeWarAxe,     LookupWeapon(Dragonborn.Weapon.DLC2StalhrimWarAxe) },
                    { Skyrim.Keyword.WeapTypeWarhammer,  LookupWeapon(Dragonborn.Weapon.DLC2StalhrimWarhammer) }
                },

                [Skyrim.Keyword.WeapMaterialDaedric] = new Dictionary<FormKey, IWeaponGetter?>()
                {
                    { Skyrim.Keyword.WeapTypeBattleaxe,  LookupWeapon(Skyrim.Weapon.DaedricBattleaxe) },
                    { Skyrim.Keyword.WeapTypeDagger,     LookupWeapon(Skyrim.Weapon.DaedricDagger) },
                    { Skyrim.Keyword.WeapTypeGreatsword, LookupWeapon(Skyrim.Weapon.DaedricGreatsword) },
                    { Skyrim.Keyword.WeapTypeMace,       LookupWeapon(Skyrim.Weapon.DaedricMace) },
                    { Skyrim.Keyword.WeapTypeSword,      LookupWeapon(Skyrim.Weapon.DaedricSword) },
                    { Skyrim.Keyword.WeapTypeWarAxe,     LookupWeapon(Skyrim.Weapon.DaedricWarAxe) },
                    { Skyrim.Keyword.WeapTypeWarhammer,  LookupWeapon(Skyrim.Weapon.DaedricWarhammer) }
                },

                [Skyrim.Keyword.WeapMaterialDraugr] = new Dictionary<FormKey, IWeaponGetter?>()
                {
                    { Skyrim.Keyword.WeapTypeBattleaxe,  LookupWeapon(Skyrim.Weapon.DraugrBattleAxe) },
                    { Skyrim.Keyword.WeapTypeDagger,     null },
                    { Skyrim.Keyword.WeapTypeGreatsword, LookupWeapon(Skyrim.Weapon.DraugrGreatsword) },
                    { Skyrim.Keyword.WeapTypeMace,       null },
                    { Skyrim.Keyword.WeapTypeSword,      LookupWeapon(Skyrim.Weapon.DraugrSword) },
                    { Skyrim.Keyword.WeapTypeWarAxe,     LookupWeapon(Skyrim.Weapon.DraugrWarAxe) },
                    { Skyrim.Keyword.WeapTypeWarhammer,  null }
                },

                [Skyrim.Keyword.WeapMaterialDraugrHoned] = new Dictionary<FormKey, IWeaponGetter?>()
                {
                    { Skyrim.Keyword.WeapTypeBattleaxe,  LookupWeapon(Skyrim.Weapon.DraugrBattleAxeHoned) },
                    { Skyrim.Keyword.WeapTypeDagger,     null },
                    { Skyrim.Keyword.WeapTypeGreatsword, LookupWeapon(Skyrim.Weapon.DraugrGreatswordHoned) },
                    { Skyrim.Keyword.WeapTypeMace,       null },
                    { Skyrim.Keyword.WeapTypeSword,      LookupWeapon(Skyrim.Weapon.DraugrSwordHoned) },
                    { Skyrim.Keyword.WeapTypeWarAxe,     LookupWeapon(Skyrim.Weapon.DraugrWarAxeHoned) },
                    { Skyrim.Keyword.WeapTypeWarhammer,  null }
                },

                [Skyrim.Keyword.WeapMaterialDwarven] = new Dictionary<FormKey, IWeaponGetter?>()
                {
                    { Skyrim.Keyword.WeapTypeBattleaxe,  LookupWeapon(Skyrim.Weapon.DwarvenBattleaxe) },
                    { Skyrim.Keyword.WeapTypeDagger,     LookupWeapon(Skyrim.Weapon.DwarvenDagger) },
                    { Skyrim.Keyword.WeapTypeGreatsword, LookupWeapon(Skyrim.Weapon.DwarvenGreatsword) },
                    { Skyrim.Keyword.WeapTypeMace,       LookupWeapon(Skyrim.Weapon.DwarvenMace) },
                    { Skyrim.Keyword.WeapTypeSword,      LookupWeapon(Skyrim.Weapon.DwarvenSword) },
                    { Skyrim.Keyword.WeapTypeWarAxe,     LookupWeapon(Skyrim.Weapon.DwarvenWarAxe) },
                    { Skyrim.Keyword.WeapTypeWarhammer,  LookupWeapon(Skyrim.Weapon.DwarvenWarhammer) }
                },

                [Skyrim.Keyword.WeapMaterialEbony] = new Dictionary<FormKey, IWeaponGetter?>()
                {
                    { Skyrim.Keyword.WeapTypeBattleaxe,  LookupWeapon(Skyrim.Weapon.EbonyBattleaxe) },
                    { Skyrim.Keyword.WeapTypeDagger,     LookupWeapon(Skyrim.Weapon.EbonyDagger) },
                    { Skyrim.Keyword.WeapTypeGreatsword, LookupWeapon(Skyrim.Weapon.EbonyGreatsword) },
                    { Skyrim.Keyword.WeapTypeMace,       LookupWeapon(Skyrim.Weapon.EbonyMace) },
                    { Skyrim.Keyword.WeapTypeSword,      LookupWeapon(Skyrim.Weapon.EbonySword) },
                    { Skyrim.Keyword.WeapTypeWarAxe,     LookupWeapon(Skyrim.Weapon.EbonyWarAxe) },
                    { Skyrim.Keyword.WeapTypeWarhammer,  LookupWeapon(Skyrim.Weapon.EbonyWarhammer) }
                },

                [Skyrim.Keyword.WeapMaterialElven] = new Dictionary<FormKey, IWeaponGetter?>()
                {
                    { Skyrim.Keyword.WeapTypeBattleaxe,  LookupWeapon(Skyrim.Weapon.ElvenBattleaxe) },
                    { Skyrim.Keyword.WeapTypeDagger,     LookupWeapon(Skyrim.Weapon.ElvenDagger) },
                    { Skyrim.Keyword.WeapTypeGreatsword, LookupWeapon(Skyrim.Weapon.ElvenGreatsword) },
                    { Skyrim.Keyword.WeapTypeMace,       LookupWeapon(Skyrim.Weapon.ElvenMace) },
                    { Skyrim.Keyword.WeapTypeSword,      LookupWeapon(Skyrim.Weapon.ElvenSword) },
                    { Skyrim.Keyword.WeapTypeWarAxe,     LookupWeapon(Skyrim.Weapon.ElvenWarAxe) },
                    { Skyrim.Keyword.WeapTypeWarhammer,  LookupWeapon(Skyrim.Weapon.ElvenWarhammer) }
                },

                [Skyrim.Keyword.WeapMaterialFalmer] = new Dictionary<FormKey, IWeaponGetter?>()
                {
                    { Skyrim.Keyword.WeapTypeBattleaxe,  null },
                    { Skyrim.Keyword.WeapTypeDagger,     null },
                    { Skyrim.Keyword.WeapTypeGreatsword, null },
                    { Skyrim.Keyword.WeapTypeMace,       null },
                    { Skyrim.Keyword.WeapTypeSword,      LookupWeapon(Skyrim.Weapon.FalmerSword) },
                    { Skyrim.Keyword.WeapTypeWarAxe,     LookupWeapon(Skyrim.Weapon.FalmerWarAxe) },
                    { Skyrim.Keyword.WeapTypeWarhammer,  null }
                },

                [Skyrim.Keyword.WeapMaterialFalmerHoned] = new Dictionary<FormKey, IWeaponGetter?>()
                {
                    { Skyrim.Keyword.WeapTypeBattleaxe,  null },
                    { Skyrim.Keyword.WeapTypeDagger,     null },
                    { Skyrim.Keyword.WeapTypeGreatsword, null },
                    { Skyrim.Keyword.WeapTypeMace,       null },
                    { Skyrim.Keyword.WeapTypeSword,      LookupWeapon(Skyrim.Weapon.FalmerSwordHoned) },
                    { Skyrim.Keyword.WeapTypeWarAxe,     LookupWeapon(Skyrim.Weapon.FalmerWarAxeHoned) },
                    { Skyrim.Keyword.WeapTypeWarhammer,  null }
                },

                [Skyrim.Keyword.WeapMaterialGlass] = new Dictionary<FormKey, IWeaponGetter?>()
                {
                    { Skyrim.Keyword.WeapTypeBattleaxe,  LookupWeapon(Skyrim.Weapon.GlassBattleaxe) },
                    { Skyrim.Keyword.WeapTypeDagger,     LookupWeapon(Skyrim.Weapon.GlassDagger) },
                    { Skyrim.Keyword.WeapTypeGreatsword, LookupWeapon(Skyrim.Weapon.GlassGreatsword) },
                    { Skyrim.Keyword.WeapTypeMace,       LookupWeapon(Skyrim.Weapon.GlassMace) },
                    { Skyrim.Keyword.WeapTypeSword,      LookupWeapon(Skyrim.Weapon.GlassSword) },
                    { Skyrim.Keyword.WeapTypeWarAxe,     LookupWeapon(Skyrim.Weapon.GlassWarAxe) },
                    { Skyrim.Keyword.WeapTypeWarhammer,  LookupWeapon(Skyrim.Weapon.GlassWarhammer) }
                },

                [Skyrim.Keyword.WeapMaterialImperial] = new Dictionary<FormKey, IWeaponGetter?>()
                {
                    { Skyrim.Keyword.WeapTypeBattleaxe,  null },
                    { Skyrim.Keyword.WeapTypeDagger,     null },
                    { Skyrim.Keyword.WeapTypeGreatsword, null },
                    { Skyrim.Keyword.WeapTypeMace,       null },
                    { Skyrim.Keyword.WeapTypeSword,      null },
                    { Skyrim.Keyword.WeapTypeWarAxe,     null },
                    { Skyrim.Keyword.WeapTypeWarhammer,  null }
                },

                [Skyrim.Keyword.WeapMaterialIron] = new Dictionary<FormKey, IWeaponGetter?>()
                {
                    { Skyrim.Keyword.WeapTypeBattleaxe,  LookupWeapon(Skyrim.Weapon.IronBattleaxe) },
                    { Skyrim.Keyword.WeapTypeDagger,     LookupWeapon(Skyrim.Weapon.IronDagger) },
                    { Skyrim.Keyword.WeapTypeGreatsword, LookupWeapon(Skyrim.Weapon.IronGreatsword) },
                    { Skyrim.Keyword.WeapTypeMace,       LookupWeapon(Skyrim.Weapon.IronMace) },
                    { Skyrim.Keyword.WeapTypeSword,      LookupWeapon(Skyrim.Weapon.IronSword) },
                    { Skyrim.Keyword.WeapTypeWarAxe,     LookupWeapon(Skyrim.Weapon.IronWarAxe) },
                    { Skyrim.Keyword.WeapTypeWarhammer,  LookupWeapon(Skyrim.Weapon.IronWarhammer) }
                },

                [Skyrim.Keyword.WeapMaterialOrcish] = new Dictionary<FormKey, IWeaponGetter?>()
                {
                    { Skyrim.Keyword.WeapTypeBattleaxe,  LookupWeapon(Skyrim.Weapon.OrcishBattleaxe) },
                    { Skyrim.Keyword.WeapTypeDagger,     LookupWeapon(Skyrim.Weapon.OrcishDagger) },
                    { Skyrim.Keyword.WeapTypeGreatsword, LookupWeapon(Skyrim.Weapon.OrcishGreatsword) },
                    { Skyrim.Keyword.WeapTypeMace,       LookupWeapon(Skyrim.Weapon.OrcishMace) },
                    { Skyrim.Keyword.WeapTypeSword,      LookupWeapon(Skyrim.Weapon.OrcishSword) },
                    { Skyrim.Keyword.WeapTypeWarAxe,     LookupWeapon(Skyrim.Weapon.OrcishWarAxe) },
                    { Skyrim.Keyword.WeapTypeWarhammer,  LookupWeapon(Skyrim.Weapon.OrcishWarhammer) }
                },

                [Skyrim.Keyword.WeapMaterialSilver] = new Dictionary<FormKey, IWeaponGetter?>()
                {
                    { Skyrim.Keyword.WeapTypeBattleaxe,  null },
                    { Skyrim.Keyword.WeapTypeDagger,     null },
                    { Skyrim.Keyword.WeapTypeGreatsword, LookupWeapon(Skyrim.Weapon.SilverGreatsword) },
                    { Skyrim.Keyword.WeapTypeMace,       null },
                    { Skyrim.Keyword.WeapTypeSword,      LookupWeapon(Skyrim.Weapon.SilverSword) },
                    { Skyrim.Keyword.WeapTypeWarAxe,     null },
                    { Skyrim.Keyword.WeapTypeWarhammer,  null }
                },

                [Skyrim.Keyword.WeapMaterialSteel] = new Dictionary<FormKey, IWeaponGetter?>()
                {
                    { Skyrim.Keyword.WeapTypeBattleaxe,  LookupWeapon(Skyrim.Weapon.SteelBattleaxe) },
                    { Skyrim.Keyword.WeapTypeDagger,     LookupWeapon(Skyrim.Weapon.SteelDagger) },
                    { Skyrim.Keyword.WeapTypeGreatsword, LookupWeapon(Skyrim.Weapon.SteelGreatsword) },
                    { Skyrim.Keyword.WeapTypeMace,       LookupWeapon(Skyrim.Weapon.SteelMace) },
                    { Skyrim.Keyword.WeapTypeSword,      LookupWeapon(Skyrim.Weapon.SteelSword) },
                    { Skyrim.Keyword.WeapTypeWarAxe,     LookupWeapon(Skyrim.Weapon.SteelWarAxe) },
                    { Skyrim.Keyword.WeapTypeWarhammer,  LookupWeapon(Skyrim.Weapon.SteelWarhammer) }
                },

                [Skyrim.Keyword.WeapMaterialWood] = new Dictionary<FormKey, IWeaponGetter?>()
                {
                    { Skyrim.Keyword.WeapTypeBattleaxe,  null },
                    { Skyrim.Keyword.WeapTypeDagger,     null },
                    { Skyrim.Keyword.WeapTypeGreatsword, null },
                    { Skyrim.Keyword.WeapTypeMace,       null },
                    { Skyrim.Keyword.WeapTypeSword,      null },
                    { Skyrim.Keyword.WeapTypeWarAxe,     null },
                    { Skyrim.Keyword.WeapTypeWarhammer,  null }
                }
            };
        }

        private void ParseSlot(JToken slot, FormKey key, string type)
        {
            var recipeTemplate = new RecipeTemplate()
            {
                Bench = Keywords[slot["bench"]!.ToString()],
            };

            if (slot["perk"]!.ToString() != "" && slot["perk"]!.ToString() != "null")
            {
                var a = slot["perk"];
                recipeTemplate.Perk = Perks[a!.ToString()];
            }

            foreach (var item in slot["items"].ToArray())
            {
                if (item == null) throw new Exception("More mistakes were made");

                recipeTemplate.Items.Add(new Item()
                {
                    Record = CraftingSupplies[item[0]!.ToString()],
                    Count = Convert.ToInt32(item[1]!.ToString(), 16)
                });
            }

            RecipeTemplates[key][type][Keywords[slot["slot"]!.ToString()]] = recipeTemplate;
        }

        private void CacheRecipe(IConstructibleObjectGetter cobj, IMajorRecordCommonGetter record, string type)
        {
            if (IsCraftingRecipe(cobj, record))
            {
                Recipes[type]["creation"][record.FormKey] = cobj;
            }
            else if (IsTemperingRecipe(cobj, record))
            {
                Recipes[type]["tempering"][record.FormKey] = cobj;
            }
            else if (IsBreakdownRecipe(cobj, record))
            {
                Recipes[type]["breakdown"][record.FormKey] = cobj;
            }
        }

        private bool IsBreakdownRecipe(IConstructibleObjectGetter cobj, IMajorRecordCommonGetter record)
        {
            if (cobj.WorkbenchKeyword.FormKey != Skyrim.Keyword.CraftingSmelter && cobj.WorkbenchKeyword != Skyrim.Keyword.CraftingTanningRack) return false;

            if (cobj.Items == null) return false;

            if (cobj.Items.Count != 1) return false;

            if (cobj.Items[0].Item.Item.FormKey != record.FormKey) return false;

            if (cobj.CreatedObject.FormKey == null) return false;

            return CraftingSupplies.ContainsValue((FormKey)cobj.CreatedObject.FormKey);
        }

        private bool IsCraftingRecipe(IConstructibleObjectGetter cobj, IMajorRecordCommonGetter record)
        {
            if (cobj.WorkbenchKeyword.FormKey != Skyrim.Keyword.CraftingSmithingForge && cobj.WorkbenchKeyword.FormKey != Skyrim.Keyword.CraftingSmithingSkyforge) return false;

            if (cobj.CreatedObject.FormKey == null) return false;

            return cobj.CreatedObject.FormKey == record.FormKey;
        }

        private bool IsTemperingRecipe(IConstructibleObjectGetter cobj, IMajorRecordCommonGetter record)
        {
            if (cobj.WorkbenchKeyword.FormKey != Skyrim.Keyword.CraftingSmithingSharpeningWheel && cobj.WorkbenchKeyword.FormKey != Skyrim.Keyword.CraftingSmithingArmorTable) return false;

            if (cobj.CreatedObject.FormKey == null) return false;

            return cobj.CreatedObject.FormKey == record.FormKey;
        }

        private IWeaponGetter? LookupWeapon(FormKey formKey)
        {
            LinkCache.TryLookup<IWeaponGetter>(formKey, out var weapon);
            return weapon;
        }

        private IArmorGetter? LookupArmor(FormKey formKey)
        {
            LinkCache.TryLookup<IArmorGetter>(formKey, out var armor);
            return armor;
        }
    }
}
