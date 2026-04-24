using UnityEditor;
using UnityEngine;

namespace Game.Portia
{
    public sealed class SceneMainGrassPanelWindow : EditorWindow
    {
        const string AssetFolder = "Assets/Game/Portia/Resources";
        const string AssetPath = AssetFolder + "/SceneMainGrassSettings.asset";

        SceneMainGrassSettings _settings;
        SerializedObject _serializedObject;

        [MenuItem("壳木AI/Portia/SceneMain Grass Panel")]
        static void Open()
        {
            GetWindow<SceneMainGrassPanelWindow>("SceneMain Grass");
        }

        void OnEnable()
        {
            LoadOrCreateSettings();
        }

        void OnGUI()
        {
            if (_settings == null || _serializedObject == null)
                LoadOrCreateSettings();

            if (_settings == null || _serializedObject == null)
                return;

            _serializedObject.Update();

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("SceneMain \u8349\u5730\u53c2\u6570", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("\u8fd9\u91cc\u7684\u53c2\u6570\u4f1a\u4fdd\u5b58\u5230\u914d\u7f6e\u8d44\u6e90\u3002Play \u6a21\u5f0f\u4e0b\u70b9\u51fb Refresh Live\uff0c\u53ef\u4ee5\u7acb\u523b\u5237\u65b0\u573a\u666f\u91cc\u7684\u52a8\u6001\u8349\u6548\u679c\u3002", MessageType.Info);

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("\u8fd0\u884c\u5f00\u5173", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(_settings.enableGrass ? "\u5f53\u524d\u72b6\u6001\uff1a\u5df2\u5f00\u542f" : "\u5f53\u524d\u72b6\u6001\uff1a\u5df2\u5173\u95ed");
            if (GUILayout.Button(_settings.enableGrass ? "\u5173\u95ed\u8349" : "\u5f00\u542f\u8349", GUILayout.Height(30)))
                ToggleGrassEnabled();

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("\u98ce\u683c\u9884\u8bbe", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("\u67d4\u548c\u8349\u576a", GUILayout.Height(28)))
                ApplyPreset("Apply Soft Lawn Preset", s => s.ApplySoftLawnPreset());
            if (GUILayout.Button("\u91ce\u8349\u7c07\u611f", GUILayout.Height(28)))
                ApplyPreset("Apply Wild Clump Preset", s => s.ApplyWildClumpPreset());
            GUILayout.EndHorizontal();

            DrawGroup("\u94fa\u5757\u8303\u56f4", "patchSize", "patchRadius", "patchDensity");
            DrawGroup("\u8349\u7247\u5f62\u72b6", "cardCount", "bladeHalfWidth", "bladeHeight", "bladeTipScale");
            DrawGroup("\u968f\u673a\u7f29\u653e", "minScale", "maxScale");
            DrawGroup("\u989c\u8272", "tipColor", "rootColor", "colorScale", "colorOffset");
            DrawGroup("\u98ce\u6446", "windSpeed", "windSize", "shaderHeight");
            DrawGroup("\u89d2\u8272\u4ea4\u4e92", "interactionRadius");

            bool changed = _serializedObject.ApplyModifiedProperties();
            if (changed)
            {
                EditorUtility.SetDirty(_settings);
                RefreshLiveScene();
            }

            EditorGUILayout.Space(8);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("\u6062\u590d\u9ed8\u8ba4", GUILayout.Height(28)))
            {
                Undo.RecordObject(_settings, "Reset SceneMain Grass Settings");
                _settings.ResetToDefaults();
                EditorUtility.SetDirty(_settings);
                _serializedObject = new SerializedObject(_settings);
                RefreshLiveScene();
            }

            if (GUILayout.Button("\u5b9a\u4f4d\u8d44\u6e90", GUILayout.Height(28)))
                Selection.activeObject = _settings;

            if (GUILayout.Button("\u5237\u65b0\u573a\u666f", GUILayout.Height(28)))
                RefreshLiveScene();
            GUILayout.EndHorizontal();
        }

        void DrawGroup(string title, params string[] propertyNames)
        {
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            for (int i = 0; i < propertyNames.Length; i++)
                EditorGUILayout.PropertyField(_serializedObject.FindProperty(propertyNames[i]));
        }

        void ApplyPreset(string undoName, global::System.Action<SceneMainGrassSettings> apply)
        {
            Undo.RecordObject(_settings, undoName);
            apply(_settings);
            EditorUtility.SetDirty(_settings);
            _serializedObject = new SerializedObject(_settings);
            RefreshLiveScene();
        }

        void ToggleGrassEnabled()
        {
            Undo.RecordObject(_settings, "Toggle SceneMain Grass");
            _settings.enableGrass = !_settings.enableGrass;
            EditorUtility.SetDirty(_settings);
            _serializedObject = new SerializedObject(_settings);
            RefreshLiveScene();
        }

        void LoadOrCreateSettings()
        {
            _settings = AssetDatabase.LoadAssetAtPath<SceneMainGrassSettings>(AssetPath);
            if (_settings == null)
            {
                EnsureFolder("Assets/Game/Portia", "Resources");
                _settings = CreateInstance<SceneMainGrassSettings>();
                _settings.ResetToDefaults();
                AssetDatabase.CreateAsset(_settings, AssetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            _serializedObject = new SerializedObject(_settings);
        }

        static void EnsureFolder(string parent, string child)
        {
            string path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, child);
        }

        static void RefreshLiveScene()
        {
            var systems = Object.FindObjectsOfType<SceneMainGrassSystem>(true);
            for (int i = 0; i < systems.Length; i++)
                systems[i].ForceRefreshFromSettings();

            SceneView.RepaintAll();
            EditorApplication.QueuePlayerLoopUpdate();
        }
    }
}
