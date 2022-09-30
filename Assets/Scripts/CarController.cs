using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    private Rigidbody rb;

    [Header("Suspention")]
    [SerializeField] private float restLength;
    [SerializeField] private float springTravel;
    [SerializeField] private float springStiffness;
    [SerializeField] private float damperStiffness;

    private float minLength;
    private float maxLength;
    private float springVelocity;
    private float springForce;
    private float damperForce;

    private Vector3 suspensionForce;

    [Header("Wheels")]
    [SerializeField] private float wheelRadius;
    [SerializeField] private Wheel[] wheels;
    
    [System.Serializable]
    public struct Wheel
    {
        public GameObject mobel;
        public Transform raycastStart;
        public WHEELPOSITION wheelPosition;

        [HideInInspector] public float lastLenght;
        [HideInInspector] public float springLength;
    }

    public enum WHEELPOSITION
    {
        NONE,
        FRONT,
        REAR
    }

    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();

        minLength = restLength - springTravel;
        maxLength = restLength + springTravel;
    }

    private void FixedUpdate()
    {

        // Wheel suspention
        for(int i = 0; i < wheels.Length; i++)
        {
            RaycastHit info;
            Debug.DrawRay(wheels[i].raycastStart.position, -transform.up, Color.red);
            if (Physics.Raycast(wheels[i].raycastStart.position, -transform.up, out info, maxLength + wheelRadius))
            {
                wheels[i].lastLenght = wheels[i].springLength;
                wheels[i].springLength = info.distance - wheelRadius;
                wheels[i].springLength = Mathf.Clamp(wheels[i].springLength, minLength, maxLength);
                springVelocity = (wheels[i].lastLenght - wheels[i].springLength) / Time.fixedDeltaTime;
                springForce = springStiffness * (restLength - wheels[i].springLength);
                damperForce = damperStiffness * springVelocity;

                suspensionForce = (springForce + damperForce) * transform.up;
                rb.AddForceAtPosition(suspensionForce, info.point);
            }
        }
    }
}
