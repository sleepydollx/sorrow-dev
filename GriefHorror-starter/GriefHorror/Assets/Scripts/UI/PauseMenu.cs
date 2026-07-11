using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GriefHorror.UI
{
        public class PauseMenu : MonoBehaviour
    {
        public static PauseMenu Instance { get; private set; }
        public static bool IsPaused { get; private set; }

        [Header("Input")]
        [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

        [Header("Scenes")]
        [Tooltip("Scene loaded by 'Leave for now'. Match your title scene's name in Build Settings.")]
        [SerializeField] private string titleScene = "Title";

        [Header("Text")]
        [SerializeField] private string heading = "PAUSED";
        [Tooltip("A quiet line under the heading. Leave empty for none.")]
        [SerializeField] private string subheading = "It waits. It is patient.";

        [Header("Palette")]
        [SerializeField] private Color overlayColor = new Color(0.02f, 0.02f, 0.03f, 0.88f);
        [SerializeField] private Color boneColor = new Color(0.914f, 0.894f, 0.855f, 1f);
        [SerializeField] private Color dimColor = new Color(0.914f, 0.894f, 0.855f, 0.35f);

        [Header("Events")]
        public UnityEvent onPaused;
        public UnityEvent onResumed;

        private Font _font;
        private GameObject _menuRoot;

        private CursorLockMode _prevLockMode;
        private bool _prevCursorVisible;

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
            EnsureEventSystem();
            _menuRoot.SetActive(false);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                // Never leave a scene frozen behind us.
                if (IsPaused) ApplyPause(false);
                Instance = null;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(pauseKey))
                Toggle();
        }

        // ---------- Public API ----------

        public void Toggle() => SetPaused(!IsPaused);

        public void Resume() => SetPaused(false);

        public void SetPaused(bool paused)
        {
            if (IsPaused == paused)
                return;

            ApplyPause(paused);
            _menuRoot.SetActive(paused);

            if (paused) onPaused?.Invoke();
            else onResumed?.Invoke();
        }

        // ---------- Internals ----------

        private void ApplyPause(bool paused)
        {
            IsPaused = paused;
            Time.timeScale = paused ? 0f : 1f;
            AudioListener.pause = paused;

            if (paused)
            {
                // Remember how the game had the cursor, then free it.
                _prevLockMode = Cursor.lockState;
                _prevCursorVisible = Cursor.visible;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = _prevLockMode;
                Cursor.visible = _prevCursorVisible;
            }
        }

        private void QuitToTitle()
        {
            ApplyPause(false); // unfreeze BEFORE leaving, or the title scene runs at timescale 0
            SceneManager.LoadScene(titleScene);
        }

        private void QuitGame()
        {
            ApplyPause(false);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // ---------- Build the UI in code ----------

        private void BuildUI()
        {
            var canvasGo = new GameObject("PauseMenuCanvas");
            canvasGo.transform.SetParent(transform, false);

            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 900; // above GameHUD (500)

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            canvasGo.AddComponent<GraphicRaycaster>();
            _menuRoot = canvasGo;

            Transform root = canvasGo.transform;

            // Full-screen dark overlay (blocks clicks to the game underneath).
            var overlay = new GameObject("Overlay").AddComponent<Image>();
            overlay.transform.SetParent(root, false);
            overlay.color = overlayColor;
            Stretch(overlay.rectTransform);

            // Heading.
            var title = NewText(root, heading, 64, boneColor);
            Anchor(title.rectTransform, new Vector2(0.5f, 0.72f), new Vector2(1200f, 90f));

            // Subheading — small, dim, unsettling.
            if (!string.IsNullOrEmpty(subheading))
            {
                var sub = NewText(root, subheading, 24, dimColor);
                Anchor(sub.rectTransform, new Vector2(0.5f, 0.63f), new Vector2(1200f, 40f));
            }

            // Buttons.
            NewButton(root, "Continue", new Vector2(0.5f, 0.46f), Resume);
            NewButton(root, "Leave for now", new Vector2(0.5f, 0.36f), QuitToTitle);
            NewButton(root, "Quit", new Vector2(0.5f, 0.26f), QuitGame);
        }

        private void NewButton(Transform parent, string label, Vector2 anchor, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject("Button_" + label);
            go.transform.SetParent(parent, false);

            var image = go.AddComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0.04f); // nearly invisible plate; hover tint does the work

            var button = go.AddComponent<Button>();
            button.onClick.AddListener(onClick);

            var colors = button.colors;
            colors.normalColor = new Color(1f, 1f, 1f, 0.0f);
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.10f);
            colors.pressedColor = new Color(1f, 1f, 1f, 0.18f);
            colors.selectedColor = colors.highlightedColor;
            colors.fadeDuration = 0.08f;
            button.colors = colors;

            Anchor((RectTransform)go.transform, anchor, new Vector2(420f, 64f));

            var text = NewText(go.transform, label, 30, boneColor);
            Stretch(text.rectTransform);
        }

        private Text NewText(Transform parent, string content, int size, Color color)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<Text>();
            t.font = _font;
            t.text = content;
            t.fontSize = size;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = color;
            t.raycastTarget = false;
            return t;
        }

        private static void Anchor(RectTransform rt, Vector2 viewportAnchor, Vector2 size)
        {
            rt.anchorMin = rt.anchorMax = viewportAnchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = Vector2.zero;
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null)
                return;

            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }
    }
}
