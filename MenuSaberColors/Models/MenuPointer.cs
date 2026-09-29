using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MenuSaberColors.Models
{
    internal class MenuPointer(IEnumerable<SetSaberGlowColor> setSaberGlowColors)
    {
        private readonly SetSaberGlowColor[] setSaberGlowColors = setSaberGlowColors.ToArray();

        public void UpdateColor(Color newColor)
        {
            foreach (var setSaberGlowColor in setSaberGlowColors)
            {
                if (setSaberGlowColor == null || !setSaberGlowColor.gameObject.activeInHierarchy ||
                    setSaberGlowColor._meshRenderer == null)
                    continue;

                var materialPropertyBlock = setSaberGlowColor._materialPropertyBlock;
                materialPropertyBlock ??= new MaterialPropertyBlock();

                foreach (var propertyTintColorPair in setSaberGlowColor._propertyTintColorPairs)
                {
                    materialPropertyBlock.SetColor(propertyTintColorPair.property, newColor * propertyTintColorPair.tintColor);
                }

                setSaberGlowColor._meshRenderer.SetPropertyBlock(materialPropertyBlock);
            }
        }
    }
}
