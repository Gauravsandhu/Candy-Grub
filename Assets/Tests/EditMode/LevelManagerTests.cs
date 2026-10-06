using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class LevelManagerTests
{
    GameObject go;
    LevelManager level;
    Transform[] slots;

    [SetUp]
    public void SetUp()
    {
        go = new GameObject("LevelManager");
        level = go.AddComponent<LevelManager>();
        slots = new[] { new GameObject("Slot1").transform, new GameObject("Slot2").transform };
        Configure(starsToWin: 1);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(go);
        foreach (Transform slot in slots) Object.DestroyImmediate(slot.gameObject);
        Time.timeScale = 1f;
    }

    void Configure(int starsToWin)
    {
        var so = new SerializedObject(level);
        so.FindProperty("starsToWin").intValue = starsToWin;
        SerializedProperty slotsProp = so.FindProperty("starSlots");
        slotsProp.arraySize = slots.Length;
        for (int i = 0; i < slots.Length; i++)
            slotsProp.GetArrayElementAtIndex(i).objectReferenceValue = slots[i];
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    [Test]
    public void StartsAiming_AndCanLaunch()
    {
        Assert.AreEqual(LevelManager.State.Aiming, level.CurrentState);
        Assert.IsTrue(level.CanLaunch);
        Assert.IsFalse(level.IsLevelOver);
    }

    [Test]
    public void NotifyLaunched_MovesToFlying_AndBlocksAnotherLaunch()
    {
        level.NotifyLaunched();
        Assert.AreEqual(LevelManager.State.Flying, level.CurrentState);
        Assert.IsFalse(level.CanLaunch);
    }

    [Test]
    public void CollectStar_ReturnsSlotsInOrder_ThenNull()
    {
        Assert.AreSame(slots[0], level.CollectStar());
        Assert.AreSame(slots[1], level.CollectStar());
        Assert.IsNull(level.CollectStar());
        Assert.AreEqual(3, level.StarsCollected);
    }

    [Test]
    public void CollectStar_RaisesEventWithNewTotal()
    {
        int total = 0;
        level.StarCollected += n => total = n;
        level.CollectStar();
        level.CollectStar();
        Assert.AreEqual(2, total);
    }

    [Test]
    public void ReachBasket_WithEnoughStars_Wins_AndStopsTime()
    {
        int wonWith = -1;
        level.LevelWon += n => wonWith = n;
        level.CollectStar();
        level.CollectStar();
        level.ReachBasket();

        Assert.AreEqual(LevelManager.State.Won, level.CurrentState);
        Assert.AreEqual(2, wonWith);
        Assert.AreEqual(0f, Time.timeScale);
    }

    [Test]
    public void ReachBasket_WithoutEnoughStars_Loses()
    {
        Configure(starsToWin: 2);
        bool lost = false;
        level.LevelLost += () => lost = true;
        level.CollectStar();
        level.ReachBasket();

        Assert.AreEqual(LevelManager.State.Lost, level.CurrentState);
        Assert.IsTrue(lost);
    }

    [Test]
    public void LevelEndsOnlyOnce()
    {
        int wins = 0, losses = 0;
        level.LevelWon += _ => wins++;
        level.LevelLost += () => losses++;

        level.GrubLost();
        level.CollectStar();
        level.ReachBasket();
        level.GrubLost();

        Assert.AreEqual(LevelManager.State.Lost, level.CurrentState);
        Assert.AreEqual(0, wins);
        Assert.AreEqual(1, losses);
    }

    [Test]
    public void CollectStar_AfterLevelEnds_IsIgnored()
    {
        level.GrubLost();
        Assert.IsNull(level.CollectStar());
        Assert.AreEqual(0, level.StarsCollected);
    }

    [Test]
    public void Pause_StopsTime_AndBlocksLaunch()
    {
        int events = 0;
        level.PauseChanged += _ => events++;

        level.SetPaused(true);
        Assert.IsTrue(level.IsPaused);
        Assert.IsFalse(level.CanLaunch);
        Assert.AreEqual(0f, Time.timeScale);

        level.SetPaused(true);
        Assert.AreEqual(1, events, "pausing twice should only notify once");

        level.SetPaused(false);
        Assert.IsTrue(level.CanLaunch);
        Assert.AreEqual(1f, Time.timeScale);
        Assert.AreEqual(2, events);
    }

    [Test]
    public void Pause_IsIgnoredAfterLevelEnds()
    {
        level.GrubLost();
        level.SetPaused(true);
        Assert.IsFalse(level.IsPaused);
    }
}
