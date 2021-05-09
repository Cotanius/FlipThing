using UnityEngine;

public class RotateY : MonoBehaviour
{
    float y = 0;
    void Update()
    {
        y += Time.deltaTime * 5.0f;
        var e = transform.localEulerAngles;
        e.y = Mathf.Sin(y) * 10 + 90;
        transform.localEulerAngles = e;
    }
}
