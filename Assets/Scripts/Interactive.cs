using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Interactive : MonoBehaviour
{
    public string id;

    public UnityEvent<XRHand, Interactive> OnHandEnter;
    public UnityEvent<XRHand, Interactive> OnHandExit;
    public UnityEvent<XRHand, Interactive> OnHandStay;


    public void HandEnter(XRHand hand)
    {
        OnHandEnter?.Invoke(hand, this);
    }

    public void HandExit(XRHand hand)
    {
        OnHandExit?.Invoke(hand, this);
    }

    public void HandStay(XRHand hand)
    {
        OnHandStay?.Invoke(hand, this);
    }

}
