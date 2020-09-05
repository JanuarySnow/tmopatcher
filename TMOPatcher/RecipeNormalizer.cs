using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using Noggog;
using System.Collections.Generic;
using System.Linq;
using static TMOPatcher.Helpers;

namespace TMOPatcher
{
    class RecipeNormalizer
    {
        public Statics Statics { get; set; }

        public SynthesisState<ISkyrimMod, ISkyrimModGetter>? State { get; set; }

        public RecipeNormalizer(Statics statics)
        {
            Statics = statics;
        }

        public void RunPatch(SynthesisState<ISkyrimMod, ISkyrimModGetter> state)
        {
            State = state;
            ModKey[] excludedMods = { "Skyrim.esm", "Update.esm", "Dawnguard.esm", "HearthFires.esm", "Dragonborn.esm", "Unofficial Skyrim Special Edition Patch.esp" };
            var loadOrder = state.LoadOrder.PriorityOrder.Where(modGetter => !excludedMods.Contains(modGetter.ModKey));

            foreach (var armor in loadOrder.WinningOverrides<IArmorGetter>())
            {
                NormalizeBreakdownRecipesForArmors(armor);
                NormalizeCreationRecipesForArmors(armor);
                NormalizeTemperingRecipesForArmors(armor);
            }

            foreach (var weapon in loadOrder.WinningOverrides<IWeaponGetter>())
            {
                NormalizeBreakdownRecipesForWeapons(weapon);
                NormalizeCreationRecipesForWeapons(weapon);
                NormalizeTemperingRecipesForWeapons(weapon);
            }
        }

        private void NormalizeBreakdownRecipesForArmors(IArmorGetter armor)
        {
            Statics.Recipes["armors"]["breakdown"].TryGetValue(armor.FormKey, out var cobjGetter);
            if (cobjGetter == null) return;

            if (!FindRecipeTemplate(armor, "breakdown", Statics.ArmorMaterials, Statics.ArmorSlots, out var recipeTemplate) || recipeTemplate == null)
            {
                return;
            }

            NormalizeBreakdownRecipe(cobjGetter, recipeTemplate);
        }

        private void NormalizeCreationRecipesForArmors(IArmorGetter armor)
        {
            Statics.Recipes["armors"]["creation"].TryGetValue(armor.FormKey, out var cobjGetter);
            if (cobjGetter == null) return;

            if (!FindRecipeTemplate(armor, "creation", Statics.ArmorMaterials, Statics.ArmorSlots, out var recipeTemplate) || recipeTemplate == null)
            {
                return;
            }

            NormalizeRecipe(cobjGetter, "creation", recipeTemplate);
        }

        private void NormalizeTemperingRecipesForArmors(IArmorGetter armor)
        {
            Statics.Recipes["armors"]["tempering"].TryGetValue(armor.FormKey, out var cobjGetter);
            if (cobjGetter == null) return;

            if (!FindRecipeTemplate(armor, "tempering", Statics.ArmorMaterials, Statics.ArmorSlots, out var recipeTemplate) || recipeTemplate == null)
            {
                return;
            }

            NormalizeRecipe(cobjGetter, "tempering", recipeTemplate);
        }

        private void NormalizeBreakdownRecipesForWeapons(IWeaponGetter weapon)
        {
            Statics.Recipes["weapons"]["breakdown"].TryGetValue(weapon.FormKey, out var cobjGetter);
            if (cobjGetter == null) return;

            if (!FindRecipeTemplate(weapon, "breakdown", Statics.WeaponMaterials, Statics.WeaponTypes, out var recipeTemplate) || recipeTemplate == null)
            {
                return;
            }

            NormalizeBreakdownRecipe(cobjGetter, recipeTemplate);
        }

