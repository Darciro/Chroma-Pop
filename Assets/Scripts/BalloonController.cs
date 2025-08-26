using System.Collections;
using UnityEngine;

[System.Serializable]
public class BalloonColorData
{
    public BalloonColorEnum color;
    public Sprite balloonSprite;          // The main balloon sprite
    public Sprite[] popAnimationSprites; // Array of sprites for pop animation
}
public class BalloonController : MonoBehaviour
{
    public float floatSpeed = 3f;
    [SerializeField] private BalloonColorData[] balloonColorData;
    [SerializeField] private BalloonColorData currentBalloonColorData;

    private Animator animator;
    private AudioSource audioSource;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }

    void FixedUpdate()
    {
        transform.Translate(Vector3.up * floatSpeed * Time.deltaTime);
    }

    public void SetBalloonColor(BalloonColorEnum color)
    {
        // Find the matching color data
        BalloonColorData colorData = GetBalloonColorData(color);
        if (colorData != null)
        {
            currentBalloonColorData = colorData;

            if (spriteRenderer != null && colorData.balloonSprite != null)
            {
                spriteRenderer.sprite = colorData.balloonSprite;
                spriteRenderer.color = Color.white;

                // Force the SpriteRenderer to update
                spriteRenderer.enabled = false;
                spriteRenderer.enabled = true;

                if (animator != null && animator.runtimeAnimatorController != null && animator.enabled)
                {
                    animator.enabled = false;
                }
            }
        }
        else
        {
            Debug.LogError($"No BalloonColorData found for color {color}. Make sure the balloonColorData array is properly set up in the Inspector.");
        }
    }

    private BalloonColorData GetBalloonColorData(BalloonColorEnum color)
    {
        if (balloonColorData == null || balloonColorData.Length == 0)
        {
            Debug.LogError("BalloonColorData array is null or empty! Please set it up in the Inspector.");
            return null;
        }

        for (int i = 0; i < balloonColorData.Length; i++)
        {
            if (balloonColorData[i] != null && balloonColorData[i].color == color)
            {
                return balloonColorData[i];
            }
        }
        return null;
    }

    public BalloonColorEnum GetBalloonColor()
    {
        return currentBalloonColorData != null ? currentBalloonColorData.color : BalloonColorEnum.Blue;
    }


    public void PlayPopAnimation()
    {
        if (currentBalloonColorData != null && currentBalloonColorData.popAnimationSprites.Length > 0)
        {
            StartCoroutine(PlaySpriteAnimation(currentBalloonColorData.popAnimationSprites));
        }
    }

    private IEnumerator PlaySpriteAnimation(Sprite[] sprites)
    {
        float frameRate = 12f;

        for (int i = 0; i < sprites.Length; i++)
        {
            if (spriteRenderer != null && sprites[i] != null)
            {
                spriteRenderer.sprite = sprites[i];
            }
            yield return new WaitForSeconds(1f / frameRate);
        }
    }

    public void Pop()
    {
        audioSource.Play();

        if (currentBalloonColorData != null && currentBalloonColorData.popAnimationSprites != null && currentBalloonColorData.popAnimationSprites.Length > 0)
        {
            if (animator != null)
                animator.enabled = false;

            PlayPopAnimation();
        }
        else if (animator != null)
        {
            animator.enabled = true;
            animator.SetTrigger("Pop");
        }

        if (rb != null)
        {
            rb.gravityScale = 1f;
        }

        // Handle the score
        if (GameManager.Instance == null)
        {
            return;
        }
        GameManager.Instance.ValidateScore(currentBalloonColorData.color);

        // Disable the sprite and collider immediately so it can't be popped again
        GetComponent<Collider2D>().enabled = false;

        // Destroy the balloon object after the sound has had time to play
        Destroy(gameObject, 1.5f);
    }
}