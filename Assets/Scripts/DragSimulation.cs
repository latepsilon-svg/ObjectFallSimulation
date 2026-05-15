using TMPro;
using UnityEngine;

public class DragSimulation : MonoBehaviour
{
    [HideInInspector] public Vector3 velocity = Vector3.zero;
    [HideInInspector] public float currentHeight = 0;
    [HideInInspector] public bool isSimulating = false;

    private float mass;
    private float projectedArea;
    private float dragCoefficient;
    private float gravity;
    private float airDensity;
    private float initialHeight;
    private float initialSpeed;

    [Range(0.0001f, 1)] public float timeScale = 1;


    private Vector3 initialPosition;
    public float elapsedTime = 0;
    Transform parent;
    float startTime;
    TextMeshProUGUI timeText;
    void Start()
    {
        parent = transform.parent;
        timeText = SimulationManager.Instance.elapsedTime;
    }
    public void StartSimulation(float m, float area, float cd, float g, float rho, float h0, float v0)
    {
        startTime = Time.time;
        mass = m;
        projectedArea = area;
        dragCoefficient = cd;
        gravity = Mathf.Abs(g);
        airDensity = rho;
        initialHeight = h0;
        initialSpeed = v0;

        initialPosition = transform.position;
        currentHeight = initialHeight;

        velocity = Vector3.down * initialSpeed;

        isSimulating = true;
        elapsedTime = 0;
    }

    void FixedUpdate()
    {
        if (!isSimulating) return;

        float delta = Time.fixedDeltaTime * timeScale;

        float dragForce = 0.5f * airDensity * projectedArea * dragCoefficient * velocity.magnitude * velocity.magnitude;

        Vector3 dragDirection = velocity.normalized;
        Vector3 dragVector = -dragDirection * dragForce;

        Vector3 gravityForce = Vector3.down * mass * gravity;

        Vector3 acceleration = (gravityForce + dragVector) / mass;
        velocity += acceleration * delta;

        transform.position += velocity * delta;

        currentHeight = transform.position.y;

        elapsedTime += delta;

        timeText.text = $"T: {elapsedTime:F3}s";
        if (currentHeight <= 0)
        {
            currentHeight = 0;
            velocity = Vector3.zero;
            isSimulating = false;
            transform.position = Vector3.zero;
        }
    }

    public void ChangeTime(float entryValue)
    {
        timeScale = entryValue;
    }

    public void StopSimulation()
    {
        isSimulating = false;
        velocity = Vector3.zero;
    }

    public void ResetSimulation()
    {
        StopSimulation();
        transform.parent = parent;
        transform.position = initialPosition;
        currentHeight = initialHeight;
        elapsedTime = 0;
    }
}