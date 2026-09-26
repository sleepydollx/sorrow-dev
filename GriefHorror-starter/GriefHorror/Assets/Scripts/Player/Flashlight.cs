using UnityEngine;
using GriefHorror.Systems;

namespace GriefHorror.Player
{
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

            // Dim as grief rises
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


public class Flashlight : MonoBehaviour
{
    [Header("Flashlight Settings")]
    public GameObject flashlightLight; // Referensi objek Light/Spotlight
    public float maxBatteryLife = 100f;
    public float currentBattery;
    public float drainRate = 5f; // Kecepatan baterai berkurang per detik saat menyala
    
    private bool isOn = false;

    void Start()
    {
        currentBattery = maxBatteryLife;
        if (flashlightLight != null)
            flashlightLight.SetActive(isOn);
    }

    void Update()
    {
        // Menyalakan/mematikan senter dengan tombol F
        if (Input.GetKeyDown(KeyCode.F))
        {
            ToggleFlashlight();
        }

        if (isOn)
        {
            currentBattery -= drainRate * Time.deltaTime;
            if (currentBattery <= 0)
            {
                currentBattery = 0;
                TurnOff();
            }
        }
    }

    void ToggleFlashlight()
    {
        if (isOn)
        {
            TurnOff();
        }
        else
        {
            if (currentBattery > 0)
            {
                TurnOn();
            }
        }
    }

    public void TurnOn()
    {
        isOn = true;
        if (flashlightLight != null) flashlightLight.SetActive(true);
    }

    public void TurnOff()
    {
        isOn = false;
        if (flashlightLight != null) flashlightLight.SetActive(false);
    }

    // Fungsi untuk mengisi ulang baterai
    public void AddBattery(float amount)
    {
        currentBattery += amount;
        if (currentBattery > maxBatteryLife)
        {
            currentBattery = maxBatteryLife;
        }
        Debug.Log("Baterai bertambah! Sisa baterai: " + currentBattery);
    }
}