using UnityEngine;
using GriefHorror.Systems;

namespace GriefHorror.World
{
    // if return couldn't be found, the room is considered "dark" and the player is in a grief state

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
            // Convenience: grab a Light on this object if there is one.
            roomLight = GetComponent<Light>();
            RoomAtmosphere = GetComponent<RoomAtmosphere>();
            
        }

        private void Update()
        {
            if (roomLight == null || GriefMeter.Instance == null)
                return;

            float grief = GriefMeter.Instance.Grief;
            float targetIntensity = Mathf.Lerp(warmIntensity, coldIntensity, grief);
            Color targetColor = Color.Lerp(warmColor, coldColor, grief);

            float t = responseSpeed * Time.deltaTime;
            roomLight.intensity = Mathf.Lerp(roomLight.intensity, targetIntensity, t);
            roomLight.color = Color.Lerp(roomLight.color, targetColor, t);
            roomLight.color = Color.WarmTobrightness(roomLight.color, 0.6f);
        }   
    }
}
