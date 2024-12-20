using UnityEngine;

public class Toolbox
{
    public static void DestroyChildren(GameObject gameObject, bool coroutine = false)
    {
        int cc = gameObject.transform.childCount;
        if (cc > 0)
        {
            foreach (Transform child in gameObject.transform)
            {
#if UNITY_EDITOR
                if (coroutine)
                {
                    UnityEditor.EditorApplication.delayCall += () =>
                    {
                        GameObject.DestroyImmediate(child.gameObject);
                    };
                }
                else
                {
                    GameObject.DestroyImmediate(child.gameObject);
                }
#else
        GameObject.Destroy(child.gameObject);
#endif
            }
        }
    }

    public static string NameFromPath(string path, bool removeExtension = true)
    {
        string[] subs = path.Split('/');
        var lastPart =  subs[subs.Length - 1];
        if (removeExtension)
        {
            string[] name = lastPart.Split('.');
            return name[0];
        }
        return lastPart;
    }
    public static bool IsWithin(int value, int minimum, int maximum)
    {
        return value >= minimum && value <= maximum;
    }
}