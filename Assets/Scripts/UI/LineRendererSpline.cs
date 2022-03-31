using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineRendererSpline : MonoBehaviour
{
    [SerializeField] private Tooltip tooltip;

    private float vertexCount = 12;
    private float point2Height = .8f;
    private float devideBy = 2.4f;

    void Update()
    {
        tooltip.TT_Mid.transform.position = new Vector3((tooltip.TT_Mid.transform.position.x + tooltip.TT_End.transform.position.x) / devideBy, point2Height, (tooltip.TT_Start.transform.position.z + tooltip.TT_End.transform.position.z) / devideBy);
        var pointList = new List<Vector3>();

        for (float ratio = 0; ratio <= 1; ratio += 1 / vertexCount)
        {
            var tangent1 = Vector3.Lerp(tooltip.TT_Start.position, tooltip.TT_Mid.position, ratio);
            var tangent2 = Vector3.Lerp(tooltip.TT_Mid.position, tooltip.TT_End.position, ratio);
            var curve = Vector3.Lerp(tangent1, tangent2, ratio);

            pointList.Add(curve);
        }

        tooltip.Linerenderer.positionCount = pointList.Count;
        tooltip.Linerenderer.SetPositions(pointList.ToArray());
    }
}