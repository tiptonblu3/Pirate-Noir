using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Serialization;

public class GrassPrefabBrushWindow : EditorWindow
{
    private enum BrushType
    {
        Circle,
        Ring,
        Center,
        Grid,
        Line,
        Spiral
    }

    [System.Serializable]
    private class PrefabEntry
    {
        public GameObject prefab;
        public string group = "Grass";
        public float weight = 1f;
    }

    [System.Serializable]
    private class PrefabGroup
    {
        public string name = "Group";
        public float weight = 1f;
        public bool expanded = true;
    }

    private const string MenuPath = "Tools/Grass Prefab Brush";
    private const float MinBrushRadius = 0.1f;
    private const float MinSpacing = 0.01f;
    private const float MinWeight = 0.01f;

    [SerializeField] private List<PrefabEntry> prefabEntries = new List<PrefabEntry>();
    [FormerlySerializedAs("grassPrefabs")]
    [SerializeField] private List<GameObject> legacyGrassPrefabs = new List<GameObject>();
    [FormerlySerializedAs("prefabGroups")]
    [SerializeField] private List<string> legacyPrefabGroups = new List<string>();
    [SerializeField] private List<PrefabGroup> prefabGroups = new List<PrefabGroup>
    {
        new PrefabGroup { name = "Grass", weight = 5f, expanded = true },
        new PrefabGroup { name = "Flowers", weight = 1f, expanded = true }
    };
    [SerializeField] private List<string> selectedPaintGroups = new List<string>();
    [SerializeField] private bool paintGroupSelectionInitialized;
    [SerializeField] private string newGroupName = string.Empty;
    [SerializeField] private Transform parentForSpawnedGrass;
    [SerializeField] private LayerMask paintMask = ~0;
    [SerializeField] private float brushRadius = 2f;
    [SerializeField] private float densityPerSquareMeter = 12f;
    [SerializeField] private float minimumSpacing = 0.2f;
    [SerializeField] private bool randomYRotation = true;
    [SerializeField] private Vector2 randomScaleRange = new Vector2(0.85f, 1.2f);
    [SerializeField] private bool alignToSurfaceNormal;
    [SerializeField] private bool paintMode = true;
    [SerializeField] private bool eraseMode;
    [SerializeField] private float eraseRadius = 2f;
    [SerializeField] private BrushType brushType = BrushType.Circle;
    [SerializeField] private float ringThickness = 0.35f;
    [SerializeField] private int gridResolution = 5;
    [SerializeField] private float lineWidth = 0.2f;
    [SerializeField] private float spiralTurns = 3f;
    [SerializeField] private int previewSampleCount = 40;

    private readonly List<Vector3> strokePositions = new List<Vector3>();
    private bool sceneHooked;

    [MenuItem(MenuPath)]
    public static void OpenWindow()
    {
        var window = GetWindow<GrassPrefabBrushWindow>();
        window.titleContent = new GUIContent("Grass Brush");
        window.minSize = new Vector2(320f, 360f);
        window.Show();
    }

    private void OnEnable()
    {
        UpgradeLegacyPrefabList();
        EnsureGroupListValid();
        HookSceneGUI();
    }

    private void OnDisable()
    {
        UnhookSceneGUI();
    }

    private void OnDestroy()
    {
        UnhookSceneGUI();
    }

    private void HookSceneGUI()
    {
        if (sceneHooked) return;
        SceneView.duringSceneGui += DuringSceneGUI;
        sceneHooked = true;
    }

