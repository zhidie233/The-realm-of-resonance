using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace GameEditor
{
    /// <summary>
    /// 编辑器里预览 Animator 的动作：挑一个状态，逐帧拖或连续播放。
    /// </summary>
    public class AnimationRunner : EditorWindow
    {
        class StateEntry
        {
            public string name;
            public int layer;
            public int hash;
        }

        const float MinSpeed = 0.05f;
        const float MaxSpeed = 3f;
        const float FramesPerSecond = 30f;

        readonly List<StateEntry> states = new List<StateEntry>();

        Animator animator;
        Action onFinish;
        Vector2 scroll;
        int stateIndex = -1;
        bool playing;
        bool loop = true;
        float speed = 1f;
        float position;
        double lastTick;

        private void OnEnable()
        {
            titleContent = new GUIContent("动画预览");
            EditorApplication.update += OnEditorUpdate;
        }

        private void OnDestroy()
        {
            EditorApplication.update -= OnEditorUpdate;
            playing = false;

            if (EditorApplication.isCompiling || EditorApplication.isUpdating) { return; }

            Action callback = onFinish;
            onFinish = null;
            if (callback != null) { callback(); }
        }

        /// <summary>
        /// 指定要预览的 Animator，可选一个窗口关闭时执行的回调。
        /// </summary>
        public void SetAnim(Animator target, Action callback = null, bool autoPlay = false)
        {
            animator = target;
            onFinish = callback;
            playing = false;
            position = 0f;

            CollectStates();
            if (states.Count > 0) { SelectState(0, autoPlay); }

            lastTick = EditorApplication.timeSinceStartup;
            Repaint();
        }

        void CollectStates()
        {
            states.Clear();

            AnimatorController controller = ResolveController();
            if (controller == null) { return; }

            AnimatorControllerLayer[] layers = controller.layers;
            for (int i = 0; i < layers.Length; i++)
            {
                AddStates(layers[i].stateMachine, i, "");
            }
        }

        AnimatorController ResolveController()
        {
            if (animator == null) { return null; }

            RuntimeAnimatorController current = animator.runtimeAnimatorController;
            AnimatorOverrideController overriden = current as AnimatorOverrideController;
            while (overriden != null)
            {
                current = overriden.runtimeAnimatorController;
                overriden = current as AnimatorOverrideController;
            }

            return current as AnimatorController;
        }

        void AddStates(AnimatorStateMachine machine, int layer, string prefix)
        {
            if (machine == null) { return; }

            ChildAnimatorState[] children = machine.states;
            for (int i = 0; i < children.Length; i++)
            {
                AnimatorState state = children[i].state;
                if (state == null) { continue; }

                states.Add(new StateEntry()
                {
                    name = prefix + state.name,
                    layer = layer,
                    hash = state.nameHash,
                });
            }

            ChildAnimatorStateMachine[] subMachines = machine.stateMachines;
            for (int i = 0; i < subMachines.Length; i++)
            {
                AnimatorStateMachine sub = subMachines[i].stateMachine;
                if (sub == null) { continue; }

                AddStates(sub, layer, prefix + sub.name + "/");
            }
        }

        void SelectState(int index, bool play)
        {
            if (states.Count == 0)
            {
                stateIndex = -1;
                playing = false;
                return;
            }

            stateIndex = Mathf.Clamp(index, 0, states.Count - 1);
            position = 0f;
            playing = play;
            lastTick = EditorApplication.timeSinceStartup;

            Sample();
        }

        bool CanSample()
        {
            return animator != null && animator.isActiveAndEnabled && stateIndex >= 0 && stateIndex < states.Count;
        }

        float CurrentLength()
        {
            if (!CanSample()) { return 1f; }

            float length = animator.GetCurrentAnimatorStateInfo(states[stateIndex].layer).length;
            return length > 0.01f ? length : 0.01f;
        }

        void Sample()
        {
            if (!CanSample()) { return; }

            StateEntry entry = states[stateIndex];
            animator.Play(entry.hash, entry.layer, Mathf.Clamp01(position));
            animator.Update(0f);

            SceneView.RepaintAll();
            Repaint();
        }

        void OnEditorUpdate()
        {
            if (!playing || !CanSample()) { return; }

            double now = EditorApplication.timeSinceStartup;
            float delta = (float)(now - lastTick) * speed;
            lastTick = now;

            if (delta <= 0f) { return; }

            position += delta / CurrentLength();
            if (position >= 1f)
            {
                if (loop) { position -= Mathf.Floor(position); }
                else { position = 1f; playing = false; }
            }

            Sample();
        }

        private void OnGUI()
        {
            if (animator == null)
            {
                EditorGUILayout.HelpBox("未指定 Animator。", MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField("目标", animator.gameObject.name + " / " + animator.name);

            if (!animator.isActiveAndEnabled)
            {
                EditorGUILayout.HelpBox("Animator 未启用，预览不会生效。", MessageType.Warning);
            }

            if (states.Count == 0)
            {
                EditorGUILayout.HelpBox("控制器里没有可预览的状态。", MessageType.Warning);
                if (GUILayout.Button("刷新状态列表")) { CollectStates(); }
                return;
            }

            DrawToolbar();
            DrawProgress();

            EditorGUILayout.Space();
            DrawStateList();
        }

        void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(playing ? "暂停" : "播放", GUILayout.Width(60f)))
            {
                playing = !playing;
                lastTick = EditorApplication.timeSinceStartup;
            }

            if (GUILayout.Button("上一帧", GUILayout.Width(60f))) { Step(-1f); }
            if (GUILayout.Button("下一帧", GUILayout.Width(60f))) { Step(1f); }

            if (GUILayout.Button("回到开头", GUILayout.Width(80f)))
            {
                playing = false;
                position = 0f;
                Sample();
            }

            loop = GUILayout.Toggle(loop, "循环", GUILayout.Width(50f));

            GUILayout.Space(8f);
            speed = EditorGUILayout.Slider("速度", speed, MinSpeed, MaxSpeed);

            EditorGUILayout.EndHorizontal();
        }

        void DrawProgress()
        {
            EditorGUI.BeginChangeCheck();

            position = EditorGUILayout.Slider("进度", position, 0f, 1f);

            if (EditorGUI.EndChangeCheck())
            {
                playing = false;
                Sample();
            }
        }

        void DrawStateList()
        {
            EditorGUILayout.LabelField("状态", EditorStyles.boldLabel);

            scroll = EditorGUILayout.BeginScrollView(scroll);
            for (int i = 0; i < states.Count; i++)
            {
                bool selected = i == stateIndex;
                string label = selected ? "▶ " + states[i].name : "    " + states[i].name;

                if (GUILayout.Button(label, selected ? EditorStyles.boldLabel : EditorStyles.label))
                {
                    SelectState(i, playing);
                }
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();
            if (GUILayout.Button("刷新状态列表")) { CollectStates(); }
        }

        void Step(float frames)
        {
            playing = false;
            position = Mathf.Repeat(position + (frames / FramesPerSecond) / CurrentLength(), 1f);
            Sample();
        }
    }
}
