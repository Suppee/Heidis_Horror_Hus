using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Key : Interactable
{
    public enum TriggerType { OneTime, OneTimeSequence };

    [Header("Trigger Mode")]
    public TriggerType triggerMode;

    [Header("One Time Trigger Settings")]
    public UnityEvent onetimeEvents;

    [Header("Sequence Trigger Settings")]
    public List<UnityEvent> sequenceEvents;
    public List<float> sequenceTiming;
    bool firstTime = true;

    [SerializeField] private AudioClip ItemCollected;
    [SerializeField] private AudioSource audioSource;
    public string keycode;

    public override void Interact()
    {
        playerController.keyring.Add(this);

        if (gameObject.GetComponent<AudioSource>() != null && gameObject.GetComponent<AudioSource>().isPlaying == false)
            AudioSource.PlayClipAtPoint(ItemCollected, transform.position);

        EventCheck();

        gameObject.GetComponent<BoxCollider>().enabled = false;

        if (gameObject.GetComponent<MeshCollider>() != null)
            gameObject.GetComponent<MeshCollider>().enabled = false;

        gameObject.GetComponent<MeshRenderer>().enabled = false;
    }

    void EventCheck()
    {
        if (firstTime)
        {
            firstTime = false;
            switch (triggerMode)
            {
                case TriggerType.OneTime:
                    onetimeEvents.Invoke();
                    gameObject.SetActive(false);
                    break;

                case TriggerType.OneTimeSequence:
                    StartCoroutine(Sequence());
                    break;

                default:
                    break;
            }
        }
    }

    IEnumerator Sequence()
    {
        for (int i = 0; i < sequenceEvents.Count; i++)
        {
            yield return new WaitForSeconds(sequenceTiming[i]);
            sequenceEvents[i].Invoke();
        }

        gameObject.SetActive(false);
    }

}