    private void UnhookSceneGUI()
    {
        if (!sceneHooked) return;
        SceneView.duringSceneGui -= DuringSceneGUI;
        sceneHooked = false;
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Grass Prefab Brush", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        DrawPrefabList();
        parentForSpawnedGrass = (Transform)EditorGUILayout.ObjectField(
            new GUIContent("Parent", "Optional parent for painted grass."),
            parentForSpawnedGrass,
            typeof(Transform),
            true
        );
        paintMask = LayerMaskField("Paint Mask", paintMask);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Tool Modes", EditorStyles.boldLabel);
        bool nextPaintMode = EditorGUILayout.ToggleLeft("Paint Mode", paintMode);
        bool nextEraseMode = EditorGUILayout.ToggleLeft("Erase Mode", eraseMode);
        if (nextPaintMode != paintMode)
        {
            paintMode = nextPaintMode;
            if (paintMode) eraseMode = false;
        }
        if (nextEraseMode != eraseMode)
        {
            eraseMode = nextEraseMode;
            if (eraseMode) paintMode = false;
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Paint Settings", EditorStyles.boldLabel);
        brushRadius = EditorGUILayout.Slider("Brush Radius", brushRadius, MinBrushRadius, 50f);
        densityPerSquareMeter = EditorGUILayout.Slider("Density / m²", densityPerSquareMeter, 1f, 80f);
        minimumSpacing = EditorGUILayout.Slider("Min Spacing", minimumSpacing, MinSpacing, 4f);
        brushType = (BrushType)EditorGUILayout.EnumPopup("Brush Type", brushType);
        if (brushType == BrushType.Ring)
        {
            ringThickness = EditorGUILayout.Slider("Ring Thickness", ringThickness, 0.05f, 0.95f);
        }
        else if (brushType == BrushType.Grid)
        {
            gridResolution = EditorGUILayout.IntSlider("Grid Resolution", gridResolution, 2, 12);
        }
        else if (brushType == BrushType.Line)
        {
            lineWidth = EditorGUILayout.Slider("Line Width", lineWidth, 0.01f, 1f);
        }
        else if (brushType == BrushType.Spiral)
        {
            spiralTurns = EditorGUILayout.Slider("Spiral Turns", spiralTurns, 1f, 8f);
        }
        previewSampleCount = EditorGUILayout.IntSlider("Preview Points", previewSampleCount, 8, 120);
        DrawPaintGroupSelection();
        alignToSurfaceNormal = EditorGUILayout.Toggle(
            new GUIContent("Align To Normal", "Align each instance up axis to hit surface normal."),
            alignToSurfaceNormal
        );
        randomYRotation = EditorGUILayout.Toggle("Random Y Rotation", randomYRotation);
        randomScaleRange = EditorGUILayout.Vector2Field("Scale Range", randomScaleRange);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Erase Settings", EditorStyles.boldLabel);
        eraseRadius = EditorGUILayout.Slider("Erase Radius", eraseRadius, MinBrushRadius, 50f);

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Controls:\n" +
            "- Toggle Paint Mode or Erase Mode, then left click/drag in Scene View.\n" +
            "- Brush Type changes how paint points are distributed.\n" +
            "- Hold Alt to orbit camera as usual.",
            MessageType.Info
        );
    }

    private void DrawPrefabList()
    {
        EditorGUILayout.LabelField("Grass Prefabs", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Groups are compact and share one weight per box.\nHigher group weight = appears more often.",
            MessageType.None
        );
        DrawGroupControls();
    }

    private static LayerMask LayerMaskField(string label, LayerMask selectedMask)
    {
        string[] layers = InternalEditorUtility.layers;
        int maskWithoutEmpty = 0;

        for (int i = 0; i < layers.Length; i++)
        {
            int layer = LayerMask.NameToLayer(layers[i]);
            if ((selectedMask.value & (1 << layer)) != 0)
            {
                maskWithoutEmpty |= 1 << i;
            }
        }

        maskWithoutEmpty = EditorGUILayout.MaskField(label, maskWithoutEmpty, layers);
        int finalMask = 0;

        for (int i = 0; i < layers.Length; i++)
        {
            if ((maskWithoutEmpty & (1 << i)) != 0)
            {
                finalMask |= 1 << LayerMask.NameToLayer(layers[i]);
            }
        }

        selectedMask.value = finalMask;
        return selectedMask;
    }

