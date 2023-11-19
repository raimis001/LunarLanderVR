using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum ModuleStatus
{
    normal, crashed, landed
}

[Serializable]
public class Leg
{
    public string name;
    public bool contact = false;
    public Image image;
}
[Serializable]
public class SupportEngine
{
    public ParticleSystem effects;
    public GameObject[] objects;
    public AudioSource audio;

    bool _enabled = false;
    public bool enabled
    {
        get => _enabled;
        set
        {
            if (_enabled == value)
                return;

            _enabled = value;
            if (value)
            {
                effects.Play();
                audio.Play();
            }
            else
            {
                effects.Stop();
                audio.Stop();
            }

            foreach (GameObject obj in objects)
            {
                obj.SetActive(value);
            }
        }
    }
}

public class LunarModule : MonoBehaviour
{
    [Header("Ship controls")]
    public float torque = 3f;
    public float speed = 3f;
    public float accelerate = 50f;
    public float friction = 1;
    public float fuelConsum = 1;

    [Header("Land check")]
    public LayerMask moonMask;
    public Transform moonCheck;

    public GameObject[] crashObjects;
    public GameObject[] landingObjects;
    public TMP_Text descriptionText;

    [Header("UI")]
    public TMP_Text verticalSpeedText;
    public TMP_Text horizontalSpeedText;
    public TMP_Text rotationText;
    public TMP_Text distanceText;
    public Image distanceProgress;
    public Color legsContactColor;
    public Color legsAirColor;
    public Gradient groundProgressColors;
    public Image fuelProgress;
    public Gradient fuelProgressColors;

    public ParticleSystem mainEngineEffect;
    public GameObject mainEngineOjects;

    public List<Leg> legs;
    public List<SupportEngine> supportEngines;

    public float maxFuel = 50;
    float fuel;

    Rigidbody body;
    public static ModuleStatus moduleStatus = ModuleStatus.normal;

    string rotateString = "none";
    string moveString = "none";

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        fuel = maxFuel;
        foreach (Leg leg in legs)
            leg.image.color = legsAirColor;
        foreach (SupportEngine engine in supportEngines)
            engine.enabled = false;

