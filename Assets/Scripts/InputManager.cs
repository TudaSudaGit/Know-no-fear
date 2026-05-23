using UnityEngine;

public static class InputManager
{
    public static KeyCode MoveLeftKey { get; private set; }
    public static KeyCode MoveRightKey { get; private set; }
    public static KeyCode AimKey { get; private set; }
    public static KeyCode ShootKey { get; private set; }
    public static KeyCode ReloadKey { get; private set; }
    public static KeyCode InteractKey { get; private set; }


    static InputManager()
    {
        LoadKeys();
    }

    public static void LoadKeys()
    {
        MoveLeftKey = (KeyCode)PlayerPrefs.GetInt("Key_MoveLeft", (int)KeyCode.A);
        MoveRightKey = (KeyCode)PlayerPrefs.GetInt("Key_MoveRight", (int)KeyCode.D);
        AimKey = (KeyCode)PlayerPrefs.GetInt("Key_Aim", (int)KeyCode.Mouse1);
        ShootKey = (KeyCode)PlayerPrefs.GetInt("Key_Shoot", (int)KeyCode.Mouse0);
        ReloadKey = (KeyCode)PlayerPrefs.GetInt("Key_Reload", (int)KeyCode.R);
        InteractKey = (KeyCode)PlayerPrefs.GetInt("Key_Interact", (int)KeyCode.E);
    }

    public static void SetKey(string keyName, KeyCode newKey)
    {
        switch (keyName)
        {
            case "MoveLeft": MoveLeftKey = newKey; break;
            case "MoveRight": MoveRightKey = newKey; break;
            case "Aim": AimKey = newKey; break;
            case "Shoot": ShootKey = newKey; break;
            case "Reload": ReloadKey = newKey; break;
            case "Interact": InteractKey = newKey; break;
        }
        PlayerPrefs.SetInt("Key_" + keyName, (int)newKey);
        PlayerPrefs.Save();
    }

    public static string GetKeyFriendlyName(KeyCode key)
    {
        if (key == KeyCode.Mouse0) return "ЛКМ";
        if (key == KeyCode.Mouse1) return "ПКМ";
        if (key == KeyCode.Mouse2) return "СКМ";
        return key.ToString().ToUpper();
    }
}