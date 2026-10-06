using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Temporary batch-mode setup for Phase 3. Deleted after it has run.
public static class Phase3Setup
{
    const string LevelCanvasPath = "Assets/Prefabs/Level/LevelCanvas.prefab";
    const string LevelManagerPath = "Assets/Prefabs/Level/LevelManager.prefab";
    const string CannonPath = "Assets/Prefabs/Level/Cannon.prefab";
    const string GrubPath = "Assets/Prefabs/Grub.prefab";
    const string StarPath = "Assets/Prefabs/star.prefab";
    const string AudioManagerPath = "Assets/Prefabs/AudioManager.prefab";
    const string OptionsPanelPath = "Assets/Prefabs/UI/OptionsPanel.prefab";
    const string SparklePath = "Assets/Prefabs/FX/Sparkle.prefab";
    const string PuffPath = "Assets/Prefabs/FX/Puff.prefab";
    const string MainMenuScene = "Assets/Scenes/Main Menu.unity";
    static readonly string[] LevelScenes =
    {
        "Assets/Scenes/Level01.unity", "Assets/Scenes/Level02.unity",
        "Assets/Scenes/Level03.unity", "Assets/Scenes/Level04.unity",
    };

    static readonly Color Candy = new Color(1f, 0.45f, 0.7f);

    static Sprite starSprite, knobSprite;
    static TMP_FontAsset font;

