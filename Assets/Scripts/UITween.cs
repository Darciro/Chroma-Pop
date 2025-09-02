using TMPro;
using UnityEngine;

namespace ChromaPop
{
    public class UITween : MonoBehaviour
    {
        public static UITween Instance { get; private set; }

        [SerializeField] private GameObject titleArea;
        [SerializeField] private GameObject buttonStart;
        [SerializeField] private TextMeshProUGUI gameVersionText;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            if (titleArea != null && buttonStart != null)
            {
                HomeTransitions();
                gameVersionText.text = $"Version {Application.version}";
            }
        }

        private void HomeTransitions()
        {
            // Logo area
            LeanTween.scale(titleArea, new Vector3(0, 0, 0), 0);
            LeanTween.moveLocal(titleArea, new Vector3(0, 0, 0), 0);

            // Buttons
            LeanTween.alphaCanvas(buttonStart.GetComponent<CanvasGroup>(), 0, 0);

            InitHomeTransitions();
        }

        private void InitHomeTransitions()
        {
            // Logo area
            LeanTween.scale(titleArea, new Vector3(0.32f, 0.32f, 0.32f), 2f).setDelay(0.5f).setEase(LeanTweenType.easeOutElastic);
            LeanTween.moveLocal(titleArea, new Vector3(0, 220f, 0), .52f).setDelay(2f).setEase(LeanTweenType.easeInOutCubic);

            // Buttons
            LeanTween.alphaCanvas(buttonStart.GetComponent<CanvasGroup>(), 1f, 1f).setDelay(3f);
        }
    }
}
