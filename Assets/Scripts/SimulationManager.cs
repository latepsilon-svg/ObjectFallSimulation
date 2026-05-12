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

        string velocityText = $"v\u20D7 = ({dragSimulation.velocity.x:F2}, {dragSimulation.velocity.y:F2}, {dragSimulation.velocity.z:F2}) m/s";
        string heightText = $"x\u20D7 = {dragSimulation.currentHeight:F2} m";

        currentData.text = $"{velocityText}\n{heightText}";

        if (!dragSimulation.isSimulating)
        {
            Simulating = false;
        }
    }

    public VvsTGraph VvsT;

    public void UpdateEstAmounts()
    {
        try
        {
            Debug.Log("[UpdateEstAmounts] Iniciando...");

            // VALIDACIÓN 1: VvsT debe estar asignado
            if (VvsT == null)
            {
                Debug.LogError("[UpdateEstAmounts] ERROR CRÍTICO: VvsT es NULL. Asígnalo en el Inspector.");
                return;
            }
            Debug.Log("[UpdateEstAmounts] VvsT validado correctamente");

            // VALIDACIÓN 2: Obtener valores de sliders con seguridad
            if (mass == null || mass.slider == null)
            {
                Debug.LogError("[UpdateEstAmounts] ERROR: mass o mass.slider es NULL");
                return;
            }

            float m = Mathf.Max(0.0001f, mass.slider.value);
            float g = Mathf.Abs(gravity.slider.value);
            float rho = Mathf.Max(0, airDensity.slider.value);
            float A = Mathf.Max(0, proyectedArea.slider.value);
            float Cd = Mathf.Max(0, dragCoefficient.slider.value);
            float h0 = Mathf.Max(0, initialHeight.slider.value);
            float v0 = Mathf.Abs(initialSpeed.slider.value);

            Debug.Log($"[UpdateEstAmounts] Valores: m={m}, g={g}, rho={rho}, A={A}, Cd={Cd}, h0={h0}, v0={v0}");

            float denominador = rho * A * Cd;
            float terminalVel = 0;

            // VALIDACIÓN 3: Actualizar posición del target
            if (target != null)
            {
                target.transform.position = new Vector3(0, h0, 0);
            }

            Debug.Log($"[UpdateEstAmounts] Denominador={denominador}");

            // Cálculo de velocidad terminal
            if (denominador > 0.0000001f && g > 0)
            {
                terminalVel = Mathf.Sqrt((2 * m * g) / denominador);
                Debug.Log($"[UpdateEstAmounts] Velocidad terminal calculada: {terminalVel:F2} m/s");
                ESTt.text = $"v\u20D7<sub>t</sub> est. = {terminalVel:F2} m/s\n";
            }
            else
            {
                Debug.Log("[UpdateEstAmounts] Vacío detectado (sin arrastre)");
                ESTt.text = $"v\u20D7<sub>t</sub> est. = \u221E (Vacío)\n";
            }

            // VALIDACIÓN 4: Asignar valores a VvsT con seguridad
            Debug.Log("[UpdateEstAmounts] Asignando valores a VvsT...");
            VvsT.airDensity = rho;
            VvsT.dragCoefficient = Cd;
            VvsT.proyectedArea = A;
            VvsT.mass = m;
            VvsT.gravity = g;
            Debug.Log("[UpdateEstAmounts] Valores asignados a VvsT correctamente");

            float time = 0;

            Debug.Log("[UpdateEstAmounts] Calculando tiempo final...");

            if (g <= 0)
            {
                time = float.PositiveInfinity;
                Debug.Log("[UpdateEstAmounts] g <= 0, tiempo = infinito");
                ESTt.text += $"t<sub>f</sub> est. = \u221E";
            }
            else if (denominador <= 0.0001f)
            {
                // Sin arrastre: caída libre
                Debug.Log("[UpdateEstAmounts] Modo caída libre (sin arrastre significativo)");
                float discriminant = v0 * v0 + 2 * g * h0;
                if (discriminant < 0)
                {
                    Debug.LogWarning($"[UpdateEstAmounts] Discriminante negativo ({discriminant}), clampeando a 0");
                    discriminant = 0;
                }
                time = (v0 + Mathf.Sqrt(discriminant)) / g;
                Debug.Log($"[UpdateEstAmounts] Tiempo caída libre: {time:F2} s");
                ESTt.text += $"t<sub>f</sub> est. = {time:F2} s";
            }
            else
            {
                // Con arrastre: fórmula compleja
                Debug.Log("[UpdateEstAmounts] Modo con arrastre (fórmula compleja)");
                float v2 = terminalVel * terminalVel;

                if (v2 < 0.0001f)
                {
                    Debug.LogWarning("[UpdateEstAmounts] v2 muy pequeño, tiempo = infinito");
                    ESTt.text += $"t<sub>f</sub> est. = \u221E";
                }
                else
                {
                    Debug.Log($"[UpdateEstAmounts] v2={v2:F6}");

                    float eTerm = Mathf.Exp(-2 * g * h0 / v2);
                    Debug.Log($"[UpdateEstAmounts] eTerm={eTerm:F6}");

                    // Validación: asegurar que el argumento de sqrt sea válido
                    float sqrtArg = 1 - (1 - (v0 * v0) / v2) * eTerm;
                    Debug.Log($"[UpdateEstAmounts] sqrtArg antes de clamp={sqrtArg:F6}");

                    if (sqrtArg < 0)
                    {
                        Debug.LogWarning($"[UpdateEstAmounts] sqrtArg negativo ({sqrtArg}), clampeando a 0");
                    }
                    sqrtArg = Mathf.Max(0, sqrtArg);
                    Debug.Log($"[UpdateEstAmounts] sqrtArg después de clamp={sqrtArg:F6}");

                    float vFinal = terminalVel * Mathf.Sqrt(sqrtArg);
                    Debug.Log($"[UpdateEstAmounts] vFinal={vFinal:F6}");

                    float vFinalSafe = Mathf.Clamp(vFinal, 0, terminalVel * 0.9999999f);
                    float v0Safe = Mathf.Clamp(v0, 0, terminalVel * 0.9999999f);

                    Debug.Log($"[UpdateEstAmounts] vFinalSafe={vFinalSafe:F6}, v0Safe={v0Safe:F6}");

                    // Validación: evitar log de números inválidos
                    float denomLog1 = terminalVel - vFinalSafe;
                    float denomLog2 = terminalVel - v0Safe;

                    Debug.Log($"[UpdateEstAmounts] denomLog1={denomLog1:F6}, denomLog2={denomLog2:F6}");

                    if (denomLog1 > 0.0001f && denomLog2 > 0.0001f)
                    {
                        Debug.Log("[UpdateEstAmounts] Calculando logs...");
                        float logArg1 = (terminalVel + vFinalSafe) / denomLog1;
                        float logArg2 = (terminalVel + v0Safe) / denomLog2;

                        Debug.Log($"[UpdateEstAmounts] logArg1={logArg1:F6}, logArg2={logArg2:F6}");

                        if (logArg1 <= 0 || logArg2 <= 0)
                        {
                            Debug.LogError($"[UpdateEstAmounts] Argumentos de log inválidos: logArg1={logArg1}, logArg2={logArg2}");
                            ESTt.text += $"t<sub>f</sub> est. = \u221E (Error matemático)";
                        }
                        else
                        {
                            float t1 = Mathf.Log(logArg1);
                            float t2 = Mathf.Log(logArg2);
                            Debug.Log($"[UpdateEstAmounts] t1={t1:F6}, t2={t2:F6}");

                            time = (terminalVel / (2 * g)) * (t1 - t2);
                            Debug.Log($"[UpdateEstAmounts] Tiempo calculado (antes de validación): {time:F6}");

                            if (float.IsNaN(time))
                            {
                                Debug.LogError("[UpdateEstAmounts] Tiempo es NaN, seteando a infinito");
                                time = float.PositiveInfinity;
                            }
                            else if (float.IsInfinity(time))
                            {
                                Debug.LogWarning("[UpdateEstAmounts] Tiempo es infinito");
                            }

                            ESTt.text += $"t<sub>f</sub> est. = {Mathf.Abs(time):F2} s";
                        }
                    }
                    else
                    {
                        Debug.LogWarning("[UpdateEstAmounts] Denominadores de log inválidos");
                        ESTt.text += $"t<sub>f</sub> est. = \u221E";
                    }
                }
            }

            Debug.Log($"[UpdateEstAmounts] Tiempo final: {time}");
            Debug.Log("[UpdateEstAmounts] Llamando a VvsT.SetDomain...");

            VvsT.SetDomain(0, time);

            Debug.Log("[UpdateEstAmounts] Llamando a VvsT.ComputeVvsT...");
            VvsT.ComputeVvsT();

            Debug.Log("[UpdateEstAmounts] ¡Completado exitosamente!");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[UpdateEstAmounts] EXCEPCIÓN CAPTURADA: {ex.Message}\n{ex.StackTrace}");
        }
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