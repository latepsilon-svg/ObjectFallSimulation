using UnityEngine;

public class VvsTGraph : FunctionDraw
{
    public float airDensity;
    public float dragCoefficient;
    public float proyectedArea;
    public float mass;
    public float gravity;
    VvsTFunction newF;

    public LineRenderer derivativeGraph;
    public override void Start()
    {
        ClearPoints();
        newF = new VvsTFunction();
    }

    public override void ClearPoints()
    {
        base.ClearPoints();
        derivativeGraph.positionCount = 0;
    }

    public void ComputeVvsT()
    {
        float k = (airDensity * dragCoefficient * proyectedArea) / (2 * mass);
        newF.gravity = gravity;

        print("<color=#0f0>" + k + "</color>");

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
            float dx = newF.Derivative(counter);
            SetPoint(counter, fx);
            SetYellowGraph(counter, dx);
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

        float targ = newF.Function(finalDomain);
        print($"Settings Range {targ} de {finalDomain}");
        SetRange(0, targ);
    }

    public override void SetPoint(float x, float y)
    {
        base.SetPoint(x, y);
    }

    public void SetYellowGraph(float x, float y)
    {
        derivativeGraph.positionCount++;

        int lastIndex = derivativeGraph.positionCount - 1;

        Vector3 newPos = transform.position + new Vector3(
            offsetMin.x + FromFuncToLocalDomainRatio * x,
            offsetMin.y + FromFuncToLocalRangeRatio * y,
            -0.3f
        );

        derivativeGraph.SetPosition(lastIndex, newPos);
    }
}