using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ReactionController : MonoBehaviour
{
    [Header("Animation (Body)")]
    [SerializeField] private Animator animator;
    [SerializeField] private string idleStateName = "Idle";


    [Header("Audio / LipSync")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip dialogueClip;
    [SerializeField] private SimpleLipSync lipSync;

    [Header("Facial BlendShapes")]
    [SerializeField] private SkinnedMeshRenderer faceMesh;

    // Your blendshape indexes
    private int smileBlend = 63;
    private int sadBlend = 66;

    [SerializeField] private float blendFadeSpeed = 6f;

    [Header("UI")]
    [SerializeField] private Button reactionButton;

    private bool isPlaying;
    private int pendingPlays;

    private void Awake()
    {
        if (reactionButton != null)
            reactionButton.onClick.AddListener(OnReactionButtonPressed);
    }

    private void OnDestroy()
    {
        if (reactionButton != null)
            reactionButton.onClick.RemoveListener(OnReactionButtonPressed);
    }

    public void OnReactionButtonPressed()
    {
        pendingPlays++;

        if (!isPlaying)
            StartCoroutine(PlayQueuedReactions());
    }

    private IEnumerator PlayQueuedReactions()
    {
        isPlaying = true;

        while (pendingPlays > 0)
        {
            pendingPlays--;

            // Play audio once at the beginning
            if (audioSource != null && dialogueClip != null && !audioSource.isPlaying)
            {
                audioSource.clip = dialogueClip;
                audioSource.Play();

                if (lipSync != null)
                    lipSync.Play(audioSource);

                yield return new WaitWhile(() => audioSource.isPlaying);
            }

            yield return PlaySequence();
        }

        // Return to idle at the end
        animator.enabled = false;

        // Reset face to neutral
        StartCoroutine(FadeBlend(smileBlend, 0));
        StartCoroutine(FadeBlend(sadBlend, 0));

        isPlaying = false;
    }

    private IEnumerator PlaySequence()
    {
        // Smile → Sad → Smile → Sad
        Debug.Log("Heelooo");
        animator.enabled = false;
        yield return PlayState(smileBlend);
        yield return PlayState(sadBlend);
       
    }

    private IEnumerator PlayState(int faceBlendIndex)
    {
        animator.enabled = false;

        // Fade IN facial expression
        StartCoroutine(FadeBlend(faceBlendIndex, 100));

        yield return new WaitForSeconds(1);

        // Fade OUT facial expression
        StartCoroutine(FadeBlend(faceBlendIndex, 0));
    }


    private IEnumerator FadeBlend(int index, float target)
    {
        if (faceMesh == null || index < 0)
            yield break;

        float current = faceMesh.GetBlendShapeWeight(index);

        while (Mathf.Abs(current - target) > 0.5f)
        {
            current = Mathf.Lerp(current, target, Time.deltaTime * blendFadeSpeed);
            Debug.Log(index + "  " + current);
            faceMesh.SetBlendShapeWeight(index, current);
            yield return null;
        }

        faceMesh.SetBlendShapeWeight(index, target);
        animator.enabled=true;
    }
}