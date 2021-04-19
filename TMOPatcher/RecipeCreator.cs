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
        public Statics Statics { get; }
        public IPatcherState<ISkyrimMod, ISkyrimModGetter> State { get; }

        public RecipeCreator(Statics statics, IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            Statics = statics;
            State = state;
        }

        public void RunPatch()
        {
            var loadOrder = State.LoadOrder.PriorityOrder
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

        private void CreateBreakdownRecipe(IItemGetter record, RecipeTemplate template)
        {
            var cobj = State!.PatchMod.ConstructibleObjects.AddNew($"TMOPatch_BreakDown_{record.EditorID}");

            cobj.CreatedObject.SetTo(template.Items[0].Record);
            cobj.CreatedObjectCount = (ushort)template.Items[0].Count;
            cobj.WorkbenchKeyword.SetTo(template.Bench);

            cobj.Items = new ExtendedList<ContainerEntry>();
            cobj.Items.Add(new ContainerEntry()
            {
                Item = new ContainerItem()
                {
                    Count = 1,
                    Item = record.AsLink()
                }
            });

            if (!template.Perk.IsNull)
            {
                cobj.Conditions.Add(new ConditionFloat()
                {
                    ComparisonValue = 1,
                    Data = new FunctionConditionData()
                    {
                        Function = Condition.Function.HasPerk,
                        ParameterOneRecord = template.Perk.AsSetter()
                    }
                });
            }
        }

        private void CreateRecipe(IConstructibleGetter record, string type, RecipeTemplate template)
        {
            var cobj = State!.PatchMod.ConstructibleObjects.AddNew($"TMOPatch_{type}_{record.EditorID}");

            cobj.CreatedObject.SetTo(record);
            cobj.CreatedObjectCount = 1;
            cobj.WorkbenchKeyword.SetTo(template.Bench);

            cobj.Items = new ExtendedList<ContainerEntry>();
            foreach (var item in template.Items)
            {
                cobj.Items.Add(new ContainerEntry()
                {
                    Item = new ContainerItem()
                    {
                        Count = item.Count,
                        Item = item.Record.AsSetter()
                    }
                });
            }

            if (!template.Perk.IsNull)
            {
                cobj.Conditions.Add(new ConditionFloat()
                {
                    ComparisonValue = 1,
                    Data = new FunctionConditionData()
                    {
                        Function = Condition.Function.HasPerk,
                        ParameterOneRecord = template.Perk.AsSetter()
                    }
                });
            }
        }

        private bool FindRecipeTemplate<T>(T record, string type, HashSet<IFormLinkGetter<IKeywordGetter>> materials, HashSet<IFormLinkGetter<IKeywordGetter>> slots, out RecipeTemplate? recipeTemplate)
            where T : IKeywordedGetter<IKeywordGetter>, IMajorRecordCommonGetter
        {
            recipeTemplate = null;

            if (!Extensions.TryHasAnyKeyword(record, materials, out var material))
            {
                Log(record, $"RecipeCreation({type}): Unable to determine material");
                return false;
            }

            if (!Extensions.TryHasAnyKeyword(record, slots, out var slot))
            {
                Log(record, $"RecipeCreation({type}): Unable to determine slot");
                return false;
            }

            if (!Statics.RecipeTemplates.TryGetValue(material, out var types))
            {
                Log(record, $"RecipeCreation({type}): Unable to find template due to Material({material})");
                return false;
            }

            if (!types.TryGetValue(type, out var templates)
                || !templates.TryGetValue(slot, out recipeTemplate))
            {
                Log(record, $"RecipeCreation({type}): Unable to find template due to Slot({slot})");
                return false;
            }

            return true;
        }
    }
}
