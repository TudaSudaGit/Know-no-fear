using UnityEngine;
using System.Collections;

public class LaserBeam : MonoBehaviour
{
    private LineRenderer line;

    [Header("Настройки луча")]
    public float duration = 0.15f;     
    public float expandSpeed = 110f;   
    public Color laserColor = new Color(1f, 0.2f, 0.2f, 1f);
    public float startWidth = 0.3f;  
    public float endWidth = 0.25f;  

    private Vector3 startPos;
    private Vector3 endPos;

    public void Fire(Vector3 from, Vector3 to)
    {
        startPos = from;
        endPos = to;
        StartCoroutine(ShootBeam());
    }
    Gradient MakeLaserGradient(float alpha = 1f)
    {
        Gradient g = new Gradient();
        g.SetKeys(
            new GradientColorKey[]
            {
            new GradientColorKey(laserColor, 0f),
            new GradientColorKey(Color.white, 0.5f), 
            new GradientColorKey(laserColor, 1f)
            },
            new GradientAlphaKey[]
            {
            new GradientAlphaKey(alpha, 0f),
            new GradientAlphaKey(alpha, 1f)
            }
        );
        return g;
    }
    IEnumerator ShootBeam()
    {
        line = gameObject.AddComponent<LineRenderer>();
        line.positionCount = 2;
        line.startWidth = startWidth;
        line.endWidth = endWidth;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.colorGradient = MakeLaserGradient();
        line.useWorldSpace = true;

        line.SetPosition(0, startPos);
        line.SetPosition(1, startPos); 

        
        float t = 0f;
        float dist = Vector3.Distance(startPos, endPos);
        float travelTime = dist / expandSpeed;

        while (t < travelTime)
        {
            t += Time.deltaTime;
            Vector3 current = Vector3.Lerp(startPos, endPos, t / travelTime);
            line.SetPosition(1, current);
            yield return null;
        }

        line.SetPosition(1, endPos);


        float fadeTime = duration;
        float elapsed = 0f;

        Destroy(gameObject);
    }
}