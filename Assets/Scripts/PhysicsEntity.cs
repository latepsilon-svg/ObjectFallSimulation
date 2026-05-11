using UnityEngine;

public class PhysicsEntity : MonoBehaviour
{
    public float gravityAcceleration = 9.8f;
    public float airResistanceCoefficient = 0f;

    public Vector3 position;
    public Vector3 velocity;
    public Vector3 acceleration;
    
    private float timeElapsed = 0f;
    
    void Awake()
    {
        Application.targetFrameRate = 60;
    }
    
    void Start()
    {
        position = transform.position;
        velocity = Vector3.zero;
        acceleration = Vector3.zero;
    }

    void Update()
    {
        if (!Manager.Instance.isRunning) return;
        
        FixedStep(Time.deltaTime);
    }

    void FixedStep(float dt)
    {
        acceleration.y = -gravityAcceleration;
        acceleration.x = 0f;
        acceleration.z = 0f;

        velocity += acceleration * dt;
        position += velocity * dt;
        
        transform.position = position;
        
        timeElapsed += dt;
    }

    public void Reset()
    {
        position = Vector3.zero;
        velocity = Vector3.zero;
        acceleration = Vector3.zero;
        timeElapsed = 0f;
    }

    public float GetTimeElapsed() => timeElapsed;
    public float GetSpeed() => velocity.magnitude;
    public float GetKineticEnergy(float mass) => 0.5f * mass * velocity.sqrMagnitude;
}