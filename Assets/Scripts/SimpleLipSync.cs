using System.Collections;
using UnityEngine;

public class SimpleLipSync : MonoBehaviour
{
    [Header("Animator (Body Only Animation)")]
    [SerializeField] private Animator animator;
    [SerializeField] private string talkAnimState = "talk";
    [SerializeField] private string idleAnimState = "Idle";

    [Header("Face Mesh / Blendshape")]
    [SerializeField] private SkinnedMeshRenderer faceMesh;
    [SerializeField] private int mouthOpenBlendShapeIndex = 0;
    [SerializeField] private float mouthOpenMax = 80f;

    [Header("Motion")]
    [SerializeField] private float speed = 14f;

  

    private Coroutine currentRoutine;

    public void Play(AudioSource audioSource)
    {
      // animator.enabled = false; // 
        if (animator != null)
            animator.CrossFade(talkAnimState, 0.15f);
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(LipSyncRoutine(audioSource));
    }

    private IEnumerator LipSyncRoutine(AudioSource audioSource)
    {
        if (faceMesh == null || audioSource == null)
            yield break;

        // Small random offset so it doesn't look robotic
        float offset = Random.Range(0f, 2f * Mathf.PI);

        while (audioSource.isPlaying)
        {
            float t = Time.time * speed + offset;

            // 0..1 based on sine
            float normalized = Mathf.Sin(t) * 0.5f + 0.5f;
            float weight = normalized * mouthOpenMax;
            Debug.Log(mouthOpenBlendShapeIndex + " " + weight);
            faceMesh.SetBlendShapeWeight(mouthOpenBlendShapeIndex, weight);

            yield return null;
        }

        // Reset mouth closed
        faceMesh.SetBlendShapeWeight(mouthOpenBlendShapeIndex, 0f);
       // animator.enabled = true;
        animator.CrossFade(idleAnimState, 0.01f);
        currentRoutine = null;
    }
}
