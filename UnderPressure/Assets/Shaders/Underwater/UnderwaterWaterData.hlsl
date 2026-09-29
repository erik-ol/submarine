#ifndef UNDERWATER_WATER_DATA_INCLUDED
#define UNDERWATER_WATER_DATA_INCLUDED

// Water optical data + spectral helpers for the underwater multiple-scattering pass.
//
// From the Monzon et. al Shadertoy:
//   Monzon, Gutierrez, Akkaynak, Munoz. "Real-Time Underwater Spectral Rendering".
//   Computer Graphics Forum 2024. https://doi.org/10.1111/cgf.15009
//   Shadertoy: https://www.shadertoy.com/view/4cVGD3
// Water data: Williamson & Hollins 2022, "Measured IOPs of Jerlov water types",
//   Applied Optics, https://doi.org/10.1364/AO.470464
//   raw data: https://doi.org/10.6084/m9.figshare.20290782
// Colour matching functions: CIE 2006 2-degree XYZ, http://cvrl.ioo.ucl.ac.uk
//
// Changes from the Shadertoy:
//   - all 6 water types in one array, so they can be indexed and blended
//   - water index uses (N-1) steps, since the data is linspace(400,700,20)
//   - CIE table indexed from 390 nm (the CIE 2006 table has 89 samples, 390-830 nm, 5 nm)
//   - XYZ -> linear sRGB matrix written out row by row for HLSL

// ---------------------------------------------------------------------------
// Water types: 0 = Jerlov IB, 1 = II, 2 = III (open ocean)
//              3 = Jerlov 1C, 4 = 3C, 5 = 5C (coastal)
// Fractional values blend neighbouring types (e.g. 0.5 = halfway IB -> II).
// Each float3 = (scattering b [1/m], extinction c [1/m], diffuse downwelling Kd [1/m])
// Wavelengths: linspace(400, 700, 20)
// ---------------------------------------------------------------------------
#define WATER_N_WLS 20
#define WATER_N_TYPES 6
#define WATER_WL_MIN 400.0
#define WATER_WL_MAX 700.0

