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

        public SequenceManager(RectTransform container, GameObject prefab, int length)
        {
            sequenceContainer = container;
            colorTargetPrefab = prefab;
            sequenceLength = length;
        }

        /// <summary>
        /// Generates a new color sequence and creates UI elements to display it.
        /// </summary>
        public void GenerateNewSequence()
        {
            ClearSequence();
            CreateSequenceObjects();
            ResetProgress();
        }

        /// <summary>
        /// Creates UI objects for the sequence display.
        /// </summary>
        private void CreateSequenceObjects()
        {
            for (int i = 0; i < sequenceLength; i++)
            {
                CreateSequenceItem();
            }
        }

        /// <summary>
        /// Creates a single sequence item with random color.
        /// </summary>
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
            if (sequenceObjects.Count >= sequenceLength && GameTween.Instance != null)
            {
                GameTween.Instance.InitSequenceTransitions(false);
            }
        }

        /// <summary>
        /// Gets a random balloon color from the available colors.
        /// </summary>
        /// <returns>Random balloon color enum value</returns>
        private BalloonColorEnum GetRandomColor()
        {
            var colorValues = System.Enum.GetValues(typeof(BalloonColorEnum));
            return (BalloonColorEnum)colorValues.GetValue(UnityEngine.Random.Range(0, colorValues.Length));
        }

        /// <summary>
        /// Sets the visual color of a sequence item based on balloon color enum.
        /// </summary>
        /// <param name="item">The sequence item GameObject</param>
        /// <param name="color">The balloon color to apply</param>
        private void SetSequenceItemColor(GameObject item, BalloonColorEnum color)
        {
            Color targetColor = ColorUtility.GetColorFromEnum(color);

            var childImage = item.transform.Find("Image")?.GetComponent<Image>();
            if (childImage != null)
            {
                childImage.color = targetColor;
            }
        }

        /// <summary>
        /// Validates if the provided color matches the next expected color in the sequence.
        /// </summary>
        /// <param name="color">The balloon color to validate</param>
        /// <returns>True if the color matches the next expected color</returns>
        public bool ValidateNextColor(BalloonColorEnum color)
        {
            // Ensure we have a valid sequence and index
            if (colorSequence == null || colorSequence.Count == 0 ||
                currentSequenceIndex < 0 || currentSequenceIndex >= colorSequence.Count)
            {
                return false;
            }

            bool isCorrect = colorSequence[currentSequenceIndex] == color;

            if (isCorrect)
            {
                MarkSequenceItemAsCompleted(currentSequenceIndex);
                currentSequenceIndex++;
            }

            return isCorrect;
        }

        /// <summary>
        /// Marks a sequence item as completed by hiding the color and showing a check indicator.
        /// </summary>
        /// <param name="index">Index of the sequence item to mark as completed</param>
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

        /// <summary>
        /// Checks if the current sequence has been completed.
        /// </summary>
        /// <returns>True if all sequence items have been correctly matched</returns>
        public bool IsSequenceComplete()
        {
            return currentSequenceIndex >= colorSequence.Count;
        }

        /// <summary>
        /// Clears all sequence objects and resets the sequence state.
        /// </summary>
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

        /// <summary>
        /// Resets the sequence progress to the beginning.
        /// </summary>
        private void ResetProgress()
        {
            currentSequenceIndex = 0;
        }
    }
}
