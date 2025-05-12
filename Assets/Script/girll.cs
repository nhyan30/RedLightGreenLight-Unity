using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class girll : MonoBehaviour
{
    [SerializeField] AudioSource grilSingingAudioSource;
    [SerializeField] AudioSource rotaionAudioSource;

    [SerializeField] AudioClip grilSinging;
    [SerializeField] AudioClip rotatSoundClip;

    [SerializeField] float totalTime = 70f;
    [SerializeField] float breakTime = 4f;

    readonly float initialSoundDuration = 5f;
    readonly float finalSoundDuration = 2.5f;

    float elapsedTime = 0f;
    
    bool isPlaying = false; 

    Coroutine rotationCoroutine = null;
    Coroutine KillCoroutine = null;

    Player player;

    Transform head;

    bool scanning = false;

    List<NPC> npcs = new List<NPC>();

     void Awake()
    {
        if (grilSingingAudioSource == null || grilSinging == null || rotaionAudioSource == null || rotatSoundClip == null)
        {
            Debug.LogError("Audio sources or sound clips not assigned!");
            return;
        }

        grilSingingAudioSource.clip = grilSinging;
        grilSingingAudioSource.loop = false;

        rotaionAudioSource.clip = rotatSoundClip;
        rotaionAudioSource.loop = false ;

        head = transform.Find("DollHead");

        player = GameObject.FindWithTag("Player").GetComponent<Player>();

        npcs.AddRange(GameObject.FindGameObjectsWithTag("NPC").Select(npc => npc.GetComponent<NPC>()));

    }

     void Update()
    {
        if (elapsedTime < totalTime)
        {
            elapsedTime += Time.deltaTime;

            if (!isPlaying )
            {
                KillCoroutine = null;
                float currentSoundDuration = Mathf.Lerp(initialSoundDuration, finalSoundDuration, elapsedTime / totalTime);
                grilSingingAudioSource.pitch = initialSoundDuration / currentSoundDuration; //Adjust pitch to change speed
                EventManager.Instance.TriggerEvent("GirlSinging", currentSoundDuration);
                grilSingingAudioSource.Play();  
                isPlaying = true;   

                Invoke(nameof(StopSound), currentSoundDuration);    
            }
        }

        if (elapsedTime >= totalTime)
        {
            KillCoroutine = null;
            if (!player.PlayerIsDead())

            {
                player.KillPlayer();
            }
            KillCoroutine = StartCoroutine(KillNPCs(npcs.ToList()));
            return;
        }

        if(scanning)
        {
            if (player.IsMoving)
            {
                player.KillPlayer();
            }
            if (KillCoroutine == null)
            {
                KillCoroutine = StartCoroutine(KillNPCs(npcs.Where(npc => npc.IsMoving).ToList()));
            }
        }
    }

    IEnumerator KillNPCs(List<NPC> npcs)
    {
        foreach (var npc in npcs)
        {
            if (npc.PlayerIsDead()) continue;
            npc.KillPlayer();
            yield return new WaitForSeconds(Random.Range(0f, 0.4f));
        }

    }


    void StopSound()
    {
        grilSingingAudioSource.Stop();
        RotateHead();
    
        Invoke(nameof(ResumePlayback), breakTime);

    }

    void ResumePlayback()
    {
        isPlaying = false;  
        scanning = false;
        RotateHead(true);
    
}

    void RotateHead(bool roateBack = false) 
    {
        if (rotationCoroutine != null)
        {
            StopCoroutine(rotationCoroutine);
        }
        rotaionAudioSource.Play();
        rotationCoroutine = StartCoroutine(RotateHeadOverTime(0.2f, roateBack));   
    } 

    IEnumerator RotateHeadOverTime (float seconds, bool rotateBack = false)
    {
        float elapsedTime = 0;
        Quaternion startRotation = head.rotation;
        Quaternion endRotation = Quaternion.Euler(0, rotateBack ? 0 : 180, 0);

        while (elapsedTime < seconds)
        {
            head.rotation = Quaternion.Slerp(startRotation, endRotation,elapsedTime/ seconds);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        scanning = !rotateBack;
    }


}