        mainEngineOjects.SetActive(false);
    }

    private void Update()
    {
        if (moduleStatus != ModuleStatus.normal)
            return;

        if (transform.position.y > 150)
            Physics.gravity = new Vector3(0, 0.5f, 0);

        verticalSpeedText.text = body.velocity.y.ToString("0.0");
        Vector2 spd = new Vector2(body.velocity.x, body.velocity.z);

        spd = Vector2.Lerp(spd, Vector2.zero, Time.deltaTime * friction);

        horizontalSpeedText.text = spd.magnitude.ToString("0.0");
        body.velocity = new Vector3(spd.x, body.velocity.y, spd.y);

        rotationText.text = transform.eulerAngles.y.ToString("0");

        if (Physics.SphereCast(moonCheck.position, 1, Vector3.down, out RaycastHit hit, 100, moonMask))
        {
            distanceText.text = hit.distance.ToString("0.00");
            distanceProgress.fillAmount = hit.distance / 100f;
            distanceProgress.color = groundProgressColors.Evaluate(distanceProgress.fillAmount);
        }
        else
        {
            distanceText.text = "---";
            distanceProgress.fillAmount = 1;
            distanceProgress.color = groundProgressColors.Evaluate(1);
        }

        fuelProgress.fillAmount = fuel / maxFuel;
        fuelProgress.color = fuelProgressColors.Evaluate(fuelProgress.fillAmount);

        OperateSupport();        


        //Check for landing
        float eulerX = Mathf.Abs(transform.eulerAngles.x);
        float eulerZ = Mathf.Abs(transform.eulerAngles.z);

        if ( (eulerX > 5 && eulerX < 355) || ( eulerZ > 5 && eulerZ < 355))
        {
            Crash();
            descriptionText.text = string.Format("landing angle to high x{0:0} z:{1:0}", transform.eulerAngles.x, transform.eulerAngles.z);
            return;
        }

        int contacts = 0;
        foreach (Leg leg in legs)
            if (leg.contact)
                contacts++;

        if (contacts == 4)
        {
            if (body.velocity.magnitude > 1.5f)
            {
                Crash();
                descriptionText.text = string.Format("speed is to high {0:0.00}", body.velocity.magnitude);
                return;
            }


            Victory();
            return;
        }

        if (contacts > 0 && body.velocity.magnitude <= Mathf.Epsilon)
        {
            Crash();
            descriptionText.text = string.Format("no full contact to the moon {0}", contacts);
            return;
        }

    }

    public void Accelerate(XRHand hand, Interactive interact)
    {
        if (moduleStatus != ModuleStatus.normal)
        {
            if (mainEngineEffect.isPlaying)
                mainEngineEffect.Stop();

            mainEngineOjects.SetActive(false);
            return;
        }

        if (!body.useGravity)
        {
            if (mainEngineEffect.isPlaying)
                mainEngineEffect.Stop();

            mainEngineOjects.SetActive(false);
            return;
        }

        if (fuel <= 0)
        {
            if (mainEngineEffect.isPlaying)
                mainEngineEffect.Stop();

            mainEngineOjects.SetActive(false);
            return;
        }

        if (!hand.triggerHold)
        {
            if (mainEngineEffect.isPlaying)
                mainEngineEffect.Stop();

            mainEngineOjects.SetActive(false);
            return;
        }

        body.AddForce(transform.up * accelerate, ForceMode.Force);
        fuel -= Time.deltaTime * fuelConsum * 2;

        if (!mainEngineEffect.isPlaying)
        {
            mainEngineEffect.Play();
            mainEngineOjects.SetActive(true);
        }
    }
    public void AccelrateExit(XRHand hand, Interactive interact)
    {
        if (mainEngineEffect.isPlaying)
            mainEngineEffect.Stop();

        mainEngineOjects.SetActive(false);
    }

    public void Rotate(XRHand hand, Interactive interact)
    {
        rotateString = "none";
        if (moduleStatus != ModuleStatus.normal)
        {
            return;
        }

        if (!body.useGravity)
        {
            return;
        }

        if (fuel <= 0)
        {
            return;
        }

        if (!hand.triggerHold)
        {
            return;
        }

        rotateString = interact.id;
        float force = interact.id == "left" ? -1 : 1;
        body.AddTorque(transform.up * force * torque, ForceMode.Force);
        fuel -= Time.deltaTime * fuelConsum;
    }

    public void Move(XRHand hand, Interactive interact)
    {
        moveString = "none";
        if (moduleStatus != ModuleStatus.normal)
        {
            return;
        }

        if (!body.useGravity)
        {
            return;
        }

        if (fuel <= 0)
        {
            return;
        }

        if (!hand.triggerHold)
        {
            return;
        }

        moveString = interact.id;
        float force = interact.id == "back" ? -1 : 1;

        body.AddForce(transform.forward * force * speed, ForceMode.Impulse);

        fuel -= Time.deltaTime * fuelConsum;
    }

    public void SupprtExit(XRHand hand, Interactive interact)
    {
        if (moveString == interact.id)
            moveString = "none";
        if (rotateString == interact.id)
            rotateString = "none";
    }

    public void StartModule(XRHand hand, Interactive interact)
    {
        if (body.useGravity && moduleStatus == ModuleStatus.normal)
            return;

        if (!hand.triggerDown)
            return;


        transform.position = Vector3.one * 100f;
        transform.eulerAngles = Vector3.zero;

        body.velocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;    

        body.useGravity = true;

        foreach (GameObject go in landingObjects)
            go.SetActive(false);

        foreach (GameObject go in crashObjects)
            go.SetActive(false);

        descriptionText.text = "";
        fuel = maxFuel;

        foreach (Leg leg in legs)
        {
            leg.image.color = legsAirColor;
            leg.contact = false;
        }

        mainEngineOjects.SetActive(false);
        moduleStatus = ModuleStatus.normal;
        foreach (SupportEngine engine in supportEngines)
            engine.enabled = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.contacts.Length == 0)
            return;

        string key = collision.contacts[0].thisCollider.name;
        //Debug.Log(key);

        Leg leg = legs.Find((l) => l.name == key);
        if (leg == null)
            return;

        leg.contact = true;
        leg.image.color = legsContactColor;

    }
    private void OnCollisionExit(Collision collision)
    {


        if (collision.contactCount == 0)
            return;

        string key = collision.contacts[0].thisCollider.name;

        Leg leg = legs.Find((l) => l.name == key);
        if (leg == null)
            return;

        leg.contact = false;
        leg.image.color = legsAirColor;
    }

    void Crash()
    {
        moduleStatus = ModuleStatus.crashed;
        //Debug.Log(body.velocity.magnitude.ToString("0.000"));

        foreach (GameObject go in crashObjects)
            go.SetActive(true);


    }
    void Victory()
    {
        moduleStatus = ModuleStatus.landed;
        foreach (GameObject go in landingObjects)
            go.SetActive(true);
    }

    void OperateSupport()
    {
        if (moveString == "none")
        {
            if (rotateString == "none")
            {
                supportEngines[0].enabled = false;
                supportEngines[1].enabled = false;
                supportEngines[2].enabled = false;
                supportEngines[3].enabled = false;
                return;
            }

            if (rotateString == "left")
            {
                supportEngines[0].enabled = true;
                supportEngines[1].enabled = false;
                supportEngines[2].enabled = false;
                supportEngines[3].enabled = true;
                return;
            }
            if (rotateString == "right")
            {
                supportEngines[0].enabled = false;
                supportEngines[1].enabled = true;
                supportEngines[2].enabled = true;
                supportEngines[3].enabled = false;
                return;
            }

        }

        if (moveString == "forward")
        {
            supportEngines[0].enabled = true;
            supportEngines[1].enabled = false;
            supportEngines[2].enabled = true;
            supportEngines[3].enabled = false;
            return;
        }
        if (moveString == "back")
        {
            supportEngines[0].enabled = false;
            supportEngines[1].enabled = true;
            supportEngines[2].enabled = false;
            supportEngines[3].enabled = true;
            return;
        }
    }
}
