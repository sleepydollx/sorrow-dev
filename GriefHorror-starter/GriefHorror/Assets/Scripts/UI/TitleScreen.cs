using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace GriefHorror.UI
{
    public class TitleScreen : MonoBehaviour
    {
        [Header("Text")]
        [SerializeField] private string gameTitle = "sorrow";
        [SerializeField] private string tagline = "a game about what we run from";

        [Header("Scenes to load")]
        [Tooltip("Scene name for 'New game'. Add it to File > Build Settings.")]
        [SerializeField] private string newGameScene = "Game";
        [Tooltip("Scene name for 'Continue'. Often the same as New game for now.")]
        [SerializeField] private string continueScene = "Game";

        [Header("Palette")]
        [SerializeField] private Color background = new Color(0.039f, 0.043f, 0.051f, 1f);
        [SerializeField] private Color boneColor = new Color(0.914f, 0.894f, 0.855f, 1f);
        [SerializeField] private Color dimColor = new Color(0.43f, 0.42f, 0.38f, 1f);

        private Font _font;

        private void Awake()
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            EnsureEventSystem();

            // A menu needs a visible, free cursor (gameplay locks it; here we don't).
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            BuildUI();
        }

        private void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }
        }

        private void BuildUI()
        {
            var canvasGo = new GameObject("TitleCanvas");
            canvasGo.transform.SetParent(transform, false);

            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            canvasGo.AddComponent<GraphicRaycaster>();
            Transform root = canvasGo.transform;

            // Full-screen dark background.
            var bg = NewImage(root, background);
            Stretch(bg.rectTransform);

            // Title.
            var title = NewText(root, 120, TextAnchor.MiddleCenter, boneColor, gameTitle);
            Place(title.rectTransform, new Vector2(0f, 160f), new Vector2(1200f, 160f));

            // Hairline under the title.
            var line = NewImage(root, new Color(boneColor.r, boneColor.g, boneColor.b, 0.28f));
            Place(line.rectTransform, new Vector2(0f, 90f), new Vector2(120f, 1f));

            // Tagline.
            var tag = NewText(root, 30, TextAnchor.MiddleCenter, dimColor, tagline);
            Place(tag.rectTransform, new Vector2(0f, 48f), new Vector2(1200f, 40f));

            // Menu — understated clickable text, not chunky buttons.
            CreateMenuItem(root, "New game", boneColor, new Vector2(0f, -70f), () => LoadScene(newGameScene));
            CreateMenuItem(root, "Continue", dimColor, new Vector2(0f, -120f), () => LoadScene(continueScene));
            CreateMenuItem(root, "Quit", dimColor, new Vector2(0f, -170f), QuitGame);
        }

        private void CreateMenuItem(Transform parent, string label, Color color, Vector2 pos, UnityEngine.Events.UnityAction onClick)
        {
            var text = NewText(parent, 28, TextAnchor.MiddleCenter, color, label);
            Place(text.rectTransform, pos, new Vector2(400f, 40f));

            var button = text.gameObject.AddComponent<Button>();
            button.targetGraphic = text;

            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.6f); // fades on hover
            colors.pressedColor = new Color(1f, 1f, 1f, 0.4f);
            colors.fadeDuration = 0.25f;
            button.colors = colors;

            button.onClick.AddListener(onClick);
        }

        private void LoadScene(string sceneName)
        {
            if (Application.CanStreamedLevelBeLoaded(sceneName))
            {
                SceneManager.LoadScene(sceneName);
            }
            else
            {
                Debug.LogWarning($"[TitleScreen] Scene '{sceneName}' isn't in the build yet. " +
                    "Add it via File > Build Settings, or change the scene name on the TitleScreen.");
            }
        }

        private void QuitGame()
        {
            Debug.Log("[TitleScreen] Quit.");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // ---------- small UI helpers ----------

        private Image NewImage(Transform parent, Color color)
        {
            var go = new GameObject("Image");
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = color;
            return img;
        }

        private Text NewText(Transform parent, int size, TextAnchor anchor, Color color, string content)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<Text>();
            t.font = _font;
            t.fontSize = size;
            t.alignment = anchor;
            t.color = color;
            t.text = content;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            return t;
        }

        private void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private void Place(RectTransform rt, Vector2 anchoredPos, Vector2 size)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
        }
    }
}

