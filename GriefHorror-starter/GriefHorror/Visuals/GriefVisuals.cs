using UnityEngine;
using UnityEngine.Rendering; // Wajib untuk akses Volume Profile

namespace GriefHorror.Visuals
{
    [RequireComponent(typeof(Volume))] // Memastikan objek ini punya komponen Volume
    public class GriefVisualEffects : MonoBehaviour
    {
        [Header("Grief Settings")]
        [Tooltip("Refer to the current Grief value (0 to 1).")]
        [Range(0f, 1f)]
        public float currentGrief = 0f; // Sementara kita set public biar bisa ditest di Inspector

        [Header("Effect Intensities at Max Grief (1.0)")]
        [SerializeField] private float maxVignetteIntensity = 0.6f;
        [SerializeField] private float maxFilmGrainIntensity = 0.8f;
        [SerializeField] private float maxLensDistortion = 0.5f;
        [SerializeField] private float minSaturation = -80f; // -100 = hitam putih

        private Volume postProcessVolume;
        
        // Referensi ke efek-efek spesifik di dalam Volume Profile
        private Vignette vignette;
        private FilmGrain filmGrain;
        private LensDistortion lensDistortion;
        private ColorAdjustments colorAdjustments;

        private void Start()
        {
            postProcessVolume = GetComponent<Volume>();

            // Mengambil profil Post-Processing yang aktif di Volume ini
            if (postProcessVolume.profile != null)
            {
                // Mencoba mencari efek-efek ini di dalam profile
                postProcessVolume.profile.TryGet(out vignette);
                postProcessVolume.profile.TryGet(out filmGrain);
                postProcessVolume.profile.TryGet(out lensDistortion);
                postProcessVolume.profile.TryGet(out colorAdjustments);
            }
            else
            {
                Debug.LogError("[GriefVisualEffects] No Volume Profile found on this object!");
            }
        }

        private void Update()
        {
            // TODO: Nanti sambungkan ini dengan sistem GriefMeter yang sebenarnya.
            // Contoh: if (GriefMeter.Instance != null) currentGrief = GriefMeter.Instance.CurrentGrief;
            
            UpdateVisualEffects(currentGrief);
        }

        private void UpdateVisualEffects(float griefValue)
        {
            // Pastikan nilai grief selalu antara 0 dan 1
            float normalizedGrief = Mathf.Clamp01(griefValue);

            // Update Vignette (Pinggiran gelap)
            if (vignette != null)
            {
                vignette.intensity.value = Mathf.Lerp(0f, maxVignetteIntensity, normalizedGrief);
            }

            // Update Film Grain (Semut/Noise)
            if (filmGrain != null)
            {
                filmGrain.intensity.value = Mathf.Lerp(0f, maxFilmGrainIntensity, normalizedGrief);
            }

            // Update Lens Distortion (Layar melengkung)
            if (lensDistortion != null)
            {
                lensDistortion.intensity.value = Mathf.Lerp(0f, maxLensDistortion, normalizedGrief);
            }

            // Update Color Adjustments (Warna memudar)
            if (colorAdjustments != null)
            {
                // Mulai dari 0 (warna normal) turun ke minSaturation (pudar/abu-abu)
                colorAdjustments.saturation.value = Mathf.Lerp(0f, minSaturation, normalizedGrief);
            }
        }
    }
}