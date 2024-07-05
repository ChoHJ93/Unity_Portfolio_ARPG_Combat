using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class SkillData : PropertyAttribute
{
    public EInputKey inputKey;
    public string aniStateName;
    public float coolTime;
    public float damage;

}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(SkillData))]
public class SkillDataDrawer : PropertyDrawer
{
    string[] aniStateNameArray;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var inputKeyProp = property.FindPropertyRelative("inputKey");
        var aniStateNameProp = property.FindPropertyRelative("aniStateName");
        var coolTimeProp = property.FindPropertyRelative("coolTime");
        var damageProp = property.FindPropertyRelative("damage");

        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), GUIContent.none);

        float padding = 2;
        float width = position.width / 4 - padding;

        Rect inputKeyRect = new Rect(position.x, position.y, width, position.height);
        Rect aniStateNameRect = new Rect(inputKeyRect.x + width + padding, position.y, width, position.height);
        Rect coolTimeRect = new Rect(aniStateNameRect.x + width + padding, position.y, width, position.height);
        Rect damageRect = new Rect(coolTimeRect.x + width + padding, position.y, width, position.height);

        EditorGUI.PropertyField(inputKeyRect, inputKeyProp, GUIContent.none);
        EditorGUI.PropertyField(aniStateNameRect, aniStateNameProp, GUIContent.none);
        EditorGUI.PropertyField(coolTimeRect, coolTimeProp, GUIContent.none);
        EditorGUI.PropertyField(damageRect, damageProp, GUIContent.none);

        EditorGUI.EndProperty();
    }
}
#endif