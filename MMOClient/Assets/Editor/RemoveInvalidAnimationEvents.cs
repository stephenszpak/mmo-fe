using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class RemoveInvalidAnimationEvents
{
    [MenuItem("Tools/Remove Invalid JumpLand Events")]
    private static void CleanJumpLandEvents()
    {
        string assetPath = "Assets/StarterAssets/ThirdPersonController/Character/Animations/Jump--Jump.anim.fbx";
        AnimationClip clip = FindClip(assetPath, "JumpLand");
        if (clip == null)
        {
            Debug.LogWarning("JumpLand clip not found");
            return;
        }

        List<AnimationEvent> validEvents = new List<AnimationEvent>();
        bool changed = false;
        foreach (var evt in AnimationUtility.GetAnimationEvents(clip))
        {
            if (string.IsNullOrEmpty(evt.functionName))
                continue;

            if (!MethodExists(evt.functionName) || evt.functionName == "OnLand")
            {
                changed = true;
                continue;
            }
            validEvents.Add(evt);
        }

        if (changed)
        {
            AnimationUtility.SetAnimationEvents(clip, validEvents.ToArray());
            EditorUtility.SetDirty(clip);
            AssetDatabase.SaveAssets();
            Debug.Log("Removed invalid events from JumpLand");
        }
        else
        {
            Debug.Log("No invalid events found on JumpLand");
        }
    }

    private static AnimationClip FindClip(string assetPath, string clipName)
    {
        foreach (var a in AssetDatabase.LoadAllAssetsAtPath(assetPath))
        {
            if (a is AnimationClip clip && clip.name == clipName)
                return clip;
        }
        return null;
    }

    private static bool MethodExists(string name)
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var type in asm.GetTypes())
            {
                if (typeof(MonoBehaviour).IsAssignableFrom(type) &&
                    type.GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) != null)
                    return true;
            }
        }
        return false;
    }
}
