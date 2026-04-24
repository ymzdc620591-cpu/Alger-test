using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Game.Portia
{
    public sealed class SceneMainGrassSystem : MonoBehaviour
    {
        const string SceneName = "SceneMain";
        const string SettingsResourcePath = "SceneMainGrassSettings";

        static SceneMainGrassSystem _instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void InstallAfterSceneLoad()
        {
            if (_instance != null)
                return;

            var go = new GameObject(nameof(SceneMainGrassSystem));
            _instance = go.AddComponent<SceneMainGrassSystem>();
            DontDestroyOnLoad(go);
        }

        sealed class GrassPatch
        {
            public GameObject GameObject;
            public Mesh Mesh;
        }

        readonly List<GrassPatch> _patches = new();

        Camera _mainCamera;
        Transform _player;
        Terrain _terrain;
        TerrainCollider _terrainCollider;
        Material _grassMaterial;
        Mesh _bladeMesh;
        SceneMainGrassSettings _settingsAsset;
        SceneMainGrassSettings _runtimeFallbackSettings;
        int _currentPatchX = int.MinValue;
        int _currentPatchZ = int.MinValue;
        int _appliedMaterialSignature = int.MinValue;
        int _appliedMeshSignature = int.MinValue;

        SceneMainGrassSettings Settings
        {
            get
            {
                EnsureSettingsLoaded();
                return _settingsAsset != null ? _settingsAsset : _runtimeFallbackSettings;
            }
        }

        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
            RebuildForScene();
        }

        void OnDestroy()
        {
            if (_instance == this)
                _instance = null;

            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            ClearPatches();

            if (_bladeMesh != null)
                Destroy(_bladeMesh);

            if (_grassMaterial != null)
                Destroy(_grassMaterial);

            if (_runtimeFallbackSettings != null)
                Destroy(_runtimeFallbackSettings);
        }

        void Update()
        {
            if (SceneManager.GetActiveScene().name != SceneName)
                return;

            if (!Settings.enableGrass)
            {
                if (_patches.Count > 0)
                    ClearPatches();

                _currentPatchX = int.MinValue;
                _currentPatchZ = int.MinValue;
                return;
            }

            if (_mainCamera == null)
                _mainCamera = Camera.main;

            if (_player == null)
            {
                var player = FindObjectOfType<Game.Player.PlayerController>();
                if (player != null)
                    _player = player.transform;
            }

            if (_terrain == null)
                ResolveLandTerrain();

            if (_grassMaterial == null)
                BuildResources();

            ApplySettingsIfDirty();

            if (_mainCamera == null || _player == null || _grassMaterial == null || _bladeMesh == null)
                return;

            UpdateMaterialParameters();
            RebuildPatchesAroundPlayer();
        }

        void OnActiveSceneChanged(Scene _, Scene next)
        {
            if (next.name == SceneName)
                RebuildForScene();
            else
                ClearPatches();
        }

        void RebuildForScene()
        {
            ClearPatches();
            _mainCamera = Camera.main;
            _terrain = null;
            _terrainCollider = null;
            _player = null;
            _currentPatchX = int.MinValue;
            _currentPatchZ = int.MinValue;
            _appliedMaterialSignature = int.MinValue;
            _appliedMeshSignature = int.MinValue;

            if (SceneManager.GetActiveScene().name != SceneName)
                return;

            ResolveLandTerrain();
            BuildResources();
        }

        public void ForceRefreshFromSettings()
        {
            _appliedMaterialSignature = int.MinValue;
            _appliedMeshSignature = int.MinValue;
            _currentPatchX = int.MinValue;
            _currentPatchZ = int.MinValue;
            ClearPatches();
            BuildResources();
        }

        void EnsureSettingsLoaded()
        {
            if (_settingsAsset != null || _runtimeFallbackSettings != null)
                return;

            _settingsAsset = Resources.Load<SceneMainGrassSettings>(SettingsResourcePath);
            if (_settingsAsset != null)
                return;

            _runtimeFallbackSettings = ScriptableObject.CreateInstance<SceneMainGrassSettings>();
            _runtimeFallbackSettings.hideFlags = HideFlags.DontSave;
            _runtimeFallbackSettings.ResetToDefaults();
        }

        void BuildResources()
        {
            EnsureSettingsLoaded();
            if (!Settings.enableGrass)
                return;

            var shader = Shader.Find("Instanced/InstancedShader");
            if (shader == null)
            {
                Debug.LogWarning("[SceneMainGrassSystem] Missing Instanced/InstancedShader.");
                return;
            }

            if (_grassMaterial == null)
            {
                _grassMaterial = new Material(shader)
                {
                    name = "SceneMainGrassRuntime",
                    enableInstancing = false,
                };
            }

            _grassMaterial.SetTexture("_ColorMap", Texture2D.whiteTexture);
            _grassMaterial.SetTexture("_WindTex", FindWindTexture());
            ApplySettingsIfDirty(true);
        }

        void ApplySettingsIfDirty(bool force = false)
        {
            var settings = Settings;

            int materialSignature = ComputeMaterialSignature(settings);
            if (force || materialSignature != _appliedMaterialSignature)
            {
                if (_grassMaterial != null)
                {
                    _grassMaterial.SetColor("_BaseColor", settings.tipColor);
                    _grassMaterial.SetColor("_Color", settings.rootColor);
                    _grassMaterial.SetFloat("_ColorScale", settings.colorScale);
                    _grassMaterial.SetFloat("_ColorOffset", settings.colorOffset);
                    _grassMaterial.SetFloat("_WindSpeed", settings.windSpeed);
                    _grassMaterial.SetFloat("_WindSize", settings.windSize);
                    _grassMaterial.SetFloat("_Height", settings.shaderHeight);
                }

                _appliedMaterialSignature = materialSignature;
            }

            int meshSignature = ComputeMeshSignature(settings);
            if (force || meshSignature != _appliedMeshSignature)
            {
                if (_bladeMesh != null)
                    Destroy(_bladeMesh);

                _bladeMesh = BuildBladeMesh(settings);
                _appliedMeshSignature = meshSignature;
                _currentPatchX = int.MinValue;
                _currentPatchZ = int.MinValue;
                ClearPatches();
            }
        }

        void RebuildPatchesAroundPlayer()
        {
            var settings = Settings;
            int patchX = Mathf.FloorToInt(_player.position.x / settings.patchSize);
            int patchZ = Mathf.FloorToInt(_player.position.z / settings.patchSize);

            if (patchX == _currentPatchX && patchZ == _currentPatchZ && _patches.Count > 0)
                return;

            _currentPatchX = patchX;
            _currentPatchZ = patchZ;

            ClearPatches();

            for (int z = patchZ - settings.patchRadius; z <= patchZ + settings.patchRadius; z++)
            {
                for (int x = patchX - settings.patchRadius; x <= patchX + settings.patchRadius; x++)
                    CreatePatch(x, z, settings);
            }
        }

        void CreatePatch(int patchX, int patchZ, SceneMainGrassSettings settings)
        {
            var patchMesh = BuildPatchMesh(patchX, patchZ, settings);
            var patchGo = new GameObject($"GrassPatch_{patchX}_{patchZ}");
            patchGo.transform.SetParent(transform, false);

            var filter = patchGo.AddComponent<MeshFilter>();
            filter.sharedMesh = patchMesh;

            var renderer = patchGo.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = _grassMaterial;
            renderer.shadowCastingMode = ShadowCastingMode.On;
            renderer.receiveShadows = true;
            renderer.lightProbeUsage = LightProbeUsage.BlendProbes;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;

            _patches.Add(new GrassPatch
            {
                GameObject = patchGo,
                Mesh = patchMesh,
            });
        }

        Mesh BuildPatchMesh(int patchX, int patchZ, SceneMainGrassSettings settings)
        {
            Vector3 patchOrigin = new Vector3(patchX * settings.patchSize, 0f, patchZ * settings.patchSize);
            int seed = unchecked((patchX * 73856093) ^ (patchZ * 19349663) ^ 1877);
            var random = new global::System.Random(seed);
            var combines = new CombineInstance[settings.patchDensity];

            for (int i = 0; i < settings.patchDensity; i++)
            {
                float x = patchOrigin.x + (float)random.NextDouble() * settings.patchSize;
                float z = patchOrigin.z + (float)random.NextDouble() * settings.patchSize;
                float y = SampleGroundHeight(x, z);
                float rotation = Mathf.Lerp(0f, 360f, (float)random.NextDouble());
                float scale = Mathf.Lerp(settings.minScale, settings.maxScale, (float)random.NextDouble());

                combines[i] = new CombineInstance
                {
                    mesh = _bladeMesh,
                    transform = Matrix4x4.TRS(
                        new Vector3(x, y, z),
                        Quaternion.Euler(0f, rotation, 0f),
                        Vector3.one * scale),
                };
            }

            var mesh = new Mesh
            {
                name = $"GrassPatchMesh_{patchX}_{patchZ}",
                indexFormat = IndexFormat.UInt32,
            };
            mesh.CombineMeshes(combines, true, true, false);
            mesh.RecalculateBounds();
            return mesh;
        }

        float SampleGroundHeight(float x, float z)
        {
            var ray = new Ray(new Vector3(x, 2000f, z), Vector3.down);
            var hits = Physics.RaycastAll(ray, 4000f, ~0, QueryTriggerInteraction.Ignore);
            if (hits != null && hits.Length > 0)
            {
                float bestY = float.NegativeInfinity;

                for (int i = 0; i < hits.Length; i++)
                {
                    var collider = hits[i].collider;
                    if (collider == null)
                        continue;

                    var hitGo = collider.gameObject;
                    if (hitGo == null || hitGo == gameObject || hitGo.transform.IsChildOf(transform))
                        continue;

                    string lowerName = hitGo.name.ToLowerInvariant();
                    if (lowerName.Contains("water") || lowerName.Contains("sea"))
                        continue;

                    if (hits[i].point.y > bestY)
                        bestY = hits[i].point.y;
                }

                if (bestY > float.NegativeInfinity)
                    return bestY;
            }

            if (_terrainCollider != null && _terrainCollider.Raycast(ray, out RaycastHit hit, 4000f))
                return hit.point.y;

            if (_terrain == null)
                return 0f;

            return _terrain.SampleHeight(new Vector3(x, _terrain.transform.position.y + 100f, z)) + _terrain.transform.position.y;
        }

        void ResolveLandTerrain()
        {
            Terrain fallbackTerrain = null;

            foreach (var terrain in FindObjectsOfType<Terrain>(true))
            {
                if (fallbackTerrain == null)
                    fallbackTerrain = terrain;

                if (terrain.name == "Land" || terrain.CompareTag("Ground"))
                {
                    _terrain = terrain;
                    _terrainCollider = terrain.GetComponent<TerrainCollider>();
                    return;
                }
            }

            _terrain = fallbackTerrain;
            _terrainCollider = _terrain != null ? _terrain.GetComponent<TerrainCollider>() : null;
        }

        void UpdateMaterialParameters()
        {
            Vector3 playerPosition = _player.position;
            _grassMaterial.SetVector("_PlayerPostion", new Vector4(playerPosition.x, playerPosition.y, playerPosition.z, Settings.interactionRadius));
        }

        void ClearPatches()
        {
            for (int i = 0; i < _patches.Count; i++)
            {
                if (_patches[i].GameObject != null)
                    Destroy(_patches[i].GameObject);

                if (_patches[i].Mesh != null)
                    Destroy(_patches[i].Mesh);
            }

            _patches.Clear();
        }

        static Texture2D FindWindTexture()
        {
            foreach (var texture in Resources.FindObjectsOfTypeAll<Texture2D>())
            {
                if (texture != null && texture.name.IndexOf("windWave", global::System.StringComparison.OrdinalIgnoreCase) >= 0)
                    return texture;
            }

            return Texture2D.grayTexture;
        }

        static Mesh BuildBladeMesh(SceneMainGrassSettings settings)
        {
            int cards = Mathf.Max(2, settings.cardCount);
            var mesh = new Mesh { name = "SceneMainGrassBlade" };

            var vertices = new Vector3[cards * 4];
            var normals = new Vector3[cards * 4];
            var uvs = new Vector2[cards * 4];
            var triangles = new int[cards * 6];

            for (int card = 0; card < cards; card++)
            {
                float angle = card * (180f / cards) * Mathf.Deg2Rad;
                Vector3 right = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * settings.bladeHalfWidth;
                int v = card * 4;
                int t = card * 6;

                vertices[v + 0] = -right;
                vertices[v + 1] = right;
                vertices[v + 2] = -right * settings.bladeTipScale + Vector3.up * settings.bladeHeight;
                vertices[v + 3] = right * settings.bladeTipScale + Vector3.up * settings.bladeHeight;

                normals[v + 0] = Vector3.up;
                normals[v + 1] = Vector3.up;
                normals[v + 2] = Vector3.up;
                normals[v + 3] = Vector3.up;

                uvs[v + 0] = new Vector2(0f, 0f);
                uvs[v + 1] = new Vector2(1f, 0f);
                uvs[v + 2] = new Vector2(0f, 1f);
                uvs[v + 3] = new Vector2(1f, 1f);

                triangles[t + 0] = v + 0;
                triangles[t + 1] = v + 2;
                triangles[t + 2] = v + 1;
                triangles[t + 3] = v + 2;
                triangles[t + 4] = v + 3;
                triangles[t + 5] = v + 1;
            }

            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            return mesh;
        }

        static int ComputeMaterialSignature(SceneMainGrassSettings settings)
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + settings.tipColor.GetHashCode();
                hash = hash * 31 + settings.rootColor.GetHashCode();
                hash = hash * 31 + settings.colorScale.GetHashCode();
                hash = hash * 31 + settings.colorOffset.GetHashCode();
                hash = hash * 31 + settings.windSpeed.GetHashCode();
                hash = hash * 31 + settings.windSize.GetHashCode();
                hash = hash * 31 + settings.shaderHeight.GetHashCode();
                hash = hash * 31 + settings.interactionRadius.GetHashCode();
                return hash;
            }
        }

        static int ComputeMeshSignature(SceneMainGrassSettings settings)
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + settings.patchSize.GetHashCode();
                hash = hash * 31 + settings.patchRadius;
                hash = hash * 31 + settings.patchDensity;
                hash = hash * 31 + settings.cardCount;
                hash = hash * 31 + settings.bladeHalfWidth.GetHashCode();
                hash = hash * 31 + settings.bladeHeight.GetHashCode();
                hash = hash * 31 + settings.bladeTipScale.GetHashCode();
                hash = hash * 31 + settings.minScale.GetHashCode();
                hash = hash * 31 + settings.maxScale.GetHashCode();
                return hash;
            }
        }
    }
}
