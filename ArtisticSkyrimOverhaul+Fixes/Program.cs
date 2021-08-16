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
                .SetTypicalOpen(GameRelease.SkyrimSE, "Artistic Skyrim Overhaul - Exterior Fixes.esp")
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

                var patchedStatic = stat.DeepCopy();
                if (patchedStatic.Flags.HasFlag(Static.Flag.ConsideredSnow))
                {
                    patchedStatic.Flags = patchedStatic.Flags.SetFlag(Static.Flag.ConsideredSnow,false);
                    state.PatchMod.Statics.Set(patchedStatic);
                }

                if (patchedStatic.Model?.AlternateTextures != null)
                {
                    var alternateTextures = patchedStatic.Model.AlternateTextures;
                    var relevantRemove = patchedStatic.Model.AlternateTextures
                                     .Any(altTex => altTex.ContainedFormLinks
                                     .Any(link => toRemove.Contains(link)));
                    var relevantReplace = patchedStatic.Model.AlternateTextures
                                     .Any(altTex => altTex.ContainedFormLinks
                                     .Any(link => toReplace.Contains(link)));

                    if (relevantRemove)
                    {
                        patchedStatic.Model?.AlternateTextures?.Remove(
                            patchedStatic.Model.AlternateTextures
                            .Where(altTex => altTex.ContainedFormLinks.Any(link => toRemove.Contains(link))));
                        state.PatchMod.Statics.Set(patchedStatic);
                    }
                    if (relevantReplace)
                    {
                        foreach (var texture in alternateTextures)
                        {
                            patchedStatic.Model?.AlternateTextures?.Where(altTex => altTex.NewTexture.Equals(Update.TextureSet.LandscapeSnow01Landscape))
                                .ForEach(altTex => altTex.NewTexture = Skyrim.TextureSet.LandscapeSnow01);
                            patchedStatic.Model?.AlternateTextures?.Where(altTex => altTex.NewTexture.Equals(Update.TextureSet.LandscapeSnow02Landscape))
                                .ForEach(altTex => altTex.NewTexture = Skyrim.TextureSet.LandscapeSnow02);
                        }
                        state.PatchMod.Statics.Set(patchedStatic);
                    }
                }
            }

            foreach (var textureSet in state.LoadOrder.PriorityOrder.TextureSet().WinningOverrides())
            {
                var patchedTextureSet = textureSet.DeepCopy();
                if (patchedTextureSet.NormalOrGloss != null
                && (patchedTextureSet.NormalOrGloss.StartsWith("Landscape")
                || patchedTextureSet.NormalOrGloss.StartsWith("landscape")
                || patchedTextureSet.NormalOrGloss.StartsWith("Architecture")
                || patchedTextureSet.NormalOrGloss.StartsWith("architecture")))
                {
                    patchedTextureSet.NormalOrGloss = "normalmap\\normal_n.dds";
                    state.PatchMod.TextureSets.Set(patchedTextureSet);
                }
            }

            foreach (var material in state.LoadOrder.PriorityOrder.MaterialObject().WinningOverrides())
            {
                var patchedMaterial = material.DeepCopy();
                if (patchedMaterial.HasSnow)
                {
                    patchedMaterial.HasSnow = false;
                    state.PatchMod.MaterialObjects.Set(patchedMaterial);
                }
            }

            foreach (var landscapeTexture in state.LoadOrder.PriorityOrder.LandscapeTexture().WinningOverrides())
            {
                var patchedLandscapeTexture = landscapeTexture.DeepCopy();
                if (patchedLandscapeTexture.Flags != null && patchedLandscapeTexture.Flags.Value.HasFlag(LandscapeTexture.Flag.IsSnow))
                {
                    patchedLandscapeTexture.Flags = patchedLandscapeTexture.Flags.Value.SetFlag(LandscapeTexture.Flag.IsSnow, false);
                    state.PatchMod.LandscapeTextures.Set(patchedLandscapeTexture);
                }
            }
        }
    }
}
