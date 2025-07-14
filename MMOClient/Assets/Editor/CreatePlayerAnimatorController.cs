using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class CreatePlayerAnimatorController
{
    private const string ControllerPath = "Assets/Models/Player/PlayerAnimatorController.controller";
    private const string SearchFolder = "Assets/Models/Player";

    [MenuItem("Tools/Create Player Animator Controller")]
    private static void CreateController()
    {
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);

        controller.AddParameter("speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("isJumping", AnimatorControllerParameterType.Bool);
        controller.AddParameter("isFalling", AnimatorControllerParameterType.Bool);
        controller.AddParameter("isGrounded", AnimatorControllerParameterType.Bool);

        AnimationClip idle = FindClip(new[] { "Idle" });
        AnimationClip walk = FindClip(new[] { "Walk" });
        AnimationClip run = FindClip(new[] { "Run" }, new[] { "Land" });
        AnimationClip jumpStart = FindClip(new[] { "JumpStart" });
        AnimationClip jumpInAir = FindClip(new[] { "InAir" });
        AnimationClip jumpLand = FindClip(new[] { "JumpLand", "Land" });

        AnimatorStateMachine sm = controller.layers[0].stateMachine;

        AnimatorState idleState = sm.AddState("Idle");
        idleState.motion = idle;
        AnimatorState walkState = sm.AddState("Walk");
        walkState.motion = walk;
        AnimatorState runState = sm.AddState("Run");
        runState.motion = run;
        AnimatorState jumpStartState = sm.AddState("JumpStart");
        jumpStartState.motion = jumpStart;
        AnimatorState inAirState = sm.AddState("InAir");
        inAirState.motion = jumpInAir;
        AnimatorState landState = sm.AddState("JumpLand");
        landState.motion = jumpLand;

        sm.defaultState = idleState;

        var idleToWalk = idleState.AddTransition(walkState);
        idleToWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "speed");

        var walkToRun = walkState.AddTransition(runState);
        walkToRun.AddCondition(AnimatorConditionMode.Greater, 1.5f, "speed");

        var anyToJump = sm.AddAnyStateTransition(jumpStartState);
        anyToJump.AddCondition(AnimatorConditionMode.If, 0, "isJumping");

        var jumpToAir = jumpStartState.AddTransition(inAirState);
        jumpToAir.hasExitTime = true;

        var airToLand = inAirState.AddTransition(landState);
        airToLand.AddCondition(AnimatorConditionMode.If, 0, "isGrounded");

        var landToIdle = landState.AddTransition(idleState);
        landToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "speed");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        AssignToSelected(controller);
    }

    private static AnimationClip FindClip(string[] keywords, string[] notKeywords = null)
    {
        string[] guids = AssetDatabase.FindAssets("t:AnimationClip", new[] { SearchFolder });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            string name = clip.name;
            bool match = keywords.Any(k => name.Contains(k));
            if (!match) continue;
            if (notKeywords != null && notKeywords.Any(k => name.Contains(k))) continue;
            return clip;
        }
        return null;
    }

    private static void AssignToSelected(RuntimeAnimatorController controller)
    {
        GameObject go = Selection.activeGameObject;
        if (go == null) return;
        Animator animator = go.GetComponent<Animator>();
        if (animator == null)
            animator = go.AddComponent<Animator>();

        animator.runtimeAnimatorController = controller;
    }
}
