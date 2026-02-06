using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Playing,
    Victory,
    Defeat
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Title("Game Settings")]
    [SerializeField] private float gameDuration = 300f;
    
    [SerializeField] private bool pauseOnEnd = true;
    
    [Title("Game State")]
    [ShowInInspector, ReadOnly]
    private GameState _currentState = GameState.Playing;
    
    [ShowInInspector, ReadOnly]
    private float _gameTimer;
    
    public GameState CurrentState => _currentState;
    public float GameTimer => _gameTimer;
    public float GameDuration => gameDuration;
    
    public event Action OnVictory;
    public event Action OnDefeat;
    public event Action<float> OnTimerChanged;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
    }
    
    private void Start()
    {
        _gameTimer = gameDuration;
        _currentState = GameState.Playing;
    }
    
    private void Update()
    {
        if (_currentState != GameState.Playing) return;
        
        _gameTimer -= Time.deltaTime;
        
        OnTimerChanged?.Invoke(_gameTimer);
        
        if (_gameTimer <= 0f)
        {
            _gameTimer = 0f;
            TriggerVictory();
        }
    }
    
    public void TriggerVictory()
    {
        if (_currentState != GameState.Playing) return;
        
        _currentState = GameState.Victory;
        
        Debug.Log("VICTORY");
        
        if (pauseOnEnd)
        {
            Time.timeScale = 0f;
        }
        
        OnVictory?.Invoke();
    }
    
    public void TriggerDefeat()
    {
        if (_currentState != GameState.Playing) return;
        
        _currentState = GameState.Defeat;
        
        Debug.Log("DEFEAT");
        
        if (pauseOnEnd)
        {
            Time.timeScale = 0f;
        }
        
        OnDefeat?.Invoke();
    }
    
    [Button("Trigger Victory (Debug)")]
    public void DebugVictory()
    {
        TriggerVictory();
    }
    
    [Button("Trigger Defeat (Debug)")]
    public void DebugDefeat()
    {
        TriggerDefeat();
    }
    
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
        
        Time.timeScale = 1f;
    }
}