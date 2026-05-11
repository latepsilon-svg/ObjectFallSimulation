using UnityEngine;

public class MathFunction
{
    public virtual float Derivative(float inX)
    {
        float ind = Mathf.Sqrt(10 * 0.01f) * inX;
        return 10 * Sech(ind) * Sech(ind);
    }
    public virtual float Function(float inX)
    {
        return Mathf.Sqrt(10 / 0.01f) * Tanh(Mathf.Sqrt(10 * 0.01f) * inX);
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