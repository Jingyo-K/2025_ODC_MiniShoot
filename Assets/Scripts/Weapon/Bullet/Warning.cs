using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Warning : MonoBehaviour
{
    private Color color;
    private Color targetColor = Color.red;
    private float duration = 1.0f; // Duration for the color change
    public bool isSet = false;
    void Start()
    {
        color = transform.GetChild(0).GetComponent<SpriteRenderer>().color;
        if(isSet)
        {
            SetWarning(); // If already set, start the color change immediately
        }
    }

    public void SetWarning()
    {
        isSet = true; // Start the color change
        transform.GetChild(0).GetComponent<SpriteRenderer>().color = color; // Ensure the initial color is set
        StartCoroutine(ChangeColor());
    }   

    private IEnumerator ChangeColor()
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.Lerp(color, targetColor, t);
            yield return null;
        }
        transform.GetChild(0).GetComponent<SpriteRenderer>().color = targetColor;
        Destroy(gameObject); // Destroy the warning object after the color change
    }
}
