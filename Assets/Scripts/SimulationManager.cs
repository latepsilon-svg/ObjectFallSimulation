using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class SimulationManager : MonoBehaviour
{
    public static SimulationManager Instance;
    public SimulationPreset[] presets;
    public bool Simulating = false;
    public SimPreset presetsL;

    public GameObject target;
    public TMP_Dropdown dropdown;

    private GameObject currentInstance;
    public DragSimulation dragSimulation;

    public TextMeshProUGUI currentData;
    public TextMeshProUGUI ESTt;
    public TextMeshProUGUI elapsedTime;
    public TextMeshProUGUI fps;

    public PropertySlot mass;
    public PropertySlot proyectedArea;
    public PropertySlot dragCoefficient;
    public PropertySlot initialHeight;
    public PropertySlot initialSpeed;
    public PropertySlot gravity;
    public PropertySlot airDensity;



    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        dropdown.ClearOptions();
        List<string> options = new List<string>();

        for (int i = 0; i < presets.Length; i++)
        {
            options.Add(presets[i].name);
        }

        dropdown.AddOptions(options);

        mass.slider.onValueChanged.AddListener(_ => UpdateEstAmounts());
        proyectedArea.slider.onValueChanged.AddListener(_ => UpdateEstAmounts());
        dragCoefficient.slider.onValueChanged.AddListener(_ => UpdateEstAmounts());
        initialHeight.slider.onValueChanged.AddListener(_ => UpdateEstAmounts());
        initialSpeed.slider.onValueChanged.AddListener(_ => UpdateEstAmounts());
        gravity.slider.onValueChanged.AddListener(_ => UpdateEstAmounts());
        airDensity.slider.onValueChanged.AddListener(_ => UpdateEstAmounts());


        SelectPreset(0);
        dropdown.onValueChanged.AddListener(SelectPreset);

        UpdateEstAmounts();
    }
    public CinemachineThirdPersonFollow shit;
    Vector3 rot;
    void Update()
    {
        if (Simulating && dragSimulation != null)
        {
            UpdateCurrentData();
        }
        fps.text = (1 / Time.deltaTime).ToString("F2");

        if (currentInstance != null)
        {
            currentInstance.transform.position = target.transform.position;
        }
    }
    public void SelectPreset(int preset)
    {
        if (Simulating) return;
        if (currentInstance != null)
        {
            Destroy(currentInstance);
        }

        target.transform.position = Vector3.zero;
        currentInstance = Instantiate(presetsL.find(presets[preset].prefab), target.transform.position, target.transform.rotation);

        if (currentInstance != null && currentInstance.TryGetComponent<DragSimulation>(out DragSimulation dragSim))
        {
            dragSimulation = dragSim;
        }

        mass.slider.value = presets[preset].mass;
        proyectedArea.slider.value = presets[preset].area;
        dragCoefficient.slider.value = presets[preset].dragoCoefficent;
    }

    public void StartSimulation()
    {
        if (Simulating || dragSimulation == null) return;

        Simulating = true;

        dragSimulation.StartSimulation(
            mass.slider.value,
            proyectedArea.slider.value,
            dragCoefficient.slider.value,
            gravity.slider.value,
            airDensity.slider.value,
            initialHeight.slider.value,
            -initialSpeed.slider.value
        );
    }


    public void StopSimulation()
    {
        if (!Simulating || dragSimulation == null) return;

        Simulating = false;
        dragSimulation.StopSimulation();
    }

    public void ResetSimulation()
    {
        if (dragSimulation == null) return;

        Simulating = false;
        dragSimulation.ResetSimulation();
    }


    void UpdateCurrentData()
    {
        if (dragSimulation == null) return;

        if (!dragSimulation.isSimulating)
        {
            Simulating = false;
            return;
        }

        string velocityText = $"v\u20D7 = ({dragSimulation.velocity.x:F2}, {dragSimulation.velocity.y:F2}, {dragSimulation.velocity.z:F2}) m/s";
        string heightText = $"x\u20D7 = {dragSimulation.currentHeight:F2} m";

        currentData.text = $"{velocityText}\n{heightText}";

    }

    public VvsTGraph VvsT;

    public void UpdateEstAmounts()
    {
        // VALIDACIÓN: VvsT debe estar asignado
        if (VvsT == null)
        {
            Debug.LogWarning("VvsT no está asignado en el Inspector");
            return;
        }

        float m = Mathf.Max(0.0001f, mass.slider.value);
        float g = Mathf.Abs(gravity.slider.value);
        float rho = Mathf.Max(0, airDensity.slider.value);
        float A = Mathf.Max(0, proyectedArea.slider.value);
        float Cd = Mathf.Max(0, dragCoefficient.slider.value);
        float h0 = Mathf.Max(0, initialHeight.slider.value);
        float v0 = Mathf.Abs(initialSpeed.slider.value);

        float denominador = rho * A * Cd;
        float terminalVel = 0;

        target.transform.position = new Vector3(0, h0, 0);

        if (denominador > 0.0000001f && g > 0)
        {
            terminalVel = Mathf.Sqrt((2 * m * g) / denominador);
            ESTt.text = $"v\u20D7<sub>t</sub> est. = {terminalVel:F2} m/s\n";
        }
        else
        {
            ESTt.text = $"v\u20D7<sub>t</sub> est. = \u221E (Vacío)\n";
        }

        VvsT.airDensity = rho;
        VvsT.dragCoefficient = Cd;
        VvsT.proyectedArea = A;
        VvsT.mass = m;
        VvsT.gravity = g;

        float time = 0;

        if (g <= 0)
        {
            time = float.PositiveInfinity;
            ESTt.text += $"t<sub>f</sub> est. = \u221E";
        }
        else if (denominador <= 0.0001f)
        {
            // Sin arrastre: caída libre
            float discriminant = v0 * v0 + 2 * g * h0;
            if (discriminant < 0) discriminant = 0;
            time = (v0 + Mathf.Sqrt(discriminant)) / g;
            ESTt.text += $"t<sub>f</sub> est. = {time:F2} s";
        }
        else
        {
            // Con arrastre: fórmula compleja
            float v2 = terminalVel * terminalVel;
            if (v2 < 0.0001f)
            {
                // terminalVel muy pequeño, usar aproximación de caída libre
                float discriminant = v0 * v0 + 2 * g * h0;
                if (discriminant < 0) discriminant = 0;
                time = (v0 + Mathf.Sqrt(discriminant)) / g;
                ESTt.text += $"t<sub>f</sub> est. = {time:F2} s";
            }
            else
            {
                float eTerm = Mathf.Exp(-2 * g * h0 / v2);

                // Validación: asegurar que el argumento de sqrt sea válido
                float sqrtArg = 1 - (1 - (v0 * v0) / v2) * eTerm;
                sqrtArg = Mathf.Max(0, sqrtArg);

                float vFinal = terminalVel * Mathf.Sqrt(sqrtArg);

                float vFinalSafe = Mathf.Clamp(vFinal, 0, terminalVel * 0.9999999f);
                float v0Safe = Mathf.Clamp(v0, 0, terminalVel * 0.9999999f);

                // Validación: evitar log de números inválidos
                float denomLog1 = terminalVel - vFinalSafe;
                float denomLog2 = terminalVel - v0Safe;

                // CORRECCIÓN CRÍTICA: Validar denominadores de forma robusta
                if (denomLog1 > 0.0001f && denomLog2 > 0.0001f)
                {
                    float logArg1 = (terminalVel + vFinalSafe) / denomLog1;
                    float logArg2 = (terminalVel + v0Safe) / denomLog2;

                    // Validar argumentos de log
                    if (logArg1 > 0.00001f && logArg2 > 0.00001f && !float.IsInfinity(logArg1) && !float.IsInfinity(logArg2))
                    {
                        float t1 = Mathf.Log(logArg1);
                        float t2 = Mathf.Log(logArg2);
                        time = (terminalVel / (2 * g)) * (t1 - t2);

                        // Validación final: asegurarse de que time sea válido
                        if (float.IsNaN(time) || float.IsInfinity(time) || time < 0)
                        {
                            // Fallback: usar caída libre
                            float discriminant = v0 * v0 + 2 * g * h0;
                            if (discriminant < 0) discriminant = 0;
                            time = (v0 + Mathf.Sqrt(discriminant)) / g;
                        }

                        ESTt.text += $"t<sub>f</sub> est. = {Mathf.Abs(time):F2} s";
                    }
                    else
                    {
                        // Argumentos de log inválidos, usar fallback
                        Debug.LogWarning("[UpdateEstAmounts] Argumentos de log inválidos, usando caída libre");
                        float discriminant = v0 * v0 + 2 * g * h0;
                        if (discriminant < 0) discriminant = 0;
                        time = (v0 + Mathf.Sqrt(discriminant)) / g;
                        ESTt.text += $"t<sub>f</sub> est. = {time:F2} s";
                    }
                }
                else
                {
                    // Denominadores de log inválidos, usar fallback
                    Debug.LogWarning("[UpdateEstAmounts] Denominadores de log inválidos, usando caída libre");
                    float discriminant = v0 * v0 + 2 * g * h0;
                    if (discriminant < 0) discriminant = 0;
                    time = (v0 + Mathf.Sqrt(discriminant)) / g;
                    ESTt.text += $"t<sub>f</sub> est. = {time:F2} s";
                }
            }
        }

        // VALIDACIÓN FINAL: Asegurar que time sea un valor válido para SetDomain
        if (float.IsNaN(time) || float.IsInfinity(time) || time <= 0)
        {
            time = 10f; // Valor por defecto seguro
        }

        // Clampear time a un rango razonable para evitar valores gigantes
        time = Mathf.Clamp(time, 0.1f, 1000f);

        VvsT.SetDomain(0, time);

        VvsT.ComputeVvsT();
    }
}

[System.Serializable]
public struct SimulationPreset
{
    public string name;
    public float mass;
    public float area;
    public float dragoCoefficent;
    public string prefab;
}