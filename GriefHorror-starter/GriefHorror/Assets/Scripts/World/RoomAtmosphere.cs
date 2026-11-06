using UnityEngine;
using GriefHorror.Systems;

namespace GriefHorror.World
{
    // If the player is in a grief state, the room goes cold and dark.
    public class RoomAtmosphere : MonoBehaviour
    {
        [Header("Light response")]
        [SerializeField] private Light roomLight;
        [SerializeField] private float warmIntensity = 1.1f;
        [SerializeField] private float coldIntensity = 0.15f;
        [SerializeField] private Color warmColor = new Color(1f, 0.86f, 0.66f);
        [SerializeField] private Color coldColor = new Color(0.55f, 0.62f, 0.78f);

        [Tooltip("How quickly the room reacts to changes in grief.")]
        [SerializeField] private float responseSpeed = 1.5f;

        private void Reset()
        {
            roomLight = GetComponent<Light>();
        }

        private void Update()
        {
            if (roomLight == null || GriefMeter.Instance == null)
                return;

            float grief = Mathf.Clamp01(GriefMeter.Instance.Grief); // assumes 0..1
            float targetIntensity = Mathf.Lerp(warmIntensity, coldIntensity, grief);
            Color targetColor = Color.Lerp(warmColor, coldColor, grief);

            float t = responseSpeed * Time.deltaTime;
            roomLight.intensity = Mathf.Lerp(roomLight.intensity, targetIntensity, t);
            roomLight.color = Color.Lerp(roomLight.color, targetColor, t);
        }
    }
}