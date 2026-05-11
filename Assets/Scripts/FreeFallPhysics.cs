using UnityEngine;

/// <summary>
/// Simulador de caída libre con resistencia del aire
/// Implementa ecuaciones de movimiento con y sin resistencia aerodinámica
/// </summary>
public class FreeFallPhysics : MonoBehaviour
{
    [System.Serializable]
    public class SimulationParameters
    {
        [Header("Parámetros Básicos")]
        public float objectMass = 1f; // kg
        public float initialHeight = 100f; // metros
        public float initialVelocity = 0f; // m/s
        public bool useAirResistance = true;

        [Header("Resistencia del Aire")]
        [Tooltip("Coeficiente de resistencia aerodinámica (0.1-2.0)")]
        public float dragCoefficient = 0.47f; // esfera estándar
        [Tooltip("Área frontal en m²")]
        public float frontalArea = 0.5f;
        [Tooltip("Densidad del aire kg/m³ (1.225 a nivel del mar)")]
        public float airDensity = 1.225f;

        [Header("Geometría (Afecta Cd y Área)")]
        public ObjectShape objectShape = ObjectShape.Sphere;
        public float objectRadius = 0.5f;

        [Header("Simulación")]
        public float timeScale = 1f;
        public float fixedDeltaTime = 0.01f;
    }

    public enum ObjectShape
    {
        Sphere,      // Cd ≈ 0.47
        Cube,        // Cd ≈ 1.05
        Cylinder,    // Cd ≈ 0.82
        Streamlined  // Cd ≈ 0.04
    }

    [SerializeField] private SimulationParameters parameters = new SimulationParameters();

    // Estado actual
    private float currentTime = 0f;
    private float currentPosition = 0f; // posición en Y (0 = punto inicial)
    private float currentVelocity = 0f; // m/s (positivo = hacia abajo)
    private float currentAcceleration = 0f;

    // Propiedades calculadas
    private float dragForce = 0f;
    private float terminalVelocity = 0f;

    // Control
    private bool isSimulating = false;
    private bool hasCrashed = false;

    // Eventos
    public System.Action<SimulationState> OnPhysicsUpdate;
    public System.Action OnCrash;

    [System.Serializable]
    public struct SimulationState
    {
        public float time;
        public float position;
        public float velocity;
        public float acceleration;
        public float dragForce;
        public float terminalVelocity;
        public float kineticEnergy;
        public float potentialEnergy;
        public float totalEnergy;
    }

    private void Start()
    {
        ResetSimulation();
    }

    private void FixedUpdate()
    {
        if (!isSimulating || hasCrashed)
            return;

        SimulationStep(parameters.fixedDeltaTime * parameters.timeScale);
    }

    /// <summary>
    /// Realiza un paso de simulación física
    /// </summary>
    private void SimulationStep(float deltaTime)
    {
        // Gravedad
        float gravity = 9.81f;

        // Calcular fuerza de resistencia del aire
        if (parameters.useAirResistance)
        {
            // F_drag = 0.5 * ρ * v² * Cd * A
            dragForce = 0.5f * parameters.airDensity * 
                       (currentVelocity * currentVelocity) * 
                       GetDragCoefficient() * 
                       GetFrontalArea();

            // Aceleración: a = g - (F_drag / m)
            currentAcceleration = gravity - (dragForce / parameters.objectMass);

            // Terminal velocity: v_t = sqrt((2*m*g) / (ρ*Cd*A))
            terminalVelocity = Mathf.Sqrt(
                (2f * parameters.objectMass * gravity) / 
                (parameters.airDensity * GetDragCoefficient() * GetFrontalArea())
            );
        }
        else
        {
            dragForce = 0f;
            currentAcceleration = gravity;
            terminalVelocity = float.MaxValue;
        }

        // Euler Integration: v = v + a*dt
        currentVelocity += currentAcceleration * deltaTime;

        // Limitar velocidad a velocidad terminal
        if (parameters.useAirResistance)
        {
            currentVelocity = Mathf.Min(currentVelocity, terminalVelocity);
        }

        // x = x + v*dt
        currentPosition += currentVelocity * deltaTime;
        currentTime += deltaTime;

        // Verificar colisión
        if (currentPosition >= parameters.initialHeight)
        {
            currentPosition = parameters.initialHeight;
            hasCrashed = true;
            OnCrash?.Invoke();
        }

        // Emitir evento de actualización
        OnPhysicsUpdate?.Invoke(GetCurrentState());
    }

    /// <summary>
    /// Obtiene el coeficiente de arrastre basado en la forma
    /// </summary>
    private float GetDragCoefficient()
    {
        return parameters.dragCoefficient switch
        {
            _ when parameters.objectShape == ObjectShape.Sphere => 0.47f,
            _ when parameters.objectShape == ObjectShape.Cube => 1.05f,
            _ when parameters.objectShape == ObjectShape.Cylinder => 0.82f,
            _ when parameters.objectShape == ObjectShape.Streamlined => 0.04f,
            _ => parameters.dragCoefficient
        };
    }

    /// <summary>
    /// Calcula el área frontal basada en la geometría
    /// </summary>
    private float GetFrontalArea()
    {
        return parameters.objectShape switch
        {
            ObjectShape.Sphere => Mathf.PI * parameters.objectRadius * parameters.objectRadius,
            ObjectShape.Cube => (2 * parameters.objectRadius) * (2 * parameters.objectRadius),
            ObjectShape.Cylinder => Mathf.PI * parameters.objectRadius * (2 * parameters.objectRadius),
            ObjectShape.Streamlined => Mathf.PI * parameters.objectRadius * parameters.objectRadius * 0.5f,
            _ => parameters.frontalArea
        };
    }

    public SimulationState GetCurrentState()
    {
        return new SimulationState
        {
            time = currentTime,
            position = currentPosition,
            velocity = currentVelocity,
            acceleration = currentAcceleration,
            dragForce = dragForce,
            terminalVelocity = terminalVelocity,
            kineticEnergy = 0.5f * parameters.objectMass * currentVelocity * currentVelocity,
            potentialEnergy = parameters.objectMass * 9.81f * (parameters.initialHeight - currentPosition),
            totalEnergy = 0.5f * parameters.objectMass * currentVelocity * currentVelocity + 
                         parameters.objectMass * 9.81f * (parameters.initialHeight - currentPosition)
        };
    }

    public void StartSimulation()
    {
        if (!isSimulating)
        {
            isSimulating = true;
            hasCrashed = false;
        }
    }

    public void PauseSimulation()
    {
        isSimulating = false;
    }

    public void ResumeSimulation()
    {
        if (!hasCrashed)
            isSimulating = true;
    }

    public void ResetSimulation()
    {
        isSimulating = false;
        hasCrashed = false;
        currentTime = 0f;
        currentPosition = 0f;
        currentVelocity = parameters.initialVelocity;
        currentAcceleration = 0f;
        dragForce = 0f;
        terminalVelocity = 0f;
    }

    // Getters
    public SimulationParameters GetParameters() => parameters;
    public void SetParameters(SimulationParameters newParams) => parameters = newParams;
    public bool IsSimulating => isSimulating;
    public bool HasCrashed => hasCrashed;
    public float CurrentTime => currentTime;
}
