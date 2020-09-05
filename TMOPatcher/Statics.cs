using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using Newtonsoft.Json.Linq;
using Noggog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
                new FormKey(Constants.Skyrim, 0x06c0ed),
                new FormKey(Constants.Skyrim, 0x06c0ec),
                new FormKey(Constants.Skyrim, 0x06c0ef),
                new FormKey(Constants.Skyrim, 0x06c0ee),
                new FormKey(Constants.Skyrim, 0x0965b2)
            };

            ArmorMaterials = new FormKey[]
            {
                new FormKey(Constants.Update, 0x0009be),
                new FormKey(Constants.Update, 0x0009c0),
                new FormKey(Constants.Skyrim, 0x06bbd4),
                new FormKey(Constants.Skyrim, 0x06bbd5),
                new FormKey(Constants.Skyrim, 0x06bbd6),
                new FormKey(Constants.Skyrim, 0x06bbd7),
                new FormKey(Constants.Skyrim, 0x06bbd8),
                new FormKey(Constants.Skyrim, 0x06bbd9),
                new FormKey(Constants.Skyrim, 0x06bbda),
                new FormKey(Constants.Update, 0x0009bd),
                new FormKey(Constants.Update, 0x0009b9),
                new FormKey(Constants.Update, 0x0009ba),
                new FormKey(Constants.Skyrim, 0x06bbdc),
                new FormKey(Constants.Skyrim, 0x06bbdd),
                new FormKey(Constants.Skyrim, 0x06bbe2),
                new FormKey(Constants.Skyrim, 0x06bbe0),
                new FormKey(Constants.Skyrim, 0x06bbe1),
                new FormKey(Constants.Skyrim, 0x06bbe3),
                new FormKey(Constants.Skyrim, 0x06bbe4),
                new FormKey(Constants.Skyrim, 0x06bbdb),
                new FormKey(Constants.Skyrim, 0x06bbe5),
                new FormKey(Constants.Update, 0x0009bb),
                new FormKey(Constants.Skyrim, 0x06bbde),
                new FormKey(Constants.Skyrim, 0x06bbe6),
                new FormKey(Constants.Skyrim, 0x06bbe7),
                new FormKey(Constants.Skyrim, 0x0ac13a),
                new FormKey(Constants.Skyrim, 0x06bbdf),
                new FormKey(Constants.Update, 0x0009bc),
                new FormKey(Constants.Update, 0x0009bf),
                new FormKey(Constants.Skyrim, 0x10FD61),
                new FormKey(Constants.Dawnguard, 0x012ccd),
                new FormKey(Constants.Dawnguard, 0x012cce),
                new FormKey(Constants.Dawnguard, 0x0050c4),
                new FormKey(Constants.Dawnguard, 0x01463e),
                new FormKey(Constants.Dawnguard, 0x012ccf),
                new FormKey(Constants.Dawnguard, 0x012cd0),
                new FormKey(Constants.Dragonborn, 0x024101),
                new FormKey(Constants.Dragonborn, 0x024100),
                new FormKey(Constants.Dragonborn, 0x024103),
                new FormKey(Constants.Dragonborn, 0x024102),
                new FormKey(Constants.Dragonborn, 0x03a328),
                new FormKey(Constants.Dragonborn, 0x024105),
                new FormKey(Constants.Dragonborn, 0x024104),
                new FormKey(Constants.Dragonborn, 0x024106),
                new FormKey(Constants.Dragonborn, 0x024107)
            };

            WeaponMaterials = new FormKey[]
            {
                new FormKey(Constants.Dawnguard, 0x019822),
                new FormKey(Constants.Dragonborn, 0x026230),
                new FormKey(Constants.Dragonborn, 0x02622f),
                new FormKey(Constants.Skyrim, 0x01e71f),
                new FormKey(Constants.Skyrim, 0x0c5c01),
                new FormKey(Constants.Skyrim, 0x0c5c02),
                new FormKey(Constants.Skyrim, 0x01e71a),
                new FormKey(Constants.Skyrim, 0x01e71e),
                new FormKey(Constants.Skyrim, 0x01e71b),
                new FormKey(Constants.Skyrim, 0x0c5c03),
                new FormKey(Constants.Skyrim, 0x0c5c04),
                new FormKey(Constants.Skyrim, 0x01e71d),
                new FormKey(Constants.Skyrim, 0x0c5c00),
                new FormKey(Constants.Skyrim, 0x01e718),
                new FormKey(Constants.Skyrim, 0x01e71c),
                new FormKey(Constants.Skyrim, 0x10aa1a),
                new FormKey(Constants.Skyrim, 0x01e719),
                new FormKey(Constants.Skyrim, 0x01e717)
            };

            WeaponTypes = new FormKey[] {
                new FormKey(Constants.Skyrim, 0x06d932),
                new FormKey(Constants.Skyrim, 0x01e715),
                new FormKey(Constants.Skyrim, 0x01e713),
                new FormKey(Constants.Skyrim, 0x06d931),
                new FormKey(Constants.Skyrim, 0x01e714),
                new FormKey(Constants.Skyrim, 0x01e716),
                new FormKey(Constants.Skyrim, 0x01e711),
                new FormKey(Constants.Skyrim, 0x01e712),
                new FormKey(Constants.Skyrim, 0x06d930)
            };

            CraftingSupplies = new Dictionary<string, FormKey>()
            {
                { "Ale",                    new FormKey(Constants.Skyrim,       0x034c5e) },
                { "BearCavePelt",           new FormKey(Constants.Skyrim,       0x03ad53) },
                { "BoneMeal",               new FormKey(Constants.Skyrim,       0x034cdd) },
                { "Coal01",                 new FormKey(Constants.Skyrim,       0x0bfb09) },
                { "Charcoal",               new FormKey(Constants.Skyrim,       0x033760) },
                { "ChaurusChitin",          new FormKey(Constants.Skyrim,       0x03ad57) },
                { "DaedraHeart",            new FormKey(Constants.Skyrim,       0x03ad5b) },
                { "DeathBell",              new FormKey(Constants.Skyrim,       0x0516c8) },
                { "DLC2ChitinPlate",        new FormKey(Constants.Dragonborn,   0x02b04e) },
                { "DLC2NetchJelly",         new FormKey(Constants.Dragonborn,   0x01cd72) },
                { "DLC2NetchLeather",       new FormKey(Constants.Dragonborn,   0x01cd7c) },
                { "DLC2OreStalhrim",        new FormKey(Constants.Dragonborn,   0x02b06b) },
                { "DeerHide",               new FormKey(Constants.Skyrim,       0x03ad90) },
                { "DragonBone",             new FormKey(Constants.Skyrim,       0x03ada4) },
                { "DragonScales",           new FormKey(Constants.Skyrim,       0x03ada3) },
                { "FireflyThorax",          new FormKey(Constants.Skyrim,       0x04da73) },
                { "FireSalts",              new FormKey(Constants.Skyrim,       0x03ad5e) },
                { "Firewood01",             new FormKey(Constants.Skyrim,       0x06f993) },
                { "FrostSalts",             new FormKey(Constants.Skyrim,       0x03ad5f) },
                { "GoatHide",               new FormKey(Constants.Skyrim,       0x03ad8e) },
                { "IngotCorundum",          new FormKey(Constants.Skyrim,       0x05ad93) },
                { "IngotDwarven",           new FormKey(Constants.Skyrim,       0x0db8a2) },
                { "IngotEbony",             new FormKey(Constants.Skyrim,       0x05ad9d) },
                { "IngotGold",              new FormKey(Constants.Skyrim,       0x05ad9e) },
                { "IngotIMoonstone",        new FormKey(Constants.Skyrim,       0x05ad9f) },
                { "IngotIron",              new FormKey(Constants.Skyrim,       0x05ace4) },
                { "IngotMalachite",         new FormKey(Constants.Skyrim,       0x05ada1) },
                { "IngotOrichalcum",        new FormKey(Constants.Skyrim,       0x05ad99) },
                { "IngotQuicksilver",       new FormKey(Constants.Skyrim,       0x05ada0) },
                { "IngotSilver",            new FormKey(Constants.Skyrim,       0x05ace3) },
                { "IngotSteel",             new FormKey(Constants.Skyrim,       0x05ace5) },
                { "Leather01",              new FormKey(Constants.Skyrim,       0x0db5d2) },
                { "LeatherStrips",          new FormKey(Constants.Skyrim,       0x0800e4) },
                { "PowderedMammothTusk",    new FormKey(Constants.Skyrim,       0x06bc10) },
                { "SabreCatPelt",           new FormKey(Constants.Skyrim,       0x03ad6d) },
                { "SaltPile",               new FormKey(Constants.Skyrim,       0x034cdf) },
                { "SoulGemPetty",           new FormKey(Constants.Skyrim,       0x02e4e2) },
                { "VoidSalts",              new FormKey(Constants.Skyrim,       0x03ad60) },
                { "WolfPelt",               new FormKey(Constants.Skyrim,       0x03ad74) }, 
 
                // Nord Hero Support 
                { "DraugrBattleAxeHoned",   new FormKey(Constants.Skyrim,       0x05bf12) },
                { "DraugrBowSupple",        new FormKey(Constants.Skyrim,       0x05d179) },
                { "DraugrGreatswordHoned",  new FormKey(Constants.Skyrim,       0x05bf13) },
                { "DraugrSwordHoned",       new FormKey(Constants.Skyrim,       0x05bf14) },
                { "DraugrWarAxeHoned",      new FormKey(Constants.Skyrim,       0x05bf15) },
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
                [Keywords["ArmorHeavy"]] = new Dictionary<FormKey, Dictionary<FormKey, IArmorGetter?>>()
                {
                    [Keywords["ArmorMaterialBlades"]] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Keywords["ArmorBoots"],      LookupArmor(new FormKey(Constants.Skyrim, 0x04b288)) },
                        { Keywords["ArmorCuirass"],    LookupArmor(new FormKey(Constants.Skyrim, 0x04b28b)) },
                        { Keywords["ArmorGauntlets"],  LookupArmor(new FormKey(Constants.Skyrim, 0x04b28d)) },
                        { Keywords["ArmorHelmet"],     LookupArmor(new FormKey(Constants.Skyrim, 0x04b28f)) },
                        { Keywords["ArmorShield"],     LookupArmor(new FormKey(Constants.Skyrim, 0x04b28f)) }
                    },

                    [Keywords["ArmorMaterialDaedric"]] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Keywords["ArmorBoots"],      LookupArmor(new FormKey(Constants.Skyrim, 0x01396a)) },
                        { Keywords["ArmorCuirass"],    LookupArmor(new FormKey(Constants.Skyrim, 0x01396b)) },
                        { Keywords["ArmorGauntlets"],  LookupArmor(new FormKey(Constants.Skyrim, 0x01396c)) },
                        { Keywords["ArmorHelmet"],     LookupArmor(new FormKey(Constants.Skyrim, 0x01396d)) },
                        { Keywords["ArmorShield"],     LookupArmor(new FormKey(Constants.Skyrim, 0x01396e)) }
                    },

                    [Keywords["ArmorMaterialDragonplate"]] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Keywords["ArmorBoots"],      LookupArmor(new FormKey(Constants.Skyrim, 0x013965)) },
                        { Keywords["ArmorCuirass"],    LookupArmor(new FormKey(Constants.Skyrim, 0x013966)) },
                        { Keywords["ArmorGauntlets"],  LookupArmor(new FormKey(Constants.Skyrim, 0x013967)) },
                        { Keywords["ArmorHelmet"],     LookupArmor(new FormKey(Constants.Skyrim, 0x013969)) },
                        { Keywords["ArmorShield"],     LookupArmor(new FormKey(Constants.Skyrim, 0x013968)) }
                    },

                    [Keywords["ArmorMaterialDwarven"]] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Keywords["ArmorBoots"],      LookupArmor(new FormKey(Constants.Skyrim, 0x01394c)) },
                        { Keywords["ArmorCuirass"],    LookupArmor(new FormKey(Constants.Skyrim, 0x01394d)) },
                        { Keywords["ArmorGauntlets"],  LookupArmor(new FormKey(Constants.Skyrim, 0x01394e)) },
                        { Keywords["ArmorHelmet"],     LookupArmor(new FormKey(Constants.Skyrim, 0x01394f)) },
                        { Keywords["ArmorShield"],     LookupArmor(new FormKey(Constants.Skyrim, 0x013950)) }
                    },

                    [Keywords["ArmorMaterialEbony"]] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Keywords["ArmorBoots"],      LookupArmor(new FormKey(Constants.Skyrim, 0x013960)) },
                        { Keywords["ArmorCuirass"],    LookupArmor(new FormKey(Constants.Skyrim, 0x013961)) },
                        { Keywords["ArmorGauntlets"],  LookupArmor(new FormKey(Constants.Skyrim, 0x013962)) },
                        { Keywords["ArmorHelmet"],     LookupArmor(new FormKey(Constants.Skyrim, 0x013963)) },
                        { Keywords["ArmorShield"],     LookupArmor(new FormKey(Constants.Skyrim, 0x013964)) }
                    },

                    [Keywords["ArmorMaterialFalmer"]] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Keywords["ArmorBoots"],      LookupArmor(new FormKey(Constants.Skyrim, 0x0b83cd)) },
                        { Keywords["ArmorCuirass"],    LookupArmor(new FormKey(Constants.Skyrim, 0x0b83cb)) },
                        { Keywords["ArmorGauntlets"],  LookupArmor(new FormKey(Constants.Skyrim, 0x0b83cf)) },
                        { Keywords["ArmorHelmet"],     LookupArmor(new FormKey(Constants.Skyrim, 0x04c3cb)) },
                        { Keywords["ArmorShield"],     LookupArmor(new FormKey(Constants.Skyrim, 0x05c06c)) }
                    },

                    [Keywords["ArmorMaterialImperialHeavy"]] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Keywords["ArmorBoots"],      LookupArmor(new FormKey(Constants.Skyrim, 0x0136d6)) },
                        { Keywords["ArmorCuirass"],    LookupArmor(new FormKey(Constants.Skyrim, 0x0136d5)) },
                        { Keywords["ArmorGauntlets"],  LookupArmor(new FormKey(Constants.Skyrim, 0x0136d4)) },
                        { Keywords["ArmorHelmet"],     LookupArmor(new FormKey(Constants.Skyrim, 0x0136cf)) },
                        { Keywords["ArmorShield"],     LookupArmor(new FormKey(Constants.Skyrim, 0x0135ba)) }
                    },

                    [Keywords["ArmorMaterialIron"]] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Keywords["ArmorBoots"],      LookupArmor(new FormKey(Constants.Skyrim, 0x012e4b)) },
                        { Keywords["ArmorCuirass"],    LookupArmor(new FormKey(Constants.Skyrim, 0x012e49)) },
                        { Keywords["ArmorGauntlets"],  LookupArmor(new FormKey(Constants.Skyrim, 0x012e46)) },
                        { Keywords["ArmorHelmet"],     LookupArmor(new FormKey(Constants.Skyrim, 0x012e4d)) },
                        { Keywords["ArmorShield"],     LookupArmor(new FormKey(Constants.Skyrim, 0x012eb6)) }
                    },

                    [Keywords["ArmorMaterialIronBanded"]] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Keywords["ArmorBoots"],      null                                                 },
                        { Keywords["ArmorCuirass"],    LookupArmor(new FormKey(Constants.Skyrim, 0x013948)) },
                        { Keywords["ArmorGauntlets"],  null                                                 },
                        { Keywords["ArmorHelmet"],     null                                                 },
                        { Keywords["ArmorShield"],     LookupArmor(new FormKey(Constants.Skyrim, 0x01394b)) }
                    },

                    [Keywords["ArmorMaterialOrcish"]] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Keywords["ArmorBoots"],      LookupArmor(new FormKey(Constants.Skyrim, 0x013956)) },
                        { Keywords["ArmorCuirass"],    LookupArmor(new FormKey(Constants.Skyrim, 0x013957)) },
                        { Keywords["ArmorGauntlets"],  LookupArmor(new FormKey(Constants.Skyrim, 0x013958)) },
                        { Keywords["ArmorHelmet"],     LookupArmor(new FormKey(Constants.Skyrim, 0x013959)) },
                        { Keywords["ArmorShield"],     LookupArmor(new FormKey(Constants.Skyrim, 0x013946)) }
                    },

                    [Keywords["ArmorMaterialSteel"]] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Keywords["ArmorBoots"],      LookupArmor(new FormKey(Constants.Skyrim, 0x013951)) },
                        { Keywords["ArmorCuirass"],    LookupArmor(new FormKey(Constants.Skyrim, 0x013952)) },
                        { Keywords["ArmorGauntlets"],  LookupArmor(new FormKey(Constants.Skyrim, 0x013953)) },
                        { Keywords["ArmorHelmet"],     LookupArmor(new FormKey(Constants.Skyrim, 0x013954)) },
                        { Keywords["ArmorShield"],     LookupArmor(new FormKey(Constants.Skyrim, 0x013955)) }
                    },

                    [Keywords["ArmorMaterialSteelPlate"]] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Keywords["ArmorBoots"],      LookupArmor(new FormKey(Constants.Skyrim, 0x01395b)) },
                        { Keywords["ArmorCuirass"],    LookupArmor(new FormKey(Constants.Skyrim, 0x01395c)) },
                        { Keywords["ArmorGauntlets"],  LookupArmor(new FormKey(Constants.Skyrim, 0x01395d)) },
                        { Keywords["ArmorHelmet"],     LookupArmor(new FormKey(Constants.Skyrim, 0x01395e)) },
                        { Keywords["ArmorShield"],     null                                                 }
                    },

                    [Keywords["DLC1ArmorMaterialDawnguard"]] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Keywords["ArmorBoots"],      LookupArmor(new FormKey(Constants.Dawnguard,  0x014757)) },
                        { Keywords["ArmorCuirass"],    LookupArmor(new FormKey(Constants.Dawnguard,  0x00f3f7)) },
                        { Keywords["ArmorGauntlets"],  LookupArmor(new FormKey(Constants.Dawnguard,  0x014758)) },
                        { Keywords["ArmorHelmet"],     LookupArmor(new FormKey(Constants.Dawnguard,  0x0050d0)) },
                        { Keywords["ArmorShield"],     LookupArmor(new FormKey(Constants.Dawnguard,  0x0150b8)) }
                    },

                    [Keywords["DLC1ArmorMaterialFalmerHardened"]] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Keywords["ArmorBoots"],      LookupArmor(new FormKey(Constants.Dawnguard,  0x00e8dd)) },
                        { Keywords["ArmorCuirass"],    LookupArmor(new FormKey(Constants.Dawnguard,  0x00e8de)) },
                        { Keywords["ArmorGauntlets"],  LookupArmor(new FormKey(Constants.Dawnguard,  0x00e8df)) },
                        { Keywords["ArmorHelmet"],     LookupArmor(new FormKey(Constants.Dawnguard,  0x00e8e0)) },
                        { Keywords["ArmorShield"],     null                                                 }
                    },

                    [Keywords["DLC1ArmorMaterialHunter"]] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Keywords["ArmorBoots"],      LookupArmor(new FormKey(Constants.Dawnguard,  0x0050c5)) },
                        { Keywords["ArmorCuirass"],    LookupArmor(new FormKey(Constants.Dawnguard,  0x0050c6)) },
                        { Keywords["ArmorGauntlets"],  LookupArmor(new FormKey(Constants.Dawnguard,  0x0050c7)) },
                        { Keywords["ArmorHelmet"],     null                                                 },
                        { Keywords["ArmorShield"],     null                                                 }
                    },

                    [Keywords["DLC1ArmorMaterielFalmerHeavy"]] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Keywords["ArmorBoots"],      LookupArmor(new FormKey(Constants.Dawnguard,  0x0023ef)) },
                        { Keywords["ArmorCuirass"],    LookupArmor(new FormKey(Constants.Dawnguard,  0x0023e9)) },
                        { Keywords["ArmorGauntlets"],  LookupArmor(new FormKey(Constants.Dawnguard,  0x0023ed)) },
                        { Keywords["ArmorHelmet"],     LookupArmor(new FormKey(Constants.Dawnguard,  0x0023eb)) },
                        { Keywords["ArmorShield"],     null                                                 }
                    },

                    [Keywords["DLC2ArmorMaterialBonemoldHeavy"]] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Keywords["ArmorBoots"],      LookupArmor(new FormKey(Constants.Dragonborn, 0x01cd92)) },
                        { Keywords["ArmorCuirass"],    LookupArmor(new FormKey(Constants.Dragonborn, 0x01cd93)) },
                        { Keywords["ArmorGauntlets"],  LookupArmor(new FormKey(Constants.Dragonborn, 0x01cd9f)) },
                        { Keywords["ArmorHelmet"],     LookupArmor(new FormKey(Constants.Dragonborn, 0x01cd95)) },
                        { Keywords["ArmorShield"],     LookupArmor(new FormKey(Constants.Dragonborn, 0x026234)) }
                    },

                    [Keywords["DLC2ArmorMaterialChitinHeavy"]] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Keywords["ArmorBoots"],      LookupArmor(new FormKey(Constants.Dragonborn, 0x01cd82)) },
                        { Keywords["ArmorCuirass"],    LookupArmor(new FormKey(Constants.Dragonborn, 0x01cd8a)) },
                        { Keywords["ArmorGauntlets"],  LookupArmor(new FormKey(Constants.Dragonborn, 0x01cd8b)) },
                        { Keywords["ArmorHelmet"],     LookupArmor(new FormKey(Constants.Dragonborn, 0x01cd8c)) },
                        { Keywords["ArmorShield"],     null                                                 }
                    },

                    [Keywords["DLC2ArmorMaterialNordicHeavy"]] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Keywords["ArmorBoots"],      LookupArmor(new FormKey(Constants.Dragonborn, 0x01cd96)) },
                        { Keywords["ArmorCuirass"],    LookupArmor(new FormKey(Constants.Dragonborn, 0x01cd97)) },
                        { Keywords["ArmorGauntlets"],  LookupArmor(new FormKey(Constants.Dragonborn, 0x01cd98)) },
                        { Keywords["ArmorHelmet"],     LookupArmor(new FormKey(Constants.Dragonborn, 0x01cd99)) },
                        { Keywords["ArmorShield"],     LookupArmor(new FormKey(Constants.Dragonborn, 0x026236)) }
                    },

                    [Keywords["DLC2ArmorMaterialStalhrimHeavy"]] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Keywords["ArmorBoots"],      LookupArmor(new FormKey(Constants.Dragonborn, 0x01cd9e)) },
                        { Keywords["ArmorCuirass"],    LookupArmor(new FormKey(Constants.Dragonborn, 0x01cd9f)) },
                        { Keywords["ArmorGauntlets"],  LookupArmor(new FormKey(Constants.Dragonborn, 0x01cda0)) },
                        { Keywords["ArmorHelmet"],     LookupArmor(new FormKey(Constants.Dragonborn, 0x01cda1)) },
                        { Keywords["ArmorShield"],     null                                                 }
                    }
                },

                [Keywords["ArmorLight"]] = new Dictionary<FormKey, Dictionary<FormKey, IArmorGetter?>>()
                {
                    [Keywords["ArmorMaterialBearStormcloak"]] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Keywords["ArmorBoots"],      LookupArmor(new FormKey(Constants.Skyrim, 0x086981)) },
                        { Keywords["ArmorCuirass"],    LookupArmor(new FormKey(Constants.Skyrim, 0x08697e)) },
                        { Keywords["ArmorGauntlets"],  LookupArmor(new FormKey(Constants.Skyrim, 0x086983)) },
                        { Keywords["ArmorHelmet"],     LookupArmor(new FormKey(Constants.Skyrim, 0x086985)) },
                        { Keywords["ArmorShield"],     null                                                 }
                    },

                    [Keywords["ArmorMaterialDragonscale"]] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Keywords["ArmorBoots"],      LookupArmor(new FormKey(Constants.Skyrim, 0x01393d)) },
                        { Keywords["ArmorCuirass"],    LookupArmor(new FormKey(Constants.Skyrim, 0x01393e)) },
                        { Keywords["ArmorGauntlets"],  LookupArmor(new FormKey(Constants.Skyrim, 0x01393f)) },
                        { Keywords["ArmorHelmet"],     LookupArmor(new FormKey(Constants.Skyrim, 0x013940)) },
                        { Keywords["ArmorShield"],     LookupArmor(new FormKey(Constants.Skyrim, 0x013941)) }
                    },

                    [Keywords["ArmorMaterialElven"]] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Keywords["ArmorBoots"],      LookupArmor(new FormKey(Constants.Skyrim, 0x01391a)) },
                        { Keywords["ArmorCuirass"],    LookupArmor(new FormKey(Constants.Skyrim, 0x0896a3)) },
                        { Keywords["ArmorGauntlets"],  LookupArmor(new FormKey(Constants.Skyrim, 0x01391c)) },
                        { Keywords["ArmorHelmet"],     LookupArmor(new FormKey(Constants.Skyrim, 0x01391d)) },
                        { Keywords["ArmorShield"],     LookupArmor(new FormKey(Constants.Skyrim, 0x01391e)) }
                    },

                    [Keywords["ArmorMaterialElvenGilded"]] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Keywords["ArmorBoots"],      null                                                 },
                        { Keywords["ArmorCuirass"],    LookupArmor(new FormKey(Constants.Skyrim, 0x01392a)) },
                        { Keywords["ArmorGauntlets"],  null                                                 },
                        { Keywords["ArmorHelmet"],     null                                                 },
                        { Keywords["ArmorShield"],     null                                                 }
                    },

                    [Keywords["ArmorMaterialForsworn"]] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Keywords["ArmorBoots"],      LookupArmor(new FormKey(Constants.Skyrim, 0x0d8d4e)) },
                        { Keywords["ArmorCuirass"],    LookupArmor(new FormKey(Constants.Skyrim, 0x0d8d50)) },
                        { Keywords["ArmorGauntlets"],  LookupArmor(new FormKey(Constants.Skyrim, 0x0d8d55)) },
                        { Keywords["ArmorHelmet"],     LookupArmor(new FormKey(Constants.Skyrim, 0x0d8d52)) },
                        { Keywords["ArmorShield"],     null                                                 }
                    },

                    [Keywords["ArmorMaterialMS02Forsworn"]] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Keywords["ArmorBoots"],      LookupArmor(new FormKey(Constants.Skyrim, 0x0eafd3)) },
                        { Keywords["ArmorCuirass"],    LookupArmor(new FormKey(Constants.Skyrim, 0x0eafd0)) },
                        { Keywords["ArmorGauntlets"],  LookupArmor(new FormKey(Constants.Skyrim, 0x0eafd2)) },
                        { Keywords["ArmorHelmet"],     LookupArmor(new FormKey(Constants.Skyrim, 0x0eafd1)) },
                        { Keywords["ArmorShield"],     null                                                 }
                    },

                    [Keywords["ArmorMaterialGlass"]] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Keywords["ArmorBoots"],      LookupArmor(new FormKey(Constants.Skyrim, 0x013938)) },
                        { Keywords["ArmorCuirass"],    LookupArmor(new FormKey(Constants.Skyrim, 0x013939)) },
                        { Keywords["ArmorGauntlets"],  LookupArmor(new FormKey(Constants.Skyrim, 0x01393a)) },
                        { Keywords["ArmorHelmet"],     LookupArmor(new FormKey(Constants.Skyrim, 0x01393b)) },
                        { Keywords["ArmorShield"],     LookupArmor(new FormKey(Constants.Skyrim, 0x01393c)) }
                    },

                    [Keywords["ArmorMaterialHide"]] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Keywords["ArmorBoots"],      LookupArmor(new FormKey(Constants.Skyrim, 0x013910)) },
                        { Keywords["ArmorCuirass"],    LookupArmor(new FormKey(Constants.Skyrim, 0x013911)) },
                        { Keywords["ArmorGauntlets"],  LookupArmor(new FormKey(Constants.Skyrim, 0x013912)) },
                        { Keywords["ArmorHelmet"],     LookupArmor(new FormKey(Constants.Skyrim, 0x013913)) },
                        { Keywords["ArmorShield"],     LookupArmor(new FormKey(Constants.Skyrim, 0x013914)) }
                    },

                    [Keywords["ArmorMaterialImperialLight"]] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Keywords["ArmorBoots"],      LookupArmor(new FormKey(Constants.Skyrim, 0x013ed7)) },
                        { Keywords["ArmorCuirass"],    LookupArmor(new FormKey(Constants.Skyrim, 0x013ed9)) },
                        { Keywords["ArmorGauntlets"],  LookupArmor(new FormKey(Constants.Skyrim, 0x013eda)) },
                        { Keywords["ArmorHelmet"],     LookupArmor(new FormKey(Constants.Skyrim, 0x013edb)) },
                        { Keywords["ArmorShield"],     LookupArmor(new FormKey(Constants.Skyrim, 0x013ab2)) }
                    },

                    [Keywords["ArmorMaterialImperialStudded"]] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Keywords["ArmorBoots"],      null                                                 },
                        { Keywords["ArmorCuirass"],    LookupArmor(new FormKey(Constants.Skyrim, 0x013ed8)) },
                        { Keywords["ArmorGauntlets"],  null                                                 },
                        { Keywords["ArmorHelmet"],     LookupArmor(new FormKey(Constants.Skyrim, 0x013edc)) },
                        { Keywords["ArmorShield"],     null                                                 }
                    },

                    [Keywords["ArmorMaterialLeather"]] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Keywords["ArmorBoots"],      LookupArmor(new FormKey(Constants.Skyrim, 0x013920)) },
                        { Keywords["ArmorCuirass"],    LookupArmor(new FormKey(Constants.Skyrim, 0x03619e)) },
                        { Keywords["ArmorGauntlets"],  LookupArmor(new FormKey(Constants.Skyrim, 0x013921)) },
                        { Keywords["ArmorHelmet"],     LookupArmor(new FormKey(Constants.Skyrim, 0x013922)) },
                        { Keywords["ArmorShield"],     null                                                 }
                    },

                    [Keywords["ArmorMaterialPenitus"]] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Keywords["ArmorBoots"],      LookupArmor(new FormKey(Constants.Skyrim, 0x0d3ea7)) },
                        { Keywords["ArmorCuirass"],    LookupArmor(new FormKey(Constants.Skyrim, 0x0d3ea0)) },
                        { Keywords["ArmorGauntlets"],  LookupArmor(new FormKey(Constants.Skyrim, 0x0d3eab)) },
                        { Keywords["ArmorHelmet"],     LookupArmor(new FormKey(Constants.Skyrim, 0x0d3eaa)) },
                        { Keywords["ArmorShield"],     null                                                 }
                    },

                    [Keywords["ArmorMaterialScaled"]] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Keywords["ArmorBoots"],      LookupArmor(new FormKey(Constants.Skyrim, 0x01b39f)) },
                        { Keywords["ArmorCuirass"],    LookupArmor(new FormKey(Constants.Skyrim, 0x01b3a3)) },
                        { Keywords["ArmorGauntlets"],  LookupArmor(new FormKey(Constants.Skyrim, 0x01b3a0)) },
                        { Keywords["ArmorHelmet"],     LookupArmor(new FormKey(Constants.Skyrim, 0x01b3a1)) },
                        { Keywords["ArmorShield"],     null                                                 }
                    },

                    [Keywords["ArmorMaterialStormcloak"]] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Keywords["ArmorBoots"],      LookupArmor(new FormKey(Constants.Skyrim, 0x0a6d7f)) },
                        { Keywords["ArmorCuirass"],    LookupArmor(new FormKey(Constants.Skyrim, 0x0ad5a0)) },
                        { Keywords["ArmorGauntlets"],  LookupArmor(new FormKey(Constants.Skyrim, 0x0a6d7d)) },
                        { Keywords["ArmorHelmet"],     LookupArmor(new FormKey(Constants.Skyrim, 0x0a6d79)) },
                        { Keywords["ArmorShield"],     null                                                 }
                    },

                    [Keywords["ArmorMaterialStudded"]] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Keywords["ArmorBoots"],      null                                                 },
                        { Keywords["ArmorCuirass"],    LookupArmor(new FormKey(Constants.Skyrim, 0x01b3a2)) },
                        { Keywords["ArmorGauntlets"],  null                                                 },
                        { Keywords["ArmorHelmet"],     null                                                 },
                        { Keywords["ArmorShield"],     null                                                 }
                    },

                    [Keywords["ArmorMaterialThievesGuild"]] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Keywords["ArmorBoots"],      LookupArmor(new FormKey(Constants.Skyrim, 0x0d3ac2)) },
                        { Keywords["ArmorCuirass"],    LookupArmor(new FormKey(Constants.Skyrim, 0x0d3ac3)) },
                        { Keywords["ArmorGauntlets"],  LookupArmor(new FormKey(Constants.Skyrim, 0x0d3ac4)) },
                        { Keywords["ArmorHelmet"],     LookupArmor(new FormKey(Constants.Skyrim, 0x0d3ac5)) },
                        { Keywords["ArmorShield"],     null                                                 }
                    },

                    [Keywords["ArmorMaterialThievesGuildLeader"]] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Keywords["ArmorBoots"],      LookupArmor(new FormKey(Constants.Skyrim, 0x0e35d6)) },
                        { Keywords["ArmorCuirass"],    LookupArmor(new FormKey(Constants.Skyrim, 0x0e35d7)) },
                        { Keywords["ArmorGauntlets"],  LookupArmor(new FormKey(Constants.Skyrim, 0x0e35d8)) },
                        { Keywords["ArmorHelmet"],     LookupArmor(new FormKey(Constants.Skyrim, 0x0e35d9)) },
                        { Keywords["ArmorShield"],     null                                                 }
                    },

                    [Keywords["ArmorNightingale"]] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Keywords["ArmorBoots"],      LookupArmor(new FormKey(Constants.Skyrim, 0x0483c1)) },
                        { Keywords["ArmorCuirass"],    LookupArmor(new FormKey(Constants.Skyrim, 0x0483c2)) },
                        { Keywords["ArmorGauntlets"],  LookupArmor(new FormKey(Constants.Skyrim, 0x0487d1)) },
                        { Keywords["ArmorHelmet"],     LookupArmor(new FormKey(Constants.Skyrim, 0x0487d8)) },
                        { Keywords["ArmorShield"],     null                                                 }
                    },

                    [Keywords["DLC1ArmorMaterialDawnguard"]] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Keywords["ArmorBoots"],      LookupArmor(new FormKey(Constants.Dawnguard,  0x00f400)) },
                        { Keywords["ArmorCuirass"],    LookupArmor(new FormKey(Constants.Dawnguard,  0x00f3fb)) },
                        { Keywords["ArmorGauntlets"],  LookupArmor(new FormKey(Constants.Dawnguard,  0x00f3fe)) },
                        { Keywords["ArmorHelmet"],     LookupArmor(new FormKey(Constants.Dawnguard,  0x01989e)) },
                        { Keywords["ArmorShield"],     LookupArmor(new FormKey(Constants.Dawnguard,  0x011baf)) }
                    },

                    [Keywords["DLC1ArmorMaterialVampire"]] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Keywords["ArmorBoots"],      LookupArmor(new FormKey(Constants.Dawnguard,  0x00b5de)) },
                        { Keywords["ArmorCuirass"],    LookupArmor(new FormKey(Constants.Dawnguard,  0x0142c7)) },
                        { Keywords["ArmorGauntlets"],  LookupArmor(new FormKey(Constants.Dawnguard,  0x01a51f)) },
                        { Keywords["ArmorHelmet"],     null                                                 },
                        { Keywords["ArmorShield"],     null                                                 }
                    },

                    // Yes... this is really light armor.  WTF Bethesda 
                    [Keywords["DLC1ArmorMaterielFalmerHeavyOriginal"]] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Keywords["ArmorBoots"],      LookupArmor(new FormKey(Constants.Skyrim, 0x0b83cd)) },
                        { Keywords["ArmorCuirass"],    LookupArmor(new FormKey(Constants.Skyrim, 0x0b83cb)) },
                        { Keywords["ArmorGauntlets"],  LookupArmor(new FormKey(Constants.Skyrim, 0x0b83cf)) },
                        { Keywords["ArmorHelmet"],     LookupArmor(new FormKey(Constants.Skyrim, 0x04c3cb)) },
                        { Keywords["ArmorShield"],     null                                                 }
                    },

                    [Keywords["DLC2ArmorMaterialBonemoldLight"]] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Keywords["ArmorBoots"],      null                                                 },
                        { Keywords["ArmorCuirass"],    null                                                 },
                        { Keywords["ArmorGauntlets"],  null                                                 },
                        { Keywords["ArmorHelmet"],     null                                                 },
                        { Keywords["ArmorShield"],     null                                                 }
                    },

                    [Keywords["DLC2ArmorMaterialChitinLight"]] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Keywords["ArmorBoots"],      LookupArmor(new FormKey(Constants.Dragonborn, 0x01cd86)) },
                        { Keywords["ArmorCuirass"],    LookupArmor(new FormKey(Constants.Dragonborn, 0x01cd87)) },
                        { Keywords["ArmorGauntlets"],  LookupArmor(new FormKey(Constants.Dragonborn, 0x01cd88)) },
                        { Keywords["ArmorHelmet"],     LookupArmor(new FormKey(Constants.Dragonborn, 0x01cd89)) },
                        { Keywords["ArmorShield"],     LookupArmor(new FormKey(Constants.Dragonborn, 0x026235)) }
                    },

                    [Keywords["DLC2ArmorMaterialMoragTong"]] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Keywords["ArmorBoots"],      LookupArmor(new FormKey(Constants.Dragonborn, 0x0292ab)) },
                        { Keywords["ArmorCuirass"],    LookupArmor(new FormKey(Constants.Dragonborn, 0x0292ac)) },
                        { Keywords["ArmorGauntlets"],  LookupArmor(new FormKey(Constants.Dragonborn, 0x0292ad)) },
                        { Keywords["ArmorHelmet"],     LookupArmor(new FormKey(Constants.Dragonborn, 0x0292ae)) },
                        { Keywords["ArmorShield"],     null                                                 }
                    },

                    [Keywords["DLC2ArmorMaterialStalhrimLight"]] = new Dictionary<FormKey, IArmorGetter?>()
                    {
                        { Keywords["ArmorBoots"],      LookupArmor(new FormKey(Constants.Dragonborn, 0x01cd7e)) },
                        { Keywords["ArmorCuirass"],    LookupArmor(new FormKey(Constants.Dragonborn, 0x01cda2)) },
                        { Keywords["ArmorGauntlets"],  LookupArmor(new FormKey(Constants.Dragonborn, 0x01cda5)) },
                        { Keywords["ArmorHelmet"],     LookupArmor(new FormKey(Constants.Dragonborn, 0x01cda3)) },
                        { Keywords["ArmorShield"],     LookupArmor(new FormKey(Constants.Dragonborn, 0x026237)) }
                    }
                }
            };

            BaseWeapons = new Dictionary<FormKey, Dictionary<FormKey, IWeaponGetter?>>()
            {
                [Keywords["DLC1WeapMaterialDragonbone"]] = new Dictionary<FormKey, IWeaponGetter?>()
                {
                    { Keywords["WeapTypeBattleaxe"],  LookupWeapon(new FormKey(Constants.Dawnguard,     0x014fc3))  },
                    { Keywords["WeapTypeDagger"],     LookupWeapon(new FormKey(Constants.Dawnguard,     0x014fcb))  },
                    { Keywords["WeapTypeGreatsword"], LookupWeapon(new FormKey(Constants.Dawnguard,     0x014fcc))  },
                    { Keywords["WeapTypeMace"],       LookupWeapon(new FormKey(Constants.Dawnguard,     0x014fcd))  },
                    { Keywords["WeapTypeSword"],      LookupWeapon(new FormKey(Constants.Dawnguard,     0x014fce))  },
                    { Keywords["WeapTypeWarAxe"],     LookupWeapon(new FormKey(Constants.Dawnguard,     0x014fcf))  },
                    { Keywords["WeapTypeWarhammer"],  LookupWeapon(new FormKey(Constants.Dawnguard,     0x014fd0))  }
                },

                [Keywords["DLC2WeaponMaterialNordic"]] = new Dictionary<FormKey, IWeaponGetter?>()
                {
                    { Keywords["WeapTypeBattleaxe"],  LookupWeapon(new FormKey(Constants.Dragonborn,    0x01cdad)) },
                    { Keywords["WeapTypeDagger"],     LookupWeapon(new FormKey(Constants.Dragonborn,    0x01cdae)) },
                    { Keywords["WeapTypeGreatsword"], LookupWeapon(new FormKey(Constants.Dragonborn,    0x01cdaf)) },
                    { Keywords["WeapTypeMace"],       LookupWeapon(new FormKey(Constants.Dragonborn,    0x01cdb0)) },
                    { Keywords["WeapTypeSword"],      LookupWeapon(new FormKey(Constants.Dragonborn,    0x01cdb1)) },
                    { Keywords["WeapTypeWarAxe"],     LookupWeapon(new FormKey(Constants.Dragonborn,    0x01cdb2)) },
                    { Keywords["WeapTypeWarhammer"],  LookupWeapon(new FormKey(Constants.Dragonborn,    0x01cdb3)) }
                },

                [Keywords["DLC2WeaponMaterialStalhrim"]] = new Dictionary<FormKey, IWeaponGetter?>()
                {
                    { Keywords["WeapTypeBattleaxe"],  LookupWeapon(new FormKey(Constants.Dragonborn,    0x01cdb4)) },
                    { Keywords["WeapTypeDagger"],     LookupWeapon(new FormKey(Constants.Dragonborn,    0x01cdb5)) },
                    { Keywords["WeapTypeGreatsword"], LookupWeapon(new FormKey(Constants.Dragonborn,    0x01cdb6)) },
                    { Keywords["WeapTypeMace"],       LookupWeapon(new FormKey(Constants.Dragonborn,    0x01cdb7)) },
                    { Keywords["WeapTypeSword"],      LookupWeapon(new FormKey(Constants.Dragonborn,    0x01cdb8)) },
                    { Keywords["WeapTypeWarAxe"],     LookupWeapon(new FormKey(Constants.Dragonborn,    0x01cdb9)) },
                    { Keywords["WeapTypeWarhammer"],  LookupWeapon(new FormKey(Constants.Dragonborn,    0x01cdba)) }
                },

                [Keywords["WeapMaterialDaedric"]] = new Dictionary<FormKey, IWeaponGetter?>()
                {
                    { Keywords["WeapTypeBattleaxe"],  LookupWeapon(new FormKey(Constants.Skyrim,        0x0139b4)) },
                    { Keywords["WeapTypeDagger"],     LookupWeapon(new FormKey(Constants.Skyrim,        0x0139b6)) },
                    { Keywords["WeapTypeGreatsword"], LookupWeapon(new FormKey(Constants.Skyrim,        0x0139b7)) },
                    { Keywords["WeapTypeMace"],       LookupWeapon(new FormKey(Constants.Skyrim,        0x0139b8)) },
                    { Keywords["WeapTypeSword"],      LookupWeapon(new FormKey(Constants.Skyrim,        0x0139b9)) },
                    { Keywords["WeapTypeWarAxe"],     LookupWeapon(new FormKey(Constants.Skyrim,        0x0139b3)) },
                    { Keywords["WeapTypeWarhammer"],  LookupWeapon(new FormKey(Constants.Skyrim,        0x0139ba)) }
                },

                [Keywords["WeapMaterialDraugr"]] = new Dictionary<FormKey, IWeaponGetter?>()
                {
                    { Keywords["WeapTypeBattleaxe"],  LookupWeapon(new FormKey(Constants.Skyrim,        0x01cb64)) },
                    { Keywords["WeapTypeDagger"],     null                                                         },
                    { Keywords["WeapTypeGreatsword"], LookupWeapon(new FormKey(Constants.Skyrim,        0x0236a5)) },
                    { Keywords["WeapTypeMace"],       null                                                         },
                    { Keywords["WeapTypeSword"],      LookupWeapon(new FormKey(Constants.Skyrim,        0x02c66f)) },
                    { Keywords["WeapTypeWarAxe"],     LookupWeapon(new FormKey(Constants.Skyrim,        0x02c672)) },
                    { Keywords["WeapTypeWarhammer"],  null                                                         }
                },

                [Keywords["WeapMaterialDraugrHoned"]] = new Dictionary<FormKey, IWeaponGetter?>()
                {
                    { Keywords["WeapTypeBattleaxe"],  LookupWeapon(new FormKey(Constants.Skyrim,        0x05bf12)) },
                    { Keywords["WeapTypeDagger"],     null                                                         },
                    { Keywords["WeapTypeGreatsword"], LookupWeapon(new FormKey(Constants.Skyrim,        0x05bf13)) },
                    { Keywords["WeapTypeMace"],       null                                                         },
                    { Keywords["WeapTypeSword"],      LookupWeapon(new FormKey(Constants.Skyrim,        0x05bf14)) },
                    { Keywords["WeapTypeWarAxe"],     LookupWeapon(new FormKey(Constants.Skyrim,        0x05bf15)) },
                    { Keywords["WeapTypeWarhammer"],  null                                                         }
                },

                [Keywords["WeapMaterialDwarven"]] = new Dictionary<FormKey, IWeaponGetter?>()
                {
                    { Keywords["WeapTypeBattleaxe"],  LookupWeapon(new FormKey(Constants.Skyrim,        0x013994)) },
                    { Keywords["WeapTypeDagger"],     LookupWeapon(new FormKey(Constants.Skyrim,        0x013996)) },
                    { Keywords["WeapTypeGreatsword"], LookupWeapon(new FormKey(Constants.Skyrim,        0x013997)) },
                    { Keywords["WeapTypeMace"],       LookupWeapon(new FormKey(Constants.Skyrim,        0x013998)) },
                    { Keywords["WeapTypeSword"],      LookupWeapon(new FormKey(Constants.Skyrim,        0x013999)) },
                    { Keywords["WeapTypeWarAxe"],     LookupWeapon(new FormKey(Constants.Skyrim,        0x013993)) },
                    { Keywords["WeapTypeWarhammer"],  LookupWeapon(new FormKey(Constants.Skyrim,        0x01399a)) }
                },

                [Keywords["WeapMaterialEbony"]] = new Dictionary<FormKey, IWeaponGetter?>()
                {
                    { Keywords["WeapTypeBattleaxe"],  LookupWeapon(new FormKey(Constants.Skyrim,        0x0139ac)) },
                    { Keywords["WeapTypeDagger"],     LookupWeapon(new FormKey(Constants.Skyrim,        0x0139ae)) },
                    { Keywords["WeapTypeGreatsword"], LookupWeapon(new FormKey(Constants.Skyrim,        0x0139af)) },
                    { Keywords["WeapTypeMace"],       LookupWeapon(new FormKey(Constants.Skyrim,        0x0139b0)) },
                    { Keywords["WeapTypeSword"],      LookupWeapon(new FormKey(Constants.Skyrim,        0x0139b1)) },
                    { Keywords["WeapTypeWarAxe"],     LookupWeapon(new FormKey(Constants.Skyrim,        0x0139ab)) },
                    { Keywords["WeapTypeWarhammer"],  LookupWeapon(new FormKey(Constants.Skyrim,        0x0139b2)) }
                },

                [Keywords["WeapMaterialElven"]] = new Dictionary<FormKey, IWeaponGetter?>()
                {
                    { Keywords["WeapTypeBattleaxe"],  LookupWeapon(new FormKey(Constants.Skyrim,        0x01399c)) },
                    { Keywords["WeapTypeDagger"],     LookupWeapon(new FormKey(Constants.Skyrim,        0x01399e)) },
                    { Keywords["WeapTypeGreatsword"], LookupWeapon(new FormKey(Constants.Skyrim,        0x01399f)) },
                    { Keywords["WeapTypeMace"],       LookupWeapon(new FormKey(Constants.Skyrim,        0x013990)) },
                    { Keywords["WeapTypeSword"],      LookupWeapon(new FormKey(Constants.Skyrim,        0x0139a1)) },
                    { Keywords["WeapTypeWarAxe"],     LookupWeapon(new FormKey(Constants.Skyrim,        0x01399b)) },
                    { Keywords["WeapTypeWarhammer"],  LookupWeapon(new FormKey(Constants.Skyrim,        0x0139a2)) }
                },

                [Keywords["WeapMaterialFalmer"]] = new Dictionary<FormKey, IWeaponGetter?>()
                {
                    { Keywords["WeapTypeBattleaxe"],  null                                                         },
                    { Keywords["WeapTypeDagger"],     null                                                         },
                    { Keywords["WeapTypeGreatsword"], null                                                         },
                    { Keywords["WeapTypeMace"],       null                                                         },
                    { Keywords["WeapTypeSword"],      LookupWeapon(new FormKey(Constants.Skyrim,        0x02e6d1)) },
                    { Keywords["WeapTypeWarAxe"],     LookupWeapon(new FormKey(Constants.Skyrim,        0x0302cd)) },
                    { Keywords["WeapTypeWarhammer"],  null                                                         }
                },

                [Keywords["WeapMaterialFalmerHoned"]] = new Dictionary<FormKey, IWeaponGetter?>()
                {
                    { Keywords["WeapTypeBattleaxe"],  null                                                         },
                    { Keywords["WeapTypeDagger"],     null                                                         },
                    { Keywords["WeapTypeGreatsword"], null                                                         },
                    { Keywords["WeapTypeMace"],       null                                                         },
                    { Keywords["WeapTypeSword"],      LookupWeapon(new FormKey(Constants.Skyrim,        0x06f6ff)) },
                    { Keywords["WeapTypeWarAxe"],     LookupWeapon(new FormKey(Constants.Skyrim,        0x06f700)) },
                    { Keywords["WeapTypeWarhammer"],  null                                                         }
                },

                [Keywords["WeapMaterialGlass"]] = new Dictionary<FormKey, IWeaponGetter?>()
                {
                    { Keywords["WeapTypeBattleaxe"],  LookupWeapon(new FormKey(Constants.Skyrim,        0x0139a4)) },
                    { Keywords["WeapTypeDagger"],     LookupWeapon(new FormKey(Constants.Skyrim,        0x0139a6)) },
                    { Keywords["WeapTypeGreatsword"], LookupWeapon(new FormKey(Constants.Skyrim,        0x0139a7)) },
                    { Keywords["WeapTypeMace"],       LookupWeapon(new FormKey(Constants.Skyrim,        0x0139a8)) },
                    { Keywords["WeapTypeSword"],      LookupWeapon(new FormKey(Constants.Skyrim,        0x0139a9)) },
                    { Keywords["WeapTypeWarAxe"],     LookupWeapon(new FormKey(Constants.Skyrim,        0x0139a3)) },
                    { Keywords["WeapTypeWarhammer"],  LookupWeapon(new FormKey(Constants.Skyrim,        0x0139aa)) }
                },

                [Keywords["WeapMaterialImperial"]] = new Dictionary<FormKey, IWeaponGetter?>()
                {
                    { Keywords["WeapTypeBattleaxe"],  null                                                         },
                    { Keywords["WeapTypeDagger"],     null                                                         },
                    { Keywords["WeapTypeGreatsword"], null                                                         },
                    { Keywords["WeapTypeMace"],       null                                                         },
                    { Keywords["WeapTypeSword"],      null                                                         },
                    { Keywords["WeapTypeWarAxe"],     null                                                         },
                    { Keywords["WeapTypeWarhammer"],  null                                                         }
                },

                [Keywords["WeapMaterialIron"]] = new Dictionary<FormKey, IWeaponGetter?>()
                {
                    { Keywords["WeapTypeBattleaxe"],  LookupWeapon(new FormKey(Constants.Skyrim,        0x013980)) },
                    { Keywords["WeapTypeDagger"],     LookupWeapon(new FormKey(Constants.Skyrim,        0x01397e)) },
                    { Keywords["WeapTypeGreatsword"], LookupWeapon(new FormKey(Constants.Skyrim,        0x01359d)) },
                    { Keywords["WeapTypeMace"],       LookupWeapon(new FormKey(Constants.Skyrim,        0x013982)) },
                    { Keywords["WeapTypeSword"],      LookupWeapon(new FormKey(Constants.Skyrim,        0x012eb7)) },
                    { Keywords["WeapTypeWarAxe"],     LookupWeapon(new FormKey(Constants.Skyrim,        0x013790)) },
                    { Keywords["WeapTypeWarhammer"],  LookupWeapon(new FormKey(Constants.Skyrim,        0x013981)) }
                },

                [Keywords["WeapMaterialOrcish"]] = new Dictionary<FormKey, IWeaponGetter?>()
                {
                    { Keywords["WeapTypeBattleaxe"],  LookupWeapon(new FormKey(Constants.Skyrim,        0x01398c)) },
                    { Keywords["WeapTypeDagger"],     LookupWeapon(new FormKey(Constants.Skyrim,        0x01398e)) },
                    { Keywords["WeapTypeGreatsword"], LookupWeapon(new FormKey(Constants.Skyrim,        0x01398f)) },
                    { Keywords["WeapTypeMace"],       LookupWeapon(new FormKey(Constants.Skyrim,        0x013990)) },
                    { Keywords["WeapTypeSword"],      LookupWeapon(new FormKey(Constants.Skyrim,        0x013991)) },
                    { Keywords["WeapTypeWarAxe"],     LookupWeapon(new FormKey(Constants.Skyrim,        0x01398b)) },
                    { Keywords["WeapTypeWarhammer"],  LookupWeapon(new FormKey(Constants.Skyrim,        0x013992)) }
                },

                [Keywords["WeapMaterialSilver"]] = new Dictionary<FormKey, IWeaponGetter?>()
                {
                    { Keywords["WeapTypeBattleaxe"],  null                                                        },
                    { Keywords["WeapTypeDagger"],     null                                                        },
                    { Keywords["WeapTypeGreatsword"], LookupWeapon(new FormKey(Constants.Skyrim,        0x10c6fb)) },
                    { Keywords["WeapTypeMace"],       null                                                        },
                    { Keywords["WeapTypeSword"],      LookupWeapon(new FormKey(Constants.Skyrim,        0x10aa19)) },
                    { Keywords["WeapTypeWarAxe"],     null                                                        },
                    { Keywords["WeapTypeWarhammer"],  null                                                        }
                },

                [Keywords["WeapMaterialSteel"]] = new Dictionary<FormKey, IWeaponGetter?>()
                {
                    { Keywords["WeapTypeBattleaxe"],  LookupWeapon(new FormKey(Constants.Skyrim,        0x013984)) },
                    { Keywords["WeapTypeDagger"],     LookupWeapon(new FormKey(Constants.Skyrim,        0x013986)) },
                    { Keywords["WeapTypeGreatsword"], LookupWeapon(new FormKey(Constants.Skyrim,        0x013987)) },
                    { Keywords["WeapTypeMace"],       LookupWeapon(new FormKey(Constants.Skyrim,        0x013988)) },
                    { Keywords["WeapTypeSword"],      LookupWeapon(new FormKey(Constants.Skyrim,        0x013989)) },
                    { Keywords["WeapTypeWarAxe"],     LookupWeapon(new FormKey(Constants.Skyrim,        0x013983)) },
                    { Keywords["WeapTypeWarhammer"],  LookupWeapon(new FormKey(Constants.Skyrim,        0x01398a)) }
                },

                [Keywords["WeapMaterialWood"]] = new Dictionary<FormKey, IWeaponGetter?>()
                {
                    { Keywords["WeapTypeBattleaxe"],  null                                                         },
                    { Keywords["WeapTypeDagger"],     null                                                         },
                    { Keywords["WeapTypeGreatsword"], null                                                         },
                    { Keywords["WeapTypeMace"],       null                                                         },
                    { Keywords["WeapTypeSword"],      null                                                         },
                    { Keywords["WeapTypeWarAxe"],     null                                                         },
                    { Keywords["WeapTypeWarhammer"],  null                                                         }
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
                recipeTemplate.Perk = Perks[slot["perk"]!.ToString()];
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

        private void CacheRecipe(IConstructibleObjectGetter cobj, dynamic record, string type)
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

        private bool IsBreakdownRecipe(IConstructibleObjectGetter cobj, dynamic record)
        {
            if (cobj.WorkbenchKeyword.FormKey != Keywords["CraftingSmelter"] && cobj.WorkbenchKeyword != Keywords["CraftingTanningRack"]) return false;

            if (cobj.Items == null) return false;

            if (cobj.Items.Count != 1) return false;

            if (cobj.Items[0].Item.Item.FormKey != record.FormKey) return false;

            if (cobj.CreatedObject.FormKey == null) return false;

            return CraftingSupplies.ContainsValue((FormKey)cobj.CreatedObject.FormKey);
        }

        private bool IsCraftingRecipe(IConstructibleObjectGetter cobj, dynamic record)
        {
            if (cobj.WorkbenchKeyword.FormKey != Keywords["CraftingSmithingForge"] && cobj.WorkbenchKeyword.FormKey != Keywords["CraftingSmithingSkyforge"]) return false;

            if (cobj.CreatedObject.FormKey == null) return false;

            return cobj.CreatedObject.FormKey == record.FormKey;
        }

        private bool IsTemperingRecipe(IConstructibleObjectGetter cobj, dynamic record)
        {
            if (cobj.WorkbenchKeyword.FormKey != Keywords["CraftingSmithingSharpeningWheel"] && cobj.WorkbenchKeyword.FormKey != Keywords["CraftingSmithingArmorTable"]) return false;

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
