using Mutagen.Bethesda;
using Mutagen.Bethesda.FormKeys.SkyrimSE;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using Noggog;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using static TMOPatcher.Helpers;

namespace TMOPatcher
{
    class RecipeNormalizer
    {
        public Statics Statics { get; }

        public SynthesisState<ISkyrimMod, ISkyrimModGetter> State { get; }

        public RecipeNormalizer(Statics statics, SynthesisState<ISkyrimMod, ISkyrimModGetter> state)
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

            if (!FindRecipeTemplate(armor, "breakdown", Statics.ArmorMaterials, Statics.ArmorSlots, out var recipeTemplate))
            {
                return;
            }

            NormalizeBreakdownRecipe(cobjGetter, recipeTemplate);
        }

        private void NormalizeCreationRecipesForArmors(IArmorGetter armor)
        {
            Statics.Recipes["armors"]["creation"].TryGetValue(armor.FormKey, out var cobjGetter);
            if (cobjGetter == null) return;

            if (!FindRecipeTemplate(armor, "creation", Statics.ArmorMaterials, Statics.ArmorSlots, out var recipeTemplate))
            {
                return;
            }

            NormalizeRecipe(cobjGetter, "creation", recipeTemplate);
        }

        private void NormalizeTemperingRecipesForArmors(IArmorGetter armor)
        {
            Statics.Recipes["armors"]["tempering"].TryGetValue(armor.FormKey, out var cobjGetter);
            if (cobjGetter == null) return;

            if (!FindRecipeTemplate(armor, "tempering", Statics.ArmorMaterials, Statics.ArmorSlots, out var recipeTemplate))
            {
                return;
            }

            NormalizeRecipe(cobjGetter, "tempering", recipeTemplate);
        }

        private void NormalizeBreakdownRecipesForWeapons(IWeaponGetter weapon)
        {
            Statics.Recipes["weapons"]["breakdown"].TryGetValue(weapon.FormKey, out var cobjGetter);
            if (cobjGetter == null) return;

            if (!FindRecipeTemplate(weapon, "breakdown", Statics.WeaponMaterials, Statics.WeaponTypes, out var recipeTemplate))
            {
                return;
            }

            NormalizeBreakdownRecipe(cobjGetter, recipeTemplate);
        }

        private void NormalizeCreationRecipesForWeapons(IWeaponGetter weapon)
        {
            Statics.Recipes["weapons"]["creation"].TryGetValue(weapon.FormKey, out var cobjGetter);
            if (cobjGetter == null) return;

            if (!FindRecipeTemplate(weapon, "creation", Statics.WeaponMaterials, Statics.WeaponTypes, out var recipeTemplate))
            {
                return;
            }

            NormalizeRecipe(cobjGetter, "creation", recipeTemplate);
        }

        private void NormalizeTemperingRecipesForWeapons(IWeaponGetter weapon)
        {
            Statics.Recipes["weapons"]["tempering"].TryGetValue(weapon.FormKey, out var cobjGetter);
            if (cobjGetter == null) return;

            if (!FindRecipeTemplate(weapon, "tempering", Statics.WeaponMaterials, Statics.WeaponTypes, out var recipeTemplate))
            {
                return;
            }

            NormalizeRecipe(cobjGetter, "tempering", recipeTemplate);
        }

        private bool FindRecipeTemplate<T>(T record, string type, IReadOnlyList<FormKey> materials, IReadOnlyList<FormKey> slots, [MaybeNullWhen(false)] out RecipeTemplate recipeTemplate)
            where T : IKeywordedGetter, IMajorRecordCommonGetter
        {
            recipeTemplate = null;

            if (!Extensions.HasAnyKeyword(record, materials, out var material))
            {
                Log(record, "RecipeNormalization: Unable to determine material");
                return false;
            }

            if (!Extensions.HasAnyKeyword(record, slots, out var slot))
            {
                Log(record, "RecipeNormalization: Unable to determine slot");
                return false;
            }

            if (!Statics.RecipeTemplates.TryGetValue(material, out var types))
            {
                Log(record, $"RecipeNormalization: Unable to find template due to Material({material})");
                return false;
            }

            if (!types.TryGetValue(type, out var templates)
                || !templates.TryGetValue(slot, out recipeTemplate))
            {
                Log(record, $"RecipeNormalization: Unable to find template due to Slot({slot})");
                return false;
            }

            return true;
        }

        private void NormalizeRecipe(IConstructibleObjectGetter cobjGetter, string type, RecipeTemplate template)
        {
            var cobj = State.PatchMod.ConstructibleObjects.GetOrAddAsOverride(cobjGetter);

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
                        ParameterOneRecord = Skyrim.Perk.ArcaneBlacksmith
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
            var cobj = State.PatchMod.ConstructibleObjects.GetOrAddAsOverride(cobjGetter);

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
