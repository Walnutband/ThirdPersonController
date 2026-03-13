
using System;
using System.Collections.Generic;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEngine;
using UnityEngine.UI;

public static class ExtensionMethods
{
    private static readonly List<Transform> s_CachedTransforms = new List<Transform>();

    /// <summary>
    /// （安全方法）保证只有运行时才执行DontDestroyOnLoad操作，
    /// </summary>
    /// <returns>返回是否成功添加到DontDestroyOnLoad场景中</returns>
    public static bool DontDestroyOnLoad(UnityEngine.Object obj)
    {
        if (Application.isPlaying)
        {
            UnityEngine.Object.DontDestroyOnLoad(obj);
            return true;
        }
        else
        {
            Debug.LogError("只能在运行时使用DontDestroyOnLoad场景");
            return false;
        }
    }

    public static T GetOrAddComponent<T>(this GameObject obj) where T : Component
    {
        T component = obj.GetComponent<T>();
        if (component == null)
        {
            component = obj.AddComponent<T>();
        }
        return component;
    }

    public static T GetOrAddComponent<T>(this Component comp) where T : Component
    {
        T component = comp.GetComponent<T>();
        if (component == null)
        {
            component = comp.gameObject.AddComponent<T>();
        }
        return component;
    }

    public static Component GetOrAddComponent(this Component obj, Type type)
    {
        return obj.gameObject.GetOrAddComponent(type);
    }

    public static Component GetOrAddComponent(this GameObject gameObject, Type type)
    {
        if (gameObject == null) return null;

        Component component = gameObject.GetComponent(type);
        if (component == null)
            component = gameObject.AddComponent(type);
        return component;
    }

    public static Vector3 TransformLocalPoint(this Transform original, Transform target, Vector3 point)
    {
        Vector3 worldPoint = original.TransformPoint(point);
        return target.InverseTransformPoint(worldPoint);
    }

    public static float TransformLocalPointX(this Transform original, Transform target, float point)
    {
        Vector3 p = original.TransformLocalPoint(target, new Vector3(point, 0f, 0f));
        return p.x;
    }

    public static float TransformLocalPointY(this Transform original, Transform target, float point)
    {
        Vector3 p = original.TransformLocalPoint(target, new Vector3(0f, point, 0f));
        return p.y;
    }

    public static Transform FindRecursively(this Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name != childName)
            {
                return FindRecursively(child, childName);
            }
            else return child;
        }
        return null;
    }

    /// <summary>
    /// 设置父对象，并且重置属性值。
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="parent"></param>
    public static void SetParentEx(this Transform transform, Transform parent)
    {
        transform.SetParent(parent);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }

    public static void SetLayerRecursively(this GameObject gameObject, int layer)
    {
        gameObject.GetComponentsInChildren(true, s_CachedTransforms);
        for (int i = 0; i < s_CachedTransforms.Count; i++)
        {
            s_CachedTransforms[i].gameObject.layer = layer;
        }
        s_CachedTransforms.Clear();
    }

    public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue value)
    {
        if (dict.ContainsKey(key))
            dict[key] = value;
        else
            dict.Add(key, value);
    }

    public static void SetAlpha(this Image image, float alpha)
    {
        alpha = Mathf.Clamp01(alpha);
        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }
}