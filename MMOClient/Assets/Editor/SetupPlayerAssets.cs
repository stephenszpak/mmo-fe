using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class SetupPlayerAssets
{
    [MenuItem("Tools/Setup Player Assets")]
    private static void Setup()
    {
        string modelDir = "Assets/Models/Player";
        string animDir = modelDir + "/Animations";
        string matDir = modelDir + "/Materials";
        string controllerDir = "Assets/Animations/Player";
        string prefabDir = "Assets/Prefabs";

        // Create folders if they don't exist
        if (!AssetDatabase.IsValidFolder("Assets/Models"))
            AssetDatabase.CreateFolder("Assets", "Models");
        if (!AssetDatabase.IsValidFolder(modelDir))
            AssetDatabase.CreateFolder("Assets/Models", "Player");
        if (!AssetDatabase.IsValidFolder(animDir))
            AssetDatabase.CreateFolder(modelDir, "Animations");
        if (!AssetDatabase.IsValidFolder(matDir))
            AssetDatabase.CreateFolder(modelDir, "Materials");
        if (!AssetDatabase.IsValidFolder("Assets/Animations"))
            AssetDatabase.CreateFolder("Assets", "Animations");
        if (!AssetDatabase.IsValidFolder(controllerDir))
            AssetDatabase.CreateFolder("Assets/Animations", "Player");
        if (!AssetDatabase.IsValidFolder(prefabDir))
            AssetDatabase.CreateFolder("Assets", "Prefabs");

        // Load clips if they exist
        AnimationClip idle = AssetDatabase.LoadAssetAtPath<AnimationClip>(animDir + "/Idle.anim");
        AnimationClip walk = AssetDatabase.LoadAssetAtPath<AnimationClip>(animDir + "/Walk.anim");
        AnimationClip run = AssetDatabase.LoadAssetAtPath<AnimationClip>(animDir + "/Run.anim");
        AnimationClip jump = AssetDatabase.LoadAssetAtPath<AnimationClip>(animDir + "/Jump.anim");

        // Create controller
        string controllerPath = controllerDir + "/PlayerAnimatorController.controller";
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        controller.AddParameter("speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("isJumping", AnimatorControllerParameterType.Bool);

        AnimatorStateMachine sm = controller.layers[0].stateMachine;

        // Blend tree for locomotion
        BlendTree blend;
        AnimatorState locomotion = sm.AddState("Locomotion");
        locomotion.motion = blend = new BlendTree { name = "LocomotionTree" };
        blend.blendType = BlendTreeType.Simple1D;
        blend.blendParameter = "speed";
        blend.useAutomaticThresholds = false;
        blend.AddChild(idle, 0f);
        blend.AddChild(walk, 0.1f);
        blend.AddChild(run, 1.5f);

        sm.defaultState = locomotion;

        AnimatorState jumpState = sm.AddState("Jump");
        jumpState.motion = jump;

        // Transitions
        sm.AddAnyStateTransition(jumpState).AddCondition(AnimatorConditionMode.If, 0, "isJumping");
        var exitJump = jumpState.AddTransition(locomotion);
        exitJump.hasExitTime = true;

        // Create prefab
        GameObject go = new GameObject("Player");
        go.tag = "Player";
        var animator = go.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;
        var rb = go.AddComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        go.AddComponent<CapsuleCollider>();

        GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(modelDir + "/PlayerModel.fbx");
        if (model != null)
        {
            GameObject modelInstance = (GameObject)PrefabUtility.InstantiatePrefab(model);
            modelInstance.transform.SetParent(go.transform);
            modelInstance.transform.localPosition = Vector3.zero;
            modelInstance.transform.localRotation = Quaternion.identity;
        }

        PrefabUtility.SaveAsPrefabAsset(go, prefabDir + "/Player.prefab");
        GameObject.DestroyImmediate(go);
        AssetDatabase.Refresh();
    }
}
