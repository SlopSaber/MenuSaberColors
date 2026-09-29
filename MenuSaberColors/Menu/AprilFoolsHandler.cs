using MenuSaberColors.Models;
using System;
using System.Collections.Generic;
using UnityEngine;
using VRUIControls;
using Zenject;

namespace MenuSaberColors.Menu
{
    internal sealed class AprilFoolsHandler : IInitializable, ITickable, IDisposable
    {
        private readonly MenuPointerColorManager menuSaberColorManager;

        private AprilFoolsHandler(MenuPointerColorManager menuSaberColorManager)
        {
            this.menuSaberColorManager = menuSaberColorManager;
        }

        private readonly ColorScheme aprilFoolsColorScheme = new(
            colorSchemeId: "memory-only_menuSaberColors_aprilFoolsColorScheme",
            colorSchemeNameLocalizationKey: "LOL",
            useNonLocalizedName: false,
            nonLocalizedName: "OWNED",
            isEditable: false,
            overrideNotes: false,
            saberAColor: Color.black,
            saberBColor: Color.black,
            overrideLights: false,
            environmentColor0: Color.black,
            environmentColor1: Color.black,
            environmentColorW: Color.black,
            supportsEnvironmentColorBoost: false,
            environmentColor0Boost: Color.black,
            environmentColor1Boost: Color.black,
            environmentColorWBoost: Color.black,
            obstaclesColor: Color.black);

        private Material? pointerMaterial;
        private readonly Dictionary<MeshRenderer, Material> originalPointerMaterials = new();

        public void Initialize()
        {
            RefreshPointerMaterials();
        }

        public void Tick()
        {
            if (originalPointerMaterials.Count < 2) RefreshPointerMaterials();
            var rainbowColor = HSBColor.ToColor(new HSBColor(Mathf.PingPong(Time.unscaledTime * 0.5f, 1f), 1f, 1f));

            aprilFoolsColorScheme._saberAColor = rainbowColor;
            aprilFoolsColorScheme._saberBColor = rainbowColor;
            if (pointerMaterial != null) pointerMaterial.color = rainbowColor;

            menuSaberColorManager.UpdateColors(aprilFoolsColorScheme);
        }

        public void Dispose()
        {
            foreach (var pair in originalPointerMaterials)
            {
                if (pair.Key != null && pair.Key.sharedMaterial == pointerMaterial)
                    pair.Key.sharedMaterial = pair.Value;
            }
            originalPointerMaterials.Clear();
            if (pointerMaterial != null) UnityEngine.Object.Destroy(pointerMaterial);
            pointerMaterial = null;
        }

        private void RefreshPointerMaterials()
        {
            foreach (var pointer in Resources.FindObjectsOfTypeAll<VRLaserPointer>())
            {
                if (pointer == null || !pointer.isActiveAndEnabled || !pointer.gameObject.scene.isLoaded) continue;
                var renderer = pointer.GetComponent<MeshRenderer>();
                if (renderer == null || renderer.sharedMaterial == null || originalPointerMaterials.ContainsKey(renderer)) continue;

                var original = renderer.sharedMaterial;
                pointerMaterial ??= new Material(original);
                originalPointerMaterials.Add(renderer, original);
                renderer.sharedMaterial = pointerMaterial;
            }
        }
    }
}
