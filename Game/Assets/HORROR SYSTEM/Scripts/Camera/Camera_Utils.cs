using System.Collections.Generic;
using UnityEngine;

public static class LayerMaskHelper
{
    public static bool IsInLayerMask(this LayerMask mask, int layer)
    {
        return ((mask.value & (1 << layer)) > 0);
    }

    public static bool IsInLayerMask(this LayerMask mask, GameObject obj)
    {
        return ((mask.value & (1 << obj.layer)) > 0);
    }
}

public enum CameraType
{
    FPS = 1,
    TPS = 2
}

public struct RayCastWithMags
{
    public RaycastHit hit;
    public float distanceFromCamera;
    public float distanceFromTarget;
}

public class TransformWithTime
{
    public Vector3 pos;
    public Quaternion rot;
    public Vector3 scale;
    public float time;
}

public class SortRayCastsTarget : IComparer<RayCastWithMags>
{
    public int Compare(RayCastWithMags a, RayCastWithMags b)
    {
        if (a.distanceFromTarget > b.distanceFromTarget) return 1;
        else if (a.distanceFromTarget < b.distanceFromTarget) return -1;
        else return 0;
    }
}

public class SortRayCastsCamera : IComparer<RayCastWithMags>
{
    public int Compare(RayCastWithMags a, RayCastWithMags b)
    {
        if (a.distanceFromCamera > b.distanceFromCamera) return 1;
        else if (a.distanceFromCamera < b.distanceFromCamera) return -1;
        else return 0;
    }
}