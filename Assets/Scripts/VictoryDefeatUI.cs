using UnityEngine;
using UnityEngine.UI;

public class VictoryDefeatUI : MonoBehaviour
{
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject defeatPanel;
    [SerializeField] private Button restartButton;
    
    private GameManager _gameManager;
    
    private void Start()
    {
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (defeatPanel != null) defeatPanel.SetActive(false);
        if(restartButton != null) restartButton.gameObject.SetActive(false);
        
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartClicked);
        }
        
        if (GameManager.Instance != null)
        {
            _gameManager = GameManager.Instance;
            _gameManager.OnVictory += ShowVictory;
            _gameManager.OnDefeat += ShowDefeat;
        }
    }
    
    private void ShowVictory()
    {
        if (victoryPanel != null && restartButton != null)
        {
            victoryPanel.SetActive(true);
            restartButton.gameObject.SetActive(true);
        }
        
        if (defeatPanel != null)
        {
            defeatPanel.SetActive(false);
        }
    }
    
    private void ShowDefeat()
    {
        if (defeatPanel != null && restartButton != null)
        {
            defeatPanel.SetActive(true);
            restartButton.gameObject.SetActive(true);
        }
        
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }
    }
    
    private void OnRestartClicked()
    {
        if (_gameManager != null)
        {
            _gameManager.RestartGame();
        }
    }
    
    private void OnDestroy()
    {
        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(OnRestartClicked);
        }
        
        if (_gameManager != null)
        {
            _gameManager.OnVictory -= ShowVictory;
            _gameManager.OnDefeat -= ShowDefeat;
        }
    }
}