using UnityEngine;
using System.Collections;
using UnityEditor;

public class MyEditor
{
    [MenuItem("test/test")]
    public static void test()
    {
        Vector3 v1 = new Vector3(1, 1, 1);
        Vector3 v2 = new Vector3(0, 0, 0);
        Vector3 vs = Vector3.Lerp(v1, v2, 1);
        Debug.Log(vs);
    }
}
