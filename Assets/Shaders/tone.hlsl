void ACESColorCorrection_float(float3 In, float Exposure, out float3 Out) {
    // 1. Aplicar exposición previa
    float3 color = In * Exposure;

    // 2. Curva ACES (Aproximación de Stephen Hill para Narkowicz)
    float a = 2.51f;
    float b = 0.03f;
    float c = 2.43f;
    float d = 0.59f;
    float e = 0.14f;

    Out = saturate((color * (a * color + b)) / (color * (c * color + d) + e));
}

void ACESColorCorrection_half(half3 In, half Exposure, out half3 Out) {
    // 1. Aplicar exposición previa
    half3 color = In * Exposure;

    // 2. Curva ACES
    half a = 2.51h;
    half b = 0.03h;
    half c = 2.43h;
    half d = 0.59h;
    half e = 0.14h;

    Out = saturate((color * (a * color + b)) / (color * (c * color + d) + e));
}