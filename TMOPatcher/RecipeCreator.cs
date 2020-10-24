using Mutagen.Bethesda;
using Mutagen.Bethesda.FormKeys.SkyrimSE;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using Noggog;
using System.Collections.Generic;
using System.Linq;
using static TMOPatcher.Helpers;

namespace TMOPatcher
{
    class RecipeCreator
    {
        public Statics Statics { get; set; }

        public SynthesisState<ISkyrimMod, ISkyrimModGetter>? State { get; set; }

        public RecipeCreator(Statics statics)
        {
            Statics = statics;
        }

        public void RunPatch(SynthesisState<ISkyrimMod, ISkyrimModGetter> state)
        {
            State = state;
            var loadOrder = state.LoadOrder.PriorityOrder
                .OnlyEnabled()
                .Where(modGetter => !Statics.ExcludedMods.Contains(modGetter.ModKey));

            foreach (var armor in loadOrder.WinningOverrides<IArmorGetter>())
            {
                if (!armor.TemplateArmor.IsNull) continue;
                if (armor.BodyTemplate == null) continue;
                if (armor.BodyTemplate.ArmorType == ArmorType.Clothing) continue;
                if (armor.BodyTemplate.Flags.HasFlag(BodyTemplate.Flag.NonPlayable)) continue;
                if (armor.HasKeyword(Skyrim.Keyword.ArmorClothing)) continue;
                if (armor.HasKeyword(Skyrim.Keyword.ArmorJewelry)) continue;
                
                CreateMissingRecipesForArmors(armor);
            }

            foreach (var weapon in loadOrder.WinningOverrides<IWeaponGetter>())
            {
                if (!weapon.Template.IsNull) continue;
                if (weapon.MajorFlags.HasFlag(Weapon.MajorFlag.NonPlayable)) continue;
                CreateMissingRecipesForWeapons(weapon);
            }
        }

        private void CreateMissingRecipesForArmors(IArmorGetter armor)
        {
            if (!Statics.Recipes["armors"]["breakdown"].TryGetValue(armor.FormKey, out var cobjGetter) || cobjGetter == null) {
                if (armor.HasKeyword(Skyrim.Keyword.MagicDisallowEnchanting)) return;

                if (!FindRecipeTemplate(armor, "breakdown", Statics.ArmorMaterials, Statics.ArmorSlots, out var recipeTemplate) || recipeTemplate == null) {
                    return;
                }

                CreateBreakdownRecipe(armor, recipeTemplate);
            }

            if (!Statics.Recipes["armors"]["creation"].TryGetValue(armor.FormKey, out cobjGetter) || cobjGetter == null)
            {
                if (armor.HasKeyword(Skyrim.Keyword.MagicDisallowEnchanting)) return;

                if (!FindRecipeTemplate(armor, "creation", Statics.ArmorMaterials, Statics.ArmorSlots, out var recipeTemplate) || recipeTemplate == null)
                {
                    return;
                }

                CreateRecipe(armor, "Creation", recipeTemplate);
            }

            if (!Statics.Recipes["armors"]["tempering"].TryGetValue(armor.FormKey, out cobjGetter) || cobjGetter == null)
            {
                if (!FindRecipeTemplate(armor, "tempering", Statics.ArmorMaterials, Statics.ArmorSlots, out var recipeTemplate) || recipeTemplate == null)
                {
                    return;
                }

                CreateRecipe(armor, "Tempering", recipeTemplate);
            }
        }

