using System;
using System.Collections.Generic;
using System.Linq;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.FormKeys.SkyrimSE;
using System.Threading.Tasks;
using Mutagen.Bethesda.Plugins;
using Noggog;

namespace ArtisticSkyrimOverhaulFixes
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            return await SynthesisPipeline.Instance
                .AddPatch<ISkyrimMod, ISkyrimModGetter>(RunPatch)
                .SetTypicalOpen(GameRelease.SkyrimSE, "Artistic Skyrim Overhaul - Texture Fixes.esp")
                .Run(args);
        }

        public static void RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {

            HashSet<IFormLinkGetter<ITextureSetGetter>> toRemove = new HashSet<IFormLinkGetter<ITextureSetGetter>>()
            {
                Update.TextureSet.LandscapeDirtCliffs01Mask,
                Update.TextureSet.LandscapeMountainSlab01Mask,
                Update.TextureSet.LandscapeMountainSlab02Mask
            };
            HashSet<IFormLinkGetter<ITextureSetGetter>> toReplace = new HashSet<IFormLinkGetter<ITextureSetGetter>>()
            {
                Update.TextureSet.LandscapeSnow01Landscape,
                Update.TextureSet.LandscapeSnow02Landscape
            };
            foreach (var stat in state.LoadOrder.PriorityOrder.Static().WinningOverrides())
            {

                var deepCopyStat = stat.DeepCopy();
                
                if (deepCopyStat.Model?.AlternateTextures != null)
                {
                    var alternateTextures = deepCopyStat.Model.AlternateTextures;
                    var relevantRemove = deepCopyStat.Model.AlternateTextures
                                     .Any(altTex => altTex.ContainedFormLinks
                                     .Any(link => toRemove.Contains(link)));
                    var relevantReplace = deepCopyStat.Model.AlternateTextures
                                     .Any(altTex => altTex.ContainedFormLinks
                                     .Any(link => toReplace.Contains(link)));

                    if (relevantRemove)
                    {
                        deepCopyStat.Model?.AlternateTextures?.Remove(
                            deepCopyStat.Model.AlternateTextures
                            .Where(altTex => altTex.ContainedFormLinks.Any(link => toRemove.Contains(link))));

                        state.PatchMod.Statics.Set(deepCopyStat);
                    }
                    if (relevantReplace)
                    {   
                        foreach (var texture in alternateTextures)
                        {
                            deepCopyStat.Model?.AlternateTextures?.Where(altTex => altTex.NewTexture.Equals(Update.TextureSet.LandscapeSnow01Landscape))
                                .ForEach(altTex => altTex.NewTexture = Skyrim.TextureSet.LandscapeSnow01);
                            deepCopyStat.Model?.AlternateTextures?.Where(altTex => altTex.NewTexture.Equals(Update.TextureSet.LandscapeSnow02Landscape))
                                .ForEach(altTex => altTex.NewTexture = Skyrim.TextureSet.LandscapeSnow02);
                        }
                        state.PatchMod.Statics.Set(deepCopyStat);
                    }
                }
            }
        }
    }
}
