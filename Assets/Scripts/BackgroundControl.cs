using System.Collections;
using UnityEngine;

public class BackgroundControl : MonoBehaviour 
{
    public float moveInterval = 1.0f;
    private Vector3 moveDirction;
    //private Vector3 originPosition;
    //private Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right };

	void Start () 
    {
        //originPosition = transform.localPosition;
        Random.InitState((int)(moveInterval * 10));
        StartCoroutine(doMove());
	}

    IEnumerator doMove()
    {
        while (true)
        {
            moveDirction = genDirection();
            StartCoroutine(moveBackground(moveDirction));
            yield return new WaitForSeconds(moveInterval);
            StopCoroutine(moveBackground(moveDirction));
            StartCoroutine(moveBackground(-moveDirction));
            yield return new WaitForSeconds(moveInterval);
            StopCoroutine(moveBackground(moveDirction));
        }
    }
	
	void Update () 
    {
        //Time.deltaTime
	}

    IEnumerator moveBackground(Vector3 direction)
    {
        //var direction = genDirection();
        while(true)
        {
            transform.localPosition += direction * Time.deltaTime * 0.1f;
            yield return null;
        }
    }

    private Vector3 genDirection()
    {
        Vector3 result;
        var random = Mathf.CeilToInt(Random.value * 4);
        switch(random)
        {
            case 0:
                result = Vector3.up;
                break;
            case 1:
                result = Vector3.down;
                break;
            case 2:
                result = Vector3.left;
                break;
            default:
                result = Vector3.right;
                break;
            //case 4:
            //    result = Vector3.forward;
            //    break;
            //default :
                //result = Vector3.back;
                //break;
        }

        return result;
    }
}
