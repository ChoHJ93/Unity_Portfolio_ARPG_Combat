using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using CHJ;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Unit))]
public class UnitSkill : MonoBehaviour
{
    public readonly EInputKey[] CombatInputKeys = new EInputKey[] { EInputKey.Attack, EInputKey.Skill_01, EInputKey.Skill_02, EInputKey.Dash };

    [SerializeField]
    private UnitSkillTable _unitSkillTable;

    [SerializeField]
    [HideInInspector]
    private List<int> _defaultSkillIdList = new List<int>();

    private Unit _unit;
    private Dictionary<int, SkillData> _skillDataDic;
    private Dictionary<EInputKey, SkillData> _currentSkillDic;
    private Dictionary<int, Coroutine> _coolingSkillDic;
    private SkillData _currentSkill = null;
    private bool _nextSkillFlag = false;
    private UnitAnimation UnitAnimation => _unit.UnitAnimation;

    public event UnityAction OnSkillStart;
    public event UnityAction OnSkillEnd;

    private void Awake()
    {
        _unit = GetComponent<Unit>();

        _skillDataDic = new Dictionary<int, SkillData>();

        if (_unitSkillTable != null)
        {
            foreach (var skillData in _unitSkillTable.SkillDatas)
            {
                _skillDataDic.Add(skillData.Id, skillData.Clone());
            }
        }

        _currentSkillDic = new Dictionary<EInputKey, SkillData>();
        if (_defaultSkillIdList.IsNullOrEmpty() == false)
        {
            for (int i = 0; i < CombatInputKeys.Length; i++)
            {
                int skillId = _defaultSkillIdList[i];
                if (skillId == 0)
                    continue;
                SkillData skillData = _skillDataDic[skillId];
                _currentSkillDic.Add(CombatInputKeys[i], skillData);
            }
        }

        _coolingSkillDic = new Dictionary<int, Coroutine>();
    }

    private void OnEnable()
    {
        EventManager.Instance.AddListener<EventCommonInput>(OnSkillInputCalled);
    }

    private void OnDisable()
    {
        EventManager.Instance.RemoveListener<EventCommonInput>(OnSkillInputCalled);
    }

    private void OnSkillInputCalled(EventCommonInput eventCommonInput)
    {
        SkillData skillData = GetSkillData(eventCommonInput.inputKey);
        if (skillData != null && IsSkillUsable(skillData.Id))
        {
            UseSkill(skillData);
            PlaySkillAni(skillData.aniStateName);
            OnSkillStart?.Invoke();
        }
    }

    private bool IsSkillUsable(int skillId)
    {
        if (_skillDataDic.TryGetValue(skillId, out SkillData skillData) == false)
            return false;

        if (_coolingSkillDic.ContainsKey(skillId))
            return false;
        if (_currentSkill != null && _currentSkill.Id > 0)
        {
            if (skillId == _currentSkill.Id)
                return false;
            if (_nextSkillFlag && _currentSkill.nextSkillId == skillId)
                return true;
            return false;
        }

        return true;
    }
    private void UseSkill(SkillData skillData)
    {
        _currentSkill = skillData;
        if (skillData.coolTime > 0f)
            _coolingSkillDic.Add(skillData.Id, StartCoroutine(SkillCoolTimeRoutine(skillData)));
    }
    private void PlaySkillAni(string skillAni)
    {
        if (string.IsNullOrEmpty(skillAni) || _unit == null || UnitAnimation == null)
            return;

        UnitAnimation.PlayAni(_currentSkill.aniStateName, true, 0.1f);
    }
    private SkillData GetSkillData(EInputKey eInputKey)
    {
        if (_currentSkillDic.TryGetValue(eInputKey, out SkillData skillData) == false)
            return null;

        if (_nextSkillFlag)
        {
            return GetNextSkill(skillData);
        }

        return skillData;
    }
    private SkillData GetNextSkill(SkillData skillData)
    {
        if (skillData.nextSkillId == 0)
            return null;

        if (_skillDataDic.TryGetValue(skillData.nextSkillId, out SkillData nextSkillData) == false)
            return null;

        if (skillData.Equals(_currentSkill))
            return nextSkillData;

        return GetNextSkill(nextSkillData);
    }

