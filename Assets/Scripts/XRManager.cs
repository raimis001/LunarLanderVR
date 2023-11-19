using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class XRManager : MonoBehaviour
{

    public static Action StartXRHeadset;

    public float playerHeight = 1.6f;
    public Transform cameraPivot;

    public TrackedPoseDriver driver;

    IEnumerator Start()
    {
        while (driver.trackingStateInput.action.ReadValue<int>() == 0)
            yield return null;

        yield return null;

        StartXRHeadset?.Invoke();

        Transform cam = Camera.main.transform;   

        float camY = playerHeight - cam.localPosition.y;

        cameraPivot.localPosition = new Vector3(0,camY, 0);
    }

}
