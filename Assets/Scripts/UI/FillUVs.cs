using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Fill in the second set of UV's of a slice9 texture with the unscaled UV values.
/// Used by the SpriteGradient shader applied to buttons.
/// </summary>
public class FillUVs : BaseMeshEffect
{
    private readonly List<UIVertex> vertList = new List<UIVertex>();

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive())
        {
            return;
        }

        vh.GetUIVertexStream(vertList);

        if (vertList.Count < 52)
        {
            vertList.Clear();
            return;
        }

        Vector3 min = vertList[0].position;
        Vector3 max = vertList[51].position;
        Vector3 diff = max - min;
        Vector3 adj;

        vh.Clear();

        for (int i = 0; i < vertList.Count; i++)
        {
            UIVertex vert = vertList[i];

            adj = vert.position - min;
            vert.uv1 = new Vector2(adj.x / diff.x, adj.y / diff.y);
            vertList[i] = vert;
        }

        vh.AddUIVertexTriangleStream(vertList);

        vertList.Clear();
    }
}
