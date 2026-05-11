using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Manager : MonoBehaviour
{
    public static Manager Instance { get; private set; }

    public Button runStop;
    public TextMeshPro infoText;
    public PhysicsEntity physicsEntity;

    public Vector3 infoPositionOffset;

    public bool isRunning = false;
    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            DestroyImmediate(gameObject);        
    }
    
    void Start()
    {
        runStop.onClick.AddListener(() => SwitchRun());
    }
    void Update()
    {
        UpdateInfoPosition();
    }

    void SwitchRun()
    {
        isRunning = !isRunning;
    }
    const string vectorNotation = "\u20D7";
    void UpdateInfoPosition()
    {
        if (physicsEntity == null || infoText == null) return;
        Vector3 infoPos = new Vector3(physicsEntity.transform.position.x, physicsEntity.transform.position.y, 0) + infoPositionOffset;
        infoText.transform.position = infoPos;
        Vector3 velocity = physicsEntity.velocity;
        Vector3 position = physicsEntity.position;
        infoText.text = vec("v") + " = " + $"({velocity.x:F2}i + {velocity.y:F2}j + {velocity.z:F2}k)\n";
        infoText.text += vec("p") + " = " + $"({position.x:F2}i + {position.y:F2}j + {position.z:F2}k)\n";
    }
    
    string vec(string ins)
    {
        return ins + vectorNotation;
    }
}
