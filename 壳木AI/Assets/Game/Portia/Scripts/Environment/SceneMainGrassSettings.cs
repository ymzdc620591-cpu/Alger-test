using UnityEngine;

namespace Game.Portia
{
    [CreateAssetMenu(fileName = "SceneMainGrassSettings", menuName = "Game/Portia/SceneMain Grass Settings")]
    public sealed class SceneMainGrassSettings : ScriptableObject
    {
        [Header("General")]
        public bool enableGrass = false;

        [Header("Patch")]
        [Min(6f)] public float patchSize = 18f;
        [Range(1, 8)] public int patchRadius = 4;
        [Range(32, 400)] public int patchDensity = 160;

        [Header("Blade Shape")]
        [Range(2, 8)] public int cardCount = 4;
        [Min(0.02f)] public float bladeHalfWidth = 0.10f;
        [Min(0.10f)] public float bladeHeight = 0.78f;
        [Range(0.01f, 1f)] public float bladeTipScale = 0.12f;

        [Header("Random Scale")]
        [Min(0.1f)] public float minScale = 0.72f;
        [Min(0.1f)] public float maxScale = 1.10f;

        [Header("Color")]
        public Color tipColor = new Color(0.66f, 0.76f, 0.41f, 1f);
        public Color rootColor = new Color(0.30f, 0.38f, 0.17f, 1f);
        [Range(0f, 0.3f)] public float colorScale = 0.04f;
        [Range(-0.2f, 0.2f)] public float colorOffset = 0f;

        [Header("Wind")]
        [Range(0f, 2f)] public float windSpeed = 0.52f;
        [Min(0.1f)] public float windSize = 11f;
        [Min(0.1f)] public float shaderHeight = 0.95f;

        [Header("Interaction")]
        [Min(0.1f)] public float interactionRadius = 2.8f;

        public void ResetToDefaults()
        {
            enableGrass = false;
            patchSize = 18f;
            patchRadius = 4;
            patchDensity = 160;
            cardCount = 4;
            bladeHalfWidth = 0.10f;
            bladeHeight = 0.78f;
            bladeTipScale = 0.12f;
            minScale = 0.72f;
            maxScale = 1.10f;
            tipColor = new Color(0.66f, 0.76f, 0.41f, 1f);
            rootColor = new Color(0.30f, 0.38f, 0.17f, 1f);
            colorScale = 0.04f;
            colorOffset = 0f;
            windSpeed = 0.52f;
            windSize = 11f;
            shaderHeight = 0.95f;
            interactionRadius = 2.8f;
        }

        public void ApplySoftLawnPreset()
        {
            patchSize = 18f;
            patchRadius = 4;
            patchDensity = 140;
            cardCount = 4;
            bladeHalfWidth = 0.085f;
            bladeHeight = 0.68f;
            bladeTipScale = 0.10f;
            minScale = 0.72f;
            maxScale = 1.02f;
            tipColor = new Color(0.69f, 0.78f, 0.45f, 1f);
            rootColor = new Color(0.34f, 0.42f, 0.20f, 1f);
            colorScale = 0.03f;
            colorOffset = 0.00f;
            windSpeed = 0.42f;
            windSize = 13f;
            shaderHeight = 0.82f;
            interactionRadius = 2.4f;
        }

        public void ApplyWildClumpPreset()
        {
            patchSize = 18f;
            patchRadius = 4;
            patchDensity = 190;
            cardCount = 5;
            bladeHalfWidth = 0.11f;
            bladeHeight = 0.92f;
            bladeTipScale = 0.16f;
            minScale = 0.78f;
            maxScale = 1.28f;
            tipColor = new Color(0.62f, 0.74f, 0.36f, 1f);
            rootColor = new Color(0.24f, 0.34f, 0.13f, 1f);
            colorScale = 0.06f;
            colorOffset = -0.01f;
            windSpeed = 0.68f;
            windSize = 9.5f;
            shaderHeight = 1.02f;
            interactionRadius = 3.0f;
        }

        void OnValidate()
        {
            patchSize = Mathf.Max(6f, patchSize);
            patchRadius = Mathf.Clamp(patchRadius, 1, 8);
            patchDensity = Mathf.Clamp(patchDensity, 32, 400);
            cardCount = Mathf.Clamp(cardCount, 2, 8);
            bladeHalfWidth = Mathf.Max(0.02f, bladeHalfWidth);
            bladeHeight = Mathf.Max(0.10f, bladeHeight);
            bladeTipScale = Mathf.Clamp(bladeTipScale, 0.01f, 1f);
            minScale = Mathf.Max(0.1f, minScale);
            maxScale = Mathf.Max(minScale, maxScale);
            windSize = Mathf.Max(0.1f, windSize);
            shaderHeight = Mathf.Max(0.1f, shaderHeight);
            interactionRadius = Mathf.Max(0.1f, interactionRadius);
        }
    }
}
