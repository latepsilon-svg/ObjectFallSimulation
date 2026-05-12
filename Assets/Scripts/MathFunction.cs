using UnityEngine;

public class MathFunction
{
    public virtual float Derivative(float inX)
    {
        return 1;
    }
    public virtual float Function(float inX)
    {
        return inX;
    }
    public float Tanh(float x)
    {
        float result = (Mathf.Exp(x) - Mathf.Exp(-x)) / (Mathf.Exp(x) + Mathf.Exp(-x));
        return result;
    }

    public float Sech(float x)
    {
        return 2f / (Mathf.Exp(x) + Mathf.Exp(-x));
    }
}