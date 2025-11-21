using System;
using System.IO;
using System.Reflection;

using UnityEngine;


public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance
    {
        get; private set;
    }

    private bool _isSaving = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }


    private void OnApplicationPause(bool pause)
    {
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            if (pause)
            {
                if(_isSaving == false)
                   SaveAllData();
            }
#endif
    }

    private void OnApplicationFocus(bool focus)
    {
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
           if(!focus )
            {
                if (_isSaving == false)
                      SaveAllData();
            }
#endif
    }

    private void OnApplicationQuit()
    {
        if (_isSaving == false)
            SaveAllData();
    }


    public void SaveData<T>()
    {
        var instance = SaveRegistry.GetInstance<T>();
        if (instance == null)
        {
            Debug.LogWarning($"No saveable instance registered for type {typeof(T).Name}");
            return;
        }

        var saveData = instance.GetSaveData();
        var attribute = typeof(T).GetCustomAttribute<SaveDataAttribute>();

        if (attribute == null)
        {
            Debug.LogWarning($"Type {typeof(T).Name} does not have SaveData attribute");
            return;
        }

        SaveToFile(saveData, attribute.FileName, attribute.FolderPath);
    }

    public T LoadData<T>()
    {
        var instance = SaveRegistry.GetInstance<T>();
        if (instance == null)
        {
            Debug.LogWarning($"No saveable instance registered for type {typeof(T).Name}");
            return default(T);
        }

        var attribute = typeof(T).GetCustomAttribute<SaveDataAttribute>();

        if (attribute == null)
        {
            Debug.LogWarning($"Type {typeof(T).Name} does not have SaveData attribute");
            return default(T);
        }

        var loadedData = LoadFromFile<T>(attribute.FileName, attribute.FolderPath);
        if (loadedData != null)
        {
            instance.SetData(loadedData);
            return loadedData;
        }

        return default(T);
    }
    public void SaveAllData()
    {
        _isSaving = true;
        foreach (var (type, instance) in SaveRegistry.GetAllInstances())
        {
            var attribute = type.GetCustomAttribute<SaveDataAttribute>();
            if (attribute == null)
                continue;

            try
            {
                var method = typeof(SaveManager).GetMethod(nameof(SaveData));
                var genericMethod = method.MakeGenericMethod(type);
                genericMethod.Invoke(this, null);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save data for type {type.Name}: {e.Message}");
            }
        }
        _isSaving = false;
    }

    public void LoadAllData()
    {
        foreach (var (type, instance) in SaveRegistry.GetAllInstances())
        {
            var attribute = type.GetCustomAttribute<SaveDataAttribute>();
            if (attribute == null)
                continue;

            try
            {
                var method = typeof(SaveManager).GetMethod(nameof(LoadData));
                var genericMethod = method.MakeGenericMethod(type);
                genericMethod.Invoke(this, null);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load data for type {type.Name}: {e.Message}");
            }
        }
    }


    //private void SaveToFile<T>(T data, string fileName, string folderPath)
    //{
    //    string baseFolder = Path.Combine(Application.dataPath, "Resources/Saves", folderPath);
    //    string fullPath = baseFolder;

    //    if (!Directory.Exists(fullPath))
    //    {
    //        Directory.CreateDirectory(fullPath);
    //    }

    //    string filePath = Path.Combine(fullPath, fileName);
    //    try
    //    {
    //        var settings = new Newtonsoft.Json.JsonSerializerSettings
    //        {
    //            TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto
    //        };
    //        string json = Newtonsoft.Json.JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented, settings);
    //        File.WriteAllText(filePath, json);
    //        Debug.Log($"Successfully saved {typeof(T).Name} to {filePath}");
    //    }
    //    catch (Exception e)
    //    {
    //        Debug.LogError($"Failed to save {typeof(T).Name}: {e.Message}");
    //    }
    //}


    /// <summary>
    /// Serializes the given data object to JSON and safely writes it to a file in the specified folder.
    /// The method first writes to a temporary file, then replaces the original file to prevent data corruption.
    /// If a previous save exists, it creates a backup before overwriting. In case of a failure during saving,
    /// it attempts to restore the previous backup to avoid leaving a broken or incomplete save file.
    /// All exceptions are logged for debugging purposes.
    /// </summary>
    /// <typeparam name="T">The type of the data object to save.</typeparam>
    /// <param name="data">The data object to serialize and save.</param>
    /// <param name="fileName">The name of the save file.</param>
    /// <param name="folderPath">The relative folder path where the save file will be stored.</param>
    private void SaveToFile<T>(T data, string fileName, string folderPath)
    {
        // string baseFolder = Path.Combine(Application.dataPath, "Resources/Saves", folderPath);
        // if (!Directory.Exists(baseFolder))
        // {
        //     Directory.CreateDirectory(baseFolder);
        // }

        string baseFolder = Path.Combine(Application.persistentDataPath, folderPath);
        if (!Directory.Exists(baseFolder))
        {
            Directory.CreateDirectory(baseFolder);
        }

        string filePath = Path.Combine(baseFolder, fileName);
        string tempFilePath = filePath + ".tmp";
        string backupFilePath = filePath + ".bak";

        try
        {
            var settings = new Newtonsoft.Json.JsonSerializerSettings
            {
                TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto
            };

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented, settings);

            // Write to temp file first
            File.WriteAllText(tempFilePath, json);

            // Backup old file if it exists
            if (File.Exists(filePath))
            {
                File.Copy(filePath, backupFilePath, overwrite: true);
            }

            // Replace original with temp
            File.Copy(tempFilePath, filePath, overwrite: true);
            File.Delete(tempFilePath);

            Debug.Log($"Successfully saved {typeof(T).Name} to {filePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save {typeof(T).Name}: {e.Message}");

            // Attempt to restore from backup if save failed
            if (File.Exists(backupFilePath))
            {
                try
                {
                    File.Copy(backupFilePath, filePath, overwrite: true);
                    Debug.LogWarning($"Restored {typeof(T).Name} from backup after failed save.");
                }
                catch (Exception restoreEx)
                {
                    Debug.LogError($"Failed to restore backup for {typeof(T).Name}: {restoreEx.Message}");
                }
            }
        }
    }

    private T LoadFromFile<T>(string fileName, string folderPath)
    {
        // string baseFolder = Path.Combine(Application.dataPath, "Resources/Saves", folderPath);
        string baseFolder = Path.Combine(Application.persistentDataPath, folderPath);
        string filePath = Path.Combine(baseFolder, fileName);
        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"Save file not found: {filePath}");
            return default(T);
        }

        try
        {
            string json = File.ReadAllText(filePath);
            var settings = new Newtonsoft.Json.JsonSerializerSettings
            {
                TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto
            };
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json, settings);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load {typeof(T).Name}: {e.Message}");
            return default(T);
        }
    }
}