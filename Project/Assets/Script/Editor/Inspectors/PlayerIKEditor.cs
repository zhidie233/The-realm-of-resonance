using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayerIK))]
public class PlayerIKEditor : Editor
{
    PlayerIK script;
    private Animator animator;
    public Transform headTransform;
    private Vector3 aimPos;

    private void OnEnable()
    {
        script = (PlayerIK)target;
        animator = script.GetComponent<Animator>();
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        base.OnInspectorGUI();

        PreviewButtons();

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }
    }

    void PreviewButtons()
    {
        if (animator == null) return;

        GUILayout.Space(6);
        if(script.editor_previewMode == 1)
        {
            script.editor_weight = EditorGUILayout.Slider("预览 IK 权重", script.editor_weight, 0, 1);
        }
        if (GUILayout.Button("预览瞄准位置"))
        {
            AnimationRunner window = (AnimationRunner)EditorWindow.GetWindow(typeof(AnimationRunner));
            window.Show();
            window.SetAnim(animator, () =>
            {

                script.editor_weight = 0;
                if (animator != null)
                {
                    animator.Update(0);
                    animator.Update(0);
                }
                script.editor_previewMode = 0;
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            });
            Selection.activeObject = script.gameObject;
            var nGun = script.transform.GetComponentInChildren<NetworkFirearm>();
            if(nGun != null)
            {
                script.GetComponentInParent<PlayerReferencesHandler>().EditorSelectedGun = nGun;
            }
            else
            {
                Debug.LogWarning("该玩家没有启用中的第三人称武器，武器未启用时无法预览手臂 IK！");
            }
            var anim = script.GetComponent<Animator>();
            if (anim != null)
            {
                script.CustomArmsIKHandler = null;
                headTransform = anim.GetBoneTransform(HumanBodyBones.Head);
                aimPos = headTransform.TransformPoint(script.AimSightPosition);
                script.Init();
                script.editor_previewMode = 1;
                script.editor_weight = 1;
            }
            else { Debug.Log("未挂载动画器，无法预览"); }
        }
    }

    private void OnSceneGUI()
    {
        if(script.editor_previewMode == 1 && headTransform != null)
        {
            aimPos = Handles.DoPositionHandle(aimPos, Quaternion.identity);
            script.AimSightPosition = headTransform.InverseTransformPoint(aimPos);
        }
    }
}