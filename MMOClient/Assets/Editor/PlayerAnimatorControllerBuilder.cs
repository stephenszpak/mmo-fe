using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class PlayerAnimatorControllerBuilder
{
    private const string AnimDir = "Assets/Models/Player/Animations";
    private const string ControllerPath = AnimDir + "/PlayerAnimatorController.controller";

    [MenuItem("Tools/Build Player Animator Controller")]
    public static void Build()
    {
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null)
        {
            controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        }

        EnsureParameter(controller, "speed", AnimatorControllerParameterType.Float);
        EnsureParameter(controller, "isJumping", AnimatorControllerParameterType.Bool);
        EnsureParameter(controller, "isGrounded", AnimatorControllerParameterType.Bool);

        AnimatorStateMachine sm = controller.layers[0].stateMachine;
        sm.states = new ChildAnimatorState[0];
        sm.anyStateTransitions = new AnimatorStateTransition[0];
        sm.defaultState = null;

        var idle = AddState(sm, "Stand--Idle.anim.fbx", "Idle");
        var walk = AddState(sm, "Locomotion--Walk_N.anim.fbx", "Walk_N");
        var run = AddState(sm, "Locomotion--Run_N.anim.fbx", "Run_N");
        var jumpStart = AddState(sm, "Jump--Jump.anim.fbx", "JumpStart");
        var jumpLand = AddState(sm, "Jump--Jump.anim.fbx", "JumpLand");

        sm.defaultState = idle;

        // Idle -> Walk_N
        var t1 = idle.AddTransition(walk);
        t1.AddCondition(AnimatorConditionMode.Greater, 0.1f, "speed");
        t1.hasExitTime = false;

        // Walk_N -> Idle
        var t2 = walk.AddTransition(idle);
        t2.AddCondition(AnimatorConditionMode.Less, 0.1f, "speed");
        t2.hasExitTime = false;

        // Walk_N -> Run_N
        var t3 = walk.AddTransition(run);
        t3.AddCondition(AnimatorConditionMode.Greater, 2f, "speed");
        t3.hasExitTime = false;

        // Run_N -> Walk_N
        var t4 = run.AddTransition(walk);
        t4.AddCondition(AnimatorConditionMode.Less, 2f, "speed");
        t4.hasExitTime = false;

        // Any State -> JumpStart
        var tAny = sm.AddAnyStateTransition(jumpStart);
        tAny.AddCondition(AnimatorConditionMode.If, 0f, "isJumping");
        tAny.hasExitTime = false;

        // JumpStart -> JumpLand
        var t5 = jumpStart.AddTransition(jumpLand);
        t5.hasExitTime = true;
        t5.hasFixedDuration = true;

        // JumpLand -> Idle
        var t6 = jumpLand.AddTransition(idle);
        t6.AddCondition(AnimatorConditionMode.If, 0f, "isGrounded");
        t6.AddCondition(AnimatorConditionMode.Less, 0.1f, "speed");
        t6.hasExitTime = false;

        AssetDatabase.SaveAssets();
    }

    private static AnimatorState AddState(AnimatorStateMachine sm, string fileName, string clipName)
    {
        string path = AnimDir + "/" + fileName;
        AnimationClip clip = FindClip(path, clipName);
        AnimatorState state = sm.AddState(clipName);
        state.motion = clip;
        return state;
    }

    private static AnimationClip FindClip(string assetPath, string clipName)
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
        foreach (var a in assets)
        {
            if (a is AnimationClip clip && clip.name == clipName)
                return clip;
        }
        Debug.LogWarning($"Clip {clipName} not found at {assetPath}");
        return null;
    }

    private static void EnsureParameter(AnimatorController controller, string name, AnimatorControllerParameterType type)
    {
        foreach (var p in controller.parameters)
        {
            if (p.name == name)
                return;
        }
        controller.AddParameter(name, type);
    }
}
