using UnityEngine;

public class ShowTip : MonoBehaviour
{
    public TextMesh myText;
    public GameObject[] icons;

    private float showTime = 2.5f;
    private float rotateTime = 0.5f;
    private float showed = 0;
    private Quaternion originRotation = new Quaternion(0, -135, 0, 0);

    void Start()
    {

    }
    
    void Update()
    {
        showed += Time.deltaTime;
        var newPos = myText.transform.position;
        newPos.y += Time.deltaTime;
        myText.transform.position = newPos;

        if(showed <= rotateTime)
        {
            //var newR = myText.transform.rotation;
            //newR.y += Time.deltaTime;
            //myText.gameObject.transform.rotation = newR;
            myText.gameObject.transform.Rotate(new Vector3(0, Time.deltaTime*810, 0));
        }
        if (showed >= showTime)
        {
            gameObject.SetActive(false);
        }
    }

    public void Show(Vector3 pos, string tip, int index)
    {
        myText.gameObject.SetActive(true);
        myText.gameObject.transform.position = pos;
        myText.gameObject.transform.rotation = originRotation;
        //myText.gameObject.transform.position = pos;
        myText.text = tip;
        showed = 0;

        for(int i = 0; i < icons.Length; i ++)
        {
            icons[i].SetActive(false);
            if (SwitchLight.state)
            {
                icons[i].GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.black);
            }
            else
            {
                icons[i].GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.white);
            }
        }
        icons[index].SetActive(true);
    }
}
