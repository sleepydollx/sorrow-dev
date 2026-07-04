using UnityEngine;
using GriefHorror.Systems;

namespace GriefHorror.World
{
    /// <summary>
    /// Makes a space respond to grief. As grief rises the room grows darker and
    /// colder; as the player confronts memories and grief falls, warmth returns.
    /// The house becomes a visible read-out of the character's inner state.
    ///
    /// Attach to a room's root and assign a Light (or put it on the Light
    /// itself — Reset() grabs one automatically in the editor). A deliberately
    /// simple starting point; later you can drive fog, post-processing, audio
    /// filters, and geometry that literally closes in.
    ///
    /// Tuned for the Built-in Render Pipeline. If you use URP/HDRP, adjust the
    /// intensity values to that pipeline's lighting units.
    /// </summary>
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
        }
    }
}
