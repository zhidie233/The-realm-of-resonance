using UnityEngine;
using UnityEditor;
using MFPSEditor;
using Photon.Pun;

[CustomEditor(typeof(bl_PlayerNetwork))]
public class bl_PlayerSyncEditor : Editor
{

    bl_PlayerNetwork m_Target;
    private SerializedProperty m_SynchronizePositionProperty;
    private SerializedProperty m_SynchronizeRotationProperty;
    private SerializedProperty m_SynchronizeScaleProperty;

    private bool m_InterpolateHelpOpen;
    private bool m_ExtrapolateHelpOpen;
    private bool m_InterpolateRotationHelpOpen;
    private bool m_InterpolateScaleHelpOpen;

    private const int EDITOR_LINE_HEIGHT = 20;

    private const string INTERPOLATE_TOOLTIP =
        "可选择直接同步数值 即禁用插值，或将其平滑过渡到最新数据。";

    private const string INTERPOLATE_HELP =
        "可以使用插值把 GameObject 平滑移动到网络收到的新位置。"
        + "这有助于减轻因网络每秒仅更新 10 次而产生的移动卡顿。\n"
        + "副作用是 GameObject 总会略微滞后于真实位置，这可以通过外推来改善。";

    private const string EXTRAPOLATE_TOOLTIP = "外推用于预测 GameObject 的真实位置";

    private const string EXTRAPOLATE_HELP =
        "处理网络数值时，收到的所有数据都略微滞后，因为这些数据需要时间 "
        + "才能到达你这里。可以基于已收到的移动数据，用外推来预测玩家的真实位置。\n"
        +
        "为保证预测效果最优，需针对每个具体游戏仔细调校。有时状态很容易外推，因为 "
        +
        "GameObject 的行为高度可预测，例如载具。有时则很难，因为用户输入会直接作用于游戏，"
        + "你无法真正预测用户接下来的操作，例如格斗游戏。";

    private const string INTERPOLATE_HELP_URL = "http://doc.exitgames.com/en/pun/current/tutorials/rpg-movement";
    private const string EXTRAPOLATE_HELP_URL = "http://doc.exitgames.com/en/pun/current/tutorials/rpg-movement";
    private SerializedProperty gunListProp;

    public void OnEnable()
    {
        SetupSerializedProperties();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        m_Target = (bl_PlayerNetwork)this.target;
        bool isProjectPrefab = EditorUtility.IsPersistent(m_Target.gameObject);
        bool allowSceneObjects = !EditorUtility.IsPersistent(m_Target);

        GUI.enabled = false;
        GUILayout.BeginVertical("box");
        m_Target.FPState = (PlayerFPState)EditorGUILayout.EnumPopup("第一人称状态", m_Target.FPState, EditorStyles.toolbarDropDown);
        GUILayout.EndVertical();
        GUI.enabled = true;
        if (m_Target.NetworkGuns == null)
        {
            m_Target.NetworkGuns = new System.Collections.Generic.List<bl_NetworkGun>();
        }

        if (m_Target.NetworkGuns.Count == 0)
        {
            m_Target.NetworkGuns.Add(null);
        }

        DrawIsPlayingWarning();
        GUI.enabled = !Application.isPlaying;

        DrawSynchronizePositionHeader();
        DrawSynchronizePositionData();

        GUI.enabled = !Application.isPlaying;
        DrawSynchronizeRotationHeader();
        DrawSynchronizeRotationData();

        GUI.enabled = !Application.isPlaying;
        DrawSynchronizeScaleHeader();
        DrawSynchronizeScaleData();

        serializedObject.ApplyModifiedProperties();

        GUI.enabled = true;
        DrawNetworkGunsList();
        EditorGUILayout.BeginVertical("box");
        m_Target.HeadTarget = EditorGUILayout.ObjectField("头部目标", m_Target.HeadTarget, typeof(Transform), isProjectPrefab) as Transform;

        GUILayout.Space(10);
        if(GUILayout.Button("定位当前第一人称武器", EditorStyles.toolbarButton))
        {
            bl_GunManager gm = m_Target.transform.GetComponentInChildren<bl_GunManager>(true);
            if (Application.isPlaying)
            {
                Selection.activeObject = gm.CurrentGun;
                EditorGUIUtility.PingObject(gm.CurrentGun);
            }
            else
            {
                Selection.activeObject = gm;
                EditorGUIUtility.PingObject(gm);
            }
        }
        if (GUILayout.Button("定位当前第三人称武器", EditorStyles.toolbarButton))
        {

            if (Application.isPlaying)
            {
                Selection.activeObject = m_Target.CurrenGun;
                EditorGUIUtility.PingObject(m_Target.CurrenGun);
            }
            else
            {
                var ng = m_Target.transform.GetComponentInChildren<bl_NetworkGun>(true);
                if (ng != null)
                {
                    Selection.activeObject = ng.transform.parent;
                    EditorGUIUtility.PingObject(ng.transform.parent);
                }
            }

        }
        EditorGUILayout.EndVertical();
    }

