using System;
using UnityEngine;
using UnityEngine.SceneManagement;

// Owns the state of a single level. Gameplay objects report to it; UI listens to its events.
public class LevelManager : MonoBehaviour
{
    public enum State { Aiming, Flying, Won, Lost }

    public static LevelManager Instance { get; private set; }

    [SerializeField] private Transform[] starSlots;
    [SerializeField] private int starsToWin = 1;

    public State CurrentState { get; private set; } = State.Aiming;
    public bool IsPaused { get; private set; }
    public int StarsCollected { get; private set; }
    public int StarsToWin => starsToWin;
    public bool IsLevelOver => CurrentState == State.Won || CurrentState == State.Lost;
    public bool CanLaunch => CurrentState == State.Aiming && !IsPaused;

    public event Action<int> StarCollected;   // new star total
    public event Action<int> LevelWon;        // stars collected
    public event Action LevelLost;
    public event Action<bool> PauseChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("More than one LevelManager in the scene.", this);
            Destroy(this);
            return;
        }

        Instance = this;
        Time.timeScale = 1f;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void NotifyLaunched()
    {
        if (CurrentState == State.Aiming) CurrentState = State.Flying;
    }

    // Returns the slot the collected star should fly to, or null if there is none.
    public Transform CollectStar()
    {
        if (IsLevelOver) return null;

        Transform slot = starSlots != null && StarsCollected < starSlots.Length ? starSlots[StarsCollected] : null;
        StarsCollected++;
        StarCollected?.Invoke(StarsCollected);
        return slot;
    }

    public void ReachBasket()
    {
        if (StarsCollected >= starsToWin) Win();
        else Lose();
    }

    public void GrubLost()
    {
        Lose();
    }

    public void SetPaused(bool paused)
    {
        if (IsLevelOver || IsPaused == paused) return;

        IsPaused = paused;
        Time.timeScale = paused ? 0f : 1f;
        PauseChanged?.Invoke(paused);
    }

    void Win()
    {
        if (IsLevelOver) return;

        CurrentState = State.Won;
        Time.timeScale = 0f;
        SaveSystem.RecordLevelComplete(SceneManager.GetActiveScene().buildIndex, StarsCollected);
        LevelWon?.Invoke(StarsCollected);
    }

    void Lose()
    {
        if (IsLevelOver) return;

        CurrentState = State.Lost;
        Time.timeScale = 0f;
        LevelLost?.Invoke();
    }
}
