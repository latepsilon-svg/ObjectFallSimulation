using UnityEngine;

public class VvsTGraph : FunctionDraw
{
    public float airDensity;
    public float dragCoefficient;
    public float proyectedArea;
    public float mass;
    public float gravity;
    VvsTFunction newF;
    public override void Start()
    {
        ClearPoints();
        newF = new VvsTFunction();
    }

    public void ComputeVvsT()
    {
        float k = airDensity * dragCoefficient * proyectedArea / (2 * mass);
        newF.gravity = gravity;
        newF.k = k;

        ClearPoints();

        float from = originDomain;
        float to = finalDomain;

        float counter = from;

        print($"from {counter} to {to}");
        while (counter < to)
        {
            if (newF == null)
            {
                print("XD");
            }
            float fx = newF.Function(counter);
            SetPoint(counter, fx);
            counter += step;
        }
    }
    public override void SetDomain(float origin, float final)
    {
        originDomain = origin;
        finalDomain = final;
        start.text = origin.ToString("F0");
        float step = final - origin;
        bool needDecimal = step % domains.Length != 0;
        step /= domains.Length;
        string decimals = needDecimal ? "F1" : "F0";
        for (int i = 0; i < domains.Length; i++)
        {
            domains[i].text = (origin + step * (i + 1)).ToString(decimals);
        }
        FromFuncToLocalDomainRatio = (offsetMax.x - offsetMin.x) / (final - origin);
        
        SetRange(0, newF.Function(finalDomain));
    }
    
    public override void SetPoint(float x, float y)
    {
        base.SetPoint(x, y);
    }
}