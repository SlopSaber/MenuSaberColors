using IPA.Utilities;
using SiraUtil.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.XR;
using VRUIControls;
using Zenject;

namespace MenuSaberColors
{
    public class MenuSaberColorManager : MonoBehaviour
    {
        private SiraLog logger = null!;

        public MethodInfo setColorsMethod = typeof(SetSaberGlowColor).GetMethod("SetColors");
        public List<SetSaberGlowColor> leftSideSabers = new List<SetSaberGlowColor>();
        public List<SetSaberGlowColor> rightSideSabers = new List<SetSaberGlowColor>();
        internal event Action SabersRefreshed;

        private ColorSchemesSettings playerColorSchemesSettings;
        private Coroutine pendingRefresh;
        private bool loggedRefreshError;

        [Inject]
        internal void Construct(PlayerDataModel dataModel, SiraLog logger)
        {
            this.logger = logger;
            playerColorSchemesSettings = dataModel.playerData.colorSchemesSettings;
            Patches.ShouldSaberColorsBeUpdated += this.RefreshData;
        }

        private void Start()
        {
            RefreshData(MenuEnvironmentManager.MenuEnvironmentType.Default);
        }

        private void OnDestroy()
        {
            Patches.ShouldSaberColorsBeUpdated -= RefreshData;
        }

        public void RefreshData(MenuEnvironmentManager.MenuEnvironmentType menuEnvironmentType)
        {
            if (menuEnvironmentType == MenuEnvironmentManager.MenuEnvironmentType.None)
            {
                if (pendingRefresh != null)
                {
                    StopCoroutine(pendingRefresh);
                    pendingRefresh = null;
                }
                return;
            }

            if (TryRefreshData())
            {
                if (pendingRefresh != null)
                {
                    StopCoroutine(pendingRefresh);
                    pendingRefresh = null;
                }
            }
            else if (pendingRefresh == null)
            {
                pendingRefresh = StartCoroutine(RefreshWhenReady());
            }
        }

        private IEnumerator RefreshWhenReady()
        {
            var wait = new WaitForSeconds(0.2f);
            for (int attempt = 0; attempt < 50; attempt++)
            {
                yield return wait;
                if (TryRefreshData())
                {
                    pendingRefresh = null;
                    yield break;
                }
            }
            pendingRefresh = null;
        }

        private bool TryRefreshData()
        {
            try
            {
                var controllers = Resources.FindObjectsOfTypeAll<VRController>()
                    .Where(controller => controller != null && controller.isActiveAndEnabled && controller.gameObject.scene.isLoaded);
                var left = controllers.FirstOrDefault(controller => controller.node == XRNode.LeftHand &&
                    controller.GetComponentInChildren<SetSaberGlowColor>(true) != null);
                var right = controllers.FirstOrDefault(controller => controller.node == XRNode.RightHand &&
                    controller.GetComponentInChildren<SetSaberGlowColor>(true) != null);
                if (left == null || right == null) return false;

                var leftGlows = left.GetComponentsInChildren<SetSaberGlowColor>(true);
                var rightGlows = right.GetComponentsInChildren<SetSaberGlowColor>(true);
                if (leftGlows.Length == 0 || rightGlows.Length == 0 ||
                    leftGlows.Any(glow => glow.GetField<ColorManager, SetSaberGlowColor>("_colorManager") == null) ||
                    rightGlows.Any(glow => glow.GetField<ColorManager, SetSaberGlowColor>("_colorManager") == null))
                    return false;

                leftSideSabers.Clear();
                leftSideSabers.AddRange(leftGlows);
                rightSideSabers.Clear();
                rightSideSabers.AddRange(rightGlows);
                UpdateSaberColors();
                SabersRefreshed?.Invoke();
                return true;
            }
            catch (Exception ex)
            {
                if (!loggedRefreshError)
                {
                    logger.Error("Error occurred while refreshing saber data.\n" + ex);
                    loggedRefreshError = true;
                }
                return false;
            }
        }

        private void UpdateSaberColors()
        {
            if (Plugin.IsAprilFools) return;

            ColorScheme colorScheme = playerColorSchemesSettings.overrideDefaultColors ? playerColorSchemesSettings.GetSelectedColorScheme() : playerColorSchemesSettings.GetColorSchemeForId("TheFirst");

            foreach (SetSaberGlowColor obj in leftSideSabers)
            {
                ColorManager colorManager = obj?.GetField<ColorManager, SetSaberGlowColor>("_colorManager");
                colorManager?.SetField("_colorScheme", colorScheme);
                if (obj != null && colorManager != null) obj.SetColors();
            }

            foreach (SetSaberGlowColor obj in rightSideSabers)
            {
                ColorManager colorManager = obj?.GetField<ColorManager, SetSaberGlowColor>("_colorManager");
                colorManager?.SetField("_colorScheme", colorScheme);
                if (obj != null && colorManager != null) obj.SetColors();
            }
        }
    }

    internal class AprilFools : MonoBehaviour
    {
        public struct HSBColor
        {
            public float h;

            public float s;

            public float b;

            public float a;

            public HSBColor(float h, float s, float b)
            {
                this.h = h;
                this.s = s;
                this.b = b;
                a = 1f;
            }

