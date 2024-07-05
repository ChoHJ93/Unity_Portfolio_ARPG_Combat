using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AnimatorController = UnityEditor.Animations.AnimatorController;


#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

[CreateAssetMenu(fileName = "UnitSkillTable", menuName = "DataTable/UnitSkillTable")]
public class UnitSkillTable : ScriptableObject 
{
    [SerializeField]
    public AnimatorController animator;

    [SerializeField]
    public List<SkillData> skillDatas;

    [SerializeField]
    public Dictionary<EInputKey, SkillData> skillDataDic;

    public void Initialize()
    {
        skillDataDic = new Dictionary<EInputKey, SkillData>();

        foreach (var skillData in skillDatas)
        {
            skillDataDic.Add(skillData.inputKey, skillData);
        }
    }

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

    private void OnEnable()
    {
        animatorProp = serializedObject.FindProperty("animator");
        //skillDatasProp = serializedObject.FindProperty("skillDatas");

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
            EditorGUI.LabelField(inputKeyRect, "  Input Key");
            EditorGUI.LabelField(aniStateNameRect, "Ani State");
            EditorGUI.LabelField(coolTimeRect, "Cool Time");
            EditorGUI.LabelField(damageRect, "Damage");
            EditorGUILayout.EndHorizontal();
        };

        skillDataList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            var element = skillDataList.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2;

            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element);
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
}
#endif
