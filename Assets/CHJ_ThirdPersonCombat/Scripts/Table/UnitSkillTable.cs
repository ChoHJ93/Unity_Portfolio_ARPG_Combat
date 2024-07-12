using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.Animations;
using AnimatorController = UnityEditor.Animations.AnimatorController;
#endif

[CreateAssetMenu(fileName = "UnitSkillTable", menuName = "DataTable/UnitSkillTable")]
public class UnitSkillTable : ScriptableObject
{
    public readonly EInputKey[] CombatInputKeys = new EInputKey[] { EInputKey.Attack, EInputKey.Skill_01, EInputKey.Skill_02, EInputKey.Dash };

    [SerializeField]
    private AnimatorController animator;

    [SerializeField]
    [HideInInspector]
    private List<SkillData> skillDatas;

    public List<SkillData> SkillDatas => skillDatas;
}

#if UNITY_EDITOR
[CustomEditor(typeof(UnitSkillTable))]
public class UnitSkillTableEditor : Editor
{
    private class SkillSetting
    {
        public EInputKey inputKey;
        public SkillData skillData;
    }

    UnitSkillTable _skillTable;
    SerializedProperty _animatorProp;
    ReorderableList _skillDataList;
    string[] _aniStateArray;

    private List<SkillSetting> _initialSkillSetting;


    private void OnEnable()
    {
        _skillTable = target as UnitSkillTable;
        _animatorProp = serializedObject.FindProperty("animator");
        _aniStateArray = GetAllAniStateName((AnimatorController)_animatorProp.objectReferenceValue);
        _skillDataList = new ReorderableList(serializedObject, serializedObject.FindProperty("skillDatas"), true, true, true, true);

        _skillDataList.drawHeaderCallback = (Rect rect) =>
        {
            rect.x += 14;
            float padding = 2;
            float width = rect.width / 5 - padding;

            Rect inputKeyRect = new Rect(rect.x, rect.y, width, rect.height);
            Rect aniStateNameRect = new Rect(inputKeyRect.x + width, rect.y, width, rect.height);
            Rect coolTimeRect = new Rect(aniStateNameRect.x + width, rect.y, width, rect.height);
            Rect damageRect = new Rect(coolTimeRect.x + width, rect.y, width, rect.height);
            Rect nextSkillIdRect = new Rect(damageRect.x + width, rect.y, width, rect.height);

            EditorGUILayout.BeginHorizontal();
            EditorGUI.LabelField(inputKeyRect, "  Skill ID");
            EditorGUI.LabelField(aniStateNameRect, "Ani State");
            EditorGUI.LabelField(coolTimeRect, "Cool Time");
            EditorGUI.LabelField(damageRect, "Damage");
            EditorGUI.LabelField(nextSkillIdRect, "Next Skill ID");
            EditorGUILayout.EndHorizontal();
        };

        _skillDataList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            var element = _skillDataList.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2;

            DrawSkillDataProperty(rect, element);
            //EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element);
        };

        _skillDataList.elementHeightCallback = (int index) =>
        {
            var element = _skillDataList.serializedProperty.GetArrayElementAtIndex(index);
            float height = EditorGUI.GetPropertyHeight(element);
            float padding = 2;

            return height + padding;
        };

    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        UnitSkillTable unitSkillTable = target as UnitSkillTable;

        serializedObject.Update();

        _skillDataList.DoLayoutList();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawSkillDataProperty(Rect rect, SerializedProperty skillDataProp)
    {
        var skillIdProp = skillDataProp.FindPropertyRelative("Id");
        var aniStateNameProp = skillDataProp.FindPropertyRelative("aniStateName");
        var coolTimeProp = skillDataProp.FindPropertyRelative("coolTime");
        var damageProp = skillDataProp.FindPropertyRelative("damage");
        var nextSkillIdProp = skillDataProp.FindPropertyRelative("nextSkillId");
        var skillNameHashProp = skillDataProp.FindPropertyRelative("skillNameHash");

        var selectedStateIndex = skillDataProp.FindPropertyRelative("selectedStateIndex");
        var selectedNextSkillIndex = skillDataProp.FindPropertyRelative("selectedNextSkillIndex");

        float padding = 2;
        float width = rect.width / 5 - padding;

        Rect skillIdRect = new Rect(rect.x, rect.y, width, rect.height);
        Rect aniStateNameRect = new Rect(skillIdRect.x + width + padding, rect.y, width, rect.height);
        Rect coolTimeRect = new Rect(aniStateNameRect.x + width + padding, rect.y, width, rect.height);
        Rect damageRect = new Rect(coolTimeRect.x + width + padding, rect.y, width, rect.height);
        Rect nextSkillIdRect = new Rect(damageRect.x + width + padding, rect.y, width, rect.height);

        EditorGUI.PropertyField(skillIdRect, skillIdProp, GUIContent.none);
        EditorGUI.BeginChangeCheck();
        selectedStateIndex.intValue = EditorGUI.Popup(aniStateNameRect, selectedStateIndex.intValue, _aniStateArray);
        aniStateNameProp.stringValue = _aniStateArray[selectedStateIndex.intValue];
        if (EditorGUI.EndChangeCheck())
        {
            skillNameHashProp.intValue = Animator.StringToHash(aniStateNameProp.stringValue);
        }

        EditorGUI.PropertyField(coolTimeRect, coolTimeProp, GUIContent.none);
        EditorGUI.PropertyField(damageRect, damageProp, GUIContent.none);
        EditorGUI.PropertyField(nextSkillIdRect, nextSkillIdProp, GUIContent.none);
    }

    private string[] GetAllAniStateName(AnimatorController controller)
    {
        List<string> aniStateList = new List<string>();
        aniStateList.Add("None");

        foreach (var layer in controller.layers)
        {
            GetStateNameFromMachine(layer.stateMachine, ref aniStateList);
        }

        return aniStateList.ToArray();
    }

    private void GetStateNameFromMachine(AnimatorStateMachine stateMachine, ref List<string> stateNameList)
    {
        foreach (var state in stateMachine.states)
        {
            stateNameList.Add(state.state.name);
        }

        foreach (var subMachine in stateMachine.stateMachines)
        {
            GetStateNameFromMachine(subMachine.stateMachine, ref stateNameList);
        }
    }
}
#endif
