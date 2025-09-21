using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;

public class UITKFrameAnimator : MonoBehaviour
{
    public string framesPath = "Animations/MyAnim"; // Inside Resources folder
    public float frameRate = 0.1f; // Seconds per frame
    public bool loop = true;

    private VisualElement targetElement;
    private List<Texture2D> frames = new List<Texture2D>();
    private int currentFrame = 0;
    private Coroutine animationCoroutine;

    void Start()
    {
        // Get root element from UIDocument
        var root = GetComponent<UIDocument>().rootVisualElement;
        targetElement = root.Q<VisualElement>("LoadingAnim");

        // Load all frames as Texture2D
        Object[] loadedFrames = Resources.LoadAll(framesPath, typeof(Texture2D));
        foreach (var frame in loadedFrames)
        {
            frames.Add(frame as Texture2D);
        }

        if (frames.Count > 0)
            animationCoroutine = StartCoroutine(PlayAnimation());
        else
            Debug.LogError("No frames found in " + framesPath);
    }

    IEnumerator PlayAnimation()
    {
        do
        {
            targetElement.style.backgroundImage = new StyleBackground(frames[currentFrame]);
            currentFrame = (currentFrame + 1) % frames.Count;
            yield return new WaitForSeconds(frameRate);
        }
        while (loop);
    }
}