    int GetGunsCount()
    {
        int count = 0;

        for (int i = 0; i < m_Target.NetworkGuns.Count; ++i)
        {
            if (m_Target.NetworkGuns[i] != null)
            {
                count++;
            }
        }

        return count;
    }

    void DrawNetworkGunsList()
    {
        GUILayout.Space(5);
        SerializedProperty listProperty = serializedObject.FindProperty(Dependency.GunListPropiertie);

        if (listProperty == null)
        {
            return;
        }

        float containerElementHeight = 22;
        float containerHeight = listProperty.arraySize * containerElementHeight;

        bool isOpen = MFPSEditorStyles.ContainerHeaderFoldout("网络武器 (" + GetGunsCount() + ")", gunListProp.isExpanded);
        gunListProp.isExpanded = isOpen;

        if (isOpen == false)
        {
            containerHeight = 0;
        }

        Rect containerRect = PhotonGUI.ContainerBody(containerHeight);
        if (isOpen == true)
        {
            for (int i = 0; i < listProperty.arraySize; ++i)
            {
                Rect elementRect = new Rect(containerRect.xMin, containerRect.yMin + containerElementHeight * i, containerRect.width, containerElementHeight);
                {
                    Rect texturePosition = new Rect(elementRect.xMin + 6, elementRect.yMin + elementRect.height / 2f - 1, 9, 5);
                    //MFPSEditorUtils.DrawTexture(texturePosition, MFPSEditorUtils.texGrabHandle);

                    Rect propertyPosition = new Rect(elementRect.xMin + 20, elementRect.yMin + 3, elementRect.width - 45, 16);
                    EditorGUI.PropertyField(propertyPosition, listProperty.GetArrayElementAtIndex(i), new GUIContent());

                    Rect removeButtonRect = new Rect(elementRect.xMax - PhotonGUI.DefaultRemoveButtonStyle.fixedWidth,
                                                        elementRect.yMin + 2,
                                                        PhotonGUI.DefaultRemoveButtonStyle.fixedWidth,
                                                        PhotonGUI.DefaultRemoveButtonStyle.fixedHeight);

                    GUI.enabled = listProperty.arraySize > 1;
                    if (GUI.Button(removeButtonRect, new GUIContent(MFPSEditorUtils.texRemoveButton), PhotonGUI.DefaultRemoveButtonStyle))
                    {
                        listProperty.DeleteArrayElementAtIndex(i);
                    }
                    GUI.enabled = true;

                    if (i < listProperty.arraySize - 1)
                    {
                        texturePosition = new Rect(elementRect.xMin + 2, elementRect.yMax, elementRect.width - 4, 1);
                        PhotonGUI.DrawSplitter(texturePosition);
                    }
                }
            }
        }

        if (PhotonGUI.AddButton())
        {
            listProperty.InsertArrayElementAtIndex(Mathf.Max(0, listProperty.arraySize - 1));
        }

        serializedObject.ApplyModifiedProperties();
    }
    private void DrawIsPlayingWarning()
    {
        if (Application.isPlaying == false)
        {
            return;
        }

        GUILayout.BeginVertical(GUI.skin.box);
        {
            GUILayout.Label("播放模式下禁用编辑，避免两个对象失去同步");
        }
        GUILayout.EndVertical();
    }

    private void SetupSerializedProperties()
    {
        this.m_SynchronizePositionProperty = serializedObject.FindProperty(Dependency.SynchronizePositionProperty);
        this.m_SynchronizeRotationProperty = serializedObject.FindProperty(Dependency.SynchronizeRotationProperty);
        this.m_SynchronizeScaleProperty = serializedObject.FindProperty(Dependency.SynchronizeScaleProperty);
        this.gunListProp = serializedObject.FindProperty("NetworkGuns");
    }

    private void DrawSynchronizePositionHeader()
    {
        DrawHeader("同步位置", this.m_SynchronizePositionProperty);
    }

