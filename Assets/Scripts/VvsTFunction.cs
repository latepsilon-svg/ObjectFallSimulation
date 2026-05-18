using UnityEngine;

public class VvsTFunction : MathFunction
{
    public override float Derivative(float inX)
    {
        float ind = Mathf.Sqrt(gravity * k) * inX;
        return gravity * Sech(ind) * Sech(ind);
    }
    public float gravity;
    public float k;
    public override float Function(float inX)
    {
        return Mathf.Sqrt(gravity / k) * Tanh(Mathf.Sqrt(gravity * k) * inX);
    }
}