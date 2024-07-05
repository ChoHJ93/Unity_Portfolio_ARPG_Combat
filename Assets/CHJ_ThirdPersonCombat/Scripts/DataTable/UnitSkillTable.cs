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
    [SerializeField]
    public AnimatorController animator;

    [SerializeField]
    [HideInInspector]
    public List<SkillData> skillDatas;

    [SerializeField]
    public Dictionary<EInputKey, SkillData> skillDataDic;


    public SkillData GetSkillData(EInputKey inputKey)
    {
        if (skillDataDic.ContainsKey(inputKey))
        {
            return skillDataDic[inputKey];
        }

        return null;
    }

}

#if UNITY_EDITOR
[CustomEditor(typeof(UnitSkillTable))]
public class UnitSkillTableEditor : Editor
{
    SerializedProperty animatorProp;
    //SerializedProperty skillDatasProp;
    ReorderableList skillDataList;

    string[] _aniStateArray;

    private void OnEnable()
    {
        animatorProp = serializedObject.FindProperty("animator");
        //skillDatasProp = serializedObject.FindProperty("skillDatas");

        _aniStateArray = GetAllAniStateName((AnimatorController)animatorProp.objectReferenceValue);


        skillDataList = new ReorderableList(serializedObject, serializedObject.FindProperty("skillDatas"), true, true, true, true);

        skillDataList.drawHeaderCallback = (Rect rect) =>
        {
            rect.x += 14;   
            float padding = 2;
            float width = rect.width / 4 - padding;

            Rect inputKeyRect = new Rect(rect.x, rect.y, width, rect.height);
            Rect aniStateNameRect = new Rect(inputKeyRect.x + width + padding, rect.y, width, rect.height);
            Rect coolTimeRect = new Rect(aniStateNameRect.x + width + padding, rect.y, width, rect.height);
            Rect damageRect = new Rect(coolTimeRect.x + width + padding, rect.y, width, rect.height);

            EditorGUILayout.BeginHorizontal();
            EditorGUI.LabelField(inputKeyRect, "  Skill ID");
            EditorGUI.LabelField(aniStateNameRect, "Ani State");
            EditorGUI.LabelField(coolTimeRect, "Cool Time");
            EditorGUI.LabelField(damageRect, "Damage");
            EditorGUILayout.EndHorizontal();
        };

        skillDataList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            var element = skillDataList.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2;

            DrawSkillDataProperty(rect, element);
            //EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element);
        };

        skillDataList.elementHeightCallback = (int index) =>
        {
            var element = skillDataList.serializedProperty.GetArrayElementAtIndex(index);
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

        skillDataList.DoLayoutList();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawSkillDataProperty(Rect rect, SerializedProperty skillDataProp) 
    {
        var skillIdProp = skillDataProp.FindPropertyRelative("Id");
        var aniStateNameProp = skillDataProp.FindPropertyRelative("aniStateName");
        var coolTimeProp = skillDataProp.FindPropertyRelative("coolTime");
        var damageProp = skillDataProp.FindPropertyRelative("damage");

        var selectedStateIndex = skillDataProp.FindPropertyRelative("selectedStateIndex");

        float padding = 2;
        float width = rect.width / 4 - padding;

        Rect skillIdRect = new Rect(rect.x, rect.y, width, rect.height);
        Rect aniStateNameRect = new Rect(skillIdRect.x + width + padding, rect.y, width, rect.height);
        Rect coolTimeRect = new Rect(aniStateNameRect.x + width + padding, rect.y, width, rect.height);
        Rect damageRect = new Rect(coolTimeRect.x + width + padding, rect.y, width, rect.height);

        EditorGUI.PropertyField(skillIdRect, skillIdProp, GUIContent.none);
        selectedStateIndex.intValue = EditorGUI.Popup(aniStateNameRect, selectedStateIndex.intValue, _aniStateArray);
        aniStateNameProp.stringValue = _aniStateArray[selectedStateIndex.intValue];
        EditorGUI.PropertyField(coolTimeRect, coolTimeProp, GUIContent.none);
        EditorGUI.PropertyField(damageRect, damageProp, GUIContent.none);
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
