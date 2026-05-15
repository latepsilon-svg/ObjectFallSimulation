using UnityEngine;

public class AvsTGraph : FunctionDraw
{
    public VvsTGraph reference;
    VvsTFunction newF;

    void Awake()
    {
        reference.callback += ComputeVvsT;
        reference.domainCallback += SetDomain;
        ClearPoints();
        newF = new VvsTFunction();
    }


    public void ComputeVvsT()
    {
        float k = (reference.airDensity * reference.dragCoefficient * reference.proyectedArea) / (2 * reference.mass);
        newF.gravity = reference.gravity;

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
            float dx = newF.Derivative(counter);
            SetPoint(counter, dx);
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

        float originF = newF.Derivative(0);
        float targ = newF.Derivative(finalDomain);

        print($"Settings Range -{originF}- a -{targ}-");
        SetRange(originF, targ);
    }

    public override void SetPoint(float x, float y)
    {
        base.SetPoint(x, y);
    }

}