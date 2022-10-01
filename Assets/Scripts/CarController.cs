using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    private Rigidbody rb;
    private bool isGrounded;
    private bool breaking;

    [Header("Movement")]
    [SerializeField] private Transform applyForcePoint;
    [SerializeField] private float engineForce;
    [SerializeField] private float aireDrag;
    [SerializeField] private float rollingResistance;
    [SerializeField] private float breakingForce;

    private Vector3 dragForce;
    private Vector3 rollingResistanceForce;
    private Vector3 tractionForce;

    private float carMasse;
    private Vector3 acceleration;
    private Vector3 velocity;
    private Vector3 lastFrameVelocity;

    private Vector3 movementForce;

    private float verticalInput;
    private float horizontalInput;

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

        carMasse = rb.mass;
    }

    private void Update()
    {
        InputHandle();

        //Debug.DrawRay(new Vector3(transform.position.x, transform.position.y + 1), rb.velocity, Color.red);
        //Debug.DrawRay(new Vector3(transform.position.x, transform.position.y + 2), tractionForce, Color.green);
    }

    private void FixedUpdate()
    {
        isGrounded = false;

        // Wheel suspention
        for (int i = 0; i < wheels.Length; i++)
        {
            RaycastHit info;
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
                isGrounded = true;

                wheels[i].mobel.transform.position = wheels[i].raycastStart.position + -transform.up * (info.distance - wheelRadius/2);
                Debug.DrawLine(wheels[i].raycastStart.position, info.point, Color.green);
            }
            else
            {
                Debug.DrawRay(wheels[i].raycastStart.position, -transform.up * maxLength, Color.red);
                wheels[i].mobel.transform.position = wheels[i].raycastStart.position + -transform.up * maxLength;
            }
        }

        // Movement
        // -------------------- A REFAIRE - MOYEN --------------------
        
        // PREMIÉRE MÉTHODE : add force à un point en dessous du centre de gravité
        // EngineForce -> 8000              RollingResistance -> 400
        // AireDrag -> 60                   BreakingForce -> 1000
        /*if (isGrounded)
        {
            tractionForce = transform.forward * (engineForce * verticalInput);
            dragForce = -aireDrag * rb.velocity * rb.velocity.magnitude;
            rollingResistanceForce = -rollingResistance * rb.velocity;

            if(!breaking)
                movementForce = tractionForce + dragForce + rollingResistanceForce;
            else
            {
                if (rb.velocity.z > 0.5f)
                    movementForce = (-transform.forward * breakingForce) + dragForce + rollingResistanceForce;
                else if (rb.velocity.z < -0.5f)
                    movementForce = (transform.forward * breakingForce) + dragForce + rollingResistanceForce;
                else
                    movementForce = dragForce + rollingResistanceForce;
            }

            rb.AddForceAtPosition(movementForce, applyForcePoint.position);
        }*/



        // DEUXIEME MÉTHODE : par incrémentation de la position
        if (isGrounded)
        {
            tractionForce = transform.forward * (engineForce * verticalInput);
            dragForce = -aireDrag * velocity * velocity.magnitude;
            rollingResistanceForce = -rollingResistance * velocity;

            movementForce = tractionForce + dragForce + rollingResistanceForce;
            acceleration = movementForce / carMasse;

            velocity = velocity + Time.fixedDeltaTime * acceleration;
            transform.root.position = transform.root.position + Time.fixedDeltaTime * velocity;
        }
    }

    private void InputHandle()
    {
        verticalInput = Input.GetAxisRaw("Vertical");
        horizontalInput = Input.GetAxisRaw("Horizontal");

        breaking = Input.GetKey(KeyCode.Space);
    }
}
