using System;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

/// <summary>
/// Edit Mode에서 바로 수정할 수 있는 HUD 초안을 만드는 간단한 제작용 도구입니다.
/// 빈 GameObject 또는 Canvas에 붙인 뒤 Context Menu를 사용하세요.
/// </summary>
public sealed class PrototypeHUDGenerator : MonoBehaviour
{
    [Header("Optional Target")]
    [SerializeField] private Canvas targetCanvas;

    [Header("Prototype Settings")]
    [SerializeField, Min(1)] private int inventorySlotCount = 8;
    [SerializeField] private string prefabSavePath = "Assets/04. Prefabs/Basic UI/PrototypeHUD.prefab";

    [SerializeField, HideInInspector] private RectTransform generatedHudRoot;
    [SerializeField, HideInInspector] private Canvas createdCanvas;

    [ContextMenu("Create Prototype UI")]
    private void CreatePrototypeUi()
    {
        if (FindGeneratedHudRoot() != null)
        {
            Debug.LogWarning("A PrototypeHUD already exists. Edit it directly, then use Save As Prefab.", this);
            return;
        }

#if UNITY_EDITOR
        Undo.SetCurrentGroupName("Create Prototype HUD UI");
        int undoGroup = Undo.GetCurrentGroup();
#endif

        Canvas canvas = ResolveCanvas();
        if (canvas == null)
        {
            GameObject canvasObject = CreateUiObject("PrototypeHUDCanvas", transform);
            canvas = AddComponent<Canvas>(canvasObject);
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = AddComponent<CanvasScaler>(canvasObject);
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            AddComponent<GraphicRaycaster>(canvasObject);
            createdCanvas = canvas;
        }

        GameObject hudRoot = CreateUiObject("PrototypeHUD", canvas.transform);
        generatedHudRoot = hudRoot.GetComponent<RectTransform>();
        StretchToParent(generatedHudRoot, Vector2.zero);

        CreateTopRightHud(hudRoot.transform);
        CreateInventoryUi(hudRoot.transform, inventorySlotCount);

#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        if (!Application.isPlaying)
        {
            EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }

        Undo.CollapseUndoOperations(undoGroup);
#endif

        Debug.Log("Prototype HUD UI created. Adjust its Images and RectTransforms, then use Save As Prefab.", hudRoot);
    }

    [ContextMenu("Save As Prefab")]
    private void SaveAsPrefab()
    {
#if UNITY_EDITOR
        RectTransform hudRoot = FindGeneratedHudRoot();
        if (hudRoot == null)
        {
            Debug.LogError("No PrototypeHUD was found. Run Create Prototype UI first.", this);
            return;
        }

        if (!EnsureAssetFolder(prefabSavePath))
        {
            return;
        }

        PrefabUtility.SaveAsPrefabAsset(hudRoot.gameObject, prefabSavePath);
        AssetDatabase.SaveAssets();
        Debug.Log($"Prototype HUD prefab saved to {prefabSavePath}", hudRoot);
#else
        Debug.LogWarning("Save As Prefab is available only in the Unity Editor.", this);
#endif
    }

    private Canvas ResolveCanvas()
    {
        if (targetCanvas != null)
        {
            return targetCanvas;
        }

        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null)
        {
            return parentCanvas;
        }

        return createdCanvas;
    }

    private RectTransform FindGeneratedHudRoot()
    {
        if (generatedHudRoot != null)
        {
            return generatedHudRoot;
        }

        Transform directChild = transform.Find("PrototypeHUD");
        if (directChild != null)
        {
            return directChild.GetComponent<RectTransform>();
        }

        Canvas canvas = ResolveCanvas();
        if (canvas != null)
        {
            Transform canvasChild = canvas.transform.Find("PrototypeHUD");
            if (canvasChild != null)
            {
                return canvasChild.GetComponent<RectTransform>();
            }
        }

        Transform generatedCanvasChild = transform.Find("PrototypeHUDCanvas/PrototypeHUD");
        return generatedCanvasChild != null ? generatedCanvasChild.GetComponent<RectTransform>() : null;
    }

#if UNITY_EDITOR
    private static bool EnsureAssetFolder(string assetPath)
    {
        if (string.IsNullOrWhiteSpace(assetPath) || !assetPath.StartsWith("Assets/", StringComparison.Ordinal))
        {
            Debug.LogError("Prefab Save Path must start with 'Assets/'.");
            return false;
        }

        string directory = System.IO.Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
        if (string.IsNullOrEmpty(directory))
        {
            return true;
        }

        string[] folders = directory.Split('/');
        string currentFolder = folders[0];
        for (int i = 1; i < folders.Length; i++)
        {
            string nextFolder = currentFolder + "/" + folders[i];
            if (!AssetDatabase.IsValidFolder(nextFolder))
            {
                AssetDatabase.CreateFolder(currentFolder, folders[i]);
            }

            currentFolder = nextFolder;
        }

        return true;
    }