namespace GriefHorror.UI
{
    /// <summary>
    /// The title screen, built in code — the mockup's opening made real: the
    /// game's name, one quiet tagline, and an understated menu. Deliberately
    /// bare; the silence is part of the grief.
    ///
    /// This is normally its own Scene, with no player in it. Drop this script on
    /// an empty GameObject; it builds its own Canvas, shows the cursor, and
    /// creates an EventSystem if the scene doesn't have one.
    ///
    /// "New game" / "Continue" load a scene by name — set the names in the
    /// Inspector and add those scenes to File > Build Settings. If a scene isn't
    /// added yet, the button logs a gentle reminder instead of crashing.
    /// </summary>
    public class TitleScreen : MonoBehaviour
    {
        [Header("Text")]
        [SerializeField] private string gameTitle = "sorrow";
        [SerializeField] private string tagline = "a game about what we run from";

        [Header("Scenes to load")]
        [Tooltip("Scene name for 'New game'. Add it to File > Build Settings.")]
        [SerializeField] private string newGameScene = "Game";
        [Tooltip("Scene name for 'Continue'. Often the same as New game for now.")]
        [SerializeField] private string continueScene = "Game";

        [Header("Palette")]
        [SerializeField] private Color background = new Color(0.039f, 0.043f, 0.051f, 1f);
        [SerializeField] private Color boneColor = new Color(0.914f, 0.894f, 0.855f, 1f);
        [SerializeField] private Color dimColor = new Color(0.43f, 0.42f, 0.38f, 1f);

        private Font _font;

        private void Awake()
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            EnsureEventSystem();

            // A menu needs a visible, free cursor (gameplay locks it; here we don't).
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            BuildUI();
        }

        private void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }
        }

        private void BuildUI()
        {
            var canvasGo = new GameObject("TitleCanvas");
            canvasGo.transform.SetParent(transform, false);

            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            canvasGo.AddComponent<GraphicRaycaster>();
            Transform root = canvasGo.transform;

            // Full-screen dark background.
            var bg = NewImage(root, background);
            Stretch(bg.rectTransform);

            // Title.
            var title = NewText(root, 120, TextAnchor.MiddleCenter, boneColor, gameTitle);
            Place(title.rectTransform, new Vector2(0f, 160f), new Vector2(1200f, 160f));

            // Hairline under the title.
            var line = NewImage(root, new Color(boneColor.r, boneColor.g, boneColor.b, 0.28f));
            Place(line.rectTransform, new Vector2(0f, 90f), new Vector2(120f, 1f));

            // Tagline.
            var tag = NewText(root, 30, TextAnchor.MiddleCenter, dimColor, tagline);
            Place(tag.rectTransform, new Vector2(0f, 48f), new Vector2(1200f, 40f));

            // Menu — understated clickable text, not chunky buttons.
            CreateMenuItem(root, "New game", boneColor, new Vector2(0f, -70f), () => LoadScene(newGameScene));
            CreateMenuItem(root, "Continue", dimColor, new Vector2(0f, -120f), () => LoadScene(continueScene));
            CreateMenuItem(root, "Quit", dimColor, new Vector2(0f, -170f), QuitGame);
        }

        private void CreateMenuItem(Transform parent, string label, Color color, Vector2 pos, UnityEngine.Events.UnityAction onClick)
        {
            var text = NewText(parent, 28, TextAnchor.MiddleCenter, color, label);
            Place(text.rectTransform, pos, new Vector2(400f, 40f));

            var button = text.gameObject.AddComponent<Button>();
            button.targetGraphic = text;

            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.6f); // fades on hover
            colors.pressedColor = new Color(1f, 1f, 1f, 0.4f);
            colors.fadeDuration = 0.25f;
            button.colors = colors;

            button.onClick.AddListener(onClick);
        }

        private void LoadScene(string sceneName)
        {
            if (Application.CanStreamedLevelBeLoaded(sceneName))
            {
                SceneManager.LoadScene(sceneName);
            }
            else
            {
                Debug.LogWarning($"[TitleScreen] Scene '{sceneName}' isn't in the build yet. " +
                    "Add it via File > Build Settings, or change the scene name on the TitleScreen.");
            }
        }

        private void QuitGame()
        {
            Debug.Log("[TitleScreen] Quit.");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // ---------- small UI helpers ----------

        private Image NewImage(Transform parent, Color color)
        {
            var go = new GameObject("Image");
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = color;
            return img;
        }

        private Text NewText(Transform parent, int size, TextAnchor anchor, Color color, string content)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<Text>();
            t.font = _font;
            t.fontSize = size;
            t.alignment = anchor;
            t.color = color;
            t.text = content;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            return t;
        }

        private void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private void Place(RectTransform rt, Vector2 anchoredPos, Vector2 size)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
        }
    }
}