    private void DrawSynchronizePositionData()
    {
        if (this.m_SynchronizePositionProperty == null || this.m_SynchronizePositionProperty.boolValue == false)
        {
            return;
        }

        SerializedProperty interpolatePositionProperty = serializedObject.FindProperty("m_PositionModel.InterpolateOption");
        PhotonTransformViewPositionModel.InterpolateOptions interpolateOption = (PhotonTransformViewPositionModel.InterpolateOptions)interpolatePositionProperty.enumValueIndex;

        SerializedProperty extrapolatePositionProperty = serializedObject.FindProperty("m_PositionModel.ExtrapolateOption");
        PhotonTransformViewPositionModel.ExtrapolateOptions extrapolateOption = (PhotonTransformViewPositionModel.ExtrapolateOptions)extrapolatePositionProperty.enumValueIndex;

        float containerHeight = 155;

        switch (interpolateOption)
        {
            case PhotonTransformViewPositionModel.InterpolateOptions.FixedSpeed:
            case PhotonTransformViewPositionModel.InterpolateOptions.Lerp:
                containerHeight += EDITOR_LINE_HEIGHT;
                break;
        }

        if (extrapolateOption != PhotonTransformViewPositionModel.ExtrapolateOptions.Disabled)
        {
            containerHeight += EDITOR_LINE_HEIGHT;
        }

        switch (extrapolateOption)
        {
            case PhotonTransformViewPositionModel.ExtrapolateOptions.FixedSpeed:
                containerHeight += EDITOR_LINE_HEIGHT;
                break;
        }

        if (this.m_InterpolateHelpOpen == true)
        {
            containerHeight += GetInterpolateHelpBoxHeight();
        }

        if (this.m_ExtrapolateHelpOpen == true)
        {
            containerHeight += GetExtrapolateHelpBoxHeight();
        }

        // removed Gizmo Options. -3 lines, -1 splitter
        containerHeight -= EDITOR_LINE_HEIGHT * 2;

        Rect rect = PhotonGUI.ContainerBody(containerHeight);

        Rect propertyRect = new Rect(rect.xMin + 5, rect.yMin + 2, rect.width - 10, EditorGUIUtility.singleLineHeight);

        DrawTeleport(ref propertyRect);
        DrawSplitter(ref propertyRect);

        DrawSynchronizePositionDataInterpolation(ref propertyRect, interpolatePositionProperty, interpolateOption);
        DrawSplitter(ref propertyRect);

        DrawSynchronizePositionDataExtrapolation(ref propertyRect, extrapolatePositionProperty, extrapolateOption);
        DrawSplitter(ref propertyRect);

        DrawSynchronizePositionDataGizmos(ref propertyRect);
    }

    private float GetInterpolateHelpBoxHeight()
    {
        return PhotonGUI.RichLabel.CalcHeight(new GUIContent(INTERPOLATE_HELP), Screen.width - 54) + 35;
    }

    private float GetExtrapolateHelpBoxHeight()
    {
        return PhotonGUI.RichLabel.CalcHeight(new GUIContent(EXTRAPOLATE_HELP), Screen.width - 54) + 35;
    }

    private void DrawSplitter(ref Rect propertyRect)
    {
        Rect splitterRect = new Rect(propertyRect.xMin - 3, propertyRect.yMin, propertyRect.width + 6, 1);
        PhotonGUI.DrawSplitter(splitterRect);

        propertyRect.y += 5;
    }

    private void DrawSynchronizePositionDataGizmos(ref Rect propertyRect)
    {
        GUI.enabled = true;

        /* EditorGUI.PropertyField(propertyRect, serializedObject.FindProperty("m_PositionModel.DrawErrorGizmo"),
             new GUIContent("绘制同步位置误差"));
         propertyRect.y += EDITOR_LINE_HEIGHT;*/
    }

    private void DrawHelpBox(ref Rect propertyRect, bool isOpen, float height, string helpText, string url)
    {
        if (isOpen == true)
        {
            Rect helpRect = new Rect(propertyRect.xMin, propertyRect.yMin, propertyRect.width, height - 5);
            GUI.BeginGroup(helpRect, GUI.skin.box);
            GUI.Label(new Rect(5, 5, propertyRect.width - 10, height - 30), helpText, PhotonGUI.RichLabel);
            if (GUI.Button(new Rect(5, height - 30, propertyRect.width - 10, 20), "详见我们的文档"))
            {
                Application.OpenURL(url);
            }
            GUI.EndGroup();

            propertyRect.y += height;
        }
    }

