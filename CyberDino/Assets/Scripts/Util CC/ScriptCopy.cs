using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Reflection;

/// <summary>
/// Contains an extension method that lets script Components (anything that dervives from MonoBehaviour) to be copies to another GameObject.
/// Is not guaranteed to work on particularly complex scripts. <para></para>
/// Use: "gameObject.AddCopyOfScript(scriptToCopy);" and "scriptComponent.CopyDataFrom(otherScriptComponent);"
/// </summary>
public static class ScriptCopy {

    /// <summary>
    /// Creates a copy of a script component that is found on another game object. Unity does not support this (at runtime) by default and it may not work right with very complex components.<para></para>
    /// Does nothing and returns null if the script passed in was null.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="gameObject"></param>
    /// <param name="component"></param>
    /// <returns></returns>
    public static T AddCopyOfComponent<T>(this GameObject gameObject, T component) where T : Component {
        if (component != null) {
            T copy = gameObject.AddComponent(component.GetType()) as T;

            copy.CopyDataFrom(component);

            return copy;
        } else {
            return null;
        }
    }

    /// <summary>
    /// Copies all the data from another component of the same type into this component.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="component"></param>
    /// <param name="other"></param>
    public static void CopyDataFrom<T>(this T component, T other) where T : Component {
        // Iterate twice, first filling out all the fields then the properties because filling out the properites may have secondary effects
        // that depend on all the fields being filled out.

        // Get all the fields and properties that have been declared only in that subtype.
        BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        Type componentType = component.GetType();

        if (componentType != other.GetType()) {
            Debug.LogError("Type mismatch between the two components in CopyDataFrom, this operation is not supported between two components with a different type!"
                + "\n Thus, nothing has happened. Double check your scripts.");
            return;
        }

        List<Type> heiarchy = new List<Type>();
        Type t = componentType;
        while (t != typeof(MonoBehaviour) && t != typeof(Component)) {
            if (t == typeof(NetworkBehaviour)) {
                return;
            }

            heiarchy.Add(t);
            t = t.BaseType;
        }
        heiarchy.Reverse();


        //Work our way down the heiarchy, first setting fields.
        foreach (Type type in heiarchy) {
            FieldInfo[] fieldInfos = type.GetFields(bindingFlags);
            foreach (FieldInfo fieldInfo in fieldInfos) {
                fieldInfo.SetValue(component, fieldInfo.GetValue(other));
            }
        }

        // Iterate through the properties of each type.
        foreach (Type type in heiarchy) {
            PropertyInfo[] propInfos = type.GetProperties(bindingFlags);
            foreach (PropertyInfo propInfo in propInfos) {
                // Don't copy any attributes that are marked as Obsolete or else errors will happen.
                Attribute obsolete = Attribute.GetCustomAttribute(propInfo, typeof(ObsoleteAttribute));
                if (propInfo.CanWrite && obsolete == null) {
                    try {
                        propInfo.SetValue(component, propInfo.GetValue(other, null), null);
                    } catch (Exception e) {
                        Debug.LogWarning("Exception happened when copying Type [" + type.Name + "] Property [" + propInfo.Name + "]"
                            + "\n[" + e.GetType().Name + "]: " + e.Message
                            + "\n That property has not been copied correctly and may cause strange behaviour!");
                    }
                }
            }
        }
    }
}
