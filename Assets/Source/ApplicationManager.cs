using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ApplicationManager : MonoBehaviour
{
    public int targetFrameRate = 60;
    public TextMeshProUGUI fps;
    void Awake()
    {
        Application.targetFrameRate = targetFrameRate;
        DontDestroyOnLoad(this);
    }

    float f = 0;
    private void Update()
    {

        if (Input.GetKeyUp(KeyCode.Escape))
            StartCoroutine(quitSequence());
        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            targetFrameRate += 10;
            Application.targetFrameRate = targetFrameRate;
        }
        else if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            targetFrameRate -= 10;
            Application.targetFrameRate = targetFrameRate;

        }
        if (fps)
        {
            f *= .9f;
            f += .1f / Time.deltaTime;
            if (Time.frameCount % 3 == 0)
                fps.text = $"{Mathf.RoundToInt(f)} / {targetFrameRate}";
        }

    }


    IEnumerator quitSequence()
    {
        yield return null;
        yield return null;
        Application.Quit();

    }
}
