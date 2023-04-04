using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Light))]
public class ShadowMapRenderer : MonoBehaviour
{
    public RenderTexture ShadowMap;
    public Camera _shadowMapCamera;

    private void Update()
    {
        Light light = GetComponent<Light>();

        if (light.type != LightType.Spot && light.type != LightType.Directional)
        {
            Debug.LogError("ShadowMapRenderer supports only Spot and Directional lights.");
            enabled = false;
            return;
        }

        Debug.Log("CREATING SHADOWMAP ---------------------------------------");
        //_shadowMapCamera = new GameObject("ShadowMapCamera").AddComponent<Camera>();
        _shadowMapCamera.transform.SetParent(transform);
        _shadowMapCamera.transform.localPosition = Vector3.zero;
        _shadowMapCamera.transform.localRotation = Quaternion.identity;
        _shadowMapCamera.enabled = false;
        _shadowMapCamera.orthographic = light.type == LightType.Directional;
        _shadowMapCamera.fieldOfView = light.spotAngle;
        _shadowMapCamera.nearClipPlane = 0.01f;
        _shadowMapCamera.farClipPlane = light.range;
        _shadowMapCamera.targetTexture = ShadowMap;
        _shadowMapCamera.clearFlags = CameraClearFlags.SolidColor;
        _shadowMapCamera.backgroundColor = Color.white;
        _shadowMapCamera.depthTextureMode = DepthTextureMode.Depth;

    }

    private void OnPreRender()
    {
        _shadowMapCamera.Render();
    }

    private void OnDestroy()
    {
        Destroy(_shadowMapCamera.gameObject);
    }
}