static const float3 WATER_B_C_KD[WATER_N_TYPES * WATER_N_WLS] =
{
    // Jerlov IB
    float3(0.144, 0.189, 0.051), float3(0.141, 0.184, 0.045), float3(0.138, 0.180, 0.040), float3(0.135, 0.176, 0.037), float3(0.133, 0.171, 0.034),
    float3(0.130, 0.167, 0.034), float3(0.129, 0.167, 0.040), float3(0.127, 0.173, 0.047), float3(0.124, 0.176, 0.055), float3(0.123, 0.182, 0.066),
    float3(0.121, 0.193, 0.080), float3(0.120, 0.208, 0.098), float3(0.119, 0.257, 0.184), float3(0.117, 0.369, 0.260), float3(0.116, 0.402, 0.304),
    float3(0.115, 0.425, 0.343), float3(0.114, 0.461, 0.381), float3(0.113, 0.525, 0.419), float3(0.112, 0.565, 0.488), float3(0.111, 0.684, 0.580),
    // Jerlov II
    float3(0.209, 0.283, 0.096), float3(0.204, 0.275, 0.087), float3(0.200, 0.267, 0.078), float3(0.196, 0.259, 0.069), float3(0.192, 0.250, 0.065),
    float3(0.189, 0.243, 0.063), float3(0.186, 0.239, 0.068), float3(0.183, 0.240, 0.073), float3(0.180, 0.241, 0.077), float3(0.178, 0.244, 0.085),
    float3(0.175, 0.253, 0.097), float3(0.173, 0.267, 0.114), float3(0.171, 0.314, 0.199), float3(0.169, 0.424, 0.276), float3(0.167, 0.456, 0.323),
    float3(0.165, 0.479, 0.366), float3(0.164, 0.514, 0.407), float3(0.162, 0.578, 0.448), float3(0.161, 0.618, 0.518), float3(0.159, 0.734, 0.610),
    // Jerlov III
    float3(0.324, 0.454, 0.185), float3(0.319, 0.447, 0.169), float3(0.315, 0.439, 0.153), float3(0.310, 0.424, 0.138), float3(0.306, 0.408, 0.125),
    float3(0.302, 0.394, 0.116), float3(0.298, 0.383, 0.115), float3(0.295, 0.378, 0.115), float3(0.292, 0.374, 0.116), float3(0.289, 0.372, 0.119),
    float3(0.285, 0.377, 0.129), float3(0.282, 0.387, 0.147), float3(0.280, 0.434, 0.233), float3(0.277, 0.543, 0.312), float3(0.275, 0.576, 0.362),
    float3(0.273, 0.598, 0.408), float3(0.270, 0.635, 0.453), float3(0.268, 0.706, 0.500), float3(0.266, 0.741, 0.572), float3(0.264, 0.845, 0.660),
    // Jerlov 1C
    float3(0.474, 0.660, 0.510), float3(0.468, 0.646, 0.415), float3(0.462, 0.630, 0.331), float3(0.456, 0.608, 0.262), float3(0.451, 0.585, 0.208),
    float3(0.446, 0.563, 0.165), float3(0.442, 0.549, 0.146), float3(0.437, 0.538, 0.136), float3(0.433, 0.530, 0.129), float3(0.429, 0.525, 0.123),
    float3(0.425, 0.526, 0.129), float3(0.421, 0.534, 0.148), float3(0.418, 0.579, 0.237), float3(0.414, 0.687, 0.315), float3(0.411, 0.719, 0.359),
    float3(0.408, 0.741, 0.408), float3(0.405, 0.778, 0.456), float3(0.402, 0.856, 0.494), float3(0.399, 0.886, 0.562), float3(0.396, 0.981, 0.650),
    // Jerlov 3C
    float3(0.713, 0.949, 0.780), float3(0.704, 0.938, 0.628), float3(0.695, 0.922, 0.501), float3(0.687, 0.893, 0.406), float3(0.680, 0.862, 0.337),
    float3(0.673, 0.834, 0.279), float3(0.666, 0.814, 0.235), float3(0.660, 0.799, 0.212), float3(0.653, 0.784, 0.199), float3(0.647, 0.774, 0.193),
    float3(0.642, 0.769, 0.196), float3(0.636, 0.772, 0.209), float3(0.631, 0.814, 0.279), float3(0.625, 0.918, 0.345), float3(0.621, 0.950, 0.389),
    float3(0.616, 0.969, 0.428), float3(0.611, 1.005, 0.471), float3(0.607, 1.091, 0.534), float3(0.603, 1.116, 0.615), float3(0.599, 1.197, 0.710),
    // Jerlov 5C
    float3(1.320, 1.789, 1.100), float3(1.304, 1.730, 0.898), float3(1.288, 1.672, 0.722), float3(1.273, 1.598, 0.583), float3(1.257, 1.532, 0.492),
    float3(1.241, 1.474, 0.419), float3(1.235, 1.442, 0.375), float3(1.219, 1.407, 0.339), float3(1.210, 1.383, 0.309), float3(1.198, 1.360, 0.303),
    float3(1.190, 1.343, 0.309), float3(1.176, 1.333, 0.328), float3(1.170, 1.371, 0.371), float3(1.160, 1.470, 0.417), float3(1.150, 1.501, 0.467),
    float3(1.143, 1.520, 0.508), float3(1.130, 1.550, 0.552), float3(1.122, 1.655, 0.621), float3(1.116, 1.671, 0.705), float3(1.110, 1.724, 0.800)
};

// (b, c, Kd) for a wavelength in nm and a water type in [0, 5]
float3 GetWaterProps(float wl, float waterType)
{
    float x = saturate((wl - WATER_WL_MIN) / (WATER_WL_MAX - WATER_WL_MIN)) * (WATER_N_WLS - 1);
    int   i = min((int)floor(x), WATER_N_WLS - 2);
    float f = x - i;

    float t  = clamp(waterType, 0.0, WATER_N_TYPES - 1.0);
    int   t0 = min((int)floor(t), WATER_N_TYPES - 2);
    float tf = t - t0;

    int a = t0 * WATER_N_WLS + i;         // type t0
    int b = (t0 + 1) * WATER_N_WLS + i;   // type t0 + 1

    float3 w0 = lerp(WATER_B_C_KD[a], WATER_B_C_KD[a + 1], f);
    float3 w1 = lerp(WATER_B_C_KD[b], WATER_B_C_KD[b + 1], f);
    return lerp(w0, w1, tf);
}