    IEnumerator SkillCoolTimeRoutine(SkillData skillData)
    {
        yield return new WaitForSeconds(skillData.coolTime);
    }

    public void EndCurrentSkill(int stateNameHash)
    {
        if (_currentSkill == null)
            return;
        if (_currentSkill.skillNameHash == stateNameHash)
        {
            Debug.Log($"SkillEnd : {_currentSkill.aniStateName}");
            _currentSkill = null;
            _nextSkillFlag = false;
            OnSkillEnd?.Invoke();
            //Debug.Log($"Data State Name Hash : {_currentSkill.skillNameHash} / Ani State Name Hash : {stateNameHash}");
        }
    }
    public void SetNextSkillFlag()
    {
        _nextSkillFlag = _currentSkill != null && _currentSkill.nextSkillId != 0;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(UnitSkill))]
public class UnitSkillEditor : Editor
{
    private static bool _defaultSkillFoldout = false;

    private class SkillSetting
    {
        public int selectedSkillIndex = 0;
        public SkillData skillData;
    }

    private UnitSkill _unitSkill;
    private SerializedProperty _skillTableProp;
    private SerializedProperty _defaultSkillIdListProp;

    private List<SkillSetting> _defaultSkillList;
    private string[] _skillNameArray;

    private UnitSkillTable UnitSkillTable => _skillTableProp.objectReferenceValue as UnitSkillTable;

    private void OnEnable()
    {
        _unitSkill = target as UnitSkill;
        _skillTableProp = serializedObject.FindProperty("_unitSkillTable");
        _defaultSkillIdListProp = serializedObject.FindProperty("_defaultSkillIdList");

        _defaultSkillList = GetDefaultSkillList();

        _skillNameArray = new string[UnitSkillTable.SkillDatas.Count + 1];
        _skillNameArray[0] = "None";
        for (int i = 0; i < UnitSkillTable.SkillDatas.Count; i++)
        {
            string skillId = UnitSkillTable.SkillDatas[i].Id.ToString();
            string aniStateName = UnitSkillTable.SkillDatas[i].aniStateName;
            _skillNameArray[i + 1] = $"{skillId} - {aniStateName}";
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();

        _defaultSkillIdListProp.arraySize = _unitSkill.CombatInputKeys.Length;

        if (_skillTableProp.objectReferenceValue == null)
        {
            EditorGUILayout.HelpBox("Please assign UnitSkillTable", MessageType.Warning);
            return;
        }

        //show foldout as defult list gui
        _defaultSkillFoldout = EditorGUILayout.Foldout(_defaultSkillFoldout, "Default Skill List", true);

        if (_defaultSkillFoldout == true)
        {
            EditorGUI.indentLevel++;
            for (int i = 0; i < _unitSkill.CombatInputKeys.Length; i++)
            {
                if (_defaultSkillList.Count <= i)
                {
                    _defaultSkillList.Add(new SkillSetting());
                }

                SkillSetting skillSetting = _defaultSkillList[i];
                SkillData skillData = skillSetting.skillData;

                skillSetting.selectedSkillIndex = EditorGUILayout.Popup(_unitSkill.CombatInputKeys[i].ToString(), skillSetting.selectedSkillIndex, _skillNameArray);
                skillData = skillSetting.selectedSkillIndex == 0 ? null : UnitSkillTable.SkillDatas[skillSetting.selectedSkillIndex - 1];

                _defaultSkillIdListProp.GetArrayElementAtIndex(i).intValue = skillData?.Id ?? 0;
            }
            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }

    private List<SkillSetting> GetDefaultSkillList()
    {
        List<SkillSetting> defaultSkillList = new List<SkillSetting>(_unitSkill.CombatInputKeys.Length);

        for (int i = 0; i < _defaultSkillIdListProp.arraySize; i++)
        {
            int skillId = _defaultSkillIdListProp.GetArrayElementAtIndex(i).intValue;
            SkillData skillData = UnitSkillTable.SkillDatas.FirstOrDefault(x => x.Id == skillId);

            defaultSkillList.Add(new SkillSetting()
            {
                selectedSkillIndex = skillData == null ? 0 : UnitSkillTable.SkillDatas.IndexOf(skillData) + 1,
                skillData = skillData
            });
        }

        return defaultSkillList;
    }
}
#endif