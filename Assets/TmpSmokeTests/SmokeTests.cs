using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

// Game code lives in Assembly-CSharp, which test assemblies can't reference, so this uses reflection.
public class SmokeTests
{
    const string ShotDir = "/private/tmp/claude-501/-Users-gauravsandhu-Unity-Projects-Systems-Candy-Grub/66acb62d-07d9-4f2c-99df-c239c6c9f6e2/scratchpad/shots";

    static Type T(string name) => AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(name)).First(t => t != null);
    static object Manager() => T("LevelManager").GetProperty("Instance").GetValue(null);
    static object Get(object o, string prop) => o.GetType().GetProperty(prop).GetValue(o);
    static object Call(object o, string method, params object[] args) =>
        o.GetType().GetMethod(method, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Invoke(o, args);
    static object CallStatic(string type, string method, params object[] args) =>
        T(type).GetMethod(method, BindingFlags.Public | BindingFlags.Static).Invoke(null, args);
    static GameObject Obj(string name) => Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(g => g.scene.IsValid() && g.name == name && g.activeInHierarchy);
    static bool Active(string name) => Obj(name) != null;

    // Tests touch PlayerPrefs; put back whatever the editor had before.
    static readonly string[] Keys = { "level_stars_1", "level_stars_2", "level_stars_3", "level_stars_4", "music_volume", "sfx_volume" };
    System.Collections.Generic.Dictionary<string, float> saved = new System.Collections.Generic.Dictionary<string, float>();
    System.Collections.Generic.Dictionary<string, int> savedInt = new System.Collections.Generic.Dictionary<string, int>();

    [OneTimeSetUp]
    public void Backup()
    {
        foreach (var k in Keys.Where(PlayerPrefs.HasKey))
            if (k.EndsWith("volume")) saved[k] = PlayerPrefs.GetFloat(k); else savedInt[k] = PlayerPrefs.GetInt(k);
    }

    [OneTimeTearDown]
    public void Restore()
    {
        foreach (var k in Keys) PlayerPrefs.DeleteKey(k);
        foreach (var kv in saved) PlayerPrefs.SetFloat(kv.Key, kv.Value);
        foreach (var kv in savedInt) PlayerPrefs.SetInt(kv.Key, kv.Value);
        PlayerPrefs.Save();
    }

    IEnumerator Load(int index)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(index);
        for (int i = 0; i < 5; i++) yield return null;
    }

    IEnumerator WaitRealtime(float s) { float t = 0; while (t < s) { t += Time.unscaledDeltaTime; yield return null; } }

    // Renders the camera plus every overlay canvas (temporarily switched to camera space) into a PNG.
    static void Shot(string name)
    {
        Directory.CreateDirectory(ShotDir);
        Camera cam = Camera.main;
        var canvases = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Where(c => c.isRootCanvas && c.renderMode == RenderMode.ScreenSpaceOverlay).ToArray();
        foreach (var c in canvases) { c.renderMode = RenderMode.ScreenSpaceCamera; c.worldCamera = cam; c.planeDistance = 1f; }
        var rt = new RenderTexture(1280, 720, 24);
        cam.targetTexture = rt;
        Canvas.ForceUpdateCanvases();
        cam.Render();
        RenderTexture.active = rt;
        var tex = new Texture2D(1280, 720, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
        tex.Apply();
        File.WriteAllBytes(Path.Combine(ShotDir, name + ".png"), tex.EncodeToPNG());
        cam.targetTexture = null; RenderTexture.active = null;
        foreach (var c in canvases) c.renderMode = RenderMode.ScreenSpaceOverlay;
    }

    [UnityTest]
    public IEnumerator EveryScene_LoadsWithoutErrors([Values(0, 1, 2, 3, 4)] int index)
    {
        yield return Load(index);
        for (int i = 0; i < 30; i++) yield return null;
        Assert.IsNotNull(Obj("AudioManager"), "AudioManager missing");
        if (index > 0)
        {
            Assert.IsNotNull(Manager(), "LevelManager missing");
            Assert.IsNotNull(T("LevelEffects").GetProperty("Instance").GetValue(null), "LevelEffects missing");
            Assert.IsFalse(Active("PauseMenu") || Active("LevelCompleteMenu") || Active("LevelFailMenu") || Active("OptionsPanel"), "a panel is open at start");
            Assert.IsTrue(Active("PauseButton"));
            Assert.IsTrue(Active("TrajectoryDots"), "trajectory dots hidden while aiming");
            Assert.AreEqual("Level " + index, Obj("LevelTitle").GetComponent<TMPro.TMP_Text>().text);
            Shot("level" + index + "_aim");
        }
    }

    [UnityTest]
    public IEnumerator PauseOptionsAndResume([Values(1, 4)] int index)
    {
        yield return Load(index);
        var ui = UnityEngine.Object.FindAnyObjectByType(T("LevelUI"));
        Call(ui, "Pause");
        Assert.AreEqual(0f, Time.timeScale);
        Assert.IsTrue(Active("PauseMenu"));
        Assert.IsFalse(Active("TrajectoryDots"));
        Obj("PauseMenu").transform.Find("Options").GetComponent<Button>().onClick.Invoke();
        yield return null;
        Assert.IsTrue(Active("OptionsPanel"));
        Assert.IsFalse(Active("ResetProgress"), "reset button should be hidden in levels");
        if (index == 1) Shot("level_options");
        Call(ui, "Resume");
        Assert.AreEqual(1f, Time.timeScale);
        Assert.IsFalse(Active("PauseMenu") || Active("OptionsPanel"));
    }

    [UnityTest]
    public IEnumerator Volume_PersistsAndApplies()
    {
        yield return Load(0);
        var audio = T("AudioManager").GetField("instance").GetValue(null);
        Assert.IsNotNull(audio);
        Obj("Main Menu").transform.Find("Options").GetComponent<Button>().onClick.Invoke();
        yield return null;
        var panel = Obj("OptionsPanel");
        Assert.IsNotNull(panel, "options did not open");
        Assert.IsTrue(Active("ResetProgress"));
        Shot("menu_options");
        panel.transform.Find("MusicSlider").GetComponent<Slider>().value = 0.3f;
        panel.transform.Find("SfxSlider").GetComponent<Slider>().value = 0.6f;
        Assert.AreEqual(0.3f, (float)Get(audio, "MusicVolume"), 1e-4);
        Assert.AreEqual(0.3f, PlayerPrefs.GetFloat("music_volume"), 1e-4);
        Assert.AreEqual(0.6f, PlayerPrefs.GetFloat("sfx_volume"), 1e-4);
        var sources = ((Component)audio).GetComponents<AudioSource>();
        Assert.IsTrue(sources.Any(s => s.loop && s.isPlaying && Mathf.Approximately(s.volume, 0.3f)), "music source not playing at new volume");
        panel.transform.Find("Back").GetComponent<Button>().onClick.Invoke();
        Assert.IsFalse(panel.activeSelf);
        T("AudioManager").GetProperty("MusicVolume").SetValue(audio, 1f);
        T("AudioManager").GetProperty("SfxVolume").SetValue(audio, 1f);
    }

    [UnityTest]
    public IEnumerator LevelSelect_ShowsLocksAndBestStars()
    {
        CallStatic("SaveSystem", "ResetProgress", 4);
        CallStatic("SaveSystem", "RecordLevelComplete", 1, 3);
        CallStatic("SaveSystem", "RecordLevelComplete", 2, 1);
        yield return Load(0);
        Obj("Main Menu").transform.parent.Find("LevelSelect").gameObject.SetActive(true);
        yield return null;
        Assert.IsTrue(Obj("1").GetComponent<Button>().interactable);
        Assert.IsTrue(Obj("2").GetComponent<Button>().interactable);
        Assert.IsTrue(Obj("3").GetComponent<Button>().interactable);
        Assert.IsFalse(Obj("4").GetComponent<Button>().interactable);
        Assert.IsFalse(Active("Stars 4") && Obj("Stars 4").transform.GetChild(0).gameObject.activeSelf);
        Shot("menu_levelselect");
        CallStatic("SaveSystem", "ResetProgress", 4);
    }

    [UnityTest]
    public IEnumerator BasketWithoutStars_Fails_ThenWinCannotOverride()
    {
        yield return Load(1);
        var m = Manager();
        Call(m, "ReachBasket");
        Assert.AreEqual("Lost", Get(m, "CurrentState").ToString());
        Assert.IsTrue(Active("LevelFailMenu"));
        Call(m, "CollectStar");
        Call(m, "ReachBasket");
        Assert.AreEqual("Lost", Get(m, "CurrentState").ToString());
        Assert.IsFalse(Active("LevelCompleteMenu"));
        Shot("level_fail");
    }

    [UnityTest]
    public IEnumerator BasketWithStars_Wins_RevealsStars_AndSaves()
    {
        PlayerPrefs.DeleteKey("level_stars_2");
        yield return Load(2);
        var m = Manager();
        Call(m, "CollectStar");
        Call(m, "CollectStar");
        Call(m, "ReachBasket");
        Assert.AreEqual("Won", Get(m, "CurrentState").ToString());
        Assert.IsTrue(Active("LevelCompleteMenu"));
        Assert.AreEqual(2, PlayerPrefs.GetInt("level_stars_2", -1));
        yield return WaitRealtime(1.6f);
        var stars = Obj("ResultStars").GetComponentsInChildren<Image>();
        Assert.AreEqual(3, stars.Length);
        Assert.AreEqual(1f, stars[0].color.a, 1e-3); Assert.AreEqual(1f, stars[1].color.a, 1e-3);
        Assert.Less(stars[2].color.a, 0.5f);
        Assert.AreEqual(Vector3.one, stars[2].transform.localScale);
        Shot("level_win_2stars");
        PlayerPrefs.DeleteKey("level_stars_2");
    }

    // Fires a real grub and lets physics run until the level ends.
    [UnityTest]
    public IEnumerator FiredGrub_EventuallyEndsLevel([Values(1, 2, 3, 4)] int index)
    {
        yield return Load(index);
        var spawner = UnityEngine.Object.FindAnyObjectByType(T("SpawnGrub"));
        Call(spawner, "Launch", Manager());
        Assert.AreEqual("Flying", Get(Manager(), "CurrentState").ToString());
        Assert.IsFalse(Active("TrajectoryDots"));
        for (int i = 0; i < 3; i++) yield return null;
        if (index == 3) Shot("level3_fired");

        float t = 0f;
        while (!(bool)Get(Manager(), "IsLevelOver") && t < 30f) { t += Time.unscaledDeltaTime; yield return null; }
        string state = Get(Manager(), "CurrentState").ToString();
        Debug.Log($"Level {index}: grub ended as {state} after {t:F1}s with {Get(Manager(), "StarsCollected")} stars");
        Assert.IsTrue((bool)Get(Manager(), "IsLevelOver"), "level never ended");
        Assert.IsTrue(Active(state == "Won" ? "LevelCompleteMenu" : "LevelFailMenu"));
        PlayerPrefs.DeleteKey("level_stars_" + index);
    }
}