// ---------------------------------------------------------------------------
// CIE 2006 2-degree colour matching functions, 390-830 nm, 5 nm steps
// ---------------------------------------------------------------------------
#define CIE_N 89
#define CIE_WL_MIN 390.0
#define CIE_WL_STEP 5.0

static const float3 CIE_XYZ[CIE_N] =
{
    float3(3.769647e-03, 4.146161e-04, 1.847260e-02), float3(9.382967e-03, 1.059646e-03, 4.609784e-02), float3(2.214302e-02, 2.452194e-03, 1.096090e-01),
    float3(4.742986e-02, 4.971717e-03, 2.369246e-01), float3(8.953803e-02, 9.079860e-03, 4.508369e-01), float3(1.446214e-01, 1.429377e-02, 7.378822e-01),
    float3(2.035729e-01, 2.027369e-02, 1.051821e+00), float3(2.488523e-01, 2.612106e-02, 1.305008e+00), float3(2.918246e-01, 3.319038e-02, 1.552826e+00),
    float3(3.227087e-01, 4.157940e-02, 1.748280e+00), float3(3.482554e-01, 5.033657e-02, 1.917479e+00), float3(3.418483e-01, 5.743393e-02, 1.918437e+00),
    float3(3.224637e-01, 6.472352e-02, 1.848545e+00), float3(2.826646e-01, 7.238339e-02, 1.664439e+00), float3(2.485254e-01, 8.514816e-02, 1.522157e+00),
    float3(2.219781e-01, 1.060145e-01, 1.428440e+00), float3(1.806905e-01, 1.298957e-01, 1.250610e+00), float3(1.291920e-01, 1.535066e-01, 9.991789e-01),
    float3(8.182895e-02, 1.788048e-01, 7.552379e-01), float3(4.600865e-02, 2.064828e-01, 5.617313e-01), float3(2.083981e-02, 2.379160e-01, 4.099313e-01),
    float3(7.097731e-03, 2.850680e-01, 3.105939e-01), float3(2.461588e-03, 3.483536e-01, 2.376753e-01), float3(3.649178e-03, 4.277595e-01, 1.720018e-01),
    float3(1.556989e-02, 5.204972e-01, 1.176796e-01), float3(4.315171e-02, 6.206256e-01, 8.283548e-02), float3(7.962917e-02, 7.180890e-01, 5.650407e-02),
    float3(1.268468e-01, 7.946448e-01, 3.751912e-02), float3(1.818026e-01, 8.575799e-01, 2.438164e-02), float3(2.405015e-01, 9.071347e-01, 1.566174e-02),
    float3(3.098117e-01, 9.544675e-01, 9.846470e-03), float3(3.804244e-01, 9.814106e-01, 6.131421e-03), float3(4.494206e-01, 9.890228e-01, 3.790291e-03),
    float3(5.280233e-01, 9.994608e-01, 2.327186e-03), float3(6.133784e-01, 9.967737e-01, 1.432128e-03), float3(7.016774e-01, 9.902549e-01, 8.822531e-04),
    float3(7.967750e-01, 9.732611e-01, 5.452416e-04), float3(8.853376e-01, 9.424569e-01, 3.386739e-04), float3(9.638388e-01, 8.963613e-01, 2.117772e-04),
    float3(1.051011e+00, 8.587203e-01, 1.335031e-04), float3(1.109767e+00, 8.115868e-01, 8.494468e-05), float3(1.143620e+00, 7.544785e-01, 5.460706e-05),
    float3(1.151033e+00, 6.918553e-01, 3.549661e-05), float3(1.134757e+00, 6.270066e-01, 2.334738e-05), float3(1.083928e+00, 5.583746e-01, 1.554631e-05),
    float3(1.007344e+00, 4.895950e-01, 1.048387e-05), float3(9.142877e-01, 4.229897e-01, 0.000000e+00), float3(8.135565e-01, 3.609245e-01, 0.000000e+00),
    float3(6.924717e-01, 2.980865e-01, 0.000000e+00), float3(5.755410e-01, 2.416902e-01, 0.000000e+00), float3(4.731224e-01, 1.943124e-01, 0.000000e+00),
    float3(3.844986e-01, 1.547397e-01, 0.000000e+00), float3(2.997374e-01, 1.193120e-01, 0.000000e+00), float3(2.277792e-01, 8.979594e-02, 0.000000e+00),
    float3(1.707914e-01, 6.671045e-02, 0.000000e+00), float3(1.263808e-01, 4.899699e-02, 0.000000e+00), float3(9.224597e-02, 3.559982e-02, 0.000000e+00),
    float3(6.639960e-02, 2.554223e-02, 0.000000e+00), float3(4.710606e-02, 1.807939e-02, 0.000000e+00), float3(3.292138e-02, 1.261573e-02, 0.000000e+00),
    float3(2.262306e-02, 8.661284e-03, 0.000000e+00), float3(1.575417e-02, 6.027677e-03, 0.000000e+00), float3(1.096778e-02, 4.195941e-03, 0.000000e+00),
    float3(7.608750e-03, 2.910864e-03, 0.000000e+00), float3(5.214608e-03, 1.995557e-03, 0.000000e+00), float3(3.569452e-03, 1.367022e-03, 0.000000e+00),
    float3(2.464821e-03, 9.447269e-04, 0.000000e+00), float3(1.703876e-03, 6.537050e-04, 0.000000e+00), float3(1.186238e-03, 4.555970e-04, 0.000000e+00),
    float3(8.269535e-04, 3.179738e-04, 0.000000e+00), float3(5.758303e-04, 2.217445e-04, 0.000000e+00), float3(4.058303e-04, 1.565566e-04, 0.000000e+00),
    float3(2.856577e-04, 1.103928e-04, 0.000000e+00), float3(2.021853e-04, 7.827442e-05, 0.000000e+00), float3(1.438270e-04, 5.578862e-05, 0.000000e+00),
    float3(1.024685e-04, 3.981884e-05, 0.000000e+00), float3(7.347551e-05, 2.860175e-05, 0.000000e+00), float3(5.259870e-05, 2.051259e-05, 0.000000e+00),
    float3(3.806114e-05, 1.487243e-05, 0.000000e+00), float3(2.758222e-05, 1.080001e-05, 0.000000e+00), float3(2.004122e-05, 7.863920e-06, 0.000000e+00),
    float3(1.458792e-05, 5.736935e-06, 0.000000e+00), float3(1.068141e-05, 4.211597e-06, 0.000000e+00), float3(7.857521e-06, 3.106561e-06, 0.000000e+00),
    float3(5.768284e-06, 2.286786e-06, 0.000000e+00), float3(4.259166e-06, 1.693147e-06, 0.000000e+00), float3(3.167765e-06, 1.262556e-06, 0.000000e+00),
    float3(2.358723e-06, 9.422514e-07, 0.000000e+00), float3(1.762465e-06, 7.053860e-07, 0.000000e+00)
};

// XYZ response of the human eye at a wavelength (nm)
float3 Sensitivity(float wl)
{
    float x = clamp((wl - CIE_WL_MIN) / CIE_WL_STEP, 0.0, CIE_N - 1.0);
    int   i = min((int)floor(x), CIE_N - 2);
    return lerp(CIE_XYZ[i], CIE_XYZ[i + 1], x - i);
}

// CIE XYZ -> linear sRGB (D65)
float3 XYZToLinearSRGB(float3 xyz)
{
    return float3(
         3.2404542 * xyz.x - 1.5371385 * xyz.y - 0.4985314 * xyz.z,
        -0.9692660 * xyz.x + 1.8760108 * xyz.y + 0.0415560 * xyz.z,
         0.0556434 * xyz.x - 0.2040259 * xyz.y + 1.0572252 * xyz.z);
}

// Low-budget RGB -> spectrum (same as the Shadertoy):
// blue below 480 nm, green below 560 nm, red above.
float UpsampleRGB(float3 col, float wl)
{
    if (wl < 480.0) return col.b;
    if (wl < 560.0) return col.g;
    return col.r;
}

#endif // UNDERWATER_WATER_DATA_INCLUDED
