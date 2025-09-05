using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChromaPop
{
    /// <summary>
    /// Handles color sequence generation and validation.
    /// </summary>
    [System.Serializable]
    public class SequenceManager
    {
        private readonly RectTransform sequenceContainer;
        private readonly GameObject colorTargetPrefab;
        private readonly int sequenceLength;

        private readonly List<GameObject> sequenceObjects = new List<GameObject>();
        private readonly List<BalloonColorEnum> colorSequence = new List<BalloonColorEnum>();
        private int currentSequenceIndex = 0;

        // Swipe animation support
        private bool isAnimatingSwipe = false;
        private bool isSwipeSequence = false; // Track if current sequence is from swipe
        public event Action OnSwipeNewSequence;

        [Header("Swipe Animation Settings")]
        private float animationDuration = 0.3f; // Moderate speed for swipe out animation
        private float slideDistance = 600f;

        public SequenceManager(RectTransform container, GameObject prefab, int length)
        {
            sequenceContainer = container;
            colorTargetPrefab = prefab;
            sequenceLength = length;
        }

        /// <summary>
        /// Generates a new sequence with swipe animation if enabled
        /// </summary>
        public void GenerateNewSequence(bool withSwipeAnimation = false, Vector2 swipeDirection = default)
        {
            Debug.Log($"[SequenceManager] GenerateNewSequence called. WithSwipeAnimation: {withSwipeAnimation}");
            isSwipeSequence = withSwipeAnimation; // Track if this is a swipe sequence

            if (withSwipeAnimation && sequenceObjects.Count > 0)
            {
                Debug.Log("[SequenceManager] Starting swipe animation transition");
                // Animate current items sliding off, then create new sequence
                AnimateSwipeTransition(swipeDirection, () =>
                {
                    CreateNewSequenceAfterSwipe();
                });
            }
            else
            {
                Debug.Log("[SequenceManager] Generating standard sequence");
                // Standard sequence generation
                ClearSequence();
                CreateSequenceObjects();
                ResetProgress();
            }
        }

        /// <summary>
        /// Standard new sequence generation
        /// </summary>
        public void GenerateNewSequence()
        {
            isSwipeSequence = false; // This is not a swipe sequence
            GenerateNewSequence(false);
        }

        /// <summary>
        /// Called to handle swipe gestures
        /// </summary>
        public void HandleSwipe(Vector2 swipeDirection)
        {
            if (!isAnimatingSwipe)
            {
                GenerateNewSequence(true, swipeDirection);
                OnSwipeNewSequence?.Invoke();
            }
        }

        /// <summary>
        /// Animates sequence items sliding off in the given direction
        /// </summary>
        private void AnimateSwipeTransition(Vector2 swipeDirection, Action onComplete = null)
        {
            if (isAnimatingSwipe || sequenceObjects.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }

            isAnimatingSwipe = true;

            // Calculate slide destination based on swipe direction
            Vector3 slideOffset = CalculateSlideOffset(swipeDirection);

            int completedAnimations = 0;
            int totalAnimations = sequenceObjects.Count;

            // Animate each item with a slight stagger
            for (int i = 0; i < sequenceObjects.Count; i++)
            {
                GameObject item = sequenceObjects[i];
                if (item == null) continue;

                float delay = i * 0.02f; // Small stagger for swipe out animation
                Vector3 targetPosition = item.transform.position + slideOffset;

                // Slide animation
                LeanTween.move(item, targetPosition, animationDuration)
                    .setDelay(delay)
                    .setEase(LeanTweenType.easeInBack)
                    .setOnComplete(() =>
                    {
                        completedAnimations++;
                        if (completedAnimations >= totalAnimations)
                        {
                            isAnimatingSwipe = false;
                            onComplete?.Invoke();
                        }
                    });

                // Add rotation effect during slide
                LeanTween.rotateZ(item, UnityEngine.Random.Range(-30f, 30f), animationDuration)
                    .setDelay(delay)
                    .setEase(LeanTweenType.easeOutQuad);

                // Fade out effect
                var canvasGroup = item.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = item.AddComponent<CanvasGroup>();
                }

                LeanTween.alphaCanvas(canvasGroup, 0f, animationDuration * 0.6f) // Faster fade out
                    .setDelay(delay + animationDuration * 0.1f) // Earlier fade start
                    .setEase(LeanTweenType.easeInQuad);
            }
        }

        /// <summary>
        /// Calculates the slide offset based on swipe direction
        /// </summary>
        private Vector3 CalculateSlideOffset(Vector2 swipeDirection)
        {
            // Determine primary direction (horizontal or vertical)
            bool isHorizontal = Mathf.Abs(swipeDirection.x) > Mathf.Abs(swipeDirection.y);

            Vector3 offset;
            if (isHorizontal)
            {
                // Swipe left or right
                float direction = swipeDirection.x > 0 ? 1f : -1f;
                offset = new Vector3(direction * slideDistance, 0, 0);
            }
            else
            {
                // Swipe up or down
                float direction = swipeDirection.y > 0 ? 1f : -1f;
                offset = new Vector3(0, direction * slideDistance, 0);
            }

            return offset;
        }

        /// <summary>
        /// Creates a new sequence after swipe animation completes
        /// </summary>
        private void CreateNewSequenceAfterSwipe()
        {
            ClearSequence();
            CreateSequenceObjects();
            ResetProgress();
        }

        private void CreateSequenceObjects()
        {
            for (int i = 0; i < sequenceLength; i++)
            {
                CreateSequenceItem();
            }
        }

        private void CreateSequenceItem()
        {
            if (colorTargetPrefab == null || sequenceContainer == null)
            {
                Debug.LogError("Sequence UI components not properly configured!");
                return;
            }

            GameObject sequenceItem = UnityEngine.Object.Instantiate(colorTargetPrefab, sequenceContainer);
            sequenceObjects.Add(sequenceItem);

            BalloonColorEnum randomColor = GetRandomColor();
            colorSequence.Add(randomColor);

            SetSequenceItemColor(sequenceItem, randomColor);

            // Only call InitSequenceTransitions for the last item to avoid multiple calls
            if (sequenceObjects.Count >= sequenceLength)
            {
                GameTween.Instance.InitSequenceTransitions(isSwipeSequence);
            }
        }

        private BalloonColorEnum GetRandomColor()
        {
            var colorValues = System.Enum.GetValues(typeof(BalloonColorEnum));
            return (BalloonColorEnum)colorValues.GetValue(UnityEngine.Random.Range(0, colorValues.Length));
        }

        private void SetSequenceItemColor(GameObject item, BalloonColorEnum color)
        {
            Color targetColor = ColorUtility.GetColorFromEnum(color);

            var childImage = item.transform.Find("Image")?.GetComponent<Image>();
            if (childImage != null)
            {
                childImage.color = targetColor;
            }
        }

        public bool ValidateNextColor(BalloonColorEnum color)
        {
            // Ensure we have a valid sequence and index
            if (colorSequence == null || colorSequence.Count == 0 ||
                currentSequenceIndex < 0 || currentSequenceIndex >= colorSequence.Count)
            {
                Debug.LogWarning($"[SequenceManager] Invalid sequence state - colorSequence count: {colorSequence?.Count ?? 0}, currentIndex: {currentSequenceIndex}");
                return false;
            }

            bool isCorrect = colorSequence[currentSequenceIndex] == color;
            Debug.Log($"[SequenceManager] Validating color {color} at index {currentSequenceIndex}. Expected: {colorSequence[currentSequenceIndex]}. Correct: {isCorrect}");

            if (isCorrect)
            {
                MarkSequenceItemAsCompleted(currentSequenceIndex);
                currentSequenceIndex++;
                Debug.Log($"[SequenceManager] Progress updated. New index: {currentSequenceIndex}/{colorSequence.Count}");
            }

            return isCorrect;
        }

        private void MarkSequenceItemAsCompleted(int index)
        {
            if (index < 0 || index >= sequenceObjects.Count || sequenceObjects[index] == null)
                return;

            GameObject item = sequenceObjects[index];

            // Hide the color image
            var childImage = item.transform.Find("Image");
            if (childImage != null)
            {
                childImage.gameObject.SetActive(false);
            }

            // Show the checked indicator
            var checkedIndicator = item.transform.Find("Checked");
            if (checkedIndicator != null)
            {
                checkedIndicator.gameObject.SetActive(true);
            }
        }

        public bool IsSequenceComplete()
        {
            bool isComplete = currentSequenceIndex >= colorSequence.Count;
            Debug.Log($"[SequenceManager] IsSequenceComplete check: {currentSequenceIndex}/{colorSequence.Count} = {isComplete}");
            return isComplete;
        }

        /// <summary>
        /// Checks if the sequence is currently animating a swipe
        /// </summary>
        public bool IsAnimatingSwipe()
        {
            return isAnimatingSwipe;
        }

        public void ClearSequence()
        {

            // Clear any existing UI objects from the container
            for (int i = sequenceContainer.childCount - 1; i >= 0; i--)
            {
                UnityEngine.Object.Destroy(sequenceContainer.GetChild(i).gameObject);
            }

            foreach (GameObject obj in sequenceObjects)
            {
                if (obj != null)
                {
                    UnityEngine.Object.Destroy(obj);
                }
            }

            sequenceObjects.Clear();
            colorSequence.Clear();
            currentSequenceIndex = 0;
        }

        private void ResetProgress()
        {
            currentSequenceIndex = 0;
        }
    }
}