#endif

    private static void CreateTopRightHud(Transform parent)
    {
        GameObject topRightHud = CreateUiObject("TopRightHUD", parent);
        RectTransform topRightRect = topRightHud.GetComponent<RectTransform>();
        SetAnchoredRect(topRightRect, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-28f, -28f), new Vector2(180f, 270f));

        CreateTimeUi(topRightHud.transform);
        CreateHungerUi(topRightHud.transform);
    }

    private static void CreateTimeUi(Transform parent)
    {
        GameObject timeUi = CreateUiObject("TimeUI", parent);
        RectTransform timeRect = timeUi.GetComponent<RectTransform>();
        SetAnchoredRect(timeRect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(136f, 136f));

        Image background = CreateImage("Background", timeUi.transform, GetCircleSprite(), new Color(0.09f, 0.08f, 0.06f, 1f));
        StretchToParent(background.rectTransform, Vector2.zero);

        Image nightSegment = CreateImage("NightSegment", timeUi.transform, GetCircleSprite(), new Color(0.09f, 0.18f, 0.35f, 1f));
        StretchToParent(nightSegment.rectTransform, new Vector2(8f, 8f));

        Image daySegment = CreateImage("DaySegment", timeUi.transform, GetCircleSprite(), new Color(0.95f, 0.72f, 0.22f, 1f));
        StretchToParent(daySegment.rectTransform, new Vector2(8f, 8f));
        daySegment.type = Image.Type.Filled;
        daySegment.fillMethod = Image.FillMethod.Radial360;
        daySegment.fillOrigin = (int)Image.Origin360.Top;
        daySegment.fillClockwise = true;
        daySegment.fillAmount = 0.55f;

        Image timeProgress = CreateImage("TimeProgress", timeUi.transform, GetCircleSprite(), Color.white);
        SetAnchoredRect(timeProgress.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-39f, -31f), new Vector2(16f, 16f));
        timeProgress.raycastTarget = false;
    }

    private static void CreateHungerUi(Transform parent)
    {
        GameObject hungerUi = CreateUiObject("HungerUI", parent);
        RectTransform hungerRect = hungerUi.GetComponent<RectTransform>();
        SetAnchoredRect(hungerRect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -150f), new Vector2(92f, 92f));

        Image background = CreateImage("Background", hungerUi.transform, GetCircleSprite(), new Color(0.18f, 0.10f, 0.06f, 1f));
        StretchToParent(background.rectTransform, Vector2.zero);

        Image fill = CreateImage("Fill (Top To Bottom)", hungerUi.transform, GetCircleSprite(), new Color(0.94f, 0.49f, 0.18f, 1f));
        StretchToParent(fill.rectTransform, new Vector2(9f, 9f));
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Vertical;
        fill.fillOrigin = (int)Image.OriginVertical.Bottom;
        fill.fillAmount = 0.72f;

        Image icon = CreateImage("Icon", hungerUi.transform, GetCircleSprite(), new Color(1f, 0.91f, 0.72f, 1f));
        SetAnchoredRect(icon.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(30f, 30f));
    }

    private static void CreateInventoryUi(Transform parent, int slotCount)
    {
        const float slotSize = 54f;
        const float spacing = 5f;
        const int padding = 10;

        GameObject inventoryUi = CreateUiObject("InventoryUI", parent);
        RectTransform inventoryRect = inventoryUi.GetComponent<RectTransform>();
        float width = slotCount * slotSize + (slotCount - 1) * spacing + padding * 2f;
        SetAnchoredRect(inventoryRect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 28f), new Vector2(width, 74f));

        Image background = AddComponent<Image>(inventoryUi);
        background.sprite = GetDefaultSprite();
        background.color = new Color(0.07f, 0.06f, 0.05f, 0.92f);
        background.raycastTarget = false;

        HorizontalLayoutGroup layout = AddComponent<HorizontalLayoutGroup>(inventoryUi);
        layout.padding = new RectOffset(padding, padding, padding, padding);
        layout.spacing = spacing;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        for (int i = 0; i < slotCount; i++)
        {
            GameObject slot = CreateUiObject($"Slot_{i + 1:00}", inventoryUi.transform);

            Image slotBackground = AddComponent<Image>(slot);
            slotBackground.sprite = GetDefaultSprite();
            slotBackground.color = new Color(0.43f, 0.31f, 0.18f, 1f);
            slotBackground.raycastTarget = false;

            LayoutElement layoutElement = AddComponent<LayoutElement>(slot);
            layoutElement.minWidth = slotSize;
            layoutElement.minHeight = slotSize;
            layoutElement.preferredWidth = slotSize;
            layoutElement.preferredHeight = slotSize;

            Image itemIcon = CreateImage("ItemIcon", slot.transform, GetDefaultSprite(), new Color(1f, 1f, 1f, 0.25f));
            StretchToParent(itemIcon.rectTransform, new Vector2(7f, 7f));
            itemIcon.preserveAspect = true;
        }
    }

    private static GameObject CreateUiObject(string objectName, Transform parent)
    {
        GameObject uiObject = new GameObject(objectName, typeof(RectTransform));
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            Undo.RegisterCreatedObjectUndo(uiObject, $"Create {objectName}");
        }
#endif
        uiObject.transform.SetParent(parent, false);
        return uiObject;
    }

    private static T AddComponent<T>(GameObject gameObject) where T : Component
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            return Undo.AddComponent<T>(gameObject);
        }
#endif
        return gameObject.AddComponent<T>();
    }

    private static Image CreateImage(string objectName, Transform parent, Sprite sprite, Color color)
    {
        GameObject imageObject = CreateUiObject(objectName, parent);
        Image image = AddComponent<Image>(imageObject);
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static void StretchToParent(RectTransform rectTransform, Vector2 inset)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = inset;
        rectTransform.offsetMax = -inset;
    }

    private static void SetAnchoredRect(RectTransform rectTransform, Vector2 anchor, Vector2 pivot, Vector2 anchoredPosition, Vector2 size)
    {
        rectTransform.anchorMin = anchor;
        rectTransform.anchorMax = anchor;
        rectTransform.pivot = pivot;
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = size;
    }

    private static Sprite GetDefaultSprite()
    {
#if UNITY_EDITOR
        return AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
#else
        return null;
#endif
    }

    private static Sprite GetCircleSprite()
    {
#if UNITY_EDITOR
        return AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
#else
        return null;
#endif
    }
}
