using UnityEngine;
using UnityEngine.InputSystem;

public class XRHand : MonoBehaviour
{
    public LayerMask mask;
    public float checkRadius = 0.1f;

    [SerializeField]
    private InputAction triggerAction;
    [SerializeField]
    private InputAction gripAction;
    [SerializeField]
    private InputAction primaryAction;
    [SerializeField]
    private InputAction secondaryAction;

    [SerializeField]
    private Transform interactPoint;

    public bool triggerHold => triggerAction.IsPressed();
    public bool triggerDown => triggerAction.triggered;

    private void OnEnable()
    {
        triggerAction.Enable();
        gripAction.Enable();
        primaryAction.Enable();
        secondaryAction.Enable();
    }

    Interactive selected;
    private void Update()
    {
        Collider[] cols = Physics.OverlapSphere(interactPoint.position, checkRadius, mask);
        if (cols.Length < 1 ) 
        {
            if (selected)
            {
                selected.HandExit(this);
                selected = null;
            }
            return;
        }
        Interactive s = null;
        foreach (Collider col in cols)
        {
            s = col.GetComponent<Interactive>();
            if (s)
                break;
        }
        if (!s)
        {
            if (selected)
            {
                selected.HandExit(this);
                selected = null;
            }
            return;
        }

        if (s == selected)
        {
            selected.HandStay(this);
            return;
        }

        if (selected)
            selected.HandExit(this);

        selected = s;
        selected.HandEnter(this);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(interactPoint.position, checkRadius);
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawSphere(interactPoint.position, checkRadius);
    }
}
