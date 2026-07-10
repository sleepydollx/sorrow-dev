using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GriefHorror.Systems;
using GriefHorror.World;

namespace GriefHorror.UI
{
    public class GameHUD : MonoBehaviour
    {
        public static GameHUD Instance { get; private set; }

        [Header("Interaction")]
        [Tooltip("How close the player must be to see an object's prompt. Match your FirstPersonController's interact range.")]
        [SerializeField] private float interactRange = 2.5f;
        [SerializeField] private KeyCode interactKey = KeyCode.E;

        [Header("Voicemail")]
        [Tooltip("How many marks to show. Match 'Truths To Face For Ending' on the GameManager.")]
        [SerializeField] private int voicemailMarks = 5;

        [Header("Palette")]
        [SerializeField] private Color boneColor = new Color(0.914f, 0.894f, 0.855f, 1f);
        [SerializeField] private Color dimColor = new Color(0.914f, 0.894f, 0.855f, 0.22f);

        private Camera _camera;
        private Font _font;

        private Text _promptText;
        private Text _subtitleText;
        private readonly List<Image> _marks = new List<Image>();

        private float _subtitleTimer;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            BuildUI();
        }

        private void Start()
        {
            _camera = Camera.main;
            RefreshMarks();

            if (GameManager.Instance != null)
                GameManager.Instance.OnVoicemailProgressed += OnVoicemailProgressed;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnVoicemailProgressed -= OnVoicemailProgressed;
            if (Instance == this)
                Instance = null;
        }

        private void Update()
        {
            UpdatePrompt();
            UpdateSubtitle();
        }

        // ---------- Interaction prompt ----------

        private void UpdatePrompt()
        {
            if (_camera == null)
                _camera = Camera.main;
            if (_camera == null)
                return;

            var ray = new Ray(_camera.transform.position, _camera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, interactRange))
            {
                var interactable = hit.collider.GetComponentInParent<Interactable>();
                if (interactable != null)
                {
                    _promptText.text = $"[{interactKey}]  {interactable.Prompt}";
                    _promptText.gameObject.SetActive(true);
                    return;
                }
            }

            _promptText.gameObject.SetActive(false);
        }

        // ---------- Subtitles ----------

        /// <summary>Show a line of subtitle text for a few seconds.</summary>
        public void ShowSubtitle(string line, float seconds = 5f)
        {
            if (_subtitleText == null)
                return;

            _subtitleText.text = line;
            _subtitleText.gameObject.SetActive(true);
            _subtitleTimer = seconds;
        }

        private void UpdateSubtitle()
        {
            if (_subtitleText == null || !_subtitleText.gameObject.activeSelf)
                return;

            _subtitleTimer -= Time.deltaTime;
            if (_subtitleTimer <= 0f)
                _subtitleText.gameObject.SetActive(false);
        }

        // ---------- Voicemail marks ----------

        private void OnVoicemailProgressed(float progress)
        {
            RefreshMarks();
        }

        private void RefreshMarks()
        {
            int filled = 0;
            if (GameManager.Instance != null)
                filled = Mathf.RoundToInt(GameManager.Instance.VoicemailProgress * voicemailMarks);

            for (int i = 0; i < _marks.Count; i++)
                _marks[i].color = i < filled ? boneColor : dimColor;
        }

        // ---------- Build the UI in code ----------

        private void BuildUI()
        {
            var canvasGo = new GameObject("GameHUDCanvas");
            canvasGo.transform.SetParent(transform, false);

            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            Transform root = canvasGo.transform;

            // Reticle — a small dot in the exact center.
            var reticle = NewImage(root, new Color(0.914f, 0.894f, 0.855f, 0.5f));
            var rr = reticle.rectTransform;
            rr.anchorMin = rr.anchorMax = new Vector2(0.5f, 0.5f);
            rr.pivot = new Vector2(0.5f, 0.5f);
            rr.sizeDelta = new Vector2(6f, 6f);
            rr.anchoredPosition = Vector2.zero;

            // Voicemail marks — a small row near the top center.
            var marksParent = new GameObject("VoicemailMarks").AddComponent<RectTransform>();
            marksParent.SetParent(root, false);
            marksParent.anchorMin = marksParent.anchorMax = new Vector2(0.5f, 1f);
            marksParent.pivot = new Vector2(0.5f, 1f);
            marksParent.anchoredPosition = new Vector2(0f, -70f);
            marksParent.sizeDelta = Vector2.zero;

            const float spacing = 12f;
            for (int i = 0; i < voicemailMarks; i++)
            {
                var mark = NewImage(marksParent, dimColor);
                var mr = mark.rectTransform;
                mr.anchorMin = mr.anchorMax = new Vector2(0.5f, 0.5f);
                mr.pivot = new Vector2(0.5f, 0.5f);
                mr.sizeDelta = new Vector2(4f, 18f);
                float offset = (i - (voicemailMarks - 1) / 2f) * spacing;
                mr.anchoredPosition = new Vector2(offset, 0f);
                _marks.Add(mark);
            }

            // Interaction prompt — bottom center, hidden until you look at something.
            _promptText = NewText(root, 30, TextAnchor.MiddleCenter, boneColor);
            var pr = _promptText.rectTransform;
            pr.anchorMin = pr.anchorMax = new Vector2(0.5f, 0f);
            pr.pivot = new Vector2(0.5f, 0f);
            pr.sizeDelta = new Vector2(1000f, 50f);
            pr.anchoredPosition = new Vector2(0f, 140f);
            _promptText.gameObject.SetActive(false);

            // Subtitle — lower center, hidden until ShowSubtitle is called.
            _subtitleText = NewText(root, 34, TextAnchor.LowerCenter, boneColor);
            var sr = _subtitleText.rectTransform;
            sr.anchorMin = sr.anchorMax = new Vector2(0.5f, 0f);
            sr.pivot = new Vector2(0.5f, 0f);
            sr.sizeDelta = new Vector2(1400f, 200f);
            sr.anchoredPosition = new Vector2(0f, 60f);
            _subtitleText.horizontalOverflow = HorizontalWrapMode.Wrap;
            _subtitleText.gameObject.SetActive(false);
        }

        private Image NewImage(Transform parent, Color color)
        {
            var go = new GameObject("Image");
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        private Text NewText(Transform parent, int size, TextAnchor anchor, Color color)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<Text>();
            t.font = _font;
            t.fontSize = size;
            t.alignment = anchor;
            t.color = color;
            t.raycastTarget = false;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            return t;
        }
    }
}
