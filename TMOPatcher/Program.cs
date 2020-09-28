using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using System.Threading.Tasks;

namespace TMOPatcher
{
    public class Program
    {
        public static Statics? Statics { get; set; }

        public static bool ShouldNormalizeArmorStats = true;
        public static bool ShouldNormalizeWeaponStats = true;
        public static bool ShouldNormalizeRecipes = true;
        public static bool ShouldCreateMissingRecipes = true;

        public static async Task<int> Main(string[] args)
        {
            return await SynthesisPipeline.Instance.Patch<ISkyrimMod, ISkyrimModGetter>(
                args: args,
                patcher: RunPatch,
                new UserPreferences()
                {
                    IncludeDisabledMods = true,
                    ActionsForEmptyArgs = new RunDefaultPatcher()
                    {
                        IdentifyingModKey = "TMOPatch.esp",
                        TargetRelease = GameRelease.SkyrimSE
                    }
                }
            );
        }

        public static async Task RunPatch(SynthesisState<ISkyrimMod, ISkyrimModGetter> state)
        {
            Statics = new Statics(state);

            if (ShouldNormalizeArmorStats)
            {
                var patcher = new ArmorNormalizer(Statics);
                patcher.RunPatch(state);
            }

            if (ShouldNormalizeWeaponStats)
            {
                var patcher = new WeaponNormalizer(Statics);
                patcher.RunPatch(state);
            }

            if (ShouldNormalizeRecipes)
            {
                var patcher = new RecipeNormalizer(Statics);
                patcher.RunPatch(state);
            }

            if (ShouldCreateMissingRecipes)
            {
                var patcher = new RecipeCreator(Statics);
                patcher.RunPatch(state);
            }
        }
    }
}
