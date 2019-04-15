using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public static class NativeInterface
{
    #region DllImport

#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void SUN_TapTicImpact();

#endif

    #endregion

    public static void TapTicImpact()
    {
#if UNITY_IOS && !UNITY_EDITOR
        SUN_TapTicImpact();
#endif
    }

}
