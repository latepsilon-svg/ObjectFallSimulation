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

        string velocityText = $"Velocidad: ({dragSimulation.velocity.x:F2}, {dragSimulation.velocity.y:F2}, {dragSimulation.velocity.z:F2}) m/s";
        string heightText = $"Altura: {dragSimulation.currentHeight:F2} m";
        string speedMagnitude = $"Rapidez: {dragSimulation.velocity.magnitude:F2} m/s";

        currentData.text = $"{velocityText}\n{heightText}\n{speedMagnitude}";

        if (!dragSimulation.isSimulating)
        {
            Simulating = false;
        }
    }

    public FunctionDraw VvsT;

    [ContextMenu("Updateest")]
    public void UpdateEstAmounts()
    {
        float m = mass.slider.value;
        float g = Mathf.Abs(gravity.slider.value);
        float rho = airDensity.slider.value;
        float A = proyectedArea.slider.value;
        float Cd = dragCoefficient.slider.value;
        float h0 = initialHeight.slider.value;
        float v0 = -initialSpeed.slider.value;
    
        float denominador = rho * A * Cd;
        float terminalVel = 0;

        target.transform.position = new Vector3(0, h0, 0);
    
        if (denominador > 0.0001f && g > 0)
        {
            terminalVel = Mathf.Sqrt((2 * m * g) / denominador);
            ESTt.text = $"v\u20D7<sub>t</sub> est. = {terminalVel:F2} m/s\n";
        }
        else
        {
            ESTt.text = $"v\u20D7<sub>t</sub> est. = \u221E (Vac\u00EDo)\n";
        }
    
        float time = 0;
    
        if (g <= 0)
        {
            time = float.PositiveInfinity;
            ESTt.text += $"t<sub>f</sub> est. = \u221E";
        }
        else if (denominador <= 0.0001f)
        {
            time = (-v0 + Mathf.Sqrt(v0 * v0 + 2 * g * h0)) / g;
            ESTt.text += $"t<sub>f</sub> est. = {time:F2} s";
        }
        else
        {
            float v2 = terminalVel * terminalVel;
            float eTerm = Mathf.Exp(-2 * g * h0 / v2);
            float vFinal = terminalVel * Mathf.Sqrt(1 - (1 - (v0 * v0) / v2) * eTerm);
    
            float vFinalSafe = Mathf.Clamp(vFinal, 0, terminalVel * 0.9999f);
            float v0Safe = Mathf.Clamp(v0, 0, terminalVel * 0.9999f);
    
            float t1 = Mathf.Log((terminalVel + vFinalSafe) / (terminalVel - vFinalSafe));
            float t2 = Mathf.Log((terminalVel + v0Safe) / (terminalVel - v0Safe));
            time = (terminalVel / (2 * g)) * (t1 - t2);
    
            ESTt.text += $"t<sub>f</sub> est. = {Mathf.Abs(time):F2} s";
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
