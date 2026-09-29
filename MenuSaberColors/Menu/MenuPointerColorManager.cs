using System.Threading.Tasks;
using System;
using Zenject;

namespace MenuSaberColors.Menu
{
    internal sealed class MenuPointerColorManager : IInitializable, IDisposable
    {
        private readonly ColorSchemesSettings colorSchemesSettings;
        private readonly MenuPointerProvider pointerProvider;

        private readonly ColorScheme defaultColorScheme;

        private MenuPointerColorManager(
            PlayerDataModel playerDataModel,
            MenuPointerProvider pointerProvider)
        {
            colorSchemesSettings = playerDataModel.playerData.colorSchemesSettings;
            this.pointerProvider = pointerProvider;

            defaultColorScheme = colorSchemesSettings.GetColorSchemeForId("TheFirst");
        }

        public void Initialize()
        {
            pointerProvider.PointersReady += NotifyColorSchemeUpdated;
            NotifyColorSchemeUpdated();
        }

        public void Dispose() => pointerProvider.PointersReady -= NotifyColorSchemeUpdated;

        public void RefreshMenuPointers() => pointerProvider.RefreshPointers();

        public void NotifyColorSchemeUpdated()
        {
            var colorScheme = colorSchemesSettings.overrideDefaultColors
                ? colorSchemesSettings.GetSelectedColorScheme()
                : defaultColorScheme;
            if (colorScheme != null) UpdateColors(colorScheme);
        }

        public void UpdateColors(ColorScheme colorScheme)
        {
            pointerProvider.LeftPointer?.UpdateColor(colorScheme.saberAColor);
            pointerProvider.RightPointer?.UpdateColor(colorScheme.saberBColor);
        }
    }
}
