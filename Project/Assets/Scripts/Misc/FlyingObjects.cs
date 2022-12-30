using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingObjects : MonoBehaviour
{
    [SerializeField] GameObject objectPrefab;
    [Range( 1, 100 )]
    [SerializeField] int objectCount = 5;
    [Range( -5.0f, 5.0f )]
    [SerializeField] float radius = 1.0f;
    [Range( 1.0f, 100.0f )]
    [SerializeField] float spinSpeed = 1.0f;
    [Range( 0.0f, 1.0f )]
    [SerializeField] float scaleRadiusByObjectCount = 0.0f;

    private float time;

    void Start()
    {
        Init();
    }

    public void Init()
    {
        for( int i = transform.childCount - 1; i >= 0; --i )
            transform.GetChild( i ).DestroyGameObject();

        if( objectPrefab != null )
            for( int i = 0; i < objectCount; ++i )
                Instantiate( objectPrefab, transform );

        UpdatePositions();
    }

    void Update()
    {
        time += Time.deltaTime * spinSpeed * Mathf.Deg2Rad;
        UpdatePositions();
    }

    void UpdatePositions()
    {
        for( int i = 0; i < transform.childCount; ++i )
        {
            float angle = ( Mathf.PI * 2.0f / objectCount ) * i + time;
            float radiusFinal = radius + ( scaleRadiusByObjectCount * objectCount );
            transform.GetChild( i ).transform.localPosition = new Vector3( Mathf.Sin( angle ), 0.0f, Mathf.Cos( angle ) ) * radiusFinal;
        }
    }
}
