using UnityEngine;
using UnityEngine.UI;
using GriefHorror.Systems;

namespace GriefHorror.UI
{
    public class GriefVignette : MonoBehaviour
    {
        [Header("Look")]
        [SerializeField] private Color vignetteColor = Color.black;

        [Tooltip("Vignette strength when the player is calm (grief = 0).")]
        [Range(0f, 1f)]
        [SerializeField] private float minStrength = 0.15f;

        [Tooltip("Vignette strength at maximum grief — near-suffocating.")]
        [Range(0f, 1f)]
        [SerializeField] private float maxStrength = 0.9f;

        [Tooltip("How quickly the vignette reacts to changes in grief.")]
        [SerializeField] private float responseSpeed = 2f;

        [Tooltip("Resolution of the generated gradient. 256 is plenty.")]
        [SerializeField] private int textureSize = 256;

        private Image _image;
        private float _current;

        private void Awake()
        {
            BuildOverlay();
        }

        private void BuildOverlay()
        {
            // A dedicated top-most canvas so the vignette sits over everything.
            var canvasGo = new GameObject("GriefVignetteCanvas");
            canvasGo.transform.SetParent(transform, false);

            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;
            canvasGo.AddComponent<CanvasScaler>();
            // Deliberately no GraphicRaycaster: the vignette must never eat input.

            // Full-screen image stretched to every edge.
            var imgGo = new GameObject("Vignette");
            imgGo.transform.SetParent(canvasGo.transform, false);

            _image = imgGo.AddComponent<Image>();
            _image.raycastTarget = false;
            _image.sprite = BuildRadialSprite(textureSize);
            _image.color = new Color(1f, 1f, 1f, minStrength);

            var rt = _image.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            _current = minStrength;
        }

        /// <summary>
        /// Bakes a radial gradient: transparent at the center, opaque toward the
        /// edges, tinted with <see cref="vignetteColor"/>. Runs once at startup.
        /// </summary>
        private Sprite BuildRadialSprite(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp
            };

            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float maxDist = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center) / maxDist;
                    // Clear inner circle, smooth ramp to solid at the edges.
                    float a = Mathf.Clamp01(Mathf.SmoothStep(0f, 1f, (dist - 0.4f) / 0.6f));
                    tex.SetPixel(x, y, new Color(vignetteColor.r, vignetteColor.g, vignetteColor.b, a));
                }
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }

        private void Update()
        {
            if (_image == null)
                return;

            float grief = GriefMeter.Instance != null ? GriefMeter.Instance.Grief : 0f;
            float target = Mathf.Lerp(minStrength, maxStrength, grief);

            _current = Mathf.Lerp(_current, target, responseSpeed * Time.deltaTime);

            Color c = _image.color;
            c.a = _current;
            _image.color = c;
        }
    }
}
