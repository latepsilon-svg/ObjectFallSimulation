using System;
using NUnit.Framework;
using TMPro;
using UnityEngine;

public class FunctionDraw : MonoBehaviour
{
    public LineRenderer computeGraph;
    public LineRenderer simulatedGraph;

    public MathFunction function;

    public TextMeshProUGUI[] domains;
    public TextMeshProUGUI[] ranges;

    public TextMeshProUGUI start;

    Vector3 offsetMin = new Vector3(-6.1f, -3.6f, 0);
    Vector3 offsetMax = new Vector3(7.5f, 5, 0);
    
    void Start()
    {
        ClearPoints();
        function = new MathFunction();
    }
    void Update()
    {

    }

    [ContextMenu("X")]
    public void xd()
    {
        SetDomain(0, 8);
        SetRange(0, function.Function(finalDomain));
    }

    public float step = 0.1f;

    [ContextMenu("Compute")]
    public void Cumputea()
    {
        print("first");
        Compute(0, 0);
    }
    public void Compute(float from, float to)
    {
        ClearPoints();
        if (from == to)
        {
            from = originDomain;
        }
        float counter = from;
        if (counter < originDomain) counter = originDomain;
        if (to > finalDomain || to == 0) to = finalDomain;

        print($"from {counter} to {to}");
        while (counter < to)
        {
            if (function == null)
            {
                print("XD");
            }
            float fx = function.Function(counter);
            float dx = function.Derivative(counter);
            SetPoint(counter, fx);
            SetPointDerivative(counter, dx);
            counter += step;
        }
    }

    float FromFuncToLocalDomainRatio = 0;
    float FromFuncToLocalRangeRatio = 0;

   public float originDomain = 0;
   public float finalDomain = 0;
    public virtual void SetDomain(int origin, int final)
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
    }
    
    public virtual void SetRange(float origin, float final)
    {
        float step = final - origin;
        bool needDecimal = step % ranges.Length != 0;
        step /= ranges.Length;
        string decimals = needDecimal ? "F1" : "F0";
        for (int i = 0; i < ranges.Length; i++)
        {
            ranges[i].text = (origin + step * (i+1)).ToString(decimals);
        }
        FromFuncToLocalRangeRatio = (offsetMax.y - offsetMin.y) / (final - origin);
    }

    public virtual void ClearPoints()
    {
        computeGraph.positionCount = 0;
        simulatedGraph.positionCount = 0;
    }

    public virtual void SetPoint(float x, float y)
    {
        computeGraph.positionCount++;

        int lastIndex = computeGraph.positionCount - 1;

        Vector3 newPos = transform.position + new Vector3(
            offsetMin.x + FromFuncToLocalDomainRatio * x,
            offsetMin.y + FromFuncToLocalRangeRatio * y,
            0
        );

        computeGraph.SetPosition(lastIndex, newPos);
    }
    
    public virtual void SetPointDerivative(float x, float y)
    {
        simulatedGraph.positionCount++;

        int lastIndex = simulatedGraph.positionCount - 1;

        Vector3 newPos = transform.position + new Vector3(
            offsetMin.x + FromFuncToLocalDomainRatio * x,
            offsetMin.y + FromFuncToLocalRangeRatio * y,
            0.1f
        );

        simulatedGraph.SetPosition(lastIndex, newPos);
    }
    
    
}
