using SiraUtil.Affinity;

namespace MenuSaberColors.Menu
{
    internal class ColorOverrideSettingsHook : IAffinity
    {
        private readonly MenuPointerColorManager pointerColorManager;

        private ColorOverrideSettingsHook(MenuPointerColorManager saberColorManager) =>
            pointerColorManager = saberColorManager;

        [AffinityPostfix]
        [AffinityPatch(typeof(ColorsOverrideSettingsPanelController),
            nameof(ColorsOverrideSettingsPanelController.HandleOverrideColorsToggleValueChanged))]
        private void HandleOverrideColorsToggleValueChanged_Postfix()
        {
            pointerColorManager.NotifyColorSchemeUpdated();
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(ColorsOverrideSettingsPanelController),
            nameof(ColorsOverrideSettingsPanelController.HandleDropDownDidSelectCellWithIdx))]
        private void HandleDropDownDidSelectCellWithIdx_PostFix()
        {
            pointerColorManager.NotifyColorSchemeUpdated();
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(ColorsOverrideSettingsPanelController),
            nameof(ColorsOverrideSettingsPanelController.HandleEditColorSchemeControllerDidFinish))]
        private void HandleEditColorSchemeControllerDidFinish()
        {
            pointerColorManager.NotifyColorSchemeUpdated();
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(MenuEnvironmentManager), nameof(MenuEnvironmentManager.ShowEnvironmentType))]
        private void ShowEnvironmentType(MenuEnvironmentManager.MenuEnvironmentType menuEnvironmentType)
        {
            if (menuEnvironmentType != MenuEnvironmentManager.MenuEnvironmentType.None &&
                menuEnvironmentType != MenuEnvironmentManager.MenuEnvironmentType.Lobby)
                pointerColorManager.RefreshMenuPointers();
        }
    }
}
