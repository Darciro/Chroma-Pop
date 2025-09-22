using System.Collections;
using ChromaPop;
using UnityEngine;
using BalloonColorEnum = ChromaPop.BalloonColorEnum;

namespace ChromaPop
{
    [System.Serializable]
    public class BalloonColorData
    {
        [SerializeField] private BalloonColorEnum _color;
        [SerializeField] private Sprite _balloonSprite;
        [SerializeField] private Sprite[] _popAnimationSprites;

        public BalloonColorEnum Color => _color;
        public Sprite BalloonSprite => _balloonSprite;
        public Sprite[] PopAnimationSprites => _popAnimationSprites;
    }

    /// <summary>
    /// Controls the behavior of balloons including movement, color setting, and popping animation.
    /// </summary>
    public class BalloonController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float floatSpeed = 3f;
        [SerializeField] private float minFloatSpeed = 1f;
        [SerializeField] private float maxFloatSpeed = 5f;

        [Header("Color Configuration")]
        [SerializeField] private BalloonColorData[] balloonColorData;

        [Header("Animation Settings")]
        [SerializeField] private float popAnimationFrameRate = 12f;
        [SerializeField] private float destroyDelay = 1.5f;

        private BalloonColorData currentBalloonColorData;
        private Animator animator;
        private AudioSource audioSource;
        private Rigidbody2D rigidBody;
        private SpriteRenderer spriteRenderer;
        private Collider2D balloonCollider;
        private bool isPopped = false;

        private void Awake()
        {
            CacheComponents();

            // Ensure this balloon has the correct tag for input detection
            if (!gameObject.CompareTag("Balloon"))
            {
                gameObject.tag = "Balloon";
                Debug.Log($"Set tag 'Balloon' on {gameObject.name}");
            }
        }

        private void FixedUpdate()
        {
            if (!isPopped)
            {
                transform.Translate(Vector3.up * floatSpeed * Time.deltaTime);
            }
        }

        private void CacheComponents()
        {
            animator = GetComponent<Animator>();
            audioSource = GetComponent<AudioSource>();
            rigidBody = GetComponent<Rigidbody2D>();
            balloonCollider = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>() ?? GetComponentInChildren<SpriteRenderer>();

            if (spriteRenderer == null)
            {
                Debug.LogError($"No SpriteRenderer found on {gameObject.name}. Balloon will not display correctly.", this);
            }
        }

        public void SetBalloonColor(BalloonColorEnum color)
        {
            BalloonColorData colorData = GetBalloonColorData(color);
            if (colorData == null)
            {
                Debug.LogError($"No BalloonColorData found for color {color}. Make sure the balloonColorData array is properly configured.", this);
                return;
            }

            currentBalloonColorData = colorData;
            ApplyBalloonSprite(colorData);
        }

        /// <summary>
        /// Sets the balloon's float speed to a random value within the configured range.
        /// </summary>
        public void SetRandomFloatSpeed()
        {
            floatSpeed = Random.Range(minFloatSpeed, maxFloatSpeed);
        }

        /// <summary>
        /// Sets the balloon's float speed to a specific value.
        /// </summary>
        /// <param name="speed">The speed value to set</param>
        public void SetFloatSpeed(float speed)
        {
            floatSpeed = Mathf.Max(0f, speed); // Ensure speed is not negative
        }

        private void ApplyBalloonSprite(BalloonColorData colorData)
        {
            if (spriteRenderer == null || colorData.BalloonSprite == null) return;

            spriteRenderer.sprite = colorData.BalloonSprite;
            spriteRenderer.color = Color.white;

            // Disable animator if we're using sprite-based coloring
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                animator.enabled = false;
            }
        }

        private BalloonColorData GetBalloonColorData(BalloonColorEnum color)
        {
            if (balloonColorData == null || balloonColorData.Length == 0)
            {
                Debug.LogError("BalloonColorData array is null or empty! Configure it in the Inspector.", this);
                return null;
            }

            foreach (var data in balloonColorData)
            {
                if (data != null && data.Color == color)
                {
                    return data;
                }
            }

            return null;
        }

        public BalloonColorEnum GetBalloonColor()
        {
            return currentBalloonColorData?.Color ?? BalloonColorEnum.Blue;
        }

        public void Pop()
        {
            if (isPopped) return;

            // Additional protection: ensure the balloon is still active and hasn't been destroyed
            if (this == null || gameObject == null || !gameObject.activeInHierarchy) return;

            isPopped = true;
            PlayPopSound();
            PlayPopAnimation();
            DisableInteraction();
            NotifyGameManager();
            ScheduleDestroy();
        }

        private void PlayPopSound()
        {
            if (audioSource != null)
            {
                audioSource.Play();
            }
        }

        private void PlayPopAnimation()
        {
            // Trigger camera shake effect
            if (GameTween.Instance != null)
            {
                GameTween.Instance.ShakeCamera();
            }

            if (HasCustomPopAnimation())
            {
                StartCoroutine(PlaySpriteAnimation(currentBalloonColorData.PopAnimationSprites));
            }
            else if (animator != null)
            {
                animator.enabled = true;
                animator.SetTrigger("Pop");
            }
        }

        private bool HasCustomPopAnimation()
        {
            return currentBalloonColorData?.PopAnimationSprites != null &&
                   currentBalloonColorData.PopAnimationSprites.Length > 0;
        }

        private IEnumerator PlaySpriteAnimation(Sprite[] sprites)
        {
            if (animator != null)
            {
                animator.enabled = false;
            }

            foreach (var sprite in sprites)
            {
                if (spriteRenderer != null && sprite != null)
                {
                    spriteRenderer.sprite = sprite;
                }
                yield return new WaitForSeconds(1f / popAnimationFrameRate);
            }
        }

        private void DisableInteraction()
        {
            if (balloonCollider != null)
            {
                balloonCollider.enabled = false;
            }

            if (rigidBody != null)
            {
                rigidBody.gravityScale = 1f;
            }
        }

        private void NotifyGameManager()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ValidateScore(GetBalloonColor());
            }
        }

        private void ScheduleDestroy()
        {
            Destroy(gameObject, destroyDelay);
        }
    }
}