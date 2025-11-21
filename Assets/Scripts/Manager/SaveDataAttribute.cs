using System;


[AttributeUsage(AttributeTargets.Class)]
public class SaveDataAttribute : Attribute
{
    public string FileName
    {
        get;
    }
    public string FolderPath
    {
        get;
    }

    public SaveDataAttribute(string fileName, string folderPath = "")
    {
        FileName = fileName;
        FolderPath = folderPath;
    }
}
