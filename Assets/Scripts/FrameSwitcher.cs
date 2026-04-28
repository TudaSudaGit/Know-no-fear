using UnityEngine;
using UnityEngine.U2D.Animation; 

public class FrameSwitcher : MonoBehaviour
{
    private SpriteResolver resolver;

    void Awake()
    {
        resolver = GetComponent<SpriteResolver>();
    }

    public void SetFrame(int frameNumber) 
    {
        string label = $"Frame_{frameNumber:00}";
        resolver.SetCategoryAndLabel("Walk", label);
        for (int i = 1; i <= 19; i++)
        {
            Transform child = transform.Find($"{i}.png");
            if (child != null)
                child.gameObject.SetActive(i == frameNumber);
        }
    }
}