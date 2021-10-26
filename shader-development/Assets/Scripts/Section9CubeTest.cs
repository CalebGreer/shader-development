using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Section9CubeTest : MonoBehaviour
{
    [SerializeField] private Material mat;

    private void Update()
    {
        mat.SetVector("_Position", this.transform.position);
    }
}
