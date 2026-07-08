using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using static AllContorl;

public class PlayerLife : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private Vector3 levelSpawnPosition;

    [SerializeField] private AudioSource deathSoundEffect;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Text livesDisplayText;

    [Header("踩陷阱：仍有生命时闪烁")]
    [Tooltip("不指定则自动查找自身或子物体上的 SpriteRenderer")]
    [SerializeField] private SpriteRenderer blinkSpriteRenderer;
    [SerializeField] private int trapBlinkCount = 4;
    [SerializeField] private float blinkToggleInterval = 0.1f;
    [SerializeField] private float postTrapInvincibilityDuration = 1.5f;
    [SerializeField] private float trapEscapeKnockbackY = 4f;

    [Header("命用尽：死亡回出生点并重置为 3 条命")]
    [SerializeField] private float outOfLivesRespawnDelay = 0.35f;

    private bool deathStarted;
    private float trapInvincibleUntil;
    private SerialController serialController;

    private bool blinkUseLateAlpha;
    private float blinkForcedAlpha = 1f;

    private void Start()
    {
        ResetLivesStateForCurrentScene();

        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = blinkSpriteRenderer != null
            ? blinkSpriteRenderer
            : (GetComponent<SpriteRenderer>() ?? GetComponentInChildren<SpriteRenderer>(true));
        if (spriteRenderer == null)
            Debug.LogWarning("PlayerLife：未找到 SpriteRenderer，陷阱闪烁不会显示。请在 Inspector 指定 Blink Sprite Renderer，或给角色/子物体挂上 SpriteRenderer。", this);

        serialController = FindObjectOfType<SerialController>();

        levelSpawnPosition = spawnPoint != null ? spawnPoint.position : transform.position;

        RefreshLivesDisplay();
    }

    private static void ResetLivesStateForCurrentScene()
    {
        GameManager.Instance.playerLives = 3;
    }

    private void LateUpdate()
    {
        if (!blinkUseLateAlpha || spriteRenderer == null)
            return;

        var c = spriteRenderer.color;
        c.a = blinkForcedAlpha;
        spriteRenderer.color = c;
    }

    private void RefreshLivesDisplay()
    {
        if (livesDisplayText == null)
            return;

        livesDisplayText.text = $"Last lives: {GameManager.Instance.playerLives}";
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Trap"))
            return;

        if (Time.time < trapInvincibleUntil || deathStarted)
            return;

        deathStarted = true;
        StartCoroutine(HandleTrapHit());
    }

    private IEnumerator HandleTrapHit()
    {
        GameManager.Instance.score = 0;

        var gm = GameManager.Instance;
        gm.playerLives--;
        if (serialController != null)
            serialController.NotifyPlayerLostLife();

        if (gm.playerLives > 0)
        {
            RefreshLivesDisplay();
            yield return RunBlinkAfterDamage();
            rb.velocity = new Vector2(rb.velocity.x, trapEscapeKnockbackY);
            trapInvincibleUntil = Time.time + postTrapInvincibilityDuration;
            deathStarted = false;
            yield break;
        }

        RefreshLivesDisplay();
        yield return RunOutOfLivesDeathAndRespawn();
    }

    private IEnumerator RunBlinkAfterDamage()
    {
        if (spriteRenderer != null)
        {
            Color colorBeforeBlink = spriteRenderer.color;
            blinkUseLateAlpha = true;
            try
            {
                for (int i = 0; i < trapBlinkCount; i++)
                {
                    blinkForcedAlpha = 0f;
                    yield return new WaitForSeconds(blinkToggleInterval);
                    blinkForcedAlpha = 1f;
                    yield return new WaitForSeconds(blinkToggleInterval);
                }
            }
            finally
            {
                blinkUseLateAlpha = false;
                spriteRenderer.color = colorBeforeBlink;
            }
        }
        else
        {
            float wait = blinkToggleInterval * 2f * trapBlinkCount;
            yield return new WaitForSeconds(wait);
        }
    }

    private IEnumerator RunOutOfLivesDeathAndRespawn()
    {
        if (deathSoundEffect != null)
            deathSoundEffect.Play();

        rb.velocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;
        anim.SetTrigger("death");

        yield return new WaitForSeconds(outOfLivesRespawnDelay);

        transform.position = levelSpawnPosition;
        GameManager.Instance.playerLives = 3;
        // 恢复满命后必须让开发板收到 [R]，三盏 LED 重新亮起（见 SerialController.NotifyLivesReset）
        if (serialController != null)
            serialController.NotifyLivesReset();

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.velocity = Vector2.zero;
        anim.Rebind();
        anim.Update(0f);

        RefreshLivesDisplay();
        trapInvincibleUntil = Time.time + postTrapInvincibilityDuration;
        deathStarted = false;
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
