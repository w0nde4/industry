using TMPro;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI metalText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private ResourceData metalResourceData;

    private PlayerResourceManager _resourceManager;
    private GameManager _gameManager;

    private void Start()
    {
        UpdateMetalDisplay();
        UpdateTimerDisplay();

        if (PlayerResourceManager.Instance != null)
        {
            _resourceManager = PlayerResourceManager.Instance;
            _resourceManager.OnResourceChanged += OnResourceChanged;
        }

        if (GameManager.Instance != null)
        {
            _gameManager = GameManager.Instance;
            _gameManager.OnTimerChanged += OnTimerChanged;
        }
    }

    private void OnResourceChanged(ResourceData data, int amount)
    {
        if (data == metalResourceData)
        {
            UpdateMetalDisplay();
        }
    }
    
    private void OnTimerChanged(float time)
    {
        UpdateTimerDisplay();
    }
    
    private void UpdateMetalDisplay()
    {
        if (metalText == null || metalResourceData == null) return;
        
        var metalAmount = 0;
        
        if (_resourceManager != null)
        {
            metalAmount = _resourceManager.GetNetAmount(metalResourceData);
        }
        
        metalText.text = $"Metal: {metalAmount}";
    }
    
    private void UpdateTimerDisplay()
    {
        if (timerText == null) return;
        
        var time = 0f;
        
        if (_gameManager != null)
        {
            time = _gameManager.GameTimer;
        }
        
        timerText.text = $"Time: {FormatTime(time)}";
    }
    
    private string FormatTime(float timeInSeconds)
    {
        var minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        var seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        
        return $"{minutes:00}:{seconds:00}";
    }

    private void UnsubscribeFromEvents()
    {
        if (_resourceManager != null)
            _resourceManager.OnResourceChanged -= OnResourceChanged;
    
        if (_gameManager != null)
            _gameManager.OnTimerChanged -= OnTimerChanged;
    }
    
    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
}