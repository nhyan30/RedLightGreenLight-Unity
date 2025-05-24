using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;

public class Girl : MonoBehaviour
{
    [SerializeField] AudioSource girlSingingAudioSource;
    [SerializeField] AudioSource rotationAudioSource;
    [SerializeField] AudioClip girlSinging;
    [SerializeField] AudioClip rotateSoundClip;
    [SerializeField] TextMeshProUGUI TimeLeft;

    [SerializeField] float totalTime = 60f; // 60 seconds 
    [SerializeField] float breakTime = 4f; // 4-second break
    readonly float initialSoundDuration = 5f;  // Initial duration of the sound in seconds
    readonly float finalSoundDuration = 2.5f;  // Final duration of the sound in seconds 
    float elapsedTime = 0f;
    bool isPlaying = false;
    Coroutine rotationCoroutine = null;
    Coroutine killCoroutine = null;
    Player player;
    Transform head;  // Girl's head
    bool scanning = false;
    List<NPC> npcs = new List<NPC>();


    void Start()
    {
        StartCoroutine(WaitForGameStart());
    }

    void Awake()
    {
        if(girlSingingAudioSource == null || girlSinging == null || rotationAudioSource == null || rotateSoundClip == null)
        {
            Debug.LogError("Audio source or Sound clips not assigned!");
            return;
        }

        girlSingingAudioSource.clip = girlSinging;
        girlSingingAudioSource.loop = false;

        rotationAudioSource.clip = rotateSoundClip;
        rotationAudioSource.loop = false;

        // Get the head object 
        head = transform.Find("DollHead");

        // Find GameObject with tag Player
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

        // Find all GameObjects with tag NPC
        npcs.AddRange(GameObject.FindGameObjectsWithTag("NPC").Select(npc => npc.GetComponent<NPC>()));
    }

    void Update()
    {
        if (!GameManager.Instance.GameStarted) return;
        if (elapsedTime < totalTime)
        {
            elapsedTime += Time.deltaTime;

            if (!isPlaying) // if girl tune is not playing yet 
            {
                killCoroutine = null;
                float currentSoundDuration = Mathf.Lerp(initialSoundDuration, finalSoundDuration, elapsedTime / totalTime);
                girlSingingAudioSource.pitch = initialSoundDuration / currentSoundDuration; // Adjust pitch to change speed 
                EventManager.Instance.TriggerEvent("GirlSinging", currentSoundDuration);
                girlSingingAudioSource.Play();
                isPlaying = true;

                // Schedule stopping the sound after its current duration 
                Invoke(nameof(StopSound), currentSoundDuration);
            }
        }

        if (elapsedTime >= totalTime)
        {
            killCoroutine = null;
            if (!player.PlayerIsDead())
            {
                player.KillPlayer();
            }
            killCoroutine = StartCoroutine(KillNPCs(npcs.ToList()));
            return;
        }

        if (scanning)
        {
            if (player.IsMoving)
            {
                player.KillPlayer();
            }
            if (killCoroutine == null)
            {
                killCoroutine = StartCoroutine(KillNPCs(npcs.Where(npc => npc.IsMoving).ToList()));
            }
        }
    }

    IEnumerator KillNPCs(List<NPC> npcs)
    {
        foreach (NPC npc in npcs)
        {
            if (npc.PlayerIsDead()) continue;
            npc.KillPlayer();
            yield return new WaitForSeconds(Random.Range(0f, 0.4f));
        }
    }

    void StopSound()
    {
        girlSingingAudioSource.Stop();
        RotateHead();

        // Ensure the next play happens after the break time
        Invoke(nameof(ResumePlayback), breakTime);
    }

    void ResumePlayback()
    {
        isPlaying = false;
        scanning = false;
        RotateHead(true);
    }

    void RotateHead(bool rotateBack = false)
    {
        if(rotationCoroutine != null)
        {
            StopCoroutine(rotationCoroutine);
        }
        rotationAudioSource.Play();
        rotationCoroutine = StartCoroutine(RotateHeadOverTime(0.2f, rotateBack));
    }

    IEnumerator RotateHeadOverTime(float seconds, bool rotateBack = false)
    {
        float elapsedTime = 0;
        Quaternion startRotation = head.rotation;
        Quaternion endRotation = Quaternion.Euler(0, rotateBack ? 0 : 180, 0);

        while (elapsedTime < seconds)
        {
            head.rotation = Quaternion.Slerp(startRotation, endRotation, elapsedTime / seconds);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        scanning = !rotateBack;
    }

    IEnumerator BroadcastTimer()
    {
        while (elapsedTime < totalTime)
        {
            EventManager.Instance.TriggerEvent("TimerUpdate", totalTime - elapsedTime);
            yield return null;
        }
        TimeLeft.text = "<color=red>Time is up!</color>";
    }

    void UpdateTimeUI(object remainingTime)
    {
        float secondsLeft = (float)remainingTime;
        TimeLeft.text = $"Time left: <color=red>{secondsLeft:F2}</color>";
    }

    IEnumerator WaitForGameStart()
    {
        yield return new WaitUntil(() => GameManager.Instance.GameStarted);
        EventManager.Instance.Subscribe("TimerUpdate", UpdateTimeUI);
        StartCoroutine(BroadcastTimer());
    }
}