    private void DuringSceneGUI(SceneView sceneView)
    {
        if (!paintMode && !eraseMode)
        {
            return;
        }

        if (paintMode && (prefabEntries.Count == 0 || AllPrefabSlotsEmpty()))
        {
            return;
        }

        Event e = Event.current;
        if (e == null || e.alt)
        {
            return;
        }

        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, 5000f, paintMask))
        {
            return;
        }

        DrawBrushPreview(hit);
        HandleUtility.Repaint();

        int controlId = GUIUtility.GetControlID(FocusType.Passive);
        HandleUtility.AddDefaultControl(controlId);

        if (e.type == EventType.MouseDown && e.button == 0)
        {
            strokePositions.Clear();
            if (eraseMode) EraseAt(hit.point, eraseRadius);
            else if (paintMode) PaintAt(hit);
            e.Use();
        }
        else if (e.type == EventType.MouseDrag && e.button == 0)
        {
            if (eraseMode) EraseAt(hit.point, eraseRadius);
            else if (paintMode) PaintAt(hit);
            e.Use();
        }
        else if (e.type == EventType.MouseUp && e.button == 0)
        {
            strokePositions.Clear();
        }
    }

    private bool AllPrefabSlotsEmpty()
    {
        for (int i = 0; i < prefabEntries.Count; i++)
        {
            PrefabEntry entry = prefabEntries[i];
            if (entry != null && entry.prefab != null) return false;
        }

        return true;
    }

    private void PaintAt(RaycastHit hit)
    {
        int spawnCount = Mathf.Max(
            1,
            Mathf.RoundToInt(densityPerSquareMeter * Mathf.PI * brushRadius * brushRadius)
        );

        for (int i = 0; i < spawnCount; i++)
        {
            Vector2 local = SampleBrushLocalPoint();
            Vector3 tangent = Vector3.Cross(hit.normal, Vector3.up);
            if (tangent.sqrMagnitude < 0.001f)
            {
                tangent = Vector3.Cross(hit.normal, Vector3.forward);
            }
            tangent.Normalize();
            Vector3 bitangent = Vector3.Cross(hit.normal, tangent).normalized;

            Vector3 samplePoint = hit.point + tangent * local.x + bitangent * local.y;
            Ray sampleRay = new Ray(samplePoint + hit.normal * 20f, -hit.normal);
            if (!Physics.Raycast(sampleRay, out RaycastHit sampleHit, 40f, paintMask))
            {
                continue;
            }

            if (IsTooClose(sampleHit.point))
            {
                continue;
            }

            GameObject prefab = GetRandomPrefab();
            if (prefab == null) continue;

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            Undo.RegisterCreatedObjectUndo(instance, "Paint Grass");

            Quaternion rot = alignToSurfaceNormal
                ? Quaternion.FromToRotation(Vector3.up, sampleHit.normal)
                : Quaternion.identity;
            if (randomYRotation)
            {
                rot = Quaternion.AngleAxis(Random.Range(0f, 360f), sampleHit.normal) * rot;
            }

            float uniformScale = Mathf.Max(0.001f, Random.Range(randomScaleRange.x, randomScaleRange.y));
            instance.transform.SetPositionAndRotation(sampleHit.point, rot);
            instance.transform.localScale *= uniformScale;

            if (parentForSpawnedGrass != null)
            {
                Undo.SetTransformParent(instance.transform, parentForSpawnedGrass, "Parent Painted Grass");
            }

            strokePositions.Add(sampleHit.point);
        }
    }

    private bool IsTooClose(Vector3 point)
    {
        float minSqrDistance = minimumSpacing * minimumSpacing;
        for (int i = 0; i < strokePositions.Count; i++)
        {
            if ((strokePositions[i] - point).sqrMagnitude < minSqrDistance)
            {
                return true;
            }
        }
        return false;
    }

    private GameObject GetRandomPrefab()
    {
        var prefabsByGroup = new Dictionary<string, List<GameObject>>();
        var groupsForSelection = new List<PrefabGroup>();
        float totalWeight = 0f;

        for (int i = 0; i < prefabEntries.Count; i++)
        {
            PrefabEntry entry = prefabEntries[i];
            if (entry == null || entry.prefab == null)
            {
                continue;
            }
            if (!IsGroupEnabledForPainting(entry.group))
            {
                continue;
            }

            if (!prefabsByGroup.TryGetValue(entry.group, out List<GameObject> prefabsInGroup))
            {
                prefabsInGroup = new List<GameObject>();
                prefabsByGroup.Add(entry.group, prefabsInGroup);
            }

            prefabsInGroup.Add(entry.prefab);
        }

        for (int i = 0; i < prefabGroups.Count; i++)
        {
            PrefabGroup group = prefabGroups[i];
            if (group == null || !prefabsByGroup.ContainsKey(group.name))
            {
                continue;
            }

            float clampedWeight = Mathf.Max(MinWeight, group.weight);
            totalWeight += clampedWeight;
            groupsForSelection.Add(group);
        }

        if (groupsForSelection.Count == 0 || totalWeight <= 0f)
        {
            return null;
        }

        float roll = Random.Range(0f, totalWeight);
        float running = 0f;
        for (int i = 0; i < groupsForSelection.Count; i++)
        {
            PrefabGroup group = groupsForSelection[i];
            running += Mathf.Max(MinWeight, group.weight);
            if (roll <= running)
            {
                List<GameObject> options = prefabsByGroup[group.name];
                return options[Random.Range(0, options.Count)];
            }
        }

        PrefabGroup fallback = groupsForSelection[groupsForSelection.Count - 1];
        List<GameObject> fallbackOptions = prefabsByGroup[fallback.name];
        return fallbackOptions[Random.Range(0, fallbackOptions.Count)];
    }

    private void EraseAt(Vector3 center, float radius)
    {
        if (parentForSpawnedGrass != null)
        {
            int childCount = parentForSpawnedGrass.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                Transform child = parentForSpawnedGrass.GetChild(i);
                if ((child.position - center).sqrMagnitude <= radius * radius)
                {
                    Undo.DestroyObjectImmediate(child.gameObject);
                }
            }
            return;
        }

        var validPrefabs = new HashSet<GameObject>();
        for (int i = 0; i < prefabEntries.Count; i++)
        {
            PrefabEntry entry = prefabEntries[i];
            if (entry != null && entry.prefab != null)
            {
                validPrefabs.Add(entry.prefab);
            }
        }

        if (validPrefabs.Count == 0) return;

        GameObject[] sceneObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        for (int i = 0; i < sceneObjects.Length; i++)
        {
            GameObject obj = sceneObjects[i];
            GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(obj);
            if (source == null || !validPrefabs.Contains(source)) continue;

            if ((obj.transform.position - center).sqrMagnitude <= radius * radius)
            {
                Undo.DestroyObjectImmediate(obj);
            }
        }
    }

    private void UpgradeLegacyPrefabList()
    {
        // Supports old serialized data where prefab list had no weight/group.
        if (prefabEntries == null)
        {
            prefabEntries = new List<PrefabEntry>();
        }

        if (prefabEntries.Count == 0 && legacyGrassPrefabs != null && legacyGrassPrefabs.Count > 0)
        {
            for (int i = 0; i < legacyGrassPrefabs.Count; i++)
            {
                GameObject prefab = legacyGrassPrefabs[i];
                if (prefab == null)
                {
                    continue;
                }

                prefabEntries.Add(new PrefabEntry
                {
                    prefab = prefab,
                    group = "Grass",
                    weight = 1f
                });
            }
        }
    }

    private void DrawGroupControls()
    {
        EnsureGroupListValid();
        int removePrefabIndex = -1;
        for (int i = 0; i < prefabGroups.Count; i++)
        {
            PrefabGroup group = prefabGroups[i];
            if (group == null)
            {
                continue;
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            group.expanded = EditorGUILayout.Foldout(group.expanded, group.name, true);
            EditorGUILayout.LabelField("Weight", GUILayout.Width(46f));
            group.weight = Mathf.Max(MinWeight, EditorGUILayout.FloatField(group.weight, GUILayout.Width(56f)));
            if (!IsBaseGroup(group.name) && GUILayout.Button("X", GUILayout.Width(24f)))
            {
                RemoveGroup(group.name);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                break;
            }
            EditorGUILayout.EndHorizontal();

            if (group.expanded)
            {
                for (int entryIndex = 0; entryIndex < prefabEntries.Count; entryIndex++)
                {
                    PrefabEntry entry = prefabEntries[entryIndex];
                    if (entry == null || entry.group != group.name)
                    {
                        continue;
                    }

                    EditorGUILayout.BeginHorizontal();
                    entry.prefab = (GameObject)EditorGUILayout.ObjectField(entry.prefab, typeof(GameObject), false);
                    if (GUILayout.Button("X", GUILayout.Width(24f)))
                    {
                        removePrefabIndex = entryIndex;
                    }
                    EditorGUILayout.EndHorizontal();
                }

                if (GUILayout.Button("Add Prefab To Group"))
                {
                    prefabEntries.Add(new PrefabEntry
                    {
                        group = group.name,
                        prefab = null,
                        weight = group.weight
                    });
                }
            }

            EditorGUILayout.EndVertical();
        }

        if (removePrefabIndex >= 0)
        {
            prefabEntries.RemoveAt(removePrefabIndex);
        }

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Add Group", EditorStyles.miniBoldLabel);
        EditorGUILayout.BeginHorizontal();
        newGroupName = EditorGUILayout.TextField(newGroupName);
        if (GUILayout.Button("Add", GUILayout.Width(48f)))
        {
            TryAddGroup(newGroupName);
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }

    private void TryAddGroup(string groupName)
    {
        string cleaned = string.IsNullOrWhiteSpace(groupName) ? string.Empty : groupName.Trim();
        if (cleaned.Length == 0)
        {
            return;
        }

        if (!HasGroup(cleaned))
        {
            prefabGroups.Add(new PrefabGroup
            {
                name = cleaned,
                weight = 1f,
                expanded = true
            });
            if (!selectedPaintGroups.Contains(cleaned))
            {
                selectedPaintGroups.Add(cleaned);
            }
        }

        newGroupName = string.Empty;
    }

    private void RemoveGroup(string groupName)
    {
        if (IsBaseGroup(groupName))
        {
            return;
        }

        for (int i = 0; i < prefabGroups.Count; i++)
        {
            if (prefabGroups[i] != null && prefabGroups[i].name == groupName)
            {
                prefabGroups.RemoveAt(i);
                break;
            }
        }
        selectedPaintGroups.Remove(groupName);
        for (int i = 0; i < prefabEntries.Count; i++)
        {
            PrefabEntry entry = prefabEntries[i];
            if (entry != null && entry.group == groupName)
            {
                entry.group = "Grass";
            }
        }
    }

    private void EnsureGroupListValid()
    {
        if (prefabGroups == null)
        {
            prefabGroups = new List<PrefabGroup>();
        }

        if (!HasGroup("Grass"))
        {
            prefabGroups.Insert(0, new PrefabGroup { name = "Grass", weight = 5f, expanded = true });
        }
        if (!HasGroup("Flowers"))
        {
            prefabGroups.Add(new PrefabGroup { name = "Flowers", weight = 1f, expanded = true });
        }

        for (int i = prefabGroups.Count - 1; i >= 0; i--)
        {
            PrefabGroup group = prefabGroups[i];
            if (group == null || string.IsNullOrWhiteSpace(group.name))
            {
                prefabGroups.RemoveAt(i);
                continue;
            }
            group.name = group.name.Trim();
            group.weight = Mathf.Max(MinWeight, group.weight);
        }

        for (int i = 0; i < prefabEntries.Count; i++)
        {
            PrefabEntry entry = prefabEntries[i];
            if (entry == null)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(entry.group) || !HasGroup(entry.group))
            {
                entry.group = "Grass";
            }
        }

        if (legacyPrefabGroups != null && legacyPrefabGroups.Count > 0)
        {
            for (int i = 0; i < legacyPrefabGroups.Count; i++)
            {
                TryAddGroup(legacyPrefabGroups[i]);
            }
            legacyPrefabGroups.Clear();
        }

        if (selectedPaintGroups == null)
        {
            selectedPaintGroups = new List<string>();
        }

        for (int i = selectedPaintGroups.Count - 1; i >= 0; i--)
        {
            if (!HasGroup(selectedPaintGroups[i]))
            {
                selectedPaintGroups.RemoveAt(i);
            }
        }

        if (!paintGroupSelectionInitialized)
        {
            for (int i = 0; i < prefabGroups.Count; i++)
            {
                PrefabGroup group = prefabGroups[i];
                if (group != null && !selectedPaintGroups.Contains(group.name))
                {
                    selectedPaintGroups.Add(group.name);
                }
            }
            paintGroupSelectionInitialized = true;
        }
    }

    private Vector2 SampleBrushLocalPoint()
    {
        float safeRadius = Mathf.Max(0.001f, brushRadius);

        switch (brushType)
        {
            case BrushType.Ring:
            {
                float innerRadius = safeRadius * (1f - ringThickness);
                float radius = Random.Range(innerRadius, brushRadius);
                return Random.insideUnitCircle.normalized * radius;
            }
            case BrushType.Center:
            {
                return Random.insideUnitCircle * (safeRadius * 0.35f);
            }
            case BrushType.Grid:
            {
                int cells = Mathf.Max(2, gridResolution);
                int x = Random.Range(0, cells);
                int y = Random.Range(0, cells);
                Vector2 basePoint = new Vector2(
                    Mathf.Lerp(-safeRadius, safeRadius, x / (float)(cells - 1)),
                    Mathf.Lerp(-safeRadius, safeRadius, y / (float)(cells - 1))
                );
                Vector2 jitter = Random.insideUnitCircle * (safeRadius / cells) * 0.35f;
                return Vector2.ClampMagnitude(basePoint + jitter, safeRadius);
            }
            case BrushType.Line:
            {
                float y = Random.Range(-safeRadius, safeRadius);
                float x = Random.Range(-safeRadius, safeRadius) * Mathf.Clamp01(lineWidth);
                return new Vector2(x, y);
            }
            case BrushType.Spiral:
            {
                float t = Random.value;
                float angle = t * spiralTurns * Mathf.PI * 2f;
                float radius = Mathf.Sqrt(t) * safeRadius;
                return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            }
            case BrushType.Circle:
            default:
            {
                return Random.insideUnitCircle * safeRadius;
            }
        }
    }

    private void DrawBrushPreview(RaycastHit hit)
    {
        float activeRadius = eraseMode ? eraseRadius : brushRadius;
        Handles.color = eraseMode
            ? new Color(1f, 0.3f, 0.3f, 0.4f)
            : new Color(0.2f, 1f, 0.2f, 0.2f);
        Handles.DrawSolidDisc(hit.point, hit.normal, activeRadius);
        Handles.color = eraseMode ? Color.red : Color.green;
        Handles.DrawWireDisc(hit.point, hit.normal, activeRadius);

        if (eraseMode || !paintMode)
        {
            return;
        }

        Vector3 tangent = Vector3.Cross(hit.normal, Vector3.up);
        if (tangent.sqrMagnitude < 0.001f)
        {
            tangent = Vector3.Cross(hit.normal, Vector3.forward);
        }
        tangent.Normalize();
        Vector3 bitangent = Vector3.Cross(hit.normal, tangent).normalized;

        Handles.color = new Color(0.1f, 0.9f, 0.3f, 0.9f);
        int samples = Mathf.Max(8, previewSampleCount);
        for (int i = 0; i < samples; i++)
        {
            Vector2 local = SamplePreviewLocalPoint(i, samples, brushRadius);
            Vector3 samplePoint = hit.point + tangent * local.x + bitangent * local.y;
            Ray sampleRay = new Ray(samplePoint + hit.normal * 20f, -hit.normal);
            if (Physics.Raycast(sampleRay, out RaycastHit sampleHit, 40f, paintMask))
            {
                float dotSize = HandleUtility.GetHandleSize(sampleHit.point) * 0.035f;
                Handles.DotHandleCap(0, sampleHit.point, Quaternion.identity, dotSize, EventType.Repaint);
            }
        }
    }

    private Vector2 SamplePreviewLocalPoint(int index, int total, float radius)
    {
        float safeRadius = Mathf.Max(0.001f, radius);
        float t = total <= 1 ? 0f : index / (float)(total - 1);

        switch (brushType)
        {
            case BrushType.Ring:
            {
                float angle = t * Mathf.PI * 2f;
                float inner = safeRadius * (1f - ringThickness);
                float radial = Mathf.Lerp(inner, safeRadius, ((index * 37) % 100) / 99f);
                return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radial;
            }
            case BrushType.Center:
            {
                float golden = 2.39996323f;
                float r = Mathf.Sqrt((index + 0.5f) / total) * (safeRadius * 0.4f);
                float a = index * golden;
                return new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r;
            }
            case BrushType.Grid:
            {
                int cells = Mathf.Max(2, gridResolution);
                int x = index % cells;
                int y = (index / cells) % cells;
                return new Vector2(
                    Mathf.Lerp(-safeRadius, safeRadius, x / (float)(cells - 1)),
                    Mathf.Lerp(-safeRadius, safeRadius, y / (float)(cells - 1))
                );
            }
            case BrushType.Line:
            {
                float y = Mathf.Lerp(-safeRadius, safeRadius, t);
                float x = (((index * 53) % 100) / 99f - 0.5f) * safeRadius * Mathf.Clamp01(lineWidth);
                return new Vector2(x, y);
            }
            case BrushType.Spiral:
            {
                float angle = t * spiralTurns * Mathf.PI * 2f;
                float spiralRadius = t * safeRadius;
                return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * spiralRadius;
            }
            case BrushType.Circle:
            default:
            {
                float golden = 2.39996323f;
                float r = Mathf.Sqrt((index + 0.5f) / total) * safeRadius;
                float a = index * golden;
                return new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r;
            }
        }
    }

    private bool HasGroup(string name)
    {
        for (int i = 0; i < prefabGroups.Count; i++)
        {
            if (prefabGroups[i] != null && prefabGroups[i].name == name)
            {
                return true;
            }
        }
        return false;
    }

    private float GetGroupWeight(string groupName)
    {
        for (int i = 0; i < prefabGroups.Count; i++)
        {
            PrefabGroup group = prefabGroups[i];
            if (group != null && group.name == groupName)
            {
                return group.weight;
            }
        }
        return 1f;
    }

    private static bool IsBaseGroup(string groupName)
    {
        return groupName == "Grass" || groupName == "Flowers";
    }

    private bool IsGroupEnabledForPainting(string groupName)
    {
        return selectedPaintGroups != null && selectedPaintGroups.Contains(groupName);
    }

    private void DrawPaintGroupSelection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Paint Groups", EditorStyles.miniBoldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("All", GUILayout.Width(44f)))
        {
            selectedPaintGroups.Clear();
            for (int i = 0; i < prefabGroups.Count; i++)
            {
                PrefabGroup group = prefabGroups[i];
                if (group != null)
                {
                    selectedPaintGroups.Add(group.name);
                }
            }
        }
        if (GUILayout.Button("None", GUILayout.Width(52f)))
        {
            selectedPaintGroups.Clear();
        }
        EditorGUILayout.EndHorizontal();

        int enabledCount = 0;
        for (int i = 0; i < prefabGroups.Count; i++)
        {
            PrefabGroup group = prefabGroups[i];
            if (group == null)
            {
                continue;
            }

            bool isEnabled = selectedPaintGroups.Contains(group.name);
            bool nextEnabled = EditorGUILayout.ToggleLeft(group.name, isEnabled);
            if (nextEnabled != isEnabled)
            {
                if (nextEnabled)
                {
                    selectedPaintGroups.Add(group.name);
                }
                else
                {
                    selectedPaintGroups.Remove(group.name);
                }
            }

            if (selectedPaintGroups.Contains(group.name))
            {
                enabledCount++;
            }
        }

        if (enabledCount == 0)
        {
            EditorGUILayout.HelpBox("No paint groups selected. Painting will place nothing.", MessageType.Warning);
        }
        EditorGUILayout.EndVertical();
    }
}
