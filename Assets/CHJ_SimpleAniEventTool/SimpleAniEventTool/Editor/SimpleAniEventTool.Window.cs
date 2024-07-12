using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CHJ.SimpleAniEventTool
{
    using System;
    using System.Reflection;

    using UnityEditor;
    using UnityEditorInternal;
    using UnityEditor.Compilation;
    using UnityEngine.SceneManagement;

    using CHJ;
    using Object = UnityEngine.Object;

    public class SimpleAniEventToolWindow : EditorWindow
    {
        private enum EventParmType
        {
            None,
            Float,
            Int,
            Object,
            String,

            ChildObject, //Custom Type
        }
        private class AniClipEvent
        {
            public int functionIndex;
            public string functionName;
            public float time;

            EventParmType paramType;
            public float floatParameter;
            public int intParameter;
            public Object objectReferenceParameter;
            public string stringParameter;

            public AniClipEvent()
            {
                functionIndex = 0;
                functionName = string.Empty;
                time = 0;
                floatParameter = 0;
                intParameter = 0;
                objectReferenceParameter = null;
                stringParameter = string.Empty;
            }
        }
        private class ChildObject
        {
            public bool initialActive { get; set; }
            public int depth { get; set; }
            public GameObject parent { get; set; }
            public GameObject obj { get; set; }

            public List<ParticleSimulateInfo> particleSimulateInfos { get; private set; }

            public void SetActive(bool active, double currentTime)
            {
                if (obj != null && obj.activeSelf != active)
                {
                    obj.SetActive(active);

                    if (active && particleSimulateInfos.IsNullOrEmpty() == false)
                        foreach (var psi in particleSimulateInfos)
                        {
                            psi.startTime = currentTime;
                        }
                }

            }

            public void SetChildrenParticleSimulateInfos(List<ParticleSimulateInfo> simulateInfos)
            {
                particleSimulateInfos = new List<ParticleSimulateInfo>();
                if (obj == null || simulateInfos.IsNullOrEmpty())
                    return;
                foreach (var childParticle in obj.GetComponentsInChildren<ParticleSystem>(true))
                {
                    ParticleSimulateInfo psi = simulateInfos.Find(x => x.particleSystem == childParticle);
                    if (psi != null)
                        particleSimulateInfos.Add(psi);
                }
            }
        }

        private class ParticleSimulateInfo
        {
            public ParticleSystem particleSystem { get; private set; }
            public bool loop { get; private set; }
            public float simulateTime { get; set; }
            public List<ParticleSimulateInfo> subEmitters { get; private set; }

            public ParticleSimulateInfo(ParticleSystem ps)
            {
                startTime = 0;
                particleSystem = ps;
                loop = ps.main.loop;
                simulateTime = 0;
                subEmitters = new List<ParticleSimulateInfo>();

                // Get sub-emitters
                for (int i = 0; i < ps.subEmitters.subEmittersCount; i++)
                {
                    ParticleSystem subEmitter = ps.subEmitters.GetSubEmitterSystem(i);
                    if (subEmitter != null)
                    {
                        subEmitters.Add(new ParticleSimulateInfo(subEmitter));
                    }
                }
            }

            public bool Simulate(float time, bool withChildren = true)
            {
                bool isSimulated = false;
                if (time != simulateTime)
                {
                    particleSystem.Simulate(time, withChildren, true);
                    SimulateSubEmitters(time);
                    simulateTime = time;
                    isSimulated = true;
                }
                return isSimulated;
            }
            private void SimulateSubEmitters(float time)
            {
                foreach (ParticleSimulateInfo psi in subEmitters)
                {
                    psi.particleSystem.Simulate(-time, false, false, false);
                }
            }
            public GameObject rootObj { get; private set; }
            public double startTime { get; set; }

            public void SetRootObject(GameObject prefabInstance)
            {
                if (particleSystem == null)
                    return;

                rootObj = GetRootTransform(particleSystem.transform, prefabInstance).gameObject;
            }
            private Transform GetRootTransform(Transform currentTransform, GameObject prefabInstance)
            {
                if (currentTransform == null)
                    return null;

                Transform parent = currentTransform.parent;
                if (parent == prefabInstance)
                    return currentTransform;

                return GetRootTransform(parent, prefabInstance);
            }
        }

        #region Static
        private const string WindowTitle = "Simple Animation Event Editor";

        private static SimpleAniEventToolWindow m_Window;
        public static SimpleAniEventToolWindow Instance => m_Window;
        public static bool IsOpen => m_Window != null;

        public bool IsFocused => this == EditorWindow.focusedWindow;

        [MenuItem("Tools/애니 클립 이벤트 에디터", false)]
        public static void OpenWindow()
        {
            if (m_Window == null)
            {
                m_Window = GetWindow<SimpleAniEventToolWindow>(false, WindowTitle, true);
                m_Window.minSize = new Vector2(512, 128); // 최소 크기 
                m_Window.Init();
            }
            else
            {
                m_Window.Focus();
            }
        }
        #endregion

        [SerializeField] private GameObject m_ObjRoot;
        [SerializeField] private GameObject m_PrefabOrigin;
        [SerializeField] private List<AnimationClip> m_ClipFiles;
        [SerializeField] private List<string> m_ClipNames;

        [SerializeField] private Type m_SimpleAniEventType;
        [SerializeField] private GameObject m_PrefabInstance;
        [SerializeField] private List<ChildObject> m_ChildObjects;
        [SerializeField] private List<ParticleSimulateInfo> m_SimulateInfoList;
        [SerializeField] private int m_SelectedClipIndex;
        [SerializeField] private AnimationClip m_Clip;
        [SerializeField] private List<AniClipEvent> m_clipEvents;
        [SerializeField] private ReorderableList m_ClipEventList;

        private double currentTime = 0;
        private double beforeTime = 0;
        private double duration = 0;
        private double deltaTime = 0;

        [SerializeField] private bool isInitialized = false;
        [SerializeField] private bool isPlaying = false;
        [SerializeField] private bool isLoop = false;
        [SerializeField] private bool isCompiling = false;
        [SerializeField] private bool m_IsExittingEditor = false;

        #region GUI
        readonly float width_Margin = 20;
        readonly float width_Function = 120;
        readonly float width_Time = 100;
        readonly float width_defaultSpace = 5;

        [SerializeField] private List<(string, EventParmType)> m_FunctionDatas;
        #endregion

        #region Getter
        private GameObject ObjRoot
        {
            get
            {
                if (m_ObjRoot == null)
                    m_ObjRoot = GameObject.Find("툴 인스턴스(씬에 저장 안됨)");

                if (m_ObjRoot == null)
                    m_ObjRoot = new GameObject("툴 인스턴스(씬에 저장 안됨)");

                return m_ObjRoot;
            }
        }
        private bool IsReadyToPlayAni => !isCompiling && !Application.isPlaying && m_PrefabInstance && m_Clip;
        private List<string> m_FunctionNames => m_FunctionDatas?.ConvertAll(x => x.Item1) ?? new List<string>();
        private List<string> m_ChildObjNames => m_ChildObjects?.ConvertAll(x => x.obj?.name) ?? new List<string>();
        #endregion

        private void Init()
        {
            if (m_Window == null)
                m_Window = this;

            InitVariables();

            isInitialized = true;
        }
        private void InitVariables()
        {
            m_ChildObjects = new List<ChildObject>();
            m_SimulateInfoList = new List<ParticleSimulateInfo>();

            m_ClipFiles = new List<AnimationClip>();
            m_ClipNames = new List<string>();
            m_SelectedClipIndex = 0;
            m_Clip = null;
            m_clipEvents = new List<AniClipEvent>();

            currentTime = 0;
            beforeTime = 0;
            duration = 0;
        }
        private void InitChildren()
        {
            foreach (var child in m_ChildObjects)
            {
                if (child.obj == null)
                    continue;

                child.obj.SetActive(child.initialActive);
            }
        }
        private void ResetWindow()
        {
            Clear();
            if (m_PrefabOrigin != null)
            {
                OnPrefabSelected();
                InitChildren();
            }
        }
        private void Clear()
        {
            if (m_ObjRoot != null)
            {
                DestroyImmediate(m_ObjRoot);
                m_ObjRoot = null;
                m_PrefabInstance = null;
            }

            m_ChildObjects?.Clear();
            m_SimulateInfoList?.Clear();
            m_ClipFiles?.Clear();
            m_ClipNames?.Clear();
            //m_SelectedClipIndex = 0;
            m_Clip = null;
            m_clipEvents?.Clear();
            m_ClipEventList = null;
            currentTime = 0;
            beforeTime = 0;
            duration = 0;
        }
        private void OnEnable()
        {
            CompilationPipeline.compilationStarted += OnCompileStarted;
            CompilationPipeline.compilationFinished += OnCompileFinished;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;

            if (isInitialized)
                ResetWindow();
            else
                Init();

        }
        private void OnDisable()
        {
            CompilationPipeline.compilationStarted -= OnCompileStarted;
            CompilationPipeline.compilationFinished -= OnCompileFinished;
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        }
        private void Update()
        {
            UpdatePlayAnimation();
            UpdatePlayAniClipEvent();
            UpdatePlayParticle();
        }
        private void OnDestroy()
        {
            Clear();

            if (m_PrefabOrigin != null)
                m_PrefabOrigin.SetActive(true);
        }
        #region GUI Methods
        private void OnGUI()
        {
            DrawGUI_TargetGameObject();
            if (m_PrefabOrigin == null)
                return;
            DrawGUI_PlayControlButtons();
            DrawGUI_TimeSlider();
            EditorGUILayout.Space();
            DrawGUI_TitleAndSaveButton();

            if (m_ClipEventList != null)
            {
                Rect rect = EditorGUILayout.GetControlRect();
                DrawGUI_ReorderableListWithoutHeader(m_ClipEventList);
                DrawGUI_ClipEventListHeader(rect);
            }
        }

        private void DrawGUI_TargetGameObject()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUI.BeginDisabledGroup(Application.isPlaying);
                EditorGUILayout.LabelField("Target Object", GUILayout.Width(100));
                if (m_PrefabOrigin == null)
                {
                    SimpleAniEvent aniEventObj = EditorGUILayout.ObjectField(m_PrefabOrigin, typeof(SimpleAniEvent), true) as SimpleAniEvent;
                    m_PrefabOrigin = aniEventObj?.gameObject;
                }
                else
                {
                    EditorGUI.BeginDisabledGroup(true);
                    m_PrefabOrigin = EditorGUILayout.ObjectField(m_PrefabOrigin, typeof(GameObject), false) as GameObject;
                    EditorGUI.EndDisabledGroup();
                    Rect rect = GUILayoutUtility.GetLastRect();
                    if (GUI.Button(new Rect(rect.x + rect.width - 20, rect.y, 20, rect.height), "X"))
                    {
                        Clear();
                        m_PrefabOrigin.SetActive(true);
                        m_PrefabOrigin = null;
                    }
                }
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.LabelField("Object Instance", GUILayout.Width(100));
                EditorGUILayout.ObjectField(m_PrefabInstance, typeof(GameObject), false);
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndHorizontal();

            if (m_PrefabOrigin == null)
                EditorGUILayout.HelpBox("Animation 혹은 Animator 컴포넌트가 있는 프리팹을 지정해주세요.", MessageType.Info);

            if (EditorGUI.EndChangeCheck())
            {
                Clear();
                if (IsValidPrefab(m_PrefabOrigin))
                {
                    OnPrefabSelected();
                }
            }
        }
        private void DrawGUI_PlayControlButtons()
        {
            if (m_Clip == null)
                return;

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUI.BeginDisabledGroup(Application.isPlaying);
                float buttonWidth = 32;
                float buttonHeight = 20;
                GUILayoutOption[] btnOption = { GUILayout.Width(buttonWidth), GUILayout.Height(buttonHeight) };
                if (GUILayout.Button(Styles.playIcon, btnOption))
                {
                    isPlaying = true;
                }
                if (GUILayout.Button(Styles.pauseIcon, btnOption))
                {
                    isPlaying = false;
                }
                if (GUILayout.Button(Styles.stopIcon, btnOption))
                {
                    isPlaying = false;
                    currentTime = 0;
                }
                EditorGUI.BeginChangeCheck();
                GUILayout.Toggle(isLoop, Styles.loopIcon, EditorStyles.miniButton, btnOption);
                if (EditorGUI.EndChangeCheck())
                {
                    isLoop = !isLoop;
                }

                GUILayout.FlexibleSpace();
                EditorGUI.BeginChangeCheck();
                m_SelectedClipIndex = EditorGUILayout.Popup(m_SelectedClipIndex, m_ClipNames.ToArray(), GUILayout.Width(300));
                if (EditorGUI.EndChangeCheck())
                {
                    m_Clip = m_ClipFiles[m_SelectedClipIndex];
                    SetAniClipEvents(m_Clip, ref m_clipEvents);
                    isPlaying = false;
                    currentTime = 0;
                }

                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndHorizontal();
        }
        private void DrawGUI_TimeSlider()
        {
            if (m_Clip == null)
                return;

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUI.BeginDisabledGroup(Application.isPlaying);
                EditorGUILayout.LabelField("Time", GUILayout.Width(100));
                currentTime = EditorGUILayout.Slider((float)currentTime, 0, (float)m_Clip.length);
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndHorizontal();
        }
        private void DrawGUI_TitleAndSaveButton()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Animation Events", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();

            EditorGUILayout.LabelField("Selected Clip File :", GUILayout.Width(110));
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField(m_Clip, typeof(AnimationClip), false);
            EditorGUI.EndDisabledGroup();
            GUI.backgroundColor = new Color(0, 1.4f, 0, 1);
            if (GUILayout.Button("Save", GUILayout.Width(50)))
            {
                SaveToClipFile();
                ResetWindow();
            }
            GUI.backgroundColor = Color.white; // Reset button color
            EditorGUILayout.EndHorizontal();
        }
        private void DrawGUI_ClipEventListHeader(Rect rect)
        {
            EditorGUILayout.BeginHorizontal();
            Rect areaRect = rect;
            areaRect.x -= 3f;
            areaRect.width = this.position.width;
            areaRect.y -= 1f;
            areaRect.height += 1;

            EditorGUI.DrawRect(areaRect, Color.gray);
            //EditorGUI.LabelField(rect, "Event List", EditorStyles.whiteBoldLabel);
            rect.x += width_Margin;
            EditorGUI.LabelField(rect, "Function", EditorStyles.whiteBoldLabel);
            rect.x += width_Function;
            EditorGUI.LabelField(rect, "Time", EditorStyles.whiteBoldLabel);
            rect.x += width_Time;
            EditorGUI.LabelField(rect, "Param", EditorStyles.whiteBoldLabel);
            EditorGUILayout.EndHorizontal();
        }

        private void GUI_InitReorderabletList()
        {
            if (m_ClipEventList == null)
            {
                m_ClipEventList = new ReorderableList(m_clipEvents, typeof(AniClipEvent), true, true, true, true);
                m_ClipEventList.drawHeaderCallback = (Rect rect) =>
                {
                    //EditorGUI.LabelField(rect, "Event List");
                };
                m_ClipEventList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    rect.y += 2;
                    rect.height = EditorGUIUtility.singleLineHeight;
                    rect.width -= 20;

                    Rect functionRect = new Rect(width_Margin, rect.y, width_Function, rect.height);
                    Rect timeRect = new Rect(functionRect.x + width_Function, rect.y, width_Time, rect.height);
                    timeRect.x += width_defaultSpace;
                    Rect paramRect = new Rect(timeRect.x + width_Time, rect.y, rect.width - width_Function - width_Time, rect.height);
                    paramRect.x += width_defaultSpace;

                    AniClipEvent clipEvent = m_clipEvents[index];
                    clipEvent.functionIndex = EditorGUI.Popup(functionRect, clipEvent.functionIndex, m_FunctionNames.ToArray());
                    clipEvent.time = EditorGUI.FloatField(timeRect, clipEvent.time);
                    EventParmType parmType = m_FunctionDatas[clipEvent.functionIndex].Item2;
                    switch (parmType)
                    {
                        case EventParmType.Float:
                            clipEvent.floatParameter = EditorGUI.FloatField(paramRect, clipEvent.floatParameter);
                            break;
                        case EventParmType.Int:
                            clipEvent.intParameter = EditorGUI.IntField(paramRect, clipEvent.intParameter);
                            break;
                        case EventParmType.Object:
                            {
                                var objRef = clipEvent.objectReferenceParameter;
                                if (objRef is UnityEngine.Object obj)
                                    clipEvent.objectReferenceParameter = EditorGUI.ObjectField(paramRect, obj, typeof(Object), true);
                            }
                            break;
                        case EventParmType.String:
                            clipEvent.stringParameter = EditorGUI.TextField(paramRect, clipEvent.stringParameter);
                            break;
                        case EventParmType.ChildObject:
                            {
                                int childIndex = m_ChildObjNames.Contains(clipEvent.stringParameter) ? m_ChildObjNames.IndexOf(clipEvent.stringParameter) : 0;
                                EditorGUI.BeginChangeCheck();
                                childIndex = EditorGUI.Popup(paramRect, childIndex, m_ChildObjNames.ToArray());
                                if (EditorGUI.EndChangeCheck())
                                {
                                    clipEvent.stringParameter = m_ChildObjNames[childIndex];
                                }
                            }
                            break;
                    }
                };
                m_ClipEventList.onAddCallback = (ReorderableList list) =>
                {
                    AniClipEvent clipEvent = new AniClipEvent();
                    clipEvent.time = (float)currentTime;
                    m_clipEvents.Add(clipEvent);
                };
                m_ClipEventList.onRemoveCallback = (ReorderableList list) =>
                {
                    m_clipEvents.RemoveAt(list.index);
                };
            }
        }
        private void DrawGUI_ReorderableListWithoutHeader(ReorderableList list)
        {
            if (list == null) return;

            Rect listRect = GUILayoutUtility.GetRect(0, list.GetHeight() - EditorGUIUtility.singleLineHeight); // Adjust height to exclude header
            listRect.y -= EditorGUIUtility.singleLineHeight + 3;
            list.DoList(listRect);
        }

        private bool IsValidPrefab(GameObject prefab)
        {
            if (prefab == null)
                return false;

            if (prefab.GetComponent<Animator>() == null && prefab.GetComponent<Animation>() == null)
                return false;

            return true;
        }
        #endregion

        private void OnPrefabSelected()
        {
            if (m_PrefabOrigin == null)
                return;

            if (PrefabUtility.IsPartOfPrefabAsset(m_PrefabOrigin))
            {
                m_PrefabInstance = PrefabUtility.InstantiatePrefab(m_PrefabOrigin, SceneManager.GetActiveScene()) as GameObject;
            }
            else if (PrefabUtility.IsPartOfPrefabInstance(m_PrefabOrigin))
            {
                m_PrefabOrigin.SetActive(false);
                GameObject prefabFile = PrefabUtility.GetCorrespondingObjectFromSource(m_PrefabOrigin);
                m_PrefabInstance = PrefabUtility.InstantiatePrefab(prefabFile, SceneManager.GetActiveScene()) as GameObject;
            }
            else
            {
                m_PrefabOrigin.SetActive(false);
                m_PrefabInstance = Instantiate(m_PrefabOrigin, ObjRoot.transform);
            }
            m_PrefabInstance.SetActive(true);
            m_PrefabInstance.name = m_PrefabOrigin.name + "_(Clone)";
            m_PrefabInstance.transform.SetParent(ObjRoot.transform);
            m_PrefabInstance.hideFlags = HideFlags.DontSave;

            OnPrefabInstanceCreated();
            GUI_InitReorderabletList();
        }
        private void OnPrefabInstanceCreated()
        {
            if (m_PrefabOrigin == null || m_PrefabInstance == null)
                return;

            if (m_PrefabInstance.GetComponent<Animator>() != null)
            {
                Animator animator = m_PrefabInstance.GetComponent<Animator>();
                if (animator.runtimeAnimatorController == null)
                    return;

                foreach (var clip in animator.runtimeAnimatorController.animationClips)
                {
                    if (clip != null && m_ClipFiles.Contains(clip) == false)
                        m_ClipFiles.Add(clip);
                }
            }
            else if (m_PrefabInstance.GetComponent<Animation>() != null)
            {
                Animation animation = m_PrefabInstance.GetComponent<Animation>();
                foreach (AnimationState state in animation)
                {
                    if (state.clip != null && m_ClipFiles.Contains(state.clip) == false)
                        m_ClipFiles.Add(state.clip);
                }
            }

            if (m_PrefabInstance.TryGetComponent(out SimpleAniEvent component) == false)
            {
                m_PrefabInstance.AddComponent<SimpleAniEvent>();
                component = m_PrefabInstance.GetComponent<SimpleAniEvent>();
            }

            //get all public methods of the SimpleAniEvent class or SimpleAniEvent's child class and add them to the m_FunctionNames list
            m_SimpleAniEventType = component.GetType();
            m_FunctionDatas = new List<(string, EventParmType)>();
            m_FunctionDatas.Add(("None", EventParmType.None));

            MethodInfo[] methods = GetAniEventMethods(m_SimpleAniEventType);//m_SimpleAniEventType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            foreach (var method in methods)
            {
                if (method.GetParameters().Length > 1)
                    continue;

                if (IsChildObjectTypeMethod(method.Name))
                {
                    m_FunctionDatas.Add((method.Name, EventParmType.ChildObject));
                    continue;
                }

                m_FunctionDatas.Add((method.Name, GetEventParmType(method)));
            }

            if (m_ClipFiles.Count > 0)
            {
                m_Clip = m_ClipFiles[m_SelectedClipIndex];
                SetAniClipEvents(m_Clip, ref m_clipEvents);
                foreach (var clip in m_ClipFiles)
                {
                    m_ClipNames.Add(clip.name);
                }
            }
            else
            {
                Debug.LogWarning("Animation Clip이 없습니다.");
            }

            m_SimulateInfoList = GetAllParticleSimulateInfos(m_PrefabInstance);

            m_ChildObjects = GetChildObjects(m_PrefabInstance);
            foreach (var child in m_ChildObjects)
            {
                child.SetChildrenParticleSimulateInfos(m_SimulateInfoList);
            }
            return;
        }

        private void UpdatePlayAnimation()
        {
            if (IsReadyToPlayAni == false)
                return;

            GameObject instanceObj = m_PrefabInstance;

            deltaTime = EditorApplication.timeSinceStartup - beforeTime;

            if (isPlaying)
            {
                double duration = m_Clip.length;
                double endFrame = m_Clip.frameRate * duration;
                double timePerFrame = duration / endFrame;

                currentTime += deltaTime;

                if (currentTime >= duration)
                {
                    if (isLoop)
                        currentTime = 0;
                    else
                    {
                        currentTime = duration;
                        isPlaying = false;
                    }
                }
            }

            m_Clip.SampleAnimation(instanceObj, (float)currentTime);
            Repaint();
            beforeTime = EditorApplication.timeSinceStartup;
        }
        private void UpdatePlayAniClipEvent()
        {
            if (m_ChildObjects.IsNullOrEmpty() || m_clipEvents.IsNullOrEmpty())
                return;

            if (currentTime == 0)
            {
                foreach (var child in m_ChildObjects)
                {
                    if (child.obj == null)
                        continue;

                    child.SetActive(child.initialActive, currentTime);
                }
            }

            foreach (AniClipEvent clipEvent in m_clipEvents)
            {
                bool isInTime = (float)currentTime >= clipEvent.time && (float)currentTime <= clipEvent.time + 1 / m_Clip.frameRate;
                if (isInTime)
                {
                    if (m_FunctionNames[clipEvent.functionIndex] == "ActiveChildObj")
                    {
                        ChildObject childObj = m_ChildObjects.Find(x => x.obj?.name == clipEvent.stringParameter);
                        childObj?.SetActive(true, currentTime);
                    }
                    else if (m_FunctionNames[clipEvent.functionIndex] == "DeactiveChildObj")
                    {
                        ChildObject childObj = m_ChildObjects.Find(x => x.obj?.name == clipEvent.stringParameter);
                        childObj?.SetActive(false, currentTime);
                    }
                    else
                    {
                        MethodInfo methodInfo = m_SimpleAniEventType.GetMethod(m_FunctionNames[clipEvent.functionIndex]);
                        if (methodInfo == null)
                            continue;

                        switch (m_FunctionDatas[clipEvent.functionIndex].Item2)
                        {
                            case EventParmType.Float:
                                methodInfo.Invoke(m_SimpleAniEventType, new object[] { clipEvent.floatParameter });
                                break;
                            case EventParmType.Int:
                                methodInfo.Invoke(m_SimpleAniEventType, new object[] { clipEvent.intParameter });
                                break;
                            case EventParmType.Object:
                                methodInfo.Invoke(m_SimpleAniEventType, new object[] { clipEvent.objectReferenceParameter });
                                break;
                            case EventParmType.String:
                                methodInfo.Invoke(m_SimpleAniEventType, new object[] { clipEvent.stringParameter });
                                break;
                            case EventParmType.ChildObject:
                                methodInfo.Invoke(m_SimpleAniEventType, new object[] { m_ChildObjects.Find(x => x.obj.name == clipEvent.stringParameter) });
                                break;
                        }
                    }
                }
            }
        }
        private void UpdatePlayParticle()
        {
            if (m_SimulateInfoList == null)
                return;

            bool isSimulated = false;
            foreach (ParticleSimulateInfo psi in m_SimulateInfoList)
            {
                float simulateTime = (float)(currentTime - psi.startTime);
                if (psi.Simulate(simulateTime, false))
                    isSimulated = true;
            }

            if (isSimulated)
                SceneView.RepaintAll();
        }

        private void SaveToClipFile()
        {
            if (m_Clip == null)
                return;
            AnimationEvent[] animationEvents = new AnimationEvent[m_clipEvents.Count];
            for (int i = 0; i < m_clipEvents.Count; i++)
            {
                AniClipEvent clipEvent = m_clipEvents[i];
                AnimationEvent aniEvent = new AnimationEvent();
                aniEvent.functionName = m_FunctionNames[clipEvent.functionIndex];
                aniEvent.time = clipEvent.time;
                aniEvent.floatParameter = clipEvent.floatParameter;
                aniEvent.intParameter = clipEvent.intParameter;
                aniEvent.objectReferenceParameter = clipEvent.objectReferenceParameter;
                aniEvent.stringParameter = clipEvent.stringParameter;

                animationEvents[i] = aniEvent;
            }
            AnimationUtility.SetAnimationEvents(m_Clip, animationEvents);
        }

        #region EventCallback
        private void OnPlayModeChanged(PlayModeStateChange playMode)
        {
            if (playMode == PlayModeStateChange.ExitingEditMode)
            {
                m_IsExittingEditor = true;
            }
            else if (playMode == PlayModeStateChange.EnteredEditMode)
            {
                m_IsExittingEditor = false;
                ResetWindow();
            }
        }
        private void OnCompileStarted(object obj)
        {
            isCompiling = true;
        }
        private void OnCompileFinished(object obj)
        {
            isCompiling = false;
        }
        #endregion

        #region Utility
        private MethodInfo[] GetAniEventMethods(Type simpleAniEventType)
        {
            //get all public methods of the SimpleAniEvent class or SimpleAniEvent's child class and add them to the m_FunctionNames list
            List<MethodInfo> methods = new List<MethodInfo>();

            MethodInfo[] allMethods = simpleAniEventType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            foreach (var method in allMethods)
            {
                if (method.GetParameters().Length > 1)
                    continue;

                if (IsChildObjectTypeMethod(method.Name))
                {
                    methods.Add(method);
                    continue;
                }

                methods.Add(method);
            }
            if (simpleAniEventType != typeof(SimpleAniEvent))
            {
                methods.AddRange(GetAniEventMethods(simpleAniEventType.BaseType));
            }

            return methods.ToArray();
        }
        private EventParmType GetEventParmType(MethodInfo methodInfo)
        {
            ParameterInfo[] parameters = methodInfo.GetParameters();
            if (parameters.Length != 1)
                return EventParmType.None;

            if (parameters[0].ParameterType == typeof(float))
                return EventParmType.Float;

            if (parameters[0].ParameterType == typeof(int))
                return EventParmType.Int;

            if (parameters[0].ParameterType == typeof(Object))
                return EventParmType.Object;

            if (parameters[0].ParameterType == typeof(string))
                return EventParmType.String;

            return EventParmType.None;
        }
        private List<ChildObject> GetChildObjects(GameObject prefabInstance)
        {
            List<ChildObject> childObjects = new List<ChildObject>();
            childObjects.Add(new ChildObject());

            Transform[] children = prefabInstance.GetComponentsInChildren<Transform>(true);
            foreach (var child in children)
            {
                if (child == prefabInstance.transform)
                    continue;

                ChildObject childObject = new ChildObject();
                childObject.parent = child.parent.gameObject;
                childObject.obj = child.gameObject;
                childObject.depth = child.GetSiblingIndex();
                childObject.initialActive = child.gameObject.activeSelf;

                childObjects.Add(childObject);
            }

            return childObjects;
        }
        private void SetAniClipEvents(AnimationClip clip, ref List<AniClipEvent> aniClipEvents)
        {
            aniClipEvents.Clear();
            foreach (var aniEvent in clip.events)
            {
                AniClipEvent aniClipEvent = new AniClipEvent();
                aniClipEvent.functionIndex = m_FunctionNames.Exists(x => x == aniEvent.functionName) ? m_FunctionNames.IndexOf(aniEvent.functionName) : 0;
                aniClipEvent.functionName = aniEvent.functionName;
                aniClipEvent.time = aniEvent.time;
                aniClipEvent.floatParameter = aniEvent.floatParameter;
                aniClipEvent.intParameter = aniEvent.intParameter;
                aniClipEvent.objectReferenceParameter = aniEvent.objectReferenceParameter;
                aniClipEvent.stringParameter = aniEvent.stringParameter;

                aniClipEvents.Add(aniClipEvent);
            }
        }

        private List<ParticleSimulateInfo> GetAllParticleSimulateInfos(GameObject prefabInstance)
        {
            List<ParticleSimulateInfo> simulateInfos = new List<ParticleSimulateInfo>();
            ParticleSystem[] particles = prefabInstance.GetComponentsInChildren<ParticleSystem>(true);
            uint randomSeed = (uint)UnityEngine.Random.Range(0, 100);

            foreach (var particle in particles)
            {
                if (IsChildOfSubEmitterParticleSystem(particle))
                    continue;
                particle.randomSeed = randomSeed;
                ParticleSimulateInfo simulateInfo = new ParticleSimulateInfo(particle);
                simulateInfos.Add(simulateInfo);
            }
            return simulateInfos;
        }
        private bool IsChildOfSubEmitterParticleSystem(ParticleSystem particle)
        {
            if (particle == null || particle.transform.parent == null || particle.transform.parent.TryGetComponent(out ParticleSystem ps) == false)
                return false;

            return ps.subEmitters.subEmittersCount > 0;

        }

        private List<ParticleSimulateInfo> GetParticleSimulateInfos(GameObject rootObject)
        {
            List<ParticleSimulateInfo> simulateInfos = new List<ParticleSimulateInfo>();
            ParticleSystem[] particles = rootObject.GetComponentsInChildren<ParticleSystem>(true);
            uint randomSeed = (uint)UnityEngine.Random.Range(0, 100);

            foreach (var particle in particles)
            {
                if (IsTopLevelParticleSystem(particle))// || IsSubEmitterOwnerParticleSystem(particle))
                {
                    particle.randomSeed = randomSeed;
                    ParticleSimulateInfo simulateInfo = new ParticleSimulateInfo(particle);
                    simulateInfos.Add(simulateInfo);
                }
            }
            return simulateInfos;
        }

        private bool IsTopLevelParticleSystem(ParticleSystem particle)
        {
            Transform currentTransform = particle.transform.parent;

            while (currentTransform != null)
            {
                if (currentTransform.GetComponent<ParticleSystem>() != null)
                {
                    return false;
                }
                currentTransform = currentTransform.parent;
            }

            return true;
        }
        private bool IsSubEmitterOwnerParticleSystem(ParticleSystem particle)
        {
            if (particle == null)
                return false;

            return particle.subEmitters.subEmittersCount > 0;
        }
        private bool IsChildObjectTypeMethod(string methodName)
        {
            return methodName == "ActiveChildObj" || methodName == "DeactiveChildObj";
        }
        #endregion
    }

    public static class Styles
    {
        public static readonly GUIContent playIcon = L10n.IconContent("Animation.Play", "Play");
        public static readonly GUIContent pauseIcon = EditorGUIUtility.IconContent("d_PauseButton On@2x");
        public static readonly GUIContent stopIcon = EditorGUIUtility.IconContent("d_PreMatQuad@2x");
        public static readonly GUIContent loopIcon = EditorGUIUtility.IconContent("d_preAudioLoopOff@2x");
    }

}