    private void DrawPropertyWithHelpIcon(ref Rect propertyRect, ref bool isHelpOpen, SerializedProperty property, string tooltip)
    {
        Rect propertyFieldRect = new Rect(propertyRect.xMin, propertyRect.yMin, propertyRect.width - 20, propertyRect.height);
        string propertyName = ObjectNames.NicifyVariableName(property.name);
        EditorGUI.PropertyField(propertyFieldRect, property, new GUIContent(propertyName, tooltip));

        Rect helpIconRect = new Rect(propertyFieldRect.xMax + 5, propertyFieldRect.yMin, 20, propertyFieldRect.height);
        isHelpOpen = GUI.Toggle(helpIconRect, isHelpOpen, PhotonGUI.HelpIcon, GUIStyle.none);

        propertyRect.y += EDITOR_LINE_HEIGHT;
    }

    private void DrawSynchronizePositionDataExtrapolation(ref Rect propertyRect, SerializedProperty extrapolatePositionProperty, PhotonTransformViewPositionModel.ExtrapolateOptions extrapolateOption)
    {
        DrawPropertyWithHelpIcon(ref propertyRect, ref this.m_ExtrapolateHelpOpen, extrapolatePositionProperty, EXTRAPOLATE_TOOLTIP);
        DrawHelpBox(ref propertyRect, this.m_ExtrapolateHelpOpen, GetExtrapolateHelpBoxHeight(), EXTRAPOLATE_HELP, EXTRAPOLATE_HELP_URL);

        if (extrapolateOption != PhotonTransformViewPositionModel.ExtrapolateOptions.Disabled)
        {
            EditorGUI.PropertyField(propertyRect, serializedObject.FindProperty("m_PositionModel.ExtrapolateIncludingRoundTripTime"));
            propertyRect.y += EDITOR_LINE_HEIGHT;
        }

        switch (extrapolateOption)
        {
            case PhotonTransformViewPositionModel.ExtrapolateOptions.FixedSpeed:
                EditorGUI.PropertyField(propertyRect, serializedObject.FindProperty("m_PositionModel.ExtrapolateSpeed"));
                propertyRect.y += EDITOR_LINE_HEIGHT;
                break;
        }
    }

    private void DrawTeleport(ref Rect propertyRect)
    {
        EditorGUI.PropertyField(propertyRect, serializedObject.FindProperty("m_PositionModel.TeleportEnabled"),
            new GUIContent("允许长距离传送"));
        propertyRect.y += EDITOR_LINE_HEIGHT;

        EditorGUI.PropertyField(propertyRect, serializedObject.FindProperty("m_PositionModel.TeleportIfDistanceGreaterThan"),
            new GUIContent("距离超过该值时传送"));
        propertyRect.y += EDITOR_LINE_HEIGHT;
    }

    private void DrawSynchronizePositionDataInterpolation(ref Rect propertyRect, SerializedProperty interpolatePositionProperty,
        PhotonTransformViewPositionModel.InterpolateOptions interpolateOption)
    {
        DrawPropertyWithHelpIcon(ref propertyRect, ref this.m_InterpolateHelpOpen, interpolatePositionProperty, INTERPOLATE_TOOLTIP);
        DrawHelpBox(ref propertyRect, this.m_InterpolateHelpOpen, GetInterpolateHelpBoxHeight(), INTERPOLATE_HELP, INTERPOLATE_HELP_URL);

        switch (interpolateOption)
        {
            case PhotonTransformViewPositionModel.InterpolateOptions.FixedSpeed:
                EditorGUI.PropertyField(propertyRect, serializedObject.FindProperty("m_PositionModel.InterpolateMoveTowardsSpeed"),
                    new GUIContent("MoveTowards 速度"));
                propertyRect.y += EDITOR_LINE_HEIGHT;
                break;

            case PhotonTransformViewPositionModel.InterpolateOptions.Lerp:
                EditorGUI.PropertyField(propertyRect, serializedObject.FindProperty("m_PositionModel.InterpolateLerpSpeed"), new GUIContent("Lerp 速度"));
                propertyRect.y += EDITOR_LINE_HEIGHT;
                break;

        }
    }

    private void DrawSynchronizeRotationHeader()
    {
        DrawHeader("同步旋转", this.m_SynchronizeRotationProperty);
    }

