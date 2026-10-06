using NUnit.Framework;
using UnityEngine;

// Uses build indices far outside the real levels so the editor's saved progress is untouched.
public class SaveSystemTests
{
    const int A = 901, B = 902;

    [TearDown]
    public void TearDown()
    {
        foreach (int i in new[] { A, B, -5 })
            PlayerPrefs.DeleteKey("level_stars_" + i);
    }

    [Test]
    public void UnplayedLevel_IsNotCompleted_AndHasNoStars()
    {
        Assert.IsFalse(SaveSystem.IsCompleted(A));
        Assert.AreEqual(0, SaveSystem.GetBestStars(A));
    }

    [Test]
    public void FirstLevel_IsAlwaysUnlocked()
    {
        Assert.IsTrue(SaveSystem.IsUnlocked(1));
    }

    [Test]
    public void Level_UnlocksWhenPreviousLevelIsCompleted()
    {
        Assert.IsFalse(SaveSystem.IsUnlocked(B));
        SaveSystem.RecordLevelComplete(A, 1);
        Assert.IsTrue(SaveSystem.IsUnlocked(B));
    }

    [Test]
    public void Record_KeepsTheBestStarCount()
    {
        SaveSystem.RecordLevelComplete(A, 2);
        SaveSystem.RecordLevelComplete(A, 1);
        Assert.AreEqual(2, SaveSystem.GetBestStars(A));

        SaveSystem.RecordLevelComplete(A, 3);
        Assert.AreEqual(3, SaveSystem.GetBestStars(A));
    }

    [Test]
    public void Record_WithZeroStars_StillCountsAsCompleted()
    {
        SaveSystem.RecordLevelComplete(A, 0);
        Assert.IsTrue(SaveSystem.IsCompleted(A));
    }

    [Test]
    public void Record_IgnoresIndicesThatAreNotLevels()
    {
        SaveSystem.RecordLevelComplete(-5, 3);
        Assert.IsFalse(SaveSystem.IsCompleted(-5));
    }
}
