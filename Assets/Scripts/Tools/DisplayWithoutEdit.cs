// Author: Zuwolf, August 18, 2016, https://forum.unity.com/threads/how-do-you-disable-inspector-editing-of-a-public-variable.142100/

using UnityEngine;

/// <summary>
/// Allow to display an attribute in inspector without allow editing
/// </summary>
public class DisplayWithoutEdit : PropertyAttribute
{
    public DisplayWithoutEdit() { }
}