            public static Color ToColor(HSBColor hsbColor)
            {
                float r = hsbColor.b;
                float g = hsbColor.b;
                float b = hsbColor.b;
                if (hsbColor.s != 0f)
                {
                    float max = hsbColor.b;
                    float dif = hsbColor.b * hsbColor.s;
                    float min = hsbColor.b - dif;
                    float h = hsbColor.h * 360f;
                    if (h < 60f)
                    {
                        r = max;
                        g = h * dif / 60f + min;
                        b = min;
                    }
                    else if (h < 120f)
                    {
                        r = (0f - (h - 120f)) * dif / 60f + min;
                        g = max;
                        b = min;
                    }
                    else if (h < 180f)
                    {
                        r = min;
                        g = max;
                        b = (h - 120f) * dif / 60f + min;
                    }
                    else if (h < 240f)
                    {
                        r = min;
                        g = (0f - (h - 240f)) * dif / 60f + min;
                        b = max;
                    }
                    else if (h < 300f)
                    {
                        r = (h - 240f) * dif / 60f + min;
                        g = min;
                        b = max;
                    }
                    else if (h <= 360f)
                    {
                        r = max;
                        g = min;
                        b = (0f - (h - 360f)) * dif / 60f + min;
                    }
                    else
                    {
                        r = 0f;
                        g = 0f;
                        b = 0f;
                    }
                }
                return new Color(Mathf.Clamp01(r), Mathf.Clamp01(g), Mathf.Clamp01(b), hsbColor.a);
            }
        }

        [Inject] private readonly MenuSaberColorManager colorManager = null!;
        private readonly FieldAccessor<ColorScheme, Color>.Accessor saberAColorAccessor = FieldAccessor<ColorScheme, Color>.GetAccessor("_saberAColor");
        private readonly FieldAccessor<ColorScheme, Color>.Accessor saberBColorAccessor = FieldAccessor<ColorScheme, Color>.GetAccessor("_saberBColor");
        private ColorScheme aprilFools = null;
        private Material pointerMaterial;
        private readonly Dictionary<ColorManager, ColorScheme> originalSchemes = new Dictionary<ColorManager, ColorScheme>();
        private readonly Dictionary<MeshRenderer, Material> originalPointerMaterials = new Dictionary<MeshRenderer, Material>();

        public void Start()
        {
            colorManager.SabersRefreshed += RefreshSabers;
            RefreshSabers();
        }

        private void RefreshSabers()
        {
            foreach (SetSaberGlowColor glow in colorManager.leftSideSabers) ApplyScheme(glow);
            foreach (SetSaberGlowColor glow in colorManager.rightSideSabers) ApplyScheme(glow);
            if (aprilFools == null) return;

            foreach (VRLaserPointer pointer in Resources.FindObjectsOfTypeAll<VRLaserPointer>())
            {
                if (pointer == null || !pointer.isActiveAndEnabled || !pointer.gameObject.scene.isLoaded) continue;
                MeshRenderer renderer = pointer.GetComponent<MeshRenderer>();
                if (renderer == null || renderer.sharedMaterial == null || originalPointerMaterials.ContainsKey(renderer)) continue;
                originalPointerMaterials.Add(renderer, renderer.sharedMaterial);
                if (pointerMaterial == null) pointerMaterial = new Material(renderer.sharedMaterial);
                renderer.sharedMaterial = pointerMaterial;
            }
        }

        private void ApplyScheme(SetSaberGlowColor glow)
        {
            if (glow == null) return;
            ColorManager manager = glow.GetField<ColorManager, SetSaberGlowColor>("_colorManager");
            if (manager == null || originalSchemes.ContainsKey(manager)) return;
            ColorScheme original = manager.GetField<ColorScheme, ColorManager>("_colorScheme");
            if (original == null) return;
            if (aprilFools == null)
                aprilFools = new ColorScheme(original, original.overrideNotes, original.saberAColor, original.saberBColor,
                    original.overrideLights, original.environmentColor0, original.environmentColor1, original.environmentColorW,
                    original.supportsEnvironmentColorBoost, original.environmentColor0Boost, original.environmentColor1Boost,
                    original.environmentColorWBoost, original.obstaclesColor);
            originalSchemes.Add(manager, original);
            manager.SetField("_colorScheme", aprilFools);
        }

        private void OnDestroy()
        {
            if (colorManager != null) colorManager.SabersRefreshed -= RefreshSabers;
            foreach (var pair in originalSchemes)
            {
                if (ReferenceEquals(pair.Key.GetField<ColorScheme, ColorManager>("_colorScheme"), aprilFools))
                    pair.Key.SetField("_colorScheme", pair.Value);
            }
            foreach (var pair in originalPointerMaterials)
            {
                if (pair.Key != null && pair.Key.sharedMaterial == pointerMaterial)
                    pair.Key.sharedMaterial = pair.Value;
            }
            if (pointerMaterial != null) Destroy(pointerMaterial);
        }

        public void Update()
        {
            if (aprilFools != null)
            {
                Color rainbow = HSBColor.ToColor(new HSBColor(Mathf.PingPong(Time.time * 0.5f, 1f), 1f, 1f));

                saberAColorAccessor(ref aprilFools) = rainbow;
                saberBColorAccessor(ref aprilFools) = rainbow;
                if (pointerMaterial != null) pointerMaterial.color = rainbow;

                foreach (SetSaberGlowColor obj in colorManager.leftSideSabers)
                {
                    if (obj != null && obj.gameObject.activeInHierarchy) obj.SetColors();
                }

                foreach (SetSaberGlowColor obj in colorManager.rightSideSabers)
                {
                    if (obj != null && obj.gameObject.activeInHierarchy) obj.SetColors();
                }
            }
        }
    }
}
