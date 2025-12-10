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
    [Header("Settings")]
    public float sensitivity = 25f;
    public float smooth = 7f;



    private Coroutine currentRoutine;

    public void Play(AudioSource audioSource)
    {
       animator.enabled = false; // 
        //if (animator != null)
        //    animator.CrossFade(talkAnimState, 0.15f);
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
        float[] samples = new float[256];

        while (audioSource.isPlaying)
        {
            audioSource.GetOutputData(samples, 0);

            float sum = 0f;
            for (int i = 0; i < samples.Length; i++)
                sum += samples[i] * samples[i];

            float rms = Mathf.Sqrt(sum / samples.Length);
            float mouthWeight = Mathf.Clamp01(rms * sensitivity) * 40f;

            float current = faceMesh.GetBlendShapeWeight(mouthOpenBlendShapeIndex);
            float smoothWeight = Mathf.Lerp(current, mouthWeight, Time.deltaTime * smooth);

            faceMesh.SetBlendShapeWeight(mouthOpenBlendShapeIndex, smoothWeight);

            yield return null;
        }


        // Reset mouth closed
        faceMesh.SetBlendShapeWeight(mouthOpenBlendShapeIndex, 0f);
        
        
       // animator.enabled = true;
      //  animator.CrossFade(idleAnimState, 0.15f);
        currentRoutine = null;
    }
}