        private void NormalizeCreationRecipesForWeapons(IWeaponGetter weapon)
        {
            Statics.Recipes["weapons"]["creation"].TryGetValue(weapon.FormKey, out var cobjGetter);
            if (cobjGetter == null) return;

            if (!FindRecipeTemplate(weapon, "creation", Statics.WeaponMaterials, Statics.WeaponTypes, out var recipeTemplate) || recipeTemplate == null)
            {
                return;
            }

            NormalizeRecipe(cobjGetter, "creation", recipeTemplate);
        }

        private void NormalizeTemperingRecipesForWeapons(IWeaponGetter weapon)
        {
            Statics.Recipes["weapons"]["tempering"].TryGetValue(weapon.FormKey, out var cobjGetter);
            if (cobjGetter == null) return;

            if (!FindRecipeTemplate(weapon, "tempering", Statics.WeaponMaterials, Statics.WeaponTypes, out var recipeTemplate) || recipeTemplate == null)
            {
                return;
            }

            NormalizeRecipe(cobjGetter, "tempering", recipeTemplate);
        }

        private bool FindRecipeTemplate(dynamic record, string type, IReadOnlyList<FormKey> materials, IReadOnlyList<FormKey> slots, out RecipeTemplate? recipeTemplate)
        {
            recipeTemplate = null;

            if (!Extensions.HasAnyKeyword(record, materials, out FormKey? material) || material == null)
            {
                Log(record, "RecipeNormalization: Unable to determine material");
                return false;
            }

            if (!Extensions.HasAnyKeyword(record, slots, out FormKey? slot) || slot == null)
            {
                Log(record, "RecipeNormalization: Unable to determine slot");
                return false;
            }

            if (!Statics.RecipeTemplates.TryGetValue((FormKey)material!, out var types))
            {
                Log(record, $"RecipeNormalization: Unable to find template due to Material({material})");
                return false;
            }

            if (!types[type].TryGetValue((FormKey)slot!, out recipeTemplate) || recipeTemplate == null)
            {
                Log(record, $"RecipeNormalization: Unable to find template due to Slot({slot})");
                return false;
            }

            return true;
        }

        private void NormalizeRecipe(IConstructibleObjectGetter cobjGetter, string type, RecipeTemplate template)
        {
            var cobj = State!.PatchMod.ConstructibleObjects.GetOrAddAsOverride(cobjGetter);

            cobj.WorkbenchKeyword = template.Bench;
            cobj.CreatedObjectCount = 1;

            if (cobj.Items == null)
            {
                cobj.Items = new ExtendedList<ContainerEntry>();
            }

            cobj.Items.Clear();

            foreach (var item in template.Items) {
                cobj.Items.Add(new ContainerEntry()
                {
                    Item = new ContainerItem()
                    {
                        Count = item.Count,
                        Item = item.Record
                    }
                });
            }

            cobj.Conditions.Clear();

            if (type == "tempering")
            {
                cobj.Conditions.Add(new ConditionFloat()
                {
                    CompareOperator = CompareOperator.NotEqualTo,
                    ComparisonValue = 1,
                    Flags = Condition.Flag.OR,
                    Data = new FunctionConditionData()
                    {
                        Function = (ushort)ConditionData.Function.EPTemperingItemIsEnchanted
                    }
                });

                cobj.Conditions.Add(new ConditionFloat()
                {
                    ComparisonValue = 1,
                    Data = new FunctionConditionData()
                    {
                        Function = (ushort)ConditionData.Function.HasPerk,
                        ParameterOneRecord = Statics.Perks["ArcaneBlacksmith"]
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

        private void NormalizeBreakdownRecipe(IConstructibleObjectGetter cobjGetter, RecipeTemplate template)
        {
            var cobj = State!.PatchMod.ConstructibleObjects.GetOrAddAsOverride(cobjGetter);

            cobj.WorkbenchKeyword = template.Bench;
            cobj.CreatedObject = template.Items[0].Record;
            cobj.CreatedObjectCount = (ushort)template.Items[0].Count;

            cobj.Conditions.Clear();

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
    }
}
