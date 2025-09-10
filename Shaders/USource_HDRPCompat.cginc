// Auto-generated HDRP compatibility fallbacks (uSource AI patch)
#ifndef USOURCE_HDRP_COMPAT
#define USOURCE_HDRP_COMPAT 1

#ifndef UNITY_MATRIX_VP
float4x4 UNITY_MATRIX_VP;
#endif

#ifndef TransformWorldToHClip
#define TransformWorldToHClip(positionWS) mul(UNITY_MATRIX_VP, float4(positionWS,1.0))
#endif

#ifndef TransformObjectToHClip
#define TransformObjectToHClip(positionOS) mul(UNITY_MATRIX_VP, mul(unity_ObjectToWorld, float4(positionOS,1.0)))
#endif

// Provide UNITY_INITIALIZE_OUTPUT no-op if missing
#ifndef UNITY_INITIALIZE_OUTPUT
#define UNITY_INITIALIZE_OUTPUT(type,name) type name = (type)0;
#endif

#endif // USOURCE_HDRP_COMPAT
