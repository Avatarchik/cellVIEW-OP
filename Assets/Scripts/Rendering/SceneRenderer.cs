using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class SceneRenderer : MonoBehaviour
{
    public Camera LightCamera;
    public RenderShadowMap RenderShadowMap;
    public LightCameraController LightCameraController;

    //public RenderTexture ShadowMap;
    //public RenderTexture ShadowMap2;

    public Material ContourMaterial;
    public Material CompositeMaterial;
    public Material ColorCompositeMaterial;
    public Material OcclusionQueriesMaterial;

    public Material RenderLipidsMaterial;
    public Material RenderProteinsMaterial;
    public Material RenderCurveIngredientsMaterial;

    /*****/

    private Camera _camera;
    private RenderTexture _floodFillTexturePing;
    private RenderTexture _floodFillTexturePong;

    /*****/

    public int WeightThreshold;

    public bool EnableGhosts;

    [Range(-1, 1)]
    public float GhostContours;

    [Range(0, 10)]
    public float myslider = 0;

    /*****/

    void OnEnable()
    {
        _camera = GetComponent<Camera>();
        _camera.depthTextureMode |= DepthTextureMode.Depth;
        _camera.depthTextureMode |= DepthTextureMode.DepthNormals;
    }

    void OnDisable()
    {
        if (_floodFillTexturePing != null)
        {
            _floodFillTexturePing.DiscardContents();
            DestroyImmediate(_floodFillTexturePing);
        }

        if (_floodFillTexturePong != null)
        {
            _floodFillTexturePong.DiscardContents();
            DestroyImmediate(_floodFillTexturePong);
        }
    }

    //void OnDisable()
    //{
    //    if (_floodFillTexturePing != null)
    //    {
    //        _floodFillTexturePing.DiscardContents();
    //        DestroyImmediate(_floodFillTexturePing);
    //    }

    //    if (_floodFillTexturePong != null)
    //    {
    //        _floodFillTexturePong.DiscardContents();
    //        DestroyImmediate(_floodFillTexturePong);
    //    }
    //}

    void SetContourShaderParams()
    {
        // Contour params
        ContourMaterial.SetInt("_ContourOptions", GlobalProperties.Get.ContourOptions);
        ContourMaterial.SetFloat("_ContourStrength", GlobalProperties.Get.ContourStrength);
    }

    //void SetCurveShaderParams()
    //{
    //    var planes = GeometryUtility.CalculateFrustumPlanes(GetComponent<MainCamera>());
    //    _renderCurveIngredientsMaterial.SetVector("_FrustrumPlane_0", MyUtility.PlaneToVector4(planes[0]));
    //    _renderCurveIngredientsMaterial.SetVector("_FrustrumPlane_1", MyUtility.PlaneToVector4(planes[1]));
    //    _renderCurveIngredientsMaterial.SetVector("_FrustrumPlane_2", MyUtility.PlaneToVector4(planes[2]));
    //    _renderCurveIngredientsMaterial.SetVector("_FrustrumPlane_3", MyUtility.PlaneToVector4(planes[3]));
    //    _renderCurveIngredientsMaterial.SetVector("_FrustrumPlane_4", MyUtility.PlaneToVector4(planes[4]));
    //    _renderCurveIngredientsMaterial.SetVector("_FrustrumPlane_5", MyUtility.PlaneToVector4(planes[5]));


    //    _renderCurveIngredientsMaterial.SetInt("_NumSegments", CPUBuffers.Get.NumDnaControlPoints);
    //    _renderCurveIngredientsMaterial.SetInt("_EnableTwist", 1);

    //    _renderCurveIngredientsMaterial.SetFloat("_Scale", GlobalProperties.Get.Scale);
    //    _renderCurveIngredientsMaterial.SetFloat("_SegmentLength", GlobalProperties.Get.DistanceContraint);
    //    _renderCurveIngredientsMaterial.SetInt("_EnableCrossSection", Convert.ToInt32(GlobalProperties.Get.EnableCrossSection));
    //    _renderCurveIngredientsMaterial.SetVector("_CrossSectionPlane", new Vector4(GlobalProperties.Get.CrossSectionPlaneNormal.x, GlobalProperties.Get.CrossSectionPlaneNormal.y, GlobalProperties.Get.CrossSectionPlaneNormal.z, GlobalProperties.Get.CrossSectionPlaneDistance));

    //    _renderCurveIngredientsMaterial.SetBuffer("_CurveIngredientsInfos", GPUBuffer.Get.CurveIngredientsInfos);
    //    _renderCurveIngredientsMaterial.SetBuffer("_CurveIngredientsColors", GPUBuffer.Get.CurveIngredientsColors);
    //    _renderCurveIngredientsMaterial.SetBuffer("_CurveIngredientsToggleFlags", GPUBuffer.Get.CurveIngredientsToggleFlags);
    //    _renderCurveIngredientsMaterial.SetBuffer("_CurveIngredientsAtoms", GPUBuffer.Get.CurveIngredientsAtoms);
    //    _renderCurveIngredientsMaterial.SetBuffer("_CurveIngredientsAtomCount", GPUBuffer.Get.CurveIngredientsAtomCount);
    //    _renderCurveIngredientsMaterial.SetBuffer("_CurveIngredientsAtomStart", GPUBuffer.Get.CurveIngredientsAtomStart);

    //    _renderCurveIngredientsMaterial.SetBuffer("_DnaControlPointsInfos", GPUBuffer.Get.CurveControlPointsInfos);
    //    _renderCurveIngredientsMaterial.SetBuffer("_DnaControlPointsNormals", GPUBuffer.Get.CurveControlPointsNormals);
    //    _renderCurveIngredientsMaterial.SetBuffer("_DnaControlPoints", GPUBuffer.Get.CurveControlPointsPositions);
    //}

    //void ComputeDNAStrands()
    //{
    //    if (!DisplaySettings.Get.EnableDNAConstraints) return;

    //    int numSegments = CPUBuffers.Get.NumDnaSegments;
    //    int numSegmentPairs1 = (int)Mathf.Ceil(numSegments / 2.0f);
    //    int numSegmentPairs2 = (int)Mathf.Ceil(numSegments / 4.0f);

    //    RopeConstraintsCS.SetFloat("_DistanceMin", DisplaySettings.Get.AngularConstraint);
    //    RopeConstraintsCS.SetFloat("_DistanceMax", DisplaySettings.Get.DistanceContraint);
    //    RopeConstraintsCS.SetInt("_NumControlPoints", CPUBuffers.Get.NumDnaControlPoints);

    //    // Do distance constraints
    //    RopeConstraintsCS.SetInt("_Offset", 0);
    //    RopeConstraintsCS.SetBuffer(0, "_DnaControlPoints", ComputeBufferManager.Get.DnaControlPointsPositions);
    //    RopeConstraintsCS.Dispatch(0, (int)Mathf.Ceil(numSegmentPairs1 / 16.0f), 1, 1);

    //    RopeConstraintsCS.SetInt("_Offset", 1);
    //    RopeConstraintsCS.SetBuffer(0, "_DnaControlPoints", ComputeBufferManager.Get.DnaControlPointsPositions);
    //    RopeConstraintsCS.Dispatch(0, (int)Mathf.Ceil(numSegmentPairs1 / 16.0f), 1, 1);

    //    // Do bending constraints
    //    RopeConstraintsCS.SetInt("_Offset", 0);
    //    RopeConstraintsCS.SetBuffer(1, "_DnaControlPoints", ComputeBufferManager.Get.DnaControlPointsPositions);
    //    RopeConstraintsCS.Dispatch(1, (int)Mathf.Ceil(numSegmentPairs2 / 16.0f), 1, 1);

    //    RopeConstraintsCS.SetInt("_Offset", 1);
    //    RopeConstraintsCS.SetBuffer(1, "_DnaControlPoints", ComputeBufferManager.Get.DnaControlPointsPositions);
    //    RopeConstraintsCS.Dispatch(1, (int)Mathf.Ceil(numSegmentPairs2 / 16.0f), 1, 1);

    //    RopeConstraintsCS.SetInt("_Offset", 2);
    //    RopeConstraintsCS.SetBuffer(1, "_DnaControlPoints", ComputeBufferManager.Get.DnaControlPointsPositions);
    //    RopeConstraintsCS.Dispatch(1, (int)Mathf.Ceil(numSegmentPairs2 / 16.0f), 1, 1);

    //    RopeConstraintsCS.SetInt("_Offset", 3);
    //    RopeConstraintsCS.SetBuffer(1, "_DnaControlPoints", ComputeBufferManager.Get.DnaControlPointsPositions);
    //    RopeConstraintsCS.Dispatch(1, (int)Mathf.Ceil(numSegmentPairs2 / 16.0f), 1, 1);
    //}

    //void ComputeHiZMap(RenderTexture depthBuffer)
    //{
    //    if (GetComponent<MainCamera>().pixelWidth == 0 || GetComponent<MainCamera>().pixelHeight == 0) return;

    //    // Hierachical depth buffer
    //    if (_HiZMap == null || _HiZMap.width != GetComponent<MainCamera>().pixelWidth || _HiZMap.height != GetComponent<MainCamera>().pixelHeight )
    //    {

    //        if (_HiZMap != null)
    //        {
    //            _HiZMap.Release();
    //            DestroyImmediate(_HiZMap);
    //            _HiZMap = null;
    //        }

    //        _HiZMap = new RenderTexture(GetComponent<MainCamera>().pixelWidth, GetComponent<MainCamera>().pixelHeight, 0, RenderTextureFormat.RFloat);
    //        _HiZMap.enableRandomWrite = true;
    //        _HiZMap.useMipMap = false;
    //        _HiZMap.isVolume = true;
    //        _HiZMap.volumeDepth = 24;
    //        //_HiZMap.filterMode = FilterMode.Point;
    //        _HiZMap.wrapMode = TextureWrapMode.Clamp;
    //        _HiZMap.hideFlags = HideFlags.HideAndDontSave;
    //        _HiZMap.Create();
    //    }

    //    ComputeShaderManager.Get.OcclusionCullingCS.SetInt("_ScreenWidth", GetComponent<MainCamera>().pixelWidth);
    //    ComputeShaderManager.Get.OcclusionCullingCS.SetInt("_ScreenHeight", GetComponent<MainCamera>().pixelHeight);

    //    ComputeShaderManager.Get.OcclusionCullingCS.SetTexture(0, "_RWHiZMap", _HiZMap);
    //    ComputeShaderManager.Get.OcclusionCullingCS.SetTexture(0, "_DepthBuffer", depthBuffer);
    //    ComputeShaderManager.Get.OcclusionCullingCS.Dispatch(0, (int)Mathf.Ceil(GetComponent<MainCamera>().pixelWidth / 8.0f), (int)Mathf.Ceil(GetComponent<MainCamera>().pixelHeight / 8.0f), 1);

    //    ComputeShaderManager.Get.OcclusionCullingCS.SetTexture(1, "_RWHiZMap", _HiZMap);
    //    for (int i = 1; i < 12; i++)
    //    {
    //        ComputeShaderManager.Get.OcclusionCullingCS.SetInt("_CurrentLevel", i);
    //        ComputeShaderManager.Get.OcclusionCullingCS.Dispatch(1, (int)Mathf.Ceil(GetComponent<MainCamera>().pixelWidth / 8.0f), (int)Mathf.Ceil(GetComponent<MainCamera>().pixelHeight / 8.0f), 1);
    //    }
    //}

    //void ComputeOcclusionCulling(int cullingFilter)
    //{
    //    if (_HiZMap == null || GlobalProperties.Get.DebugObjectCulling) return;

    //    ComputeShaderManager.Get.OcclusionCullingCS.SetInt("_CullingFilter", cullingFilter);
    //    ComputeShaderManager.Get.OcclusionCullingCS.SetInt("_ScreenWidth", GetComponent<MainCamera>().pixelWidth);
    //    ComputeShaderManager.Get.OcclusionCullingCS.SetInt("_ScreenHeight", GetComponent<MainCamera>().pixelHeight);
    //    ComputeShaderManager.Get.OcclusionCullingCS.SetFloat("_Scale", GlobalProperties.Get.Scale);

    //    ComputeShaderManager.Get.OcclusionCullingCS.SetFloats("_CameraViewMatrix", MyUtility.Matrix4X4ToFloatArray(GetComponent<MainCamera>().worldToCameraMatrix));
    //    ComputeShaderManager.Get.OcclusionCullingCS.SetFloats("_CameraProjMatrix", MyUtility.Matrix4X4ToFloatArray(GL.GetGPUProjectionMatrix(GetComponent<MainCamera>().projectionMatrix, false)));

    //    ComputeShaderManager.Get.OcclusionCullingCS.SetTexture(2, "_HiZMap", _HiZMap);

    //    if (CPUBuffers.Get.NumProteinInstances > 0)
    //    {
    //        ComputeShaderManager.Get.OcclusionCullingCS.SetInt("_NumInstances", CPUBuffers.Get.NumProteinInstances);
    //        ComputeShaderManager.Get.OcclusionCullingCS.SetBuffer(2, "_ProteinRadii", GPUBuffer.Get.ProteinIngredientsRadii);
    //        ComputeShaderManager.Get.OcclusionCullingCS.SetBuffer(2, "_ProteinInstanceInfos", GPUBuffer.Get.ProteinInstanceInfos);
    //        ComputeShaderManager.Get.OcclusionCullingCS.SetBuffer(2, "_ProteinInstanceCullFlags", _proteinInstanceCullFlags);
    //        ComputeShaderManager.Get.OcclusionCullingCS.SetBuffer(2, "_ProteinInstancePositions", GPUBuffer.Get.ProteinInstancePositions);
    //        ComputeShaderManager.Get.OcclusionCullingCS.Dispatch(2, CPUBuffers.Get.NumProteinInstances, 1, 1);
    //    }
    //}

    void DebugSphereBatchCount()
    {
        var batchCount = new int[1];
        GPUBuffers.Get.ArgBuffer.GetData(batchCount);
        Debug.Log(batchCount[0]);
    }









    void DrawCurveIngredients(RenderTexture colorBuffer, RenderTexture idBuffer, RenderTexture depthBuffer)
    {
        RenderCurveIngredientsMaterial.SetInt("_IngredientIdOffset", SceneManager.Get.NumProteinIngredients);
        RenderCurveIngredientsMaterial.SetInt("_NumCutObjects", SceneManager.Get.NumCutObjects);
        RenderCurveIngredientsMaterial.SetInt("_NumIngredientTypes", SceneManager.Get.NumAllIngredients);
        RenderCurveIngredientsMaterial.SetBuffer("_CutInfos", GPUBuffers.Get.CutInfo);
        RenderCurveIngredientsMaterial.SetBuffer("_CutScales", GPUBuffers.Get.CutScales);
        RenderCurveIngredientsMaterial.SetBuffer("_CutPositions", GPUBuffers.Get.CutPositions);
        RenderCurveIngredientsMaterial.SetBuffer("_CutRotations", GPUBuffers.Get.CutRotations);

        var planes = GeometryUtility.CalculateFrustumPlanes(GetComponent<Camera>());
        RenderCurveIngredientsMaterial.SetVector("_FrustrumPlane_0", MyUtility.PlaneToVector4(planes[0]));
        RenderCurveIngredientsMaterial.SetVector("_FrustrumPlane_1", MyUtility.PlaneToVector4(planes[1]));
        RenderCurveIngredientsMaterial.SetVector("_FrustrumPlane_2", MyUtility.PlaneToVector4(planes[2]));
        RenderCurveIngredientsMaterial.SetVector("_FrustrumPlane_3", MyUtility.PlaneToVector4(planes[3]));
        RenderCurveIngredientsMaterial.SetVector("_FrustrumPlane_4", MyUtility.PlaneToVector4(planes[4]));
        RenderCurveIngredientsMaterial.SetVector("_FrustrumPlane_5", MyUtility.PlaneToVector4(planes[5]));

        RenderCurveIngredientsMaterial.SetInt("_NumSegments", SceneManager.Get.NumDnaControlPoints);
        RenderCurveIngredientsMaterial.SetInt("_EnableTwist", 1);

        RenderCurveIngredientsMaterial.SetFloat("_Scale", GlobalProperties.Get.Scale);
        RenderCurveIngredientsMaterial.SetFloat("_SegmentLength", GlobalProperties.Get.DistanceContraint);
        //RenderCurveIngredientsMaterial.SetInt("_EnableCrossSection", Convert.ToInt32(GlobalProperties.Get.EnableCrossSection));
        //RenderCurveIngredientsMaterial.SetVector("_CrossSectionPlane", new Vector4(GlobalProperties.Get.CrossSectionPlaneNormal.x, GlobalProperties.Get.CrossSectionPlaneNormal.y, GlobalProperties.Get.CrossSectionPlaneNormal.z, GlobalProperties.Get.CrossSectionPlaneDistance));

        RenderCurveIngredientsMaterial.SetBuffer("_CurveIngredientsInfos", GPUBuffers.Get.CurveIngredientsInfo);
        RenderCurveIngredientsMaterial.SetBuffer("_CurveIngredientsColors", GPUBuffers.Get.CurveIngredientsColors);
        RenderCurveIngredientsMaterial.SetBuffer("_CurveIngredientsToggleFlags", GPUBuffers.Get.CurveIngredientsToggleFlags);
        RenderCurveIngredientsMaterial.SetBuffer("_CurveIngredientsAtoms", GPUBuffers.Get.CurveIngredientsAtoms);
        RenderCurveIngredientsMaterial.SetBuffer("_CurveIngredientsAtomCount", GPUBuffers.Get.CurveIngredientsAtomCount);
        RenderCurveIngredientsMaterial.SetBuffer("_CurveIngredientsAtomStart", GPUBuffers.Get.CurveIngredientsAtomStart);

        RenderCurveIngredientsMaterial.SetBuffer("_DnaControlPointsInfos", GPUBuffers.Get.CurveControlPointsInfo);
        RenderCurveIngredientsMaterial.SetBuffer("_DnaControlPointsNormals", GPUBuffers.Get.CurveControlPointsNormals);
        RenderCurveIngredientsMaterial.SetBuffer("_DnaControlPoints", GPUBuffers.Get.CurveControlPointsPositions);

        Graphics.SetRenderTarget(new[] { colorBuffer.colorBuffer, idBuffer.colorBuffer }, depthBuffer.depthBuffer);
        RenderCurveIngredientsMaterial.SetPass(0);
        Graphics.DrawProcedural(MeshTopology.Points, Mathf.Max(SceneManager.Get.NumDnaSegments - 2, 0)); // Do not draw first and last segments
    }



    //[ImageEffectOpaque]
    //void OnRenderImage(RenderTexture src, RenderTexture dst)
    //{
    //    if (GetComponent<MainCamera>().pixelWidth == 0 || GetComponent<MainCamera>().pixelHeight == 0) return;

    //    if (CPUBuffers.Get.NumProteinInstances == 0 && CPUBuffers.Get.NumLipidInstances == 0)
    //    {
    //        Graphics.Blit(src, dst);
    //        return;
    //    }

    //    ComputeProteinObjectSpaceCutAways();
    //    ComputeLipidObjectSpaceCutAways();
    //    ComputeViewSpaceCutAways();

    //    ///**** Start rendering routine ***

    //    // Declare temp buffers
    //    var colorBuffer = RenderTexture.GetTemporary(GetComponent<MainCamera>().pixelWidth, GetComponent<MainCamera>().pixelHeight, 0, RenderTextureFormat.ARGB32);
    //    var depthBuffer = RenderTexture.GetTemporary(GetComponent<MainCamera>().pixelWidth, GetComponent<MainCamera>().pixelHeight, 32, RenderTextureFormat.Depth);
    //    var itemBuffer = RenderTexture.GetTemporary(GetComponent<MainCamera>().pixelWidth, GetComponent<MainCamera>().pixelHeight, 0, RenderTextureFormat.RInt);
    //    //var depthNormalsBuffer = RenderTexture.GetTemporary(GetComponent<MainCamera>().pixelWidth, GetComponent<MainCamera>().pixelHeight, 0, RenderTextureFormat.ARGBFloat);
    //    var compositeColorBuffer = RenderTexture.GetTemporary(GetComponent<MainCamera>().pixelWidth, GetComponent<MainCamera>().pixelHeight, 0, RenderTextureFormat.ARGB32);
    //    var compositeDepthBuffer = RenderTexture.GetTemporary(GetComponent<MainCamera>().pixelWidth, GetComponent<MainCamera>().pixelHeight, 32, RenderTextureFormat.Depth);

    //    // Clear temp buffers
    //    Graphics.SetRenderTarget(itemBuffer);
    //    GL.Clear(true, true, new Color(-1, 0, 0, 0));

    //    Graphics.SetRenderTarget(colorBuffer.colorBuffer, depthBuffer.depthBuffer);
    //    GL.Clear(true, true, Color.black);

    //    // Draw proteins
    //    if (CPUBuffers.Get.NumProteinInstances > 0)
    //    {
    //        RenderUtils.ComputeSphereBatches(_camera);
    //        RenderUtils.DrawProteinSphereBatches(RenderProteinsMaterial, _camera, itemBuffer.colorBuffer, depthBuffer.depthBuffer, 0);
    //    }

    //    // Draw Lipids
    //    if (CPUBuffers.Get.LipidInstanceInfos.Count > 0)
    //    {
    //        ComputeLipidSphereBatches();
    //        DrawLipidSphereBatches(itemBuffer, depthBuffer);
    //    }

    //    // Draw curve ingredients
    //    if (CPUBuffers.Get.NumDnaSegments > 0)
    //    {
    //        DrawCurveIngredients(colorBuffer, itemBuffer, depthBuffer);
    //    }

    //    ComputeVisibility(itemBuffer);
    //    FetchHistogramValues();

    //    ///////*** Post processing ***/        

    //    // Get color from id buffer
    //    CompositeMaterial.SetTexture("_IdTexture", itemBuffer);
    //    CompositeMaterial.SetBuffer("_IngredientStates", GPUBuffers.Get.IngredientStates);
    //    CompositeMaterial.SetBuffer("_ProteinColors", GPUBuffers.Get.IngredientsColors);
    //    CompositeMaterial.SetBuffer("_ProteinInstanceInfo", GPUBuffers.Get.ProteinInstancesInfo);
    //    CompositeMaterial.SetBuffer("_LipidInstanceInfo", GPUBuffers.Get.LipidInstancesInfo);
    //    Graphics.Blit(null, colorBuffer, CompositeMaterial, 3);

    //    // Compute contours detection
    //    SetContourShaderParams();
    //    ContourMaterial.SetTexture("_IdTexture", itemBuffer);
    //    Graphics.Blit(colorBuffer, compositeColorBuffer, ContourMaterial, 0);

    //    //Graphics.Blit(compositeColorBuffer, dst);

    //    //////*********** Experimental **************//

    //    //////// Clear Buffer
    //    ////ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(0, "_FlagBuffer", GPUBuffers.Get.ProteinInstanceOcclusionFlags);
    //    ////ComputeShaderManager.Get.ComputeVisibilityCS.Dispatch(0, Mathf.CeilToInt(CPUBuffers.Get.NumProteinInstances / 1), 1, 1);

    //    ////// Compute item visibility
    //    ////ComputeShaderManager.Get.ComputeVisibilityCS.SetTexture(5, "_ItemBuffer2", ShadowMap2);
    //    ////ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(5, "_ProteinInstanceVisibilityFlags", GPUBuffers.Get.ProteinInstanceOcclusionFlags);
    //    ////ComputeShaderManager.Get.ComputeVisibilityCS.Dispatch(5, Mathf.CeilToInt(ShadowMap2.width / 8.0f), Mathf.CeilToInt(ShadowMap2.height / 8.0f), 1);

    //    //////CompositeMaterial.SetBuffer("_ProteinInstanceInfo", GPUBuffers.Get.ProteinInstancesInfo);
    //    //////CompositeMaterial.SetBuffer("_ProteinAtomCount", GPUBuffers.Get.ProteinAtomCount);

    //    //////CompositeMaterial.SetTexture("_IdTexture", itemBuffer);
    //    //////CompositeMaterial.SetTexture("_IdTexture", itemBuffer);
    //    //////CompositeMaterial.SetBuffer("_ProteinInstanceShadowFlags", GPUBuffers.Get.ProteinInstanceOcclusionFlags);
    //    //////Graphics.Blit(compositeColorBuffer, colorBuffer, CompositeMaterial, 6);

    //    //////***************************************//

    //    if(EnableLight)
    //    {
    //        // Collect shadows
    //        CompositeMaterial.SetFloat("_LightSize", LightSize);
    //        CompositeMaterial.SetFloat("_ShadowOffset", ShadowOffset);
    //        CompositeMaterial.SetFloat("_ShadowFactor", ShadowFactor);
    //        CompositeMaterial.SetFloat("_ShadowBias", ShadowBias);
    //        CompositeMaterial.SetMatrix("_InverseView", GetComponent<MainCamera>().cameraToWorldMatrix);
    //        CompositeMaterial.SetTexture("_ShadowMap", RenderShadowMap.ShadowMap2);
    //        CompositeMaterial.SetMatrix("_ShadowCameraViewMatrix", LightCamera.worldToCameraMatrix);
    //        CompositeMaterial.SetMatrix("_ShadowCameraViewProjMatrix", GL.GetGPUProjectionMatrix(LightCamera.projectionMatrix, false) * LightCamera.worldToCameraMatrix);
    //        CompositeMaterial.SetTexture("_DepthTexture", depthBuffer);
    //        Graphics.Blit(compositeColorBuffer, colorBuffer, CompositeMaterial, 5);

    //        CompositeMaterial.SetTexture("_ColorTexture", colorBuffer);
    //    }
    //    else
    //    {            
    //        CompositeMaterial.SetTexture("_ColorTexture", compositeColorBuffer);
    //    }

    //    // Composite with scene color
    //    CompositeMaterial.SetTexture("_DepthTexture", depthBuffer);
    //    Graphics.Blit(null, src, CompositeMaterial, 0);
    //    Graphics.Blit(src, dst);

    //    //Composite with scene depth
    //    CompositeMaterial.SetTexture("_DepthTexture", depthBuffer);
    //    Graphics.Blit(null, compositeDepthBuffer, CompositeMaterial, 1);

    //    ////Composite with scene depth normals
    //    ////_compositeMaterial.SetTexture("_DepthTexture", depthBuffer);
    //    ////Graphics.Blit(null, depthNormalsBuffer, _compositeMaterial, 2);

    //    //// Set global shader properties
    //    Shader.SetGlobalTexture("_CameraDepthTexture", compositeDepthBuffer);
    //    ////Shader.SetGlobalTexture("_CameraDepthNormalsTexture", depthNormalsBuffer);

    //    /*** Object Picking ***/

    //    if (SelectionManager.Get.MouseRightClickFlag)
    //    {
    //        SelectionManager.Get.SetSelectedObject(MyUtility.ReadPixelId(itemBuffer, SelectionManager.Get.MousePosition));
    //        SelectionManager.Get.MouseRightClickFlag = false;
    //    }

    //    // Release temp buffers
    //    RenderTexture.ReleaseTemporary(itemBuffer);
    //    RenderTexture.ReleaseTemporary(colorBuffer);
    //    RenderTexture.ReleaseTemporary(depthBuffer);
    //    //RenderTexture.ReleaseTemporary(depthNormalsBuffer);
    //    RenderTexture.ReleaseTemporary(compositeColorBuffer);
    //    RenderTexture.ReleaseTemporary(compositeDepthBuffer);
    //}



    //// With edges
    //[ImageEffectOpaque]
    //void OnRenderImage(RenderTexture src, RenderTexture dst)
    //{
    //    //return;

    //    if (_camera.pixelWidth == 0 || _camera.pixelHeight == 0) return;

    //    if (SceneManager.Get.NumProteinInstances == 0 && SceneManager.Get.NumLipidInstances == 0)
    //    {
    //        Graphics.Blit(src, dst);
    //        return;
    //    }

    //    CutAwayUtils.ComputeProteinObjectSpaceCutAways(_camera, WeightThreshold);

    //    //CutAwayUtils.ComputeLipidObjectSpaceCutAways();
    //    //CutAwayUtils.ComputeViewSpaceCutAways();

    //    ///**** Start rendering routine ***

    //    // Declare temp buffers
    //    var colorBuffer = RenderTexture.GetTemporary(_camera.pixelWidth, _camera.pixelHeight, 0, RenderTextureFormat.ARGB32);
    //    var depthBuffer = RenderTexture.GetTemporary(_camera.pixelWidth, _camera.pixelHeight, 32, RenderTextureFormat.Depth);

    //    var atomIdBuffer = RenderTexture.GetTemporary(_camera.pixelWidth, _camera.pixelHeight, 0, RenderTextureFormat.RInt);
    //    var instanceIdBuffer = RenderTexture.GetTemporary(_camera.pixelWidth, _camera.pixelHeight, 0, RenderTextureFormat.RInt);

    //    var compositeColorBuffer = RenderTexture.GetTemporary(_camera.pixelWidth, _camera.pixelHeight, 0, RenderTextureFormat.ARGB32);
    //    var compositeColorBuffer2 = RenderTexture.GetTemporary(_camera.pixelWidth, _camera.pixelHeight, 0, RenderTextureFormat.ARGB32);

    //    var compositeDepthBuffer = RenderTexture.GetTemporary(_camera.pixelWidth, _camera.pixelHeight, 32, RenderTextureFormat.Depth);
    //    var compositeDepthBuffer2 = RenderTexture.GetTemporary(_camera.pixelWidth, _camera.pixelHeight, 32, RenderTextureFormat.Depth);

    //    // Clear temp buffers
    //    Graphics.SetRenderTarget(instanceIdBuffer);
    //    GL.Clear(true, true, new Color(-1, 0, 0, 0));

    //    Graphics.SetRenderTarget(colorBuffer.colorBuffer, depthBuffer.depthBuffer);
    //    GL.Clear(true, true, Color.white);

    //    // Draw proteins
    //    if (SceneManager.Get.NumProteinInstances > 0)
    //    {
    //        RenderUtils.ComputeSphereBatches(_camera);
    //        //DebugSphereBatchCount();
    //        RenderUtils.DrawProteinsAtoms(RenderProteinsMaterial, _camera, instanceIdBuffer.colorBuffer, atomIdBuffer.colorBuffer, depthBuffer.depthBuffer, 0);
    //    }

    //    CutAwayUtils.ComputeVisibility(instanceIdBuffer);
    //    CutAwayUtils.FetchHistogramValues();

    //    //// Fetch color
    //    //CompositeMaterial.SetTexture("_IdTexture", itemBuffer);
    //    //Graphics.Blit(null, colorBuffer, CompositeMaterial, 3);

    //    //Compute color composition
    //    ColorCompositeUtils.ComputeCoverage(instanceIdBuffer);
    //    ColorCompositeUtils.CountInstances();
    //    ColorCompositeUtils.ComputeColorComposition(ColorCompositeMaterial, colorBuffer, instanceIdBuffer, atomIdBuffer, depthBuffer);

    //    // Compute contours detection
    //    SetContourShaderParams();
    //    ContourMaterial.SetTexture("_IdTexture", instanceIdBuffer);
    //    Graphics.Blit(colorBuffer, compositeColorBuffer, ContourMaterial, 0);

    //    //***** Collect Shadows *****//

    //    //if (EnableLight)
    //    //{
    //    //    // Collect shadows
    //    //    CompositeMaterial.SetFloat("_LightSize", LightSize);
    //    //    CompositeMaterial.SetFloat("_ShadowOffset", ShadowOffset);
    //    //    CompositeMaterial.SetFloat("_ShadowFactor", ShadowFactor);
    //    //    CompositeMaterial.SetFloat("_ShadowBias", ShadowBias);
    //    //    CompositeMaterial.SetMatrix("_InverseView", GetComponent<MainCamera>().cameraToWorldMatrix);
    //    //    CompositeMaterial.SetTexture("_ShadowMap", RenderShadowMap.ShadowMap2);
    //    //    CompositeMaterial.SetMatrix("_ShadowCameraViewMatrix", LightCamera.worldToCameraMatrix);
    //    //    CompositeMaterial.SetMatrix("_ShadowCameraViewProjMatrix", GL.GetGPUProjectionMatrix(LightCamera.projectionMatrix, false) * LightCamera.worldToCameraMatrix);
    //    //    CompositeMaterial.SetTexture("_DepthTexture", depthBuffer);
    //    //    Graphics.Blit(compositeColorBuffer, colorBuffer, CompositeMaterial, 5);
    //    //    Graphics.Blit(colorBuffer, compositeColorBuffer);
    //    //}

    //    // Composite with scene color
    //    CompositeMaterial.SetTexture("_ColorTexture", compositeColorBuffer);
    //    CompositeMaterial.SetTexture("_DepthTexture", depthBuffer);
    //    Graphics.Blit(null, src, CompositeMaterial, 0);
    //    Graphics.Blit(src, dst);

    //    Shader.SetGlobalTexture("_CameraDepthTexture", depthBuffer);

    //    ////Composite with scene depth
    //    //CompositeMaterial.SetTexture("_DepthTexture", depthBuffer);
    //    //Graphics.Blit(null, compositeDepthBuffer, CompositeMaterial, 1);

    //    //***** Draw Ghosts *****//

    //    //if (EnableGhosts)
    //    //{
    //    //    //// Clear temp buffers
    //    //    //Graphics.SetRenderTarget(instanceIdBuffer);
    //    //    //GL.Clear(true, true, new Color(-1, 0, 0, 0));

    //    //    //Graphics.SetRenderTarget(colorBuffer.colorBuffer, compositeDepthBuffer.depthBuffer);
    //    //    //GL.Clear(true, true, Color.white);

    //    //    //CompositeMaterial.SetTexture("_DepthTexture", depthBuffer);
    //    //    //Graphics.Blit(null, compositeDepthBuffer, CompositeMaterial, 7);

    //    //    //// Draw proteins
    //    //    //if (SceneManager.Get.NumProteinInstances > 0)
    //    //    //{
    //    //    //    RenderUtils.ComputeSphereBatchesClipped(_camera);
    //    //    //    RenderUtils.DrawClippedProteins(RenderProteinsMaterial, _camera, instanceIdBuffer.colorBuffer, compositeDepthBuffer.depthBuffer);
    //    //    //}

    //    //    ////Graphics.SetRenderTarget(colorBuffer);
    //    //    ////GL.Clear(true, true, Color.black);

    //    //    //// Get color from id buffer
    //    //    //ColorCompositeMaterial.SetTexture("_IdTexture", instanceIdBuffer);
    //    //    //ColorCompositeMaterial.SetTexture("_AtomIdTexture", atomIdBuffer);

    //    //    //ColorCompositeMaterial.SetBuffer("_AtomColors", GPUBuffers.Get.AtomColors);
    //    //    //ColorCompositeMaterial.SetBuffer("_ProteinColors", GPUBuffers.Get.IngredientsColors);
    //    //    //ColorCompositeMaterial.SetBuffer("_AminoAcidColors", GPUBuffers.Get.AminoAcidColors);
    //    //    //ColorCompositeMaterial.SetBuffer("_ProteinAtomInfos", GPUBuffers.Get.ProteinAtomInfo);
    //    //    //ColorCompositeMaterial.SetBuffer("_ProteinInstanceInfo", GPUBuffers.Get.ProteinInstancesInfo);
    //    //    //ColorCompositeMaterial.SetBuffer("_IngredientProperties", GPUBuffers.Get.IngredientsInfo);
    //    //    //ColorCompositeMaterial.SetBuffer("_IngredientGroupsColor", GPUBuffers.Get.IngredientGroupsColor);

    //    //    ///********/

    //    //    //ColorCompositeMaterial.SetBuffer("_IngredientGroupsLerpFactors", GPUBuffers.Get.IngredientGroupsLerpFactors);
    //    //    //ColorCompositeMaterial.SetBuffer("_IngredientGroupsColorValues", GPUBuffers.Get.IngredientGroupsColorValues);
    //    //    //ColorCompositeMaterial.SetBuffer("_IngredientGroupsColorRanges", GPUBuffers.Get.IngredientGroupsColorRanges);
    //    //    //ColorCompositeMaterial.SetBuffer("_ProteinIngredientsRandomValues", GPUBuffers.Get.ProteinIngredientsRandomValues);

    //    //    ////CompositeMaterial.SetBuffer("_ProteinAtomColors", GPUBuffers.Get.ProteinAtomColors);
    //    //    ////CompositeMaterial.SetBuffer("_ProteinAtomInfo", GPUBuffers.Get.ProteinAtomInfo);
    //    //    ////CompositeMaterial.SetBuffer("_LipidInstanceInfo", GPUBuffers.Get.LipidInstancesInfo);        

    //    //    ////Graphics.Blit(null, colorBuffer, CompositeMaterial, 3);
    //    //    //Graphics.Blit(null, colorBuffer, ColorCompositeMaterial, 1);

    //    //    //// Compute contours detection
    //    //    //SetContourShaderParams();
    //    //    //ContourMaterial.SetTexture("_IdTexture", instanceIdBuffer);
    //    //    //Graphics.Blit(colorBuffer, compositeColorBuffer2, ContourMaterial, 1);

    //    //    //Graphics.Blit(compositeColorBuffer2, dst);

    //    //    //// Compose final image
    //    //    //CompositeMaterial.SetFloat("_GhostContours", GhostContours);
    //    //    //CompositeMaterial.SetTexture("_MainTex2", compositeColorBuffer);
    //    //    //Graphics.Blit(compositeColorBuffer2, colorBuffer, CompositeMaterial, 8);

    //    //    //Graphics.Blit(colorBuffer, compositeColorBuffer);
    //    //}

    //    /*** Object Picking ***/

    //    if (SelectionManager.Instance.MouseRightClickFlag)
    //    {
    //        SelectionManager.Instance.SetSelectedObject(MyUtility.ReadPixelId(instanceIdBuffer, SelectionManager.Instance.MousePosition));
    //        SelectionManager.Instance.MouseRightClickFlag = false;
    //    }

    //    // Release temp buffers
    //    RenderTexture.ReleaseTemporary(instanceIdBuffer);
    //    RenderTexture.ReleaseTemporary(atomIdBuffer);

    //    RenderTexture.ReleaseTemporary(colorBuffer);
    //    RenderTexture.ReleaseTemporary(depthBuffer);

    //    RenderTexture.ReleaseTemporary(compositeColorBuffer);
    //    RenderTexture.ReleaseTemporary(compositeColorBuffer2);

    //    RenderTexture.ReleaseTemporary(compositeDepthBuffer);
    //    RenderTexture.ReleaseTemporary(compositeDepthBuffer2);
    //}

    void ComputeDistanceTransform(RenderTexture inputTexture)
    {
        var tempBuffer = RenderTexture.GetTemporary(512, 512, 32, RenderTextureFormat.ARGB32);
        Graphics.SetRenderTarget(tempBuffer);
        Graphics.Blit(inputTexture, tempBuffer);

        // Prepare and set the render target
        if (_floodFillTexturePing == null)
        {
            _floodFillTexturePing = new RenderTexture(tempBuffer.width, tempBuffer.height, 32, RenderTextureFormat.ARGBFloat);
            _floodFillTexturePing.enableRandomWrite = true;
            _floodFillTexturePing.filterMode = FilterMode.Point;
        }

        Graphics.SetRenderTarget(_floodFillTexturePing);
        GL.Clear(true, true, new Color(-1, -1, -1, -1));

        if (_floodFillTexturePong == null)
        {
            _floodFillTexturePong = new RenderTexture(tempBuffer.width, tempBuffer.height, 32, RenderTextureFormat.ARGBFloat);
            _floodFillTexturePong.enableRandomWrite = true;
            _floodFillTexturePong.filterMode = FilterMode.Point;
        }

        Graphics.SetRenderTarget(_floodFillTexturePong);
        GL.Clear(true, true, new Color(-1, -1, -1, -1));

        float widthScale = inputTexture.width / 512.0f;
        float heightScale = inputTexture.height / 512.0f;

        ComputeShaderManager.Get.FloodFillCS.SetInt("_StepSize", 512 / 2);
        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Mask", tempBuffer);
        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Ping", _floodFillTexturePing);
        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Pong", _floodFillTexturePong);

        ComputeShaderManager.Get.FloodFillCS.SetFloat("_WidthScale", widthScale);
        ComputeShaderManager.Get.FloodFillCS.SetFloat("_HeightScale", heightScale);
        ComputeShaderManager.Get.FloodFillCS.Dispatch(0, Mathf.CeilToInt(tempBuffer.width / 8.0f), Mathf.CeilToInt(tempBuffer.height / 8.0f), 1);

        ComputeShaderManager.Get.FloodFillCS.SetInt("_StepSize", 512 / 4);
        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Mask", tempBuffer);
        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Ping", _floodFillTexturePong);
        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Pong", _floodFillTexturePing);
        ComputeShaderManager.Get.FloodFillCS.SetFloat("_WidthScale", widthScale);
        ComputeShaderManager.Get.FloodFillCS.SetFloat("_HeightScale", heightScale);
        ComputeShaderManager.Get.FloodFillCS.Dispatch(0, Mathf.CeilToInt(tempBuffer.width / 8.0f), Mathf.CeilToInt(tempBuffer.height / 8.0f), 1);

        ComputeShaderManager.Get.FloodFillCS.SetInt("_StepSize", 512 / 8);
        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Mask", tempBuffer);
        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Ping", _floodFillTexturePing);
        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Pong", _floodFillTexturePong);
        ComputeShaderManager.Get.FloodFillCS.SetFloat("_WidthScale", widthScale);
        ComputeShaderManager.Get.FloodFillCS.SetFloat("_HeightScale", heightScale);
        ComputeShaderManager.Get.FloodFillCS.Dispatch(0, Mathf.CeilToInt(tempBuffer.width / 8.0f), Mathf.CeilToInt(tempBuffer.height / 8.0f), 1);

        ComputeShaderManager.Get.FloodFillCS.SetInt("_StepSize", 512 / 16);
        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Mask", tempBuffer);
        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Ping", _floodFillTexturePong);
        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Pong", _floodFillTexturePing);
        ComputeShaderManager.Get.FloodFillCS.SetFloat("_WidthScale", widthScale);
        ComputeShaderManager.Get.FloodFillCS.SetFloat("_HeightScale", heightScale);
        ComputeShaderManager.Get.FloodFillCS.Dispatch(0, Mathf.CeilToInt(tempBuffer.width / 8.0f), Mathf.CeilToInt(tempBuffer.height / 8.0f), 1);

        ComputeShaderManager.Get.FloodFillCS.SetInt("_StepSize", 512 / 32);
        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Mask", tempBuffer);
        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Ping", _floodFillTexturePing);
        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Pong", _floodFillTexturePong);
        ComputeShaderManager.Get.FloodFillCS.SetFloat("_WidthScale", widthScale);
        ComputeShaderManager.Get.FloodFillCS.SetFloat("_HeightScale", heightScale);
        ComputeShaderManager.Get.FloodFillCS.Dispatch(0, Mathf.CeilToInt(tempBuffer.width / 8.0f), Mathf.CeilToInt(tempBuffer.height / 8.0f), 1);

        ComputeShaderManager.Get.FloodFillCS.SetInt("_StepSize", 512 / 64);
        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Mask", tempBuffer);
        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Ping", _floodFillTexturePong);
        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Pong", _floodFillTexturePing);
        ComputeShaderManager.Get.FloodFillCS.SetFloat("_WidthScale", widthScale);
        ComputeShaderManager.Get.FloodFillCS.SetFloat("_HeightScale", heightScale);
        ComputeShaderManager.Get.FloodFillCS.Dispatch(0, Mathf.CeilToInt(tempBuffer.width / 8.0f), Mathf.CeilToInt(tempBuffer.height / 8.0f), 1);

        ComputeShaderManager.Get.FloodFillCS.SetInt("_StepSize", 512 / 128);
        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Mask", tempBuffer);
        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Ping", _floodFillTexturePing);
        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Pong", _floodFillTexturePong);
        ComputeShaderManager.Get.FloodFillCS.SetFloat("_WidthScale", widthScale);
        ComputeShaderManager.Get.FloodFillCS.SetFloat("_HeightScale", heightScale);
        ComputeShaderManager.Get.FloodFillCS.Dispatch(0, Mathf.CeilToInt(tempBuffer.width / 8.0f), Mathf.CeilToInt(tempBuffer.height / 8.0f), 1);

        ComputeShaderManager.Get.FloodFillCS.SetInt("_StepSize", 512 / 256);
        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Mask", tempBuffer);
        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Ping", _floodFillTexturePong);
        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Pong", _floodFillTexturePing);
        ComputeShaderManager.Get.FloodFillCS.SetFloat("_WidthScale", widthScale);
        ComputeShaderManager.Get.FloodFillCS.SetFloat("_HeightScale", heightScale);
        ComputeShaderManager.Get.FloodFillCS.Dispatch(0, Mathf.CeilToInt(tempBuffer.width / 8.0f), Mathf.CeilToInt(tempBuffer.height / 8.0f), 1);

        ComputeShaderManager.Get.FloodFillCS.SetInt("_StepSize", 512 / 512);
        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Mask", tempBuffer);
        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Ping", _floodFillTexturePing);
        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Pong", _floodFillTexturePong);
        ComputeShaderManager.Get.FloodFillCS.SetFloat("_WidthScale", widthScale);
        ComputeShaderManager.Get.FloodFillCS.SetFloat("_HeightScale", heightScale);
        ComputeShaderManager.Get.FloodFillCS.Dispatch(0, Mathf.CeilToInt(tempBuffer.width / 8.0f), Mathf.CeilToInt(tempBuffer.height / 8.0f), 1);

        RenderTexture.ReleaseTemporary(tempBuffer);
    }

    void ComputeOcclusionMaskLEqual(RenderTexture tempBuffer, bool maskProtein, bool maskLipid)
    {
        // First clear mask buffer
        Graphics.SetRenderTarget(tempBuffer);
        GL.Clear(true, true, Color.blue);

        //***** Compute Protein Mask *****//
        if (maskProtein)
        {
            // Always clear append buffer before usage
            GPUBuffers.Get.SphereBatches.ClearAppendBuffer();

            //Fill the buffer with occludees
            ComputeShaderManager.Get.SphereBatchCS.SetUniform("_NumInstances", SceneManager.Get.NumProteinInstances);
            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(1, "_ProteinInstanceInfo", GPUBuffers.Get.ProteinInstancesInfo);
            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(1, "_ProteinInstanceCullFlags", GPUBuffers.Get.ProteinInstanceCullFlags);

            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(1, "_IngredientMaskParams", GPUBuffers.Get.IngredientMaskParams);
            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(1, "_OccludeeSphereBatches", GPUBuffers.Get.SphereBatches);
            ComputeShaderManager.Get.SphereBatchCS.Dispatch(1, Mathf.CeilToInt(SceneManager.Get.NumProteinInstances / 64.0f), 1, 1);

            // Count occludees instances
            ComputeBuffer.CopyCount(GPUBuffers.Get.SphereBatches, GPUBuffers.Get.ArgBuffer, 0);

            // Prepare draw call
            OcclusionQueriesMaterial.SetFloat("_Scale", GlobalProperties.Get.Scale);
            OcclusionQueriesMaterial.SetBuffer("_ProteinRadii", GPUBuffers.Get.ProteinRadii);
            OcclusionQueriesMaterial.SetBuffer("_ProteinInstanceInfo", GPUBuffers.Get.ProteinInstancesInfo);
            OcclusionQueriesMaterial.SetBuffer("_ProteinInstancePositions", GPUBuffers.Get.ProteinInstancePositions);
            OcclusionQueriesMaterial.SetBuffer("_OccludeeSphereBatches", GPUBuffers.Get.SphereBatches);
            OcclusionQueriesMaterial.SetPass(0);

            // Draw occludees - bounding sphere only - write to depth and stencil buffer
            Graphics.SetRenderTarget(tempBuffer);
            Graphics.DrawProceduralIndirect(MeshTopology.Points, GPUBuffers.Get.ArgBuffer);
        }

        //***** Compute Lipid Mask *****//
        if (maskLipid)
        {
            // Always clear append buffer before usage
            GPUBuffers.Get.SphereBatches.ClearAppendBuffer();

            //Fill the buffer with occludees
            ComputeShaderManager.Get.SphereBatchCS.SetUniform("_NumInstances", SceneManager.Get.NumLipidInstances);
            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(3, "_LipidInstanceInfo", GPUBuffers.Get.LipidInstancesInfo);
            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(3, "_LipidInstanceCullFlags", GPUBuffers.Get.LipidInstanceCullFlags);

            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(3, "_IngredientMaskParams", GPUBuffers.Get.IngredientMaskParams);
            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(3, "_OccludeeSphereBatches", GPUBuffers.Get.SphereBatches);
            ComputeShaderManager.Get.SphereBatchCS.Dispatch(3, Mathf.CeilToInt(SceneManager.Get.NumLipidInstances / 64.0f), 1, 1);

            // Count occludees instances
            ComputeBuffer.CopyCount(GPUBuffers.Get.SphereBatches, GPUBuffers.Get.ArgBuffer, 0);

            //DebugSphereBatchCount();

            // Prepare draw call
            OcclusionQueriesMaterial.SetFloat("_Scale", GlobalProperties.Get.Scale);
            OcclusionQueriesMaterial.SetBuffer("_LipidInstanceInfo", GPUBuffers.Get.LipidInstancesInfo);
            OcclusionQueriesMaterial.SetBuffer("_LipidInstancePositions", GPUBuffers.Get.LipidInstancePositions);
            OcclusionQueriesMaterial.SetBuffer("_OccludeeSphereBatches", GPUBuffers.Get.SphereBatches);
            OcclusionQueriesMaterial.SetPass(2);

            // Draw occludees - bounding sphere only - write to depth and stencil buffer
            Graphics.SetRenderTarget(tempBuffer);
            Graphics.DrawProceduralIndirect(MeshTopology.Points, GPUBuffers.Get.ArgBuffer);
        }
    }



    void ComputeOcclusionMaskGEqual(RenderTexture tempBuffer, bool maskProtein, bool maskLipid)
    {
        // First clear mask buffer
        Graphics.SetRenderTarget(tempBuffer);
        GL.Clear(true, true, Color.blue, 0);

        //***** Compute Protein Mask *****//
        if (maskProtein)
        {
            // Always clear append buffer before usage
            GPUBuffers.Get.SphereBatches.ClearAppendBuffer();

            //Fill the buffer with occludees
            ComputeShaderManager.Get.SphereBatchCS.SetUniform("_NumInstances", SceneManager.Get.NumProteinInstances);
            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(1, "_ProteinInstanceInfo", GPUBuffers.Get.ProteinInstancesInfo);
            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(1, "_ProteinInstanceCullFlags", GPUBuffers.Get.ProteinInstanceCullFlags);

            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(1, "_IngredientMaskParams", GPUBuffers.Get.IngredientMaskParams);
            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(1, "_OccludeeSphereBatches", GPUBuffers.Get.SphereBatches);
            ComputeShaderManager.Get.SphereBatchCS.Dispatch(1, Mathf.CeilToInt(SceneManager.Get.NumProteinInstances / 64.0f), 1, 1);

            // Count occludees instances
            ComputeBuffer.CopyCount(GPUBuffers.Get.SphereBatches, GPUBuffers.Get.ArgBuffer, 0);

            // Prepare draw call
            OcclusionQueriesMaterial.SetFloat("_Scale", GlobalProperties.Get.Scale);
            OcclusionQueriesMaterial.SetBuffer("_ProteinRadii", GPUBuffers.Get.ProteinRadii);
            OcclusionQueriesMaterial.SetBuffer("_ProteinInstanceInfo", GPUBuffers.Get.ProteinInstancesInfo);
            OcclusionQueriesMaterial.SetBuffer("_ProteinInstancePositions", GPUBuffers.Get.ProteinInstancePositions);
            OcclusionQueriesMaterial.SetBuffer("_OccludeeSphereBatches", GPUBuffers.Get.SphereBatches);
            OcclusionQueriesMaterial.SetPass(4);

            // Draw occludees - bounding sphere only - write to depth and stencil buffer
            Graphics.SetRenderTarget(tempBuffer);
            Graphics.DrawProceduralIndirect(MeshTopology.Points, GPUBuffers.Get.ArgBuffer);
        }

        //***** Compute Lipid Mask *****//
        if (maskLipid)
        {
            // Always clear append buffer before usage
            GPUBuffers.Get.SphereBatches.ClearAppendBuffer();

            //Fill the buffer with occludees
            ComputeShaderManager.Get.SphereBatchCS.SetUniform("_NumInstances", SceneManager.Get.NumLipidInstances);
            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(3, "_LipidInstanceInfo", GPUBuffers.Get.LipidInstancesInfo);
            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(3, "_LipidInstanceCullFlags", GPUBuffers.Get.LipidInstanceCullFlags);

            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(3, "_IngredientMaskParams", GPUBuffers.Get.IngredientMaskParams);
            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(3, "_OccludeeSphereBatches", GPUBuffers.Get.SphereBatches);
            ComputeShaderManager.Get.SphereBatchCS.Dispatch(3, Mathf.CeilToInt(SceneManager.Get.NumLipidInstances / 64.0f), 1, 1);

            // Count occludees instances
            ComputeBuffer.CopyCount(GPUBuffers.Get.SphereBatches, GPUBuffers.Get.ArgBuffer, 0);

            //DebugSphereBatchCount();

            // Prepare draw call
            OcclusionQueriesMaterial.SetFloat("_Scale", GlobalProperties.Get.Scale);
            OcclusionQueriesMaterial.SetBuffer("_LipidInstanceInfo", GPUBuffers.Get.LipidInstancesInfo);
            OcclusionQueriesMaterial.SetBuffer("_LipidInstancePositions", GPUBuffers.Get.LipidInstancePositions);
            OcclusionQueriesMaterial.SetBuffer("_OccludeeSphereBatches", GPUBuffers.Get.SphereBatches);
            OcclusionQueriesMaterial.SetPass(5);

            // Draw occludees - bounding sphere only - write to depth and stencil buffer
            Graphics.SetRenderTarget(tempBuffer);
            Graphics.DrawProceduralIndirect(MeshTopology.Points, GPUBuffers.Get.ArgBuffer);
        }
    }

    void ComputeOcclusionQueries(RenderTexture tempBuffer, CutObject cutObject, int cutObjectIndex, int internalState, bool cullProtein, bool cullLipid)
    {
        if (cullProtein)
        {
            // Always clear append buffer before usage
            GPUBuffers.Get.SphereBatches.ClearAppendBuffer();

            //Fill the buffer with occluders
            ComputeShaderManager.Get.SphereBatchCS.SetUniform("_NumInstances", SceneManager.Get.NumProteinInstances);
            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(1, "_ProteinInstanceInfo", GPUBuffers.Get.ProteinInstancesInfo);
            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(1, "_ProteinInstanceCullFlags", GPUBuffers.Get.ProteinInstanceCullFlags);

            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(1, "_IngredientMaskParams", GPUBuffers.Get.IngredientMaskParams);
            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(1, "_OccludeeSphereBatches", GPUBuffers.Get.SphereBatches);
            ComputeShaderManager.Get.SphereBatchCS.Dispatch(1, Mathf.CeilToInt(SceneManager.Get.NumProteinInstances / 64.0f), 1, 1);

            // Count occluder instances
            ComputeBuffer.CopyCount(GPUBuffers.Get.SphereBatches, GPUBuffers.Get.ArgBuffer, 0);

            //DebugSphereBatchCount();

            // Clear protein occlusion buffer 
            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(0, "_FlagBuffer", GPUBuffers.Get.ProteinInstanceOcclusionFlags);
            ComputeShaderManager.Get.ComputeVisibilityCS.Dispatch(0, Mathf.CeilToInt(SceneManager.Get.NumProteinInstances / 64.0f), 1, 1);

            // Bind the read/write occlusion buffer to the shader
            // After this draw call the occlusion buffer will be filled with ones if an instance occluded and occludee, zero otherwise
            Graphics.SetRandomWriteTarget(1, GPUBuffers.Get.ProteinInstanceOcclusionFlags);
            MyUtility.DummyBlit();   // Dunny why yet, but without this I cannot write to the buffer from the shader, go figure

            // Set the render target
            Graphics.SetRenderTarget(tempBuffer);

            OcclusionQueriesMaterial.SetInt("_CutObjectIndex", cutObjectIndex);
            OcclusionQueriesMaterial.SetInt("_NumIngredients", SceneManager.Get.NumAllIngredients);
            OcclusionQueriesMaterial.SetBuffer("_CutInfo", GPUBuffers.Get.CutInfo);
            OcclusionQueriesMaterial.SetTexture("_DistanceField", _floodFillTexturePong);

            OcclusionQueriesMaterial.SetFloat("_Scale", GlobalProperties.Get.Scale);
            OcclusionQueriesMaterial.SetBuffer("_ProteinRadii", GPUBuffers.Get.ProteinRadii);
            OcclusionQueriesMaterial.SetBuffer("_ProteinInstanceInfo", GPUBuffers.Get.ProteinInstancesInfo);
            OcclusionQueriesMaterial.SetBuffer("_ProteinInstancePositions", GPUBuffers.Get.ProteinInstancePositions);
            OcclusionQueriesMaterial.SetBuffer("_OccludeeSphereBatches", GPUBuffers.Get.SphereBatches);
            OcclusionQueriesMaterial.SetPass(1);

            // Issue draw call for occluders - bounding quads only - depth/stencil test enabled - no write to color/depth/stencil
            Graphics.DrawProceduralIndirect(MeshTopology.Points, GPUBuffers.Get.ArgBuffer);
            Graphics.ClearRandomWriteTargets();

            ComputeShaderManager.Get.ComputeVisibilityCS.SetUniform("_CutObjectIndex", cutObjectIndex);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetUniform("_NumIngredients", SceneManager.Get.NumAllIngredients);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(3, "_CutInfo", GPUBuffers.Get.CutInfo);

            //// Discard occluding instances according to value2
            ComputeShaderManager.Get.ComputeVisibilityCS.SetUniform("_CutObjectId", cutObject.Id);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetUniform("_ConsumeRestoreState", internalState);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(3, "_Histograms", GPUBuffers.Get.Histograms);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(3, "_HistogramsLookup", GPUBuffers.Get.HistogramsLookup);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(3, "_ProteinInstanceInfo", GPUBuffers.Get.ProteinInstancesInfo);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(3, "_ProteinInstanceCullFlags", GPUBuffers.Get.ProteinInstanceCullFlags);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(3, "_ProteinInstanceOcclusionFlags", GPUBuffers.Get.ProteinInstanceOcclusionFlags);
            ComputeShaderManager.Get.ComputeVisibilityCS.Dispatch(3, Mathf.CeilToInt(SceneManager.Get.NumProteinInstances / 64.0f), 1, 1);
        }

        if (cullLipid)
        {
            // Always clear append buffer before usage
            GPUBuffers.Get.SphereBatches.ClearAppendBuffer();

            //Fill the buffer with occluders
            ComputeShaderManager.Get.SphereBatchCS.SetUniform("_NumInstances", SceneManager.Get.NumLipidInstances);
            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(3, "_LipidInstanceInfo", GPUBuffers.Get.LipidInstancesInfo);
            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(3, "_LipidInstanceCullFlags", GPUBuffers.Get.LipidInstanceCullFlags);

            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(3, "_IngredientMaskParams", GPUBuffers.Get.IngredientMaskParams);
            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(3, "_OccludeeSphereBatches", GPUBuffers.Get.SphereBatches);
            ComputeShaderManager.Get.SphereBatchCS.Dispatch(3, Mathf.CeilToInt(SceneManager.Get.NumLipidInstances / 64.0f), 1, 1);

            // Count occluder instances
            ComputeBuffer.CopyCount(GPUBuffers.Get.SphereBatches, GPUBuffers.Get.ArgBuffer, 0);

            // Clear lipid occlusion buffer 
            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(0, "_FlagBuffer", GPUBuffers.Get.LipidInstanceOcclusionFlags);
            ComputeShaderManager.Get.ComputeVisibilityCS.Dispatch(0, Mathf.CeilToInt(SceneManager.Get.NumLipidInstances / 64.0f), 1, 1);

            // Bind the read/write occlusion buffer to the shader
            // After this draw call the occlusion buffer will be filled with ones if an instance occluded and occludee, zero otherwise
            Graphics.SetRandomWriteTarget(1, GPUBuffers.Get.LipidInstanceOcclusionFlags);
            MyUtility.DummyBlit();   // Dunny why yet, but without this I cannot write to the buffer from the shader, go figure

            // Set the render target
            Graphics.SetRenderTarget(tempBuffer);

            OcclusionQueriesMaterial.SetInt("_CutObjectIndex", cutObjectIndex);
            OcclusionQueriesMaterial.SetInt("_NumIngredients", SceneManager.Get.NumAllIngredients);
            OcclusionQueriesMaterial.SetBuffer("_CutInfo", GPUBuffers.Get.CutInfo);
            OcclusionQueriesMaterial.SetTexture("_DistanceField", _floodFillTexturePong);

            OcclusionQueriesMaterial.SetFloat("_Scale", GlobalProperties.Get.Scale);
            OcclusionQueriesMaterial.SetBuffer("_LipidInstanceInfo", GPUBuffers.Get.LipidInstancesInfo);
            OcclusionQueriesMaterial.SetBuffer("_LipidInstancePositions", GPUBuffers.Get.LipidInstancePositions);
            OcclusionQueriesMaterial.SetBuffer("_OccludeeSphereBatches", GPUBuffers.Get.SphereBatches);
            OcclusionQueriesMaterial.SetPass(3);

            // Issue draw call for occluders - bounding quads only - depth/stencil test enabled - no write to color/depth/stencil
            Graphics.DrawProceduralIndirect(MeshTopology.Points, GPUBuffers.Get.ArgBuffer);
            Graphics.ClearRandomWriteTargets();

            ComputeShaderManager.Get.ComputeVisibilityCS.SetUniform("_CutObjectIndex", cutObjectIndex);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetUniform("_NumIngredients", SceneManager.Get.NumAllIngredients);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(4, "_CutInfo", GPUBuffers.Get.CutInfo);

            //// Discard occluding instances according to value2
            ComputeShaderManager.Get.ComputeVisibilityCS.SetUniform("_CutObjectId", cutObject.Id);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetUniform("_ConsumeRestoreState", internalState);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(4, "_Histograms", GPUBuffers.Get.Histograms);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(4, "_HistogramsLookup", GPUBuffers.Get.HistogramsLookup);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(4, "_LipidInstanceInfo", GPUBuffers.Get.LipidInstancesInfo);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(4, "_LipidInstanceCullFlags", GPUBuffers.Get.LipidInstanceCullFlags);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(4, "_LipidInstanceOcclusionFlags", GPUBuffers.Get.LipidInstanceOcclusionFlags);
            ComputeShaderManager.Get.ComputeVisibilityCS.Dispatch(4, Mathf.CeilToInt(SceneManager.Get.NumLipidInstances / 64.0f), 1, 1);
        }
    }

    //[ImageEffectOpaque]
    //void OnRenderImage(RenderTexture src, RenderTexture dst)
    void ComputeViewSpaceCutAways()
    {
        //ComputeProteinObjectSpaceCutAways();
        //ComputeLipidObjectSpaceCutAways();

        // Prepare and set the render target
        var tempBuffer = RenderTexture.GetTemporary(GetComponent<Camera>().pixelWidth, GetComponent<Camera>().pixelHeight, 32, RenderTextureFormat.ARGB32);
        //var tempBuffer = RenderTexture.GetTemporary(512, 512, 32, RenderTextureFormat.ARGB32);

        var resetCutSnapshot = CutObjectManager.Get.ResetCutSnapshot;
        CutObjectManager.Get.ResetCutSnapshot = -1;

        if (resetCutSnapshot > 0)
        {
            // Discard occluding instances according to value2
            ComputeShaderManager.Get.ComputeVisibilityCS.SetUniform("_CutObjectId", resetCutSnapshot);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetUniform("_ConsumeRestoreState", 2);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(3, "_Histograms", GPUBuffers.Get.Histograms);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(3, "_HistogramsLookup", GPUBuffers.Get.HistogramsLookup);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(3, "_IngredientMaskParams", GPUBuffers.Get.IngredientMaskParams);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(3, "_ProteinInstanceInfo", GPUBuffers.Get.ProteinInstancesInfo);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(3, "_ProteinInstanceCullFlags", GPUBuffers.Get.ProteinInstanceCullFlags);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(3, "_ProteinInstanceOcclusionFlags", GPUBuffers.Get.ProteinInstanceOcclusionFlags);
            ComputeShaderManager.Get.ComputeVisibilityCS.Dispatch(3, Mathf.CeilToInt(SceneManager.Get.NumProteinInstances / 64.0f), 1, 1);

            //// Discard occluding instances according to value2
            ComputeShaderManager.Get.ComputeVisibilityCS.SetUniform("_CutObjectId", resetCutSnapshot);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetUniform("_ConsumeRestoreState", 2);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(4, "_Histograms", GPUBuffers.Get.Histograms);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(4, "_HistogramsLookup", GPUBuffers.Get.HistogramsLookup);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(4, "_IngredientMaskParams", GPUBuffers.Get.IngredientMaskParams);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(4, "_LipidInstanceInfo", GPUBuffers.Get.LipidInstancesInfo);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(4, "_LipidInstanceCullFlags", GPUBuffers.Get.LipidInstanceCullFlags);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(4, "_LipidInstanceOcclusionFlags", GPUBuffers.Get.LipidInstanceOcclusionFlags);
            ComputeShaderManager.Get.ComputeVisibilityCS.Dispatch(4, Mathf.CeilToInt(SceneManager.Get.NumLipidInstances / 64.0f), 1, 1);
        }

        var cutObjectId = -1;

        foreach (var cutObject in CutObjectManager.Get.CutObjects)
        {
            cutObjectId++;

            //********************************************************//

            var internalState = 0;
            //Debug.Log(cutObject.CurrentLockState.ToString());

            if (cutObject.CurrentLockState == LockState.Restore)
            {
                Debug.Log("We restore");
                internalState = 2;
                cutObject.CurrentLockState = LockState.Unlocked;
            }

            if (cutObject.CurrentLockState == LockState.Consumed)
            {
                continue;
            }

            if (cutObject.CurrentLockState == LockState.Locked)
            {
                Debug.Log("We consume");
                internalState = 1;
                cutObject.CurrentLockState = LockState.Consumed;
            }

            //********************************************************//

            var maskLipid = false;
            var cullLipid = false;

            var maskProtein = false;
            var cullProtein = false;

            //Fill the buffer with occludees mask falgs
            var maskFlags = new List<int>();
            var cullFlags = new List<int>();

            foreach (var cutParam in cutObject.IngredientCutParameters)
            {
                var isMask = cutParam.IsFocus;
                var isCulled = !cutParam.IsFocus && (cutParam.Aperture > 0 || cutParam.value2 < 1);

                if (cutParam.IsFocus)
                {
                    if (cutParam.Id < SceneManager.Get.NumProteinIngredients) maskProtein = true;
                    else maskLipid = true;
                }

                if (isCulled)
                {
                    if (cutParam.Id < SceneManager.Get.NumProteinIngredients) cullProtein = true;
                    else cullLipid = true;
                }

                maskFlags.Add(isMask ? 1 : 0);
                cullFlags.Add(isCulled ? 1 : 0);
            }

            //if (!cullProtein && !cullLipid) continue;

            //********************************************************//

            //***** Compute Depth-Stencil mask *****//

            // Upload Occludees flags to GPU
            GPUBuffers.Get.IngredientMaskParams.SetData(maskFlags.ToArray());
            ComputeOcclusionMaskLEqual(tempBuffer, maskProtein, maskLipid);
            //ComputeOcclusionMaskGEqual(tempBuffer, maskProtein, maskLipid);
            //Graphics.Blit(tempBuffer, dst);
            //break;

            ComputeDistanceTransform(tempBuffer);

            //Graphics.Blit(_floodFillTexturePong, dst, CompositeMaterial, 4);
            //break;

            /////**** Compute Queries ***//

            // Upload Occluders flags to GPU
            GPUBuffers.Get.IngredientMaskParams.SetData(cullFlags.ToArray());
            ComputeOcclusionQueries(tempBuffer, cutObject, cutObjectId, internalState, cullProtein, cullLipid);
        }

        // Release render target
        RenderTexture.ReleaseTemporary(tempBuffer);
    }









    // With edges
    [ImageEffectOpaque]
    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        //return;

        if (_camera.pixelWidth == 0 || _camera.pixelHeight == 0) return;

        if (SceneManager.Get.NumProteinInstances == 0 && SceneManager.Get.NumLipidInstances == 0)
        {
            Graphics.Blit(src, dst);
            return;
        }

        CutAwayUtils.ComputeProteinObjectSpaceCutAways(_camera, WeightThreshold);
        CutAwayUtils.ComputeLipidObjectSpaceCutAways();
        ComputeViewSpaceCutAways();

        ///**** Start rendering routine ***

        // Declare temp buffers
        var colorBuffer = RenderTexture.GetTemporary(_camera.pixelWidth, _camera.pixelHeight, 0, RenderTextureFormat.ARGB32);
        var depthBuffer = RenderTexture.GetTemporary(_camera.pixelWidth, _camera.pixelHeight, 32, RenderTextureFormat.Depth);

        var atomIdBuffer = RenderTexture.GetTemporary(_camera.pixelWidth, _camera.pixelHeight, 0, RenderTextureFormat.RInt);
        var instanceIdBuffer = RenderTexture.GetTemporary(_camera.pixelWidth, _camera.pixelHeight, 0, RenderTextureFormat.RInt);

        var compositeColorBuffer = RenderTexture.GetTemporary(_camera.pixelWidth, _camera.pixelHeight, 0, RenderTextureFormat.ARGB32);
        var compositeColorBuffer2 = RenderTexture.GetTemporary(_camera.pixelWidth, _camera.pixelHeight, 0, RenderTextureFormat.ARGB32);

        var compositeDepthBuffer = RenderTexture.GetTemporary(_camera.pixelWidth, _camera.pixelHeight, 32, RenderTextureFormat.Depth);
        var compositeDepthBuffer2 = RenderTexture.GetTemporary(_camera.pixelWidth, _camera.pixelHeight, 32, RenderTextureFormat.Depth);

        // Clear temp buffers
        Graphics.SetRenderTarget(instanceIdBuffer);
        GL.Clear(true, true, new Color(-1, 0, 0, 0));

        Graphics.SetRenderTarget(colorBuffer.colorBuffer, depthBuffer.depthBuffer);
        GL.Clear(true, true, Color.white);

        // Draw proteins
        if (SceneManager.Get.NumProteinInstances > 0)
        {
            RenderUtils.ComputeSphereBatches(_camera);
            //DebugSphereBatchCount();
            RenderUtils.DrawProteinsAtoms(RenderProteinsMaterial, _camera, instanceIdBuffer.colorBuffer, atomIdBuffer.colorBuffer, depthBuffer.depthBuffer, 0);
        }

        // Draw Lipids
        if (SceneManager.Get.NumLipidInstances > 0)
        {
            RenderUtils.ComputeLipidSphereBatches(_camera);
            RenderUtils.DrawLipidSphereBatches(RenderLipidsMaterial, instanceIdBuffer, depthBuffer);
        }

        CutAwayUtils.ComputeVisibility(instanceIdBuffer);
        CutAwayUtils.FetchHistogramValues();

        //// Fetch color
        //CompositeMaterial.SetTexture("_IdTexture", itemBuffer);
        //Graphics.Blit(null, colorBuffer, CompositeMaterial, 3);

        //Compute color composition
        //ColorCompositeUtils.ComputeCoverage(instanceIdBuffer);
        //ColorCompositeUtils.CountInstances();
        ColorCompositeUtils.ComputeColorComposition(_camera, ColorCompositeMaterial, colorBuffer, instanceIdBuffer, atomIdBuffer, depthBuffer);

        // Compute contours detection
        SetContourShaderParams();
        ContourMaterial.SetTexture("_IdTexture", instanceIdBuffer);
        Graphics.Blit(colorBuffer, compositeColorBuffer, ContourMaterial, 0);

        //***** Collect Shadows *****//

        if (LightCameraController.EnableLight)
        {
            // Collect shadows
            CompositeMaterial.SetFloat("_LightSize", LightCameraController.LightSize);
            CompositeMaterial.SetFloat("_ShadowOffset", LightCameraController.ShadowOffset);
            CompositeMaterial.SetFloat("_ShadowFactor", LightCameraController.ShadowFactor);
            CompositeMaterial.SetFloat("_ShadowBias", LightCameraController.ShadowBias);
            CompositeMaterial.SetMatrix("_InverseView", _camera.cameraToWorldMatrix);
            CompositeMaterial.SetTexture("_ShadowMap", RenderShadowMap.ShadowMap2);
            CompositeMaterial.SetMatrix("_ShadowCameraViewMatrix", LightCamera.worldToCameraMatrix);
            CompositeMaterial.SetMatrix("_ShadowCameraViewProjMatrix", GL.GetGPUProjectionMatrix(LightCamera.projectionMatrix, false) * LightCamera.worldToCameraMatrix);
            CompositeMaterial.SetTexture("_DepthTexture", depthBuffer);
            Graphics.Blit(compositeColorBuffer, colorBuffer, CompositeMaterial, 5);
            Graphics.Blit(colorBuffer, compositeColorBuffer);
        }

        //Graphics.Blit(compositeColorBuffer, dst);

        // Composite with scene color
        CompositeMaterial.SetTexture("_ColorTexture", compositeColorBuffer);
        CompositeMaterial.SetTexture("_DepthTexture", depthBuffer);
        Graphics.Blit(null, src, CompositeMaterial, 0);
        Graphics.Blit(src, dst);

        Shader.SetGlobalTexture("_CameraDepthTexture", depthBuffer);

        ////Composite with scene depth
        //CompositeMaterial.SetTexture("_DepthTexture", depthBuffer);
        //Graphics.Blit(null, compositeDepthBuffer, CompositeMaterial, 1);

        //***** Draw Ghosts *****//

        //if (EnableGhosts)
        //{
        //    //// Clear temp buffers
        //    //Graphics.SetRenderTarget(instanceIdBuffer);
        //    //GL.Clear(true, true, new Color(-1, 0, 0, 0));

        //    //Graphics.SetRenderTarget(colorBuffer.colorBuffer, compositeDepthBuffer.depthBuffer);
        //    //GL.Clear(true, true, Color.white);

        //    //CompositeMaterial.SetTexture("_DepthTexture", depthBuffer);
        //    //Graphics.Blit(null, compositeDepthBuffer, CompositeMaterial, 7);

        //    //// Draw proteins
        //    //if (SceneManager.Get.NumProteinInstances > 0)
        //    //{
        //    //    RenderUtils.ComputeSphereBatchesClipped(_camera);
        //    //    RenderUtils.DrawClippedProteins(RenderProteinsMaterial, _camera, instanceIdBuffer.colorBuffer, compositeDepthBuffer.depthBuffer);
        //    //}

        //    ////Graphics.SetRenderTarget(colorBuffer);
        //    ////GL.Clear(true, true, Color.black);

        //    //// Get color from id buffer
        //    //ColorCompositeMaterial.SetTexture("_IdTexture", instanceIdBuffer);
        //    //ColorCompositeMaterial.SetTexture("_AtomIdTexture", atomIdBuffer);

        //    //ColorCompositeMaterial.SetBuffer("_AtomColors", GPUBuffers.Get.AtomColors);
        //    //ColorCompositeMaterial.SetBuffer("_ProteinColors", GPUBuffers.Get.IngredientsColors);
        //    //ColorCompositeMaterial.SetBuffer("_AminoAcidColors", GPUBuffers.Get.AminoAcidColors);
        //    //ColorCompositeMaterial.SetBuffer("_ProteinAtomInfos", GPUBuffers.Get.ProteinAtomInfo);
        //    //ColorCompositeMaterial.SetBuffer("_ProteinInstanceInfo", GPUBuffers.Get.ProteinInstanceInfo);
        //    //ColorCompositeMaterial.SetBuffer("_IngredientProperties", GPUBuffers.Get.ProteinIngredientProperties);
        //    //ColorCompositeMaterial.SetBuffer("_IngredientGroupsColor", GPUBuffers.Get.IngredientGroupsColor);

        //    ///********/

        //    //ColorCompositeMaterial.SetBuffer("_IngredientGroupsLerpFactors", GPUBuffers.Get.IngredientGroupsLerpFactors);
        //    //ColorCompositeMaterial.SetBuffer("_IngredientGroupsColorValues", GPUBuffers.Get.IngredientGroupsColorValues);
        //    //ColorCompositeMaterial.SetBuffer("_IngredientGroupsColorRanges", GPUBuffers.Get.IngredientGroupsColorRanges);
        //    //ColorCompositeMaterial.SetBuffer("_ProteinIngredientsRandomValues", GPUBuffers.Get.ProteinIngredientsRandomValues);

        //    ////CompositeMaterial.SetBuffer("_ProteinAtomColors", GPUBuffers.Get.ProteinAtomColors);
        //    ////CompositeMaterial.SetBuffer("_ProteinAtomInfo", GPUBuffers.Get.ProteinAtomInfo);
        //    ////CompositeMaterial.SetBuffer("_LipidInstanceInfo", GPUBuffers.Get.LipidInstancesInfo);        

        //    ////Graphics.Blit(null, colorBuffer, CompositeMaterial, 3);
        //    //Graphics.Blit(null, colorBuffer, ColorCompositeMaterial, 1);

        //    //// Compute contours detection
        //    //SetContourShaderParams();
        //    //ContourMaterial.SetTexture("_IdTexture", instanceIdBuffer);
        //    //Graphics.Blit(colorBuffer, compositeColorBuffer2, ContourMaterial, 1);

        //    //Graphics.Blit(compositeColorBuffer2, dst);

        //    //// Compose final image
        //    //CompositeMaterial.SetFloat("_GhostContours", GhostContours);
        //    //CompositeMaterial.SetTexture("_MainTex2", compositeColorBuffer);
        //    //Graphics.Blit(compositeColorBuffer2, colorBuffer, CompositeMaterial, 8);

        //    //Graphics.Blit(colorBuffer, compositeColorBuffer);
        //}

        /*** Object Picking ***/

        if (SelectionManager.Instance.MouseRightClickFlag)
        {
            SelectionManager.Instance.SetSelectedObject(MyUtility.ReadPixelId(instanceIdBuffer, SelectionManager.Instance.MousePosition));
            SelectionManager.Instance.MouseRightClickFlag = false;
        }

        // Release temp buffers
        RenderTexture.ReleaseTemporary(instanceIdBuffer);
        RenderTexture.ReleaseTemporary(atomIdBuffer);

        RenderTexture.ReleaseTemporary(colorBuffer);
        RenderTexture.ReleaseTemporary(depthBuffer);

        RenderTexture.ReleaseTemporary(compositeColorBuffer);
        RenderTexture.ReleaseTemporary(compositeColorBuffer2);

        RenderTexture.ReleaseTemporary(compositeDepthBuffer);
        RenderTexture.ReleaseTemporary(compositeDepthBuffer2);
    }
}
