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
    const double oiler = System.Math.E;
    public float Tanh(float x)
    {
        float e = (float)oiler;
        float result = (Mathf.Pow(e, x) - Mathf.Pow(e, -x)) / (Mathf.Pow(e, x) + Mathf.Pow(e, -x));
        return result;
    }
    
    public float Sech(float x)
    {     
        return 2f / (Mathf.Exp(x) + Mathf.Exp(-x));
    }
}