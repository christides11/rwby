//Unity Toon Shader/HDRP
//nobuyuki@unity3d.com
//toshiyuki@unity3d.com (Universal RP/HDRP) 

float3 UTS_OtherLights(FragInputs input, float3 i_normalDir,
	float3 additionalLightColor, float3 lightDirection, float notDirectional, out float channelOutAlpha)
{
	channelOutAlpha = 1.0f;
#ifdef _IS_CLIPPING_MATTE
    if (_ClippingMatteMode != 0)
    {

        return float3(0.0f, 0.0f, 0.0f);
    }
#endif // _IS_CLIPPING_MATTE

    /* todo. these should be put into struct */
#ifdef VARYINGS_NEED_POSITION_WS
    float3 V = GetWorldSpaceNormalizeViewDir(input.positionRWS);
#else
    // Unused
    float3 V = float3(1.0, 1.0, 1.0); // Avoid the division by 0
#endif

    float4 Set_UV0 = input.texCoord0;
    float3x3 tangentTransform = input.tangentToWorld;
    //UnpackNormalmapRGorAG(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, texCoords))
    float4 n = SAMPLE_TEXTURE2D_LOD(_NormalMap, sampler_NormalMap, Set_UV0.xy, 0);
//    float3 _NormalMap_var = UnpackNormalScale(tex2D(_NormalMap, TRANSFORM_TEX(Set_UV0, _NormalMap)), _BumpScale);
    float3 _NormalMap_var = UnpackNormalScale(n, _BumpScale);
    float3 normalLocal = _NormalMap_var.rgb;
    float3 normalDirection = normalize(mul(normalLocal, tangentTransform)); // Perturbed normals
 //   float3 i_normalDir = surfaceData.normalWS;
    float3 viewDirection = V;
    float4 _MainTex_var = SAMPLE_TEXTURE2D_LOD(_MainTex, sampler_MainTex, TRANSFORM_TEX(Set_UV0, _MainTex), 0.0f);
    /* end of todo.*/



    //v.2.0.5: 
    float3 addPassLightColor = (0.5 * dot(lerp(i_normalDir, normalDirection, _Is_NormalMapToBase), lightDirection) + 0.5) * additionalLightColor.rgb;
    float  pureIntencity = max(0.001, (0.299 * additionalLightColor.r + 0.587 * additionalLightColor.g + 0.114 * additionalLightColor.b));
    float3 lightColor = max(0, lerp(addPassLightColor, lerp(0, min(addPassLightColor, addPassLightColor / pureIntencity), notDirectional), _Is_Filter_LightColor));
    float3 halfDirection = normalize(viewDirection + lightDirection); // has to be recalced here.
    //v.2.0.5:
    _BaseColor_Step = saturate(_BaseColor_Step + _StepOffset);
    _ShadeColor_Step = saturate(_ShadeColor_Step + _StepOffset);
    //
    //v.2.0.5: If Added lights is directional, set 0 as _LightIntensity
    float _LightIntensity = lerp(0, (0.299 * additionalLightColor.r + 0.587 * additionalLightColor.g + 0.114 * additionalLightColor.b), notDirectional);
    //v.2.0.5: Filtering the high intensity zone of PointLights
    float3 Set_LightColor = lightColor;
    //
    float3 Set_BaseColor = lerp((_BaseColor.rgb * _MainTex_var.rgb * _LightIntensity), ((_BaseColor.rgb * _MainTex_var.rgb) * Set_LightColor), _Is_LightColor_Base);

#ifdef UTS_LAYER_VISIBILITY
    float4 overridingColor = lerp(_BaseColorMaskColor, float4(_BaseColorMaskColor.w, _BaseColorMaskColor.w, _BaseColorMaskColor.w, 1.0f), _ComposerMaskMode);
    float  maskEnabled = max(_BaseColorOverridden, _ComposerMaskMode);
    Set_BaseColor = lerp(Set_BaseColor, overridingColor.xyz, maskEnabled);
    Set_BaseColor *= _BaseColorVisible;
	float Set_BaseColorAlpha = _BaseColorVisible;
#endif //#ifdef UTS_LAYER_VISIBILITY    //v.2.0.5
    float4 _1st_ShadeMap_var = lerp(SAMPLE_TEXTURE2D_LOD(_1st_ShadeMap, sampler_MainTex, TRANSFORM_TEX(Set_UV0, _1st_ShadeMap),0.0f), _MainTex_var, _Use_BaseAs1st);
    float3 Set_1st_ShadeColor = lerp((_1st_ShadeColor.rgb * _1st_ShadeMap_var.rgb * _LightIntensity), ((_1st_ShadeColor.rgb * _1st_ShadeMap_var.rgb) * Set_LightColor), _Is_LightColor_1st_Shade);
#ifdef UTS_LAYER_VISIBILITY
    {
        float4 overridingColor = lerp(_FirstShadeMaskColor, float4(_FirstShadeMaskColor.w, _FirstShadeMaskColor.w, _FirstShadeMaskColor.w, 1.0f), _ComposerMaskMode);
        float  maskEnabled = max(_FirstShadeOverridden, _ComposerMaskMode);
        Set_1st_ShadeColor = lerp(Set_1st_ShadeColor, overridingColor.xyz, maskEnabled);
        Set_1st_ShadeColor = lerp(Set_1st_ShadeColor, Set_BaseColor, 1.0f - _FirstShadeVisible);
    }
    float Set_1st_ShadeAlpha = _FirstShadeVisible;
#endif //#ifdef UTS_LAYER_VISIBILITY    //v.2.0.5
    float4 _2nd_ShadeMap_var = lerp(SAMPLE_TEXTURE2D_LOD(_2nd_ShadeMap, sampler_MainTex, TRANSFORM_TEX(Set_UV0, _2nd_ShadeMap), 0.0), _1st_ShadeMap_var, _Use_1stAs2nd);
    float3 Set_2nd_ShadeColor = lerp((_2nd_ShadeColor.rgb * _2nd_ShadeMap_var.rgb * _LightIntensity), ((_2nd_ShadeColor.rgb * _2nd_ShadeMap_var.rgb) * Set_LightColor), _Is_LightColor_2nd_Shade);
    float _HalfLambert_var = 0.5 * dot(lerp(i_normalDir, normalDirection, _Is_NormalMapToBase), lightDirection) + 0.5;
    float4 _Set_2nd_ShadePosition_var = tex2Dlod(_Set_2nd_ShadePosition, float4(TRANSFORM_TEX(Set_UV0, _Set_2nd_ShadePosition),0.0f,0.0f));
    float4 _Set_1st_ShadePosition_var = tex2Dlod(_Set_1st_ShadePosition, float4(TRANSFORM_TEX(Set_UV0, _Set_1st_ShadePosition),0.0f, 0.0f));
    //v.2.0.5:
    float _1stColorFeatherForMask = lerp(_BaseShade_Feather, 0.0f, max(_FirstShadeOverridden, _ComposerMaskMode));
    float _2ndColorFeatherForMask = lerp(_1st2nd_Shades_Feather, 0.0f, max(_SecondShadeOverridden, _ComposerMaskMode));
    float Set_FinalShadowMask = saturate((1.0 + ((lerp(_HalfLambert_var, (_HalfLambert_var * saturate(1.0 + _Tweak_SystemShadowsLevel)), _Set_SystemShadowsToBase) - (_BaseColor_Step - _1stColorFeatherForMask)) * ((1.0 - _Set_1st_ShadePosition_var.rgb).r - 1.0)) / (_BaseColor_Step - (_BaseColor_Step - _1stColorFeatherForMask))));




    //Composition: 3 Basic Colors as finalColor
#ifdef UTS_LAYER_VISIBILITY
    {
        float4 overridingColor = lerp(_SecondShadeMaskColor, float4(_SecondShadeMaskColor.w, _SecondShadeMaskColor.w, _SecondShadeMaskColor.w, 1.0f), _ComposerMaskMode);
        float  maskEnabled = max(_SecondShadeOverridden, _ComposerMaskMode);
        Set_2nd_ShadeColor = lerp(Set_2nd_ShadeColor, overridingColor.xyz, maskEnabled);
        Set_2nd_ShadeColor = lerp(Set_2nd_ShadeColor, Set_BaseColor, 1.0f - _SecondShadeVisible);
    }
#endif //#ifdef UTS_LAYER_VISIBILITY
    float3 finalColor = lerp(Set_BaseColor, lerp(Set_1st_ShadeColor, Set_2nd_ShadeColor, saturate((1.0 + ((_HalfLambert_var - (_ShadeColor_Step - _2ndColorFeatherForMask)) * ((1.0 - _Set_2nd_ShadePosition_var.rgb).r - 1.0)) / (_ShadeColor_Step - (_ShadeColor_Step - _2ndColorFeatherForMask))))), Set_FinalShadowMask); // Final Color
#ifdef UTS_LAYER_VISIBILITY
    float Set_2nd_ShadeAlpha = _SecondShadeVisible;
    channelOutAlpha = lerp(Set_BaseColorAlpha, lerp(Set_1st_ShadeAlpha, Set_2nd_ShadeAlpha, saturate((1.0 + ((_HalfLambert_var - (_ShadeColor_Step - _2ndColorFeatherForMask)) * ((1.0 - _Set_2nd_ShadePosition_var.rgb).r - 1.0)) / (_ShadeColor_Step - (_ShadeColor_Step - _2ndColorFeatherForMask))))), Set_FinalShadowMask);
#endif
    //v.2.0.6: Add HighColor if _Is_Filter_HiCutPointLightColor is False

    float4 _Set_HighColorMask_var = tex2Dlod(_Set_HighColorMask, float4(TRANSFORM_TEX(Set_UV0, _Set_HighColorMask),0.0f,0.0f));
    float _Specular_var = 0.5 * dot(halfDirection, lerp(i_normalDir, normalDirection, _Is_NormalMapToHighColor)) + 0.5; //  Specular                
    float _TweakHighColorMask_var = (saturate((_Set_HighColorMask_var.g + _Tweak_HighColorMaskLevel)) * lerp((1.0 - step(_Specular_var, (1.0 - pow(abs(_HighColor_Power), 5)))), pow(abs(_Specular_var), exp2(lerp(11, 1, _HighColor_Power))), _Is_SpecularToHighColor));
    float4 _HighColor_Tex_var = tex2Dlod(_HighColor_Tex, float4( TRANSFORM_TEX(Set_UV0, _HighColor_Tex),0.0f,0.0f));

    float3 _HighColor_var = lerp((_HighColor_Tex_var.rgb * _HighColor.rgb), ((_HighColor_Tex_var.rgb * _HighColor.rgb) * Set_LightColor), _Is_LightColor_HighColor);
#ifdef UTS_LAYER_VISIBILITY
    {
        float4 overridingColor = lerp(_HighlightMaskColor, float4(_HighlightMaskColor.w, _HighlightMaskColor.w, _HighlightMaskColor.w, 1.0f), _ComposerMaskMode);
        float  maskEnabled = max(_HighlightOverridden, _ComposerMaskMode);
        _HighColor_var *= _TweakHighColorMask_var;
        _HighColor_var *= _HighlightVisible;
        finalColor =
            lerp(saturate(finalColor - _TweakHighColorMask_var), finalColor,
                lerp(_Is_BlendAddToHiColor, 1.0
                    , _Is_SpecularToHighColor));
        float3 addColor =
            lerp(_HighColor_var, (_HighColor_var * ((1.0 - Set_FinalShadowMask) + (Set_FinalShadowMask * _TweakHighColorOnShadow)))
                , _Is_UseTweakHighColorOnShadow);
        finalColor += addColor;
        if (any(addColor))
        {
            finalColor = lerp(finalColor, overridingColor.xyz, maskEnabled);
            channelOutAlpha = _HighlightVisible;
        }

    }
#else
    _HighColor_var *= _TweakHighColorMask_var;
    finalColor = finalColor + lerp(lerp(_HighColor_var, (_HighColor_var * ((1.0 - Set_FinalShadowMask) + (Set_FinalShadowMask * _TweakHighColorOnShadow))), _Is_UseTweakHighColorOnShadow), float3(0, 0, 0), _Is_Filter_HiCutPointLightColor);
#endif //#ifdef UTS_LAYER_VISIBILITY
    //

    finalColor = SATURATE_IF_SDR(finalColor);

    //    pointLightColor += finalColor;



    return finalColor;
}