        private void CreateMissingRecipesForWeapons(IWeaponGetter weapon)
        {
            if (!Statics.Recipes["weapons"]["breakdown"].TryGetValue(weapon.FormKey, out var cobjGetter) || cobjGetter == null)
            {
                if (weapon.HasKeyword(Skyrim.Keyword.MagicDisallowEnchanting)) return;

                if (!FindRecipeTemplate(weapon, "breakdown", Statics.ArmorMaterials, Statics.WeaponTypes, out var recipeTemplate) || recipeTemplate == null)
                {
                    return;
                }

                CreateBreakdownRecipe(weapon, recipeTemplate);
            }

            if (!Statics.Recipes["weapons"]["creation"].TryGetValue(weapon.FormKey, out cobjGetter) || cobjGetter == null)
            {
                if (weapon.HasKeyword(Skyrim.Keyword.MagicDisallowEnchanting)) return;

                if (!FindRecipeTemplate(weapon, "creation", Statics.WeaponMaterials, Statics.WeaponTypes, out var recipeTemplate) || recipeTemplate == null)
                {
                    return;
                }

                CreateRecipe(weapon, "Creation", recipeTemplate);
            }

            if (!Statics.Recipes["weapons"]["tempering"].TryGetValue(weapon.FormKey, out cobjGetter) || cobjGetter == null)
            {
                if (!FindRecipeTemplate(weapon, "tempering", Statics.WeaponMaterials, Statics.WeaponTypes, out var recipeTemplate) || recipeTemplate == null)
                {
                    return;
                }

                CreateRecipe(weapon, "Tempering", recipeTemplate);
            }
        }

        private void CreateBreakdownRecipe(dynamic record, RecipeTemplate template)
        {
            var cobj = State!.PatchMod.ConstructibleObjects.AddNew($"TMOPatch_BreakDown_{record.EditorID}");

            cobj.CreatedObject = template.Items[0].Record;
            cobj.CreatedObjectCount = (ushort)template.Items[0].Count;
            cobj.WorkbenchKeyword = template.Bench;

            cobj.Items = new ExtendedList<ContainerEntry>();
            cobj.Items.Add(new ContainerEntry()
            {
                Item = new ContainerItem()
                {
                    Count = 1,
                    Item = record.FormKey
                }
            });

            if (template.Perk != null)
            {
                cobj.Conditions.Add(new ConditionFloat()
                {
                    ComparisonValue = 1,
                    Data = new FunctionConditionData()
                    {
                        Function = (ushort)ConditionData.Function.HasPerk,
                        ParameterOneRecord = (FormKey)template.Perk
                    }
                });
            }
        }

        private void CreateRecipe(dynamic record, string type, RecipeTemplate template)
        {
            var cobj = State!.PatchMod.ConstructibleObjects.AddNew($"TMOPatch_{type}_{record.EditorID}");

            cobj.CreatedObject = record.FormKey;
            cobj.CreatedObjectCount = 1;
            cobj.WorkbenchKeyword = template.Bench;

            cobj.Items = new ExtendedList<ContainerEntry>();
            foreach (var item in template.Items)
            {
                cobj.Items.Add(new ContainerEntry()
                {
                    Item = new ContainerItem()
                    {
                        Count = item.Count,
                        Item = item.Record
                    }
                });
            }

            if (template.Perk != null)
            {
                cobj.Conditions.Add(new ConditionFloat()
                {
                    ComparisonValue = 1,
                    Data = new FunctionConditionData()
                    {
                        Function = (ushort)ConditionData.Function.HasPerk,
                        ParameterOneRecord = (FormKey)template.Perk
                    }
                });
            }
        }

        private bool FindRecipeTemplate(IKeywordedGetter record, string type, IReadOnlyList<FormKey> materials, IReadOnlyList<FormKey> slots, out RecipeTemplate? recipeTemplate)
        {
            recipeTemplate = null;

            if (!Extensions.HasAnyKeyword(record, materials, out var material))
            {
                Log(record, $"RecipeCreation({type}): Unable to determine material");
                return false;
            }

            if (!Extensions.HasAnyKeyword(record, slots, out var slot))
            {
                Log(record, $"RecipeCreation({type}): Unable to determine slot");
                return false;
            }

            if (!Statics.RecipeTemplates.TryGetValue(material, out var types))
            {
                Log(record, $"RecipeCreation({type}): Unable to find template due to Material({material})");
                return false;
            }

            if (!types[type].TryGetValue(slot, out recipeTemplate) || recipeTemplate == null)
            {
                Log(record, $"RecipeCreation({type}): Unable to find template due to Slot({slot})");
                return false;
            }

            return true;
        }
    }
}