    public static void Run()
    {
        try
        {
            starSprite = Load<GameObject>(StarPath).GetComponent<SpriteRenderer>().sprite;
            knobSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath("8f586378b4e144a9851e7b34d9b748ee"));
            if (knobSprite == null || font == null) throw new Exception("Missing knob sprite or font");

            EnsureFolder("Assets/Prefabs", "FX");
            EnsureFolder("Assets/Prefabs", "UI");

            SetupGrub();
            SetupFx();
            SetupAudioManager();
            SetupLevelManager();
            SetupCannon();
            SetupOptionsPanel();
            SetupLevelCanvas();
            SetupMainMenu();
            foreach (string scene in LevelScenes) SetupLevelScene(scene);

            AssetDatabase.SaveAssets();
            Debug.Log("PHASE3 SETUP OK");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            EditorApplication.Exit(1);
        }
    }

    static T Load<T>(string path) where T : UnityEngine.Object
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset == null) throw new Exception("Missing asset " + path);
        return asset;
    }

    static void EnsureFolder(string parent, string name)
    {
        if (!AssetDatabase.IsValidFolder(parent + "/" + name)) AssetDatabase.CreateFolder(parent, name);
    }

    static void SetRef(UnityEngine.Object target, string field, UnityEngine.Object value)
    {
        var so = new SerializedObject(target);
        var prop = so.FindProperty(field) ?? throw new Exception("No field " + field + " on " + target);
        prop.objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void SetRefArray(UnityEngine.Object target, string field, UnityEngine.Object[] values)
    {
        var so = new SerializedObject(target);
        var prop = so.FindProperty(field) ?? throw new Exception("No field " + field + " on " + target);
        prop.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++)
            prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void ClearListeners(Button button)
    {
        for (int i = button.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
            UnityEventTools.RemovePersistentListener(button.onClick, i);
    }

    static Transform Find(Transform root, string path)
    {
        return root.Find(path) ?? throw new Exception("Missing " + path + " under " + root.name);
    }

    // ---------- Prefabs ----------

    static void SetupGrub()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(GrubPath);
        root.GetComponent<Rigidbody2D>().interpolation = RigidbodyInterpolation2D.Interpolate;
        PrefabUtility.SaveAsPrefabAsset(root, GrubPath);
        PrefabUtility.UnloadPrefabContents(root);
    }

    static void SetupFx()
    {
        Material mat = Load<Material>("Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat");

        // Star pickup and basket win: gold star sparks.
        CreateBurst(SparklePath, mat, starSprite, count: 14,
            lifetime: new Vector2(0.35f, 0.65f), speed: new Vector2(2f, 5f), size: new Vector2(0.15f, 0.3f),
            colorA: new Color(1f, 0.95f, 0.5f), colorB: Color.white, gravity: 0.4f);

        // Cannon fire and bouncy pads: soft white puff.
        CreateBurst(PuffPath, mat, knobSprite, count: 10,
            lifetime: new Vector2(0.25f, 0.45f), speed: new Vector2(1f, 2.5f), size: new Vector2(0.2f, 0.4f),
            colorA: new Color(1f, 1f, 1f, 0.85f), colorB: new Color(1f, 0.85f, 0.92f, 0.85f), gravity: 0f);
    }

    static void CreateBurst(string path, Material mat, Sprite sprite, int count, Vector2 lifetime, Vector2 speed,
        Vector2 size, Color colorA, Color colorB, float gravity)
    {
        var go = new GameObject(System.IO.Path.GetFileNameWithoutExtension(path));
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.duration = 0.5f;
        main.loop = false;
        main.playOnAwake = true;
        main.useUnscaledTime = true;
        main.stopAction = ParticleSystemStopAction.Destroy;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startLifetime = new ParticleSystem.MinMaxCurve(lifetime.x, lifetime.y);
        main.startSpeed = new ParticleSystem.MinMaxCurve(speed.x, speed.y);
        main.startSize = new ParticleSystem.MinMaxCurve(size.x, size.y);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.startColor = new ParticleSystem.MinMaxGradient(colorA, colorB);
        main.gravityModifier = gravity;
        main.maxParticles = count * 2;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.1f;

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        var fade = new Gradient();
        fade.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.6f), new GradientAlphaKey(0f, 1f) });
        colorOverLifetime.color = fade;

        var sheet = ps.textureSheetAnimation;
        sheet.enabled = true;
        sheet.mode = ParticleSystemAnimationMode.Sprites;
        sheet.AddSprite(sprite);

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.sharedMaterial = mat;
        renderer.sortingOrder = 60;

        PrefabUtility.SaveAsPrefabAsset(go, path);
        UnityEngine.Object.DestroyImmediate(go);
    }

    static void SetupAudioManager()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(AudioManagerPath);
        AudioManager manager = root.GetComponent<AudioManager>();

        // The prefab's existing looping source plays music; a second one plays effects.
        AudioSource music = root.GetComponent<AudioSource>();
        AudioSource sfx = root.GetComponents<AudioSource>().FirstOrDefault(s => s != music);
        if (sfx == null) sfx = root.AddComponent<AudioSource>();
        music.loop = true;
        music.playOnAwake = false;
        sfx.loop = false;
        sfx.playOnAwake = false;

        const string sounds = "Assets/ImportedAssets/brackeys_platformer_assets/sounds/";
        SetRef(manager, "musicSource", music);
        SetRef(manager, "sfxSource", sfx);
        SetRef(manager, "clip", Load<AudioClip>("Assets/ImportedAssets/Sheep.ogg"));
        SetRef(manager, "fireClip", Load<AudioClip>(sounds + "jump.wav"));
        SetRef(manager, "bounceClip", Load<AudioClip>(sounds + "tap.wav"));
        SetRef(manager, "starClip", Load<AudioClip>(sounds + "coin.wav"));
        SetRef(manager, "winClip", Load<AudioClip>(sounds + "power_up.wav"));
        SetRef(manager, "failClip", Load<AudioClip>(sounds + "hurt.wav"));
        SetRef(manager, "clickClip", Load<AudioClip>("Assets/ImportedAssets/UI Soundpack/WAV/Minimalist1.wav"));

        PrefabUtility.SaveAsPrefabAsset(root, AudioManagerPath);
        PrefabUtility.UnloadPrefabContents(root);
    }

    static void SetupLevelManager()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(LevelManagerPath);
        LevelEffects effects = root.GetComponent<LevelEffects>();
        if (effects == null) effects = root.AddComponent<LevelEffects>();
        SetRef(effects, "sparklePrefab", Load<GameObject>(SparklePath).GetComponent<ParticleSystem>());
        SetRef(effects, "puffPrefab", Load<GameObject>(PuffPath).GetComponent<ParticleSystem>());
        PrefabUtility.SaveAsPrefabAsset(root, LevelManagerPath);
        PrefabUtility.UnloadPrefabContents(root);
    }

    static void SetupCannon()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(CannonPath);
        TrajectoryPreview preview = root.GetComponent<TrajectoryPreview>();
        if (preview == null) preview = root.AddComponent<TrajectoryPreview>();
        SetRef(preview, "dotSprite", knobSprite);
        PrefabUtility.SaveAsPrefabAsset(root, CannonPath);
        PrefabUtility.UnloadPrefabContents(root);
    }

    // ---------- Options panel ----------

    static void SetupOptionsPanel()
    {
        GameObject canvasRoot = PrefabUtility.LoadPrefabContents(LevelCanvasPath);
        Transform pauseMenu = Find(canvasRoot.transform, "PauseMenu");
        Button sourceButton = Find(pauseMenu, "Restart").GetComponent<Button>();
        Image sourcePanel = pauseMenu.GetComponent<Image>();

        var panel = new GameObject("OptionsPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        var rect = (RectTransform)panel.transform;
        Stretch(rect);
        Image bg = panel.GetComponent<Image>();
        bg.sprite = sourcePanel.sprite;
        bg.type = sourcePanel.type;
        bg.color = sourcePanel.color;
        OptionsMenu options = panel.AddComponent<OptionsMenu>();

        AddText(rect, "Title", "Options", new Vector2(0f, 330f), new Vector2(900f, 140f), 110f, TextAlignmentOptions.Center);

        AddText(rect, "MusicLabel", "Music", new Vector2(-330f, 150f), new Vector2(360f, 90f), 64f, TextAlignmentOptions.Right);
        Slider music = AddSlider(rect, "MusicSlider", new Vector2(140f, 150f));
        AddText(rect, "SfxLabel", "Sound FX", new Vector2(-330f, 20f), new Vector2(360f, 90f), 64f, TextAlignmentOptions.Right);
        Slider sfx = AddSlider(rect, "SfxSlider", new Vector2(140f, 20f));
        SetRef(options, "musicSlider", music);
        SetRef(options, "sfxSlider", sfx);

        // Copies of an existing menu button keep the game's button style.
        Vector3 buttonScale = Vector3.Scale(sourceButton.transform.localScale, pauseMenu.localScale);
        buttonScale.z = 1f;
        Button reset = CloneButton(sourceButton, rect, "ResetProgress", "Reset Progress", new Vector2(0f, -170f), buttonScale, 140f);
        UnityEventTools.AddPersistentListener(reset.onClick, options.ResetProgress);
        Button back = CloneButton(sourceButton, rect, "Back", "Back", new Vector2(0f, -330f), buttonScale, 106.1f);
        UnityEventTools.AddPersistentListener(back.onClick, options.Close);

        panel.SetActive(false);
        PrefabUtility.SaveAsPrefabAsset(panel, OptionsPanelPath);
        UnityEngine.Object.DestroyImmediate(panel);
        PrefabUtility.UnloadPrefabContents(canvasRoot);
    }

    static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
    }

    static RectTransform Place(GameObject go, Transform parent, Vector2 position, Vector2 size)
    {
        var rect = (RectTransform)go.transform;
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        rect.localScale = Vector3.one;
        return rect;
    }

    static TextMeshProUGUI AddText(Transform parent, string name, string text, Vector2 position, Vector2 size,
        float fontSize, TextAlignmentOptions alignment)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        Place(go, parent, position, size);
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.font = font;
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = Color.white;
        tmp.raycastTarget = false;
        return tmp;
    }

    static Slider AddSlider(Transform parent, string name, Vector2 position)
    {
        var resources = new DefaultControls.Resources
        {
            standard = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd"),
            background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd"),
            knob = knobSprite,
        };
        GameObject go = DefaultControls.CreateSlider(resources);
        go.name = name;
        Place(go, parent, position, new Vector2(520f, 44f));

        Slider slider = go.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;

        Find(go.transform, "Fill Area/Fill").GetComponent<Image>().color = Candy;
        var handleArea = (RectTransform)Find(go.transform, "Handle Slide Area");
        handleArea.offsetMin = new Vector2(22f, 0f);
        handleArea.offsetMax = new Vector2(-22f, 0f);
        var handle = (RectTransform)Find(go.transform, "Handle Slide Area/Handle");
        handle.sizeDelta = new Vector2(44f, 22f);
        return slider;
    }

    static Button CloneButton(Button source, Transform parent, string name, string label, Vector2 position,
        Vector3 scale, float width)
    {
        GameObject go = UnityEngine.Object.Instantiate(source.gameObject, parent, false);
        go.name = name;
        var rect = (RectTransform)go.transform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(width, ((RectTransform)source.transform).sizeDelta.y);
        rect.localScale = scale;

        Button button = go.GetComponent<Button>();
        ClearListeners(button);
        var text = go.GetComponentInChildren<TextMeshProUGUI>();
        text.text = label;
        text.rectTransform.anchoredPosition = Vector2.zero;
        return button;
    }

    // ---------- Level canvas ----------

    static void SetupLevelCanvas()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(LevelCanvasPath);
        Transform canvas = root.transform;
        LevelUI ui = root.GetComponent<LevelUI>();

        // Options panel, opened from the pause and fail menus. Progress reset stays in the main menu.
        var optionsPanel = (GameObject)PrefabUtility.InstantiatePrefab(Load<GameObject>(OptionsPanelPath), canvas);
        optionsPanel.transform.SetAsLastSibling();
        optionsPanel.SetActive(false);
        Find(optionsPanel.transform, "ResetProgress").gameObject.SetActive(false);
        SetRef(ui, "optionsPanel", optionsPanel);
        foreach (string path in new[] { "PauseMenu/Options", "LevelFailMenu/Options" })
        {
            Button button = Find(canvas, path).GetComponent<Button>();
            ClearListeners(button);
            UnityEventTools.AddPersistentListener(button.onClick, ui.OpenOptions);
        }

        // Level title along the top edge.
        TextMeshProUGUI title = AddText(canvas, "LevelTitle", "Level 1", Vector2.zero, new Vector2(600f, 110f), 72f,
            TextAlignmentOptions.Center);
        title.rectTransform.anchorMin = title.rectTransform.anchorMax = new Vector2(0.5f, 1f);
        title.rectTransform.anchoredPosition = new Vector2(0f, -70f);
        title.transform.SetSiblingIndex(Find(canvas, "PauseButton").GetSiblingIndex() + 1);
        SetRef(ui, "levelTitle", title);

        // Result stars above "Level Passed!". The panel is scaled, so the row undoes that scale.
        Transform complete = Find(canvas, "LevelCompleteMenu");
        var row = new GameObject("ResultStars", typeof(RectTransform));
        RectTransform rowRect = Place(row, complete, new Vector2(0f, 560f), new Vector2(420f, 140f));
        rowRect.localScale = new Vector3(1f / complete.localScale.x, 1f / complete.localScale.y, 1f);
        var stars = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            var star = new GameObject("Star" + (i + 1), typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            Place(star, rowRect, new Vector2((i - 1) * 135f, i == 1 ? 18f : 0f), new Vector2(120f, 120f));
            Image image = star.GetComponent<Image>();
            image.sprite = starSprite;
            image.preserveAspect = true;
            image.raycastTarget = false;
            stars[i] = image;
        }
        SetRefArray(ui, "resultStars", stars);

        PrefabUtility.SaveAsPrefabAsset(root, LevelCanvasPath);
        PrefabUtility.UnloadPrefabContents(root);
    }

    // ---------- Scenes ----------

    static void SetupMainMenu()
    {
        var scene = EditorSceneManager.OpenScene(MainMenuScene, OpenSceneMode.Single);
        Transform canvas = scene.GetRootGameObjects().First(g => g.name == "Canvas").transform;

        ReplaceAudioManager(scene);

        var optionsPanel = (GameObject)PrefabUtility.InstantiatePrefab(Load<GameObject>(OptionsPanelPath), canvas);
        optionsPanel.transform.SetAsLastSibling();
        optionsPanel.SetActive(false);
        Button optionsButton = Find(canvas, "Main Menu/Options").GetComponent<Button>();
        ClearListeners(optionsButton);
        UnityEventTools.AddBoolPersistentListener(optionsButton.onClick, optionsPanel.SetActive, true);

        Transform levelSelect = Find(canvas, "LevelSelect");
        for (int level = 1; level <= 4; level++)
        {
            Transform button = Find(levelSelect, level.ToString());
            var rect = (RectTransform)button;
            float halfHeight = rect.sizeDelta.y * rect.localScale.y * 0.5f;

            var row = new GameObject("Stars " + level, typeof(RectTransform));
            Place(row, levelSelect, rect.anchoredPosition + new Vector2(0f, -halfHeight - 45f), new Vector2(220f, 70f));
            row.transform.SetSiblingIndex(button.GetSiblingIndex() + 1);
            var stars = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                var star = new GameObject("Star" + (i + 1), typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                Place(star, row.transform, new Vector2((i - 1) * 70f, 0f), new Vector2(62f, 62f));
                Image image = star.GetComponent<Image>();
                image.sprite = starSprite;
                image.preserveAspect = true;
                image.raycastTarget = false;
                stars[i] = image;
            }

            LevelSelectButton select = button.gameObject.AddComponent<LevelSelectButton>();
            var so = new SerializedObject(select);
            so.FindProperty("buildIndex").intValue = level;
            so.ApplyModifiedPropertiesWithoutUndo();
            SetRef(select, "label", button.GetComponentInChildren<TextMeshProUGUI>(true));
            SetRefArray(select, "stars", stars);
        }

        EditorSceneManager.SaveScene(scene);
    }

    static void SetupLevelScene(string path)
    {
        var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        ReplaceAudioManager(scene);
        EditorSceneManager.SaveScene(scene);
    }

    // Uses the AudioManager prefab in every scene, so audio works when a level is played directly.
    static void ReplaceAudioManager(UnityEngine.SceneManagement.Scene scene)
    {
        foreach (GameObject go in scene.GetRootGameObjects().Where(g => g.GetComponent<AudioManager>() != null))
            UnityEngine.Object.DestroyImmediate(go);

        var instance = (GameObject)PrefabUtility.InstantiatePrefab(Load<GameObject>(AudioManagerPath), scene);
        instance.name = "AudioManager";
    }
}
