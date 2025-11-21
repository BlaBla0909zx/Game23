// using System;
// using System.Collections.Generic;


// public static class SaveRegistry
// {
//     private static Dictionary<Type, object> saveableInstances = new Dictionary<Type, object>();

//     public static void Register<T>(ISaveable<T> instance)
//     {
//         saveableInstances[typeof(T)] = instance;
//     }

//     public static void Unregister<T>()
//     {
//         saveableInstances.Remove(typeof(T));
//     }

//     public static ISaveable<T> GetInstance<T>()
//     {
//         if (saveableInstances.TryGetValue(typeof(T), out object instance))
//         {
//             return instance as ISaveable<T>;
//         }
//         return null;
//     }

//     public static IEnumerable<(Type type, object instance)> GetAllInstances()
//     {
//         foreach (var kvp in saveableInstances)
//         {
//             yield return (kvp.Key, kvp.Value);
//         }
//     }
// }
using System;
using System.Collections.Generic;
using UnityEngine;

public static class SaveRegistry
{
    // Lưu instances theo T (kiểu generic của ISaveable<T>)
    private static Dictionary<Type, object> saveableInstances = new Dictionary<Type, object>();

    // Đăng ký instance, bắt buộc chỉ định T rõ ràng
    public static void Register<T>(ISaveable<T> instance)
    {
        Type key = typeof(T);

        if (saveableInstances.ContainsKey(key))
        {
            Debug.LogWarning($"SaveRegistry: Overwriting existing ISaveable<{key.Name}> instance!");
        }

        saveableInstances[key] = instance;
    }

    // Hủy đăng ký
    public static void Unregister<T>()
    {
        saveableInstances.Remove(typeof(T));
    }

    // Lấy instance an toàn
    public static ISaveable<T> GetInstance<T>()
    {
        Type key = typeof(T);

        if (saveableInstances.TryGetValue(key, out object instance))
        {
            if (instance is ISaveable<T> typed)
            {
                return typed;
            }

            Debug.LogError($"SaveRegistry: Found instance for {key.Name} but it is not ISaveable<{key.Name}>! Actual type: {instance.GetType()}");
            return null;
        }

        Debug.LogWarning($"SaveRegistry: No ISaveable<{key.Name}> instance found.");
        return null;
    }

    // Duyệt tất cả instance, dùng cho debug
    public static IEnumerable<(Type type, object instance)> GetAllInstances()
    {
        foreach (var kvp in saveableInstances)
        {
            yield return (kvp.Key, kvp.Value);
        }
    }
}

public interface ISaveable<T>
{
    T GetSaveData();
    void SetData(T data);
}