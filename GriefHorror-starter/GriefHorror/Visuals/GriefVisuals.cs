using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal; // use HighDefinition instead if you're on HDRP
using GriefHorror.Systems;

namespace GriefHorror.Visuals
{
    [RequireComponent(typeof(Volume))]
    public class GriefVisualEffects : MonoBehaviour
    {
        [Header("Grief Settings")]
        [Tooltip("Current grief (0 to 1). Overwritten by GriefMeter if one exists.")]
        [Range(0f, 1f)]
        public float currentGrief = 0f;

        [Header("Effect Intensities at Max Grief (1.0)")]
        [SerializeField] private float maxVignetteIntensity = 0.6f;
        [SerializeField] private float maxFilmGrainIntensity = 0.8f;
        [SerializeField] private float maxLensDistortion = 0.5f;
        [SerializeField] private float minSaturation = -80f; // -100 = black and white

        private Volume postProcessVolume;

        private Vignette vignette;
        private FilmGrain filmGrain;
        private LensDistortion lensDistortion;
        private ColorAdjustments colorAdjustments;

        private void Start()
        {
            postProcessVolume = GetComponent<Volume>();

            if (postProcessVolume.profile != null)
            {
                postProcessVolume.profile.TryGet(out vignette);
                postProcessVolume.profile.TryGet(out filmGrain);
                postProcessVolume.profile.TryGet(out lensDistortion);
                postProcessVolume.profile.TryGet(out colorAdjustments);
            }
            else
            {
                Debug.LogError("[GriefVisualEffects] No Volume Profile found on this object!", this);
            }
        }

        private void Update()
        {
            if (GriefMeter.Instance != null)
                currentGrief = GriefMeter.Instance.Grief;

            UpdateVisualEffects(currentGrief);
        }

        private void UpdateVisualEffects(float griefValue)
        {
            float g = Mathf.Clamp01(griefValue);

            if (vignette != null)
                vignette.intensity.Override(Mathf.Lerp(0f, maxVignetteIntensity, g));

            if (filmGrain != null)
                filmGrain.intensity.Override(Mathf.Lerp(0f, maxFilmGrainIntensity, g));

            if (lensDistortion != null)
                lensDistortion.intensity.Override(Mathf.Lerp(0f, maxLensDistortion, g));

            if (colorAdjustments != null)
                colorAdjustments.saturation.Override(Mathf.Lerp(0f, minSaturation, g));
        }
    }
}