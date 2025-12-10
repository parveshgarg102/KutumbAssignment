using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ReactionController : MonoBehaviour
{
    [Header("Animator")]
    public Animator animator;
    public string idleAnim = "Idle";
    public string smileAnim = "Smile";
    public string sadAnim = "Sad";

    [Header("Audio + LipSync")]
    public AudioSource audioSource;
    public AudioClip dialogueClip;
    public SimpleLipSync lipSync;

    [Header("Face BlendShapes")]
    public SkinnedMeshRenderer face;
    public int smileIndex = 44;
    public int sadIndex = 47;
    public float emotionSpeed = 6f;

    [Header("Emotion Sensitivity")]
    public float smileThreshold = 0.03f;  // louder parts
    public float sadThreshold = 0.015f;  // softer parts

    [Header("UI")]
    public Button playButton;

    private float smileWeight = 0;
    private float sadWeight = 0;
    private float targetSmile = 0;
    private float targetSad = 0;

    private void Start()
    {
        playButton.onClick.AddListener(StartSequence);
    }

    void Update()
    {
        if (!audioSource.isPlaying)
        {
            ResetFace();
            return;
        }

        // --- AUDIO ANALYSIS ---
        float[] samples = new float[256];
        audioSource.GetOutputData(samples, 0);

        float sum = 0f;
        for (int i = 0; i < samples.Length; i++)
            sum += samples[i] * samples[i];

        float rms = Mathf.Sqrt(sum / samples.Length); // amplitude

        // --- EMOTION DECISION ---
        if (rms > smileThreshold)
        {
            // More energy → Smile
            targetSmile = 100;
            targetSad = 0;
            animator.CrossFade(smileAnim, 0.1f);
        }
        else if (rms < sadThreshold)
        {
            // Low energy → Sad
            targetSmile = 0;
            targetSad = 100;
            animator.CrossFade(sadAnim, 0.1f);
        }

        // --- APPLY EMOTIONS SMOOTHLY ---
        float currentSmile = face.GetBlendShapeWeight(smileIndex);
        float currentSad = face.GetBlendShapeWeight(sadIndex);

        currentSmile = Mathf.Lerp(currentSmile, targetSmile, Time.deltaTime * emotionSpeed);
        currentSad = Mathf.Lerp(currentSad, targetSad, Time.deltaTime * emotionSpeed);

        face.SetBlendShapeWeight(smileIndex, currentSmile);
        face.SetBlendShapeWeight(sadIndex, currentSad);
    }

    void ResetFace()
    {
        
       face.SetBlendShapeWeight(smileIndex, 0);
        face.SetBlendShapeWeight(sadIndex, 0);
       
        //animator.CrossFade(idleAnim, 0.1f);
    }

    private void StartSequence()
    {
        StartCoroutine(PlayDialog());
    }

    private IEnumerator PlayDialog()
    {
        animator.enabled = false;
        audioSource.clip = dialogueClip;
        audioSource.Play();

        lipSync.Play(audioSource);

        animator.CrossFade(idleAnim, 0.1f);

        // Wait until audio ends
        yield return new WaitWhile(() => audioSource.isPlaying);

        // Reset face
        targetSmile = 0;
        targetSad = 0;
        StartCoroutine(FadeBlend(smileIndex, 80));
        //animator.CrossFade(idleAnim, 0.1f);
         animator.enabled = true;
    }

    private IEnumerator FadeBlend(int index, float target)
    {
        float current = face.GetBlendShapeWeight(index);

        while (Mathf.Abs(current - target) > 0.5f)
        {
            current = Mathf.Lerp(current, target, Time.deltaTime * emotionSpeed);
            face.SetBlendShapeWeight(index, current);
            yield return null;
        }

        face.SetBlendShapeWeight(index, target);
       // animator.enabled = true;
    }
}