    private void DrawSynchronizeRotationData()
    {
        if (this.m_SynchronizeRotationProperty == null || this.m_SynchronizeRotationProperty.boolValue == false)
        {
            return;
        }

        SerializedProperty interpolateRotationProperty = serializedObject.FindProperty("m_RotationModel.InterpolateOption");
        PhotonTransformViewRotationModel.InterpolateOptions interpolateOption =
            (PhotonTransformViewRotationModel.InterpolateOptions)interpolateRotationProperty.enumValueIndex;

        float containerHeight = 20;

        switch (interpolateOption)
        {
            case PhotonTransformViewRotationModel.InterpolateOptions.RotateTowards:
            case PhotonTransformViewRotationModel.InterpolateOptions.Lerp:
                containerHeight += EDITOR_LINE_HEIGHT;
                break;
        }

        if (this.m_InterpolateRotationHelpOpen == true)
        {
            containerHeight += GetInterpolateHelpBoxHeight();
        }

        Rect rect = PhotonGUI.ContainerBody(containerHeight);
        Rect propertyRect = new Rect(rect.xMin + 5, rect.yMin + 2, rect.width - 10, EditorGUIUtility.singleLineHeight);

        DrawPropertyWithHelpIcon(ref propertyRect, ref this.m_InterpolateRotationHelpOpen, interpolateRotationProperty, INTERPOLATE_TOOLTIP);
        DrawHelpBox(ref propertyRect, this.m_InterpolateRotationHelpOpen, GetInterpolateHelpBoxHeight(), INTERPOLATE_HELP, INTERPOLATE_HELP_URL);

        switch (interpolateOption)
        {
            case PhotonTransformViewRotationModel.InterpolateOptions.RotateTowards:
                EditorGUI.PropertyField(propertyRect, serializedObject.FindProperty("m_RotationModel.InterpolateRotateTowardsSpeed"),
                    new GUIContent("RotateTowards 速度"));
                break;
            case PhotonTransformViewRotationModel.InterpolateOptions.Lerp:
                EditorGUI.PropertyField(propertyRect, serializedObject.FindProperty("m_RotationModel.InterpolateLerpSpeed"), new GUIContent("Lerp 速度"));
                break;
        }
    }

    private void DrawSynchronizeScaleHeader()
    {
        DrawHeader("同步缩放", this.m_SynchronizeScaleProperty);
    }

    private void DrawSynchronizeScaleData()
    {
        if (this.m_SynchronizeScaleProperty == null || this.m_SynchronizeScaleProperty.boolValue == false)
        {
            return;
        }

        SerializedProperty interpolateScaleProperty = serializedObject.FindProperty("m_ScaleModel.InterpolateOption");
        PhotonTransformViewScaleModel.InterpolateOptions interpolateOption = (PhotonTransformViewScaleModel.InterpolateOptions)interpolateScaleProperty.enumValueIndex;

        float containerHeight = EDITOR_LINE_HEIGHT;

        switch (interpolateOption)
        {
            case PhotonTransformViewScaleModel.InterpolateOptions.MoveTowards:
            case PhotonTransformViewScaleModel.InterpolateOptions.Lerp:
                containerHeight += EDITOR_LINE_HEIGHT;
                break;
        }

        if (this.m_InterpolateScaleHelpOpen == true)
        {
            containerHeight += GetInterpolateHelpBoxHeight();
        }

        Rect rect = PhotonGUI.ContainerBody(containerHeight);
        Rect propertyRect = new Rect(rect.xMin + 5, rect.yMin + 2, rect.width - 10, EditorGUIUtility.singleLineHeight);

        DrawPropertyWithHelpIcon(ref propertyRect, ref this.m_InterpolateScaleHelpOpen, interpolateScaleProperty, INTERPOLATE_TOOLTIP);
        DrawHelpBox(ref propertyRect, this.m_InterpolateScaleHelpOpen, GetInterpolateHelpBoxHeight(), INTERPOLATE_HELP, INTERPOLATE_HELP_URL);

        switch (interpolateOption)
        {
            case PhotonTransformViewScaleModel.InterpolateOptions.MoveTowards:
                EditorGUI.PropertyField(propertyRect, serializedObject.FindProperty("m_ScaleModel.InterpolateMoveTowardsSpeed"),
                    new GUIContent("MoveTowards 速度"));
                break;
            case PhotonTransformViewScaleModel.InterpolateOptions.Lerp:
                EditorGUI.PropertyField(propertyRect, serializedObject.FindProperty("m_ScaleModel.InterpolateLerpSpeed"), new GUIContent("Lerp 速度"));
                break;
        }
    }

    private void DrawHeader(string label, SerializedProperty property)
    {
        if (property == null)
        {
            return;
        }

        bool newValue = PhotonGUI.ContainerHeaderToggle(label, property.boolValue);

        if (newValue != property.boolValue)
        {
            Undo.RecordObject(this.m_Target, "Change " + label);
            property.boolValue = newValue;
            EditorUtility.SetDirty(this.m_Target);
        }
    }
}
