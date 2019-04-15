using UnityEngine;

public class LightFlash : MonoBehaviour
{
    private int flashTime = 5; //闪烁次数，一亮一暗算一次
    private int flag = 1; //正在变亮为 1，变暗为 -1
    private const int LIGHT_STRENGTH = 4; //亮度
    private Material mMaterial;
    private Light mLight;

    void Start()
    {
        Init();
    }
    
    void Update()
    {

    }

    private void FixedUpdate()
    {
        if (flashTime == 0)
            return;

        mLight.intensity += Time.fixedDeltaTime * flag * LIGHT_STRENGTH;
        var color = mLight.intensity / 2 / LIGHT_STRENGTH;
        mMaterial.SetColor("_EmissionColor", new Color(color, color, color));

        if(mLight.intensity >= LIGHT_STRENGTH)
        {
            flag = -1;
        }
        if(mLight.intensity <= 0)
        {
            flag = 1;
            flashTime--;
        }
    }

    private void Init()
    {
        gameObject.AddComponent<Light>();
        mLight = gameObject.GetComponent<Light>();
        mLight.intensity = 0;
        mLight.range = 7;
        mLight.type = LightType.Point;

        mMaterial = GetComponent<Renderer>().material;
        mMaterial.EnableKeyword("_EMISSION");
        mMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        mMaterial.SetColor("_EmissionColor", Color.black);
    }
}
