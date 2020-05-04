/// Author: Kilosoft, February 7, 2018, https://unitylist.com/p/po1/Editor-Button

using System;

/// <summary>
/// Attribute from method
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class EditorButtonAttribute : Attribute
{
    /// <summary>
    /// Button text
    /// </summary>
    public string name;

    /// <summary>
    /// Add Button to Inspector
    /// </summary>
    /// <param name="name">Button text</param>
    public EditorButtonAttribute(string name)
    {
        this.name = name;
    }
}
