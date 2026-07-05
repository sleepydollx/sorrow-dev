using UnityEngine;
using GriefHorror.Systems;

namespace GriefHorror.Player
{
    /// <summary>
    /// A handheld light the player can toggle (default: F). It is not a reliable
    /// comfort: as grief rises the beam weakens and flickers harder, so the more
    /// the player runs, the less they can see. The light failing is one more
    /// quiet pressure pushing them to stop and face things instead of fleeing.
    ///
    /// Setup is just: drop this script on the player Camera. It creates its own
    /// Spotlight at runtime, so no manual Light is required. If you'd rather
    /// control an existing Light yourself, assign it to <see cref="beam"/>.
    ///
    /// Degrades gracefully: if there is no GriefMeter in the scene, the light
    /// still works — it just won't react to grief.
    /// </summary>
    public class Flashlight : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private KeyCode toggleKey = KeyCode.F;
        [SerializeField] private bool startOn = true;

        [Header("Beam")]
        [Tooltip("Optional. If left empty, a Spotlight is created as a child of this object.")]
        [SerializeField] private Light beam;
        [SerializeField] private float baseIntensity = 3.5f;
        [SerializeField] private float spotAngle = 45f;
        [SerializeField] private float range = 18f;

        [Header("Grief response")]
        [Tooltip("How much the beam dims at maximum grief. 0 = no dimming, 1 = goes fully dark.")]
        [Range(0f, 1f)]
        [SerializeField] private float griefDimming = 0.6f;

        [Tooltip("How violently the beam flickers at maximum grief.")]
        [SerializeField] private float maxFlicker = 0.35f;

        [Tooltip("A faint, constant unease in the light even when the player is calm.")]
        [SerializeField] private float ambientFlicker = 0.04f;

        [SerializeField] private float flickerSpeed = 14f;

        private bool _isOn;
        private float _noiseSeed;

        private void Awake()
        {
            if (beam == null)
            {
                var go = new GameObject("FlashlightBeam");
                go.transform.SetParent(transform, false);
                beam = go.AddComponent<Light>();
                beam.type = LightType.Spot;
                beam.shadows = LightShadows.Soft;
                beam.color = new Color(1f, 0.95f, 0.85f);
            }

            beam.spotAngle = spotAngle;
            beam.range = range;

            // A per-instance offset so multiple lights don't flicker in sync.
            _noiseSeed = Random.value * 100f;

            SetOn(startOn);
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
                SetOn(!_isOn);

            if (!_isOn)
                return;

            float grief = GriefMeter.Instance != null ? GriefMeter.Instance.Grief : 0f;

            // Dim as grief rises.
            float target = baseIntensity * (1f - griefDimming * grief);

            // Flicker harder as grief rises. Perlin noise gives an organic, flame-like unsteadiness.
            float flickerAmount = Mathf.Lerp(ambientFlicker, maxFlicker, grief);
            float noise = Mathf.PerlinNoise(_noiseSeed + Time.time * flickerSpeed, 0f);
            float flicker = (noise - 0.5f) * 2f * flickerAmount * baseIntensity;

            beam.intensity = Mathf.Max(0f, target + flicker);
        }

        private void SetOn(bool on)
        {
            _isOn = on;
            if (beam != null)
                beam.enabled = on;
        }
    }
}
