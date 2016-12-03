using UnityEngine;
using System.Collections;

public class SwitchLight : MonoBehaviour
{
    Color bgColor = Color.cyan;
    Color ambientColor = Color.gray;
    Color emissionColor = Color.black;

    [HideInInspector]
    public Light headLight;
    [HideInInspector]
    public Material headMaterial;

    Light light;
    Camera camera;
    
    bool begin = false;
    private int onoff = -1;
    private float accumulation = 1;

    void Start()
    {
        
    }

    public void Init()
    {
        light = GameObject.Find("Light").GetComponent<Light>();
        camera = GameObject.Find("Camera").GetComponent<Camera>();

        camera.backgroundColor = Color.cyan;
        light.intensity = 1;
        headLight.intensity = 0;
        RenderSettings.ambientLight = Color.gray;

        headMaterial.EnableKeyword("_EMISSION");
        headMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        headMaterial.SetColor("_EmissionColor", Color.black);
    }

    void Update()
    {
        if(!begin)
        {
            return;
        }

        if(onoff == -1 && bgColor.g < 0)
        {
            camera.backgroundColor = Color.black;
            light.intensity = 0;
            headLight.intensity = 5;
            headMaterial.SetColor("_EmissionColor", Color.white);
            RenderSettings.ambientLight = Color.black;
            accumulation = 0;
            begin = false;
            this.enabled = false;
        }
        if(onoff == 1 && bgColor.g > 1)
        {
            camera.backgroundColor = Color.cyan;
            light.intensity = 1;
            headLight.intensity = 0;
            headMaterial.SetColor("_EmissionColor", Color.black);
            RenderSettings.ambientLight = Color.gray;
            accumulation = 1;
            begin = false;
            this.enabled = false;
        }

        accumulation += Time.deltaTime * onoff;

        bgColor.g = accumulation;
        bgColor.b = accumulation;
        ambientColor.r = accumulation / 2;
        ambientColor.g = ambientColor.r;
        ambientColor.b = ambientColor.r;
        emissionColor.r = 1 - accumulation;
        emissionColor.g = emissionColor.r;
        emissionColor.b = emissionColor.r;

        camera.backgroundColor = bgColor;
        light.intensity = accumulation;
        headLight.intensity = 5 - accumulation * 5;
        headMaterial.SetColor("_EmissionColor", emissionColor);
        RenderSettings.ambientLight = ambientColor;
    }


    public void turnOff()
    {
        onoff = -1;
        begin = true;
    }

    public void turnOn()
    {
        onoff = 1;
        begin = true;
    }
}
