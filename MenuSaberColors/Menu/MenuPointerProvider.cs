using MenuSaberColors.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using Zenject;

namespace MenuSaberColors.Menu
{
    internal sealed class MenuPointerProvider : IInitializable, ITickable
    {
        private const int MaxAttempts = 50;
        private const float RetrySeconds = 0.2f;
        private int attempts;
        private float nextAttempt;
        private readonly List<XRDisplaySubsystem> displays = new();

        public MenuPointer? LeftPointer { get; private set; }
        public MenuPointer? RightPointer { get; private set; }
        public event Action? PointersReady;

        public void Initialize() => RefreshPointers();

        public void RefreshPointers()
        {
            LeftPointer = null;
            RightPointer = null;
            attempts = 0;
            nextAttempt = Time.unscaledTime + RetrySeconds;
            TryFindPointers();
        }

        public void Tick()
        {
            if (LeftPointer != null && RightPointer != null || attempts >= MaxAttempts || Time.unscaledTime < nextAttempt)
                return;

            nextAttempt = Time.unscaledTime + RetrySeconds;
            TryFindPointers();
        }

        private void TryFindPointers()
        {
            displays.Clear();
            SubsystemManager.GetSubsystems(displays);
            var displayRunning = false;
            foreach (var display in displays)
            {
                if (display == null || !display.running) continue;
                displayRunning = true;
                break;
            }
            if (!displayRunning) return;

            attempts++;
            var controllers = Resources.FindObjectsOfTypeAll<VRController>()
                .Where(controller => controller != null && controller.isActiveAndEnabled && controller.gameObject.scene.isLoaded)
                .ToArray();
            var left = controllers.FirstOrDefault(controller => controller.node == XRNode.LeftHand);
            var right = controllers.FirstOrDefault(controller => controller.node == XRNode.RightHand);
            if (left == null || right == null) return;

            var leftGlows = left.GetComponentsInChildren<SetSaberGlowColor>(true)
                .Where(glow => glow != null && glow.gameObject.activeInHierarchy).ToArray();
            var rightGlows = right.GetComponentsInChildren<SetSaberGlowColor>(true)
                .Where(glow => glow != null && glow.gameObject.activeInHierarchy).ToArray();
            if (leftGlows.Length == 0 || rightGlows.Length == 0 ||
                leftGlows.Any(glow => glow._meshRenderer == null || glow._propertyTintColorPairs == null) ||
                rightGlows.Any(glow => glow._meshRenderer == null || glow._propertyTintColorPairs == null))
                return;

            LeftPointer = new MenuPointer(leftGlows);
            RightPointer = new MenuPointer(rightGlows);
            PointersReady?.Invoke();
        }
    }
}
