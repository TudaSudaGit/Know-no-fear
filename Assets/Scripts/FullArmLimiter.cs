using UnityEngine;

public class FullArmLimiter : MonoBehaviour
{
    [Header("Ссылки на кости")]
    public Transform bone1;
    public Transform bone2;
    public Transform bone3;
    public Transform bone4;

    private const float B1_MIN = -85f;
    private const float B1_MAX = -85f;

    private const float B2_MIN = 0f;
    private const float B2_MAX = 0f;

    private const float B3_MIN = 41f;
    private const float B3_MAX = 98f;

    private const float B4_MIN = -180f;
    private const float B4_MAX = 180f;

    void LateUpdate()
    {
        if (bone1) ApplyLimit(bone1, B1_MIN, B1_MAX);
        if (bone2) ApplyLimit(bone2, B2_MIN, B2_MAX);
        if (bone3) ApplyLimit(bone3, B3_MIN, B3_MAX);
        if (bone4) ApplyLimit(bone4, B4_MIN, B4_MAX);
    }

    private void ApplyLimit(Transform bone, float min, float max)
    {
        float angle = bone.localEulerAngles.z;
        float signedAngle = Mathf.DeltaAngle(0, angle);

        float clampedAngle = Mathf.Clamp(signedAngle, min, max);

        bone.localRotation = Quaternion.Euler(0, 0, clampedAngle);
    }
}