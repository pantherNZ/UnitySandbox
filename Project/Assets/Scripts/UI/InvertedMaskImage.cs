using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class InvertedMaskImage : Image
{
    public override Material materialForRendering
    {
        get
        {
            var result = new Material(base.materialForRendering);
            result.SetInt( "_StencilComp", ( int )CompareFunction.NotEqual );
            return result;

        }
    }
}