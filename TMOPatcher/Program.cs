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
            return await SynthesisPipeline.Instance
                .AddPatch<ISkyrimMod, ISkyrimModGetter>(RunPatch, new PatcherPreferences()
                {
                    IncludeDisabledMods = true
                })
                .Run(args, new RunPreferences()
                {
                    ActionsForEmptyArgs = new RunDefaultPatcher()
                    {
                        IdentifyingModKey = "TMOPatch.esp",
                        TargetRelease = GameRelease.SkyrimSE
                    }
                });
        }

        public static async Task RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            Statics = new Statics(state);

            if (ShouldNormalizeArmorStats)
            {
                new ArmorNormalizer(Statics, state)
                    .RunPatch();
            }

            if (ShouldNormalizeWeaponStats)
            {
                new WeaponNormalizer(Statics, state)
                    .RunPatch();
            }

            if (ShouldNormalizeRecipes)
            {
                new RecipeNormalizer(Statics, state)
                    .RunPatch();
            }

            if (ShouldCreateMissingRecipes)
            {
                new RecipeCreator(Statics, state)
                    .RunPatch();
            }
        }
    }
}
