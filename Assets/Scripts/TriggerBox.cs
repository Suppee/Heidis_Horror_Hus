using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TriggerBox : MonoBehaviour
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

    private void OnTriggerEnter(Collider other)
    {
        if (firstTime)
        {
            firstTime = false;
            switch (triggerMode)
            {
                case TriggerType.OneTime:
                    onetimeEvents.Invoke();
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
        for(int i = 0; i < sequenceEvents.Count; i++)
        {
            yield return new WaitForSeconds(sequenceTiming[i]);
            sequenceEvents[i].Invoke();
        }
    }
}
