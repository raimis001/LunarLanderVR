using Unity.XR.CoreUtils;
using UnityEngine;

public class HandCollider : MonoBehaviour
{

    XRHand hand;
    void Awake()
    {
        hand = GetComponentInParent<XRHand>();
    }


    private void OnTriggerEnter(Collider other)
    {

        if (!hand.mask.Contains(other.gameObject.layer))
            return;

        Interactive interact = other.GetComponentInParent<Interactive>();
        if (!interact)
            return;

        interact.HandEnter(hand);

    }
    private void OnTriggerExit(Collider other)
    {

        if (!hand.mask.Contains(other.gameObject.layer))
            return;

        Interactive interact = other.GetComponentInParent<Interactive>();
        if (!interact)
            return;

        interact.HandExit(hand);

    }
    private void OnTriggerStay(Collider other)
    {

        if (!hand.mask.Contains(other.gameObject.layer))
            return;

        Interactive interact = other.GetComponentInParent<Interactive>();
        if (!interact)
            return;

        interact.HandStay(hand);

    }


}
