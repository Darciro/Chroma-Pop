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

        public void GenerateNewSequence()
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

            GameObject sequenceItem = Object.Instantiate(colorTargetPrefab, sequenceContainer);
            sequenceObjects.Add(sequenceItem);

            BalloonColorEnum randomColor = GetRandomColor();
            colorSequence.Add(randomColor);

            SetSequenceItemColor(sequenceItem, randomColor);
            GameTween.Instance.InitSequenceTransitions();
        }

        private BalloonColorEnum GetRandomColor()
        {
            var colorValues = System.Enum.GetValues(typeof(BalloonColorEnum));
            return (BalloonColorEnum)colorValues.GetValue(Random.Range(0, colorValues.Length));
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
            if (currentSequenceIndex >= colorSequence.Count)
                return false;

            bool isCorrect = colorSequence[currentSequenceIndex] == color;

            if (isCorrect)
            {
                MarkSequenceItemAsCompleted(currentSequenceIndex);
                currentSequenceIndex++;
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
            return currentSequenceIndex >= colorSequence.Count;
        }

        public void ClearSequence()
        {

            // Clear any existing UI objects from the container
            for (int i = sequenceContainer.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(sequenceContainer.GetChild(i).gameObject);
            }

            foreach (GameObject obj in sequenceObjects)
            {
                if (obj != null)
                {
                    Object.Destroy(obj);
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
