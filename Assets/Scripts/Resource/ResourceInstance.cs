using System;
using TMPro;
using UnityEngine;

public class ResourceInstance : MonoBehaviour
{
    [SerializeField] private ResourceData data;
    [SerializeField] private int amount = 1;
    
    private SpriteRenderer _spriteRenderer;
    private TextMeshPro _amountText;
    
    public ResourceData Data => data;
    public int Amount => amount;
    
    public event Action<ResourceInstance> OnResourceDepleted;
    
    public void Initialize(ResourceData data, int amount = 1)
    {
        this.data = data;
        this.amount = Mathf.Clamp(amount, 1, data.maxStack);
        
        SetupVisuals();
        UpdateVisuals();

        var pos = transform.position;
        pos.z = -1f; //ИСПРАВИТЬ ПОСЛЕ НА ORDER IN LAYER
        transform.position = pos; 
    }
    
    private void SetupVisuals()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer == null)
        {
            _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        var textObj = transform.Find("AmountText");
        if (textObj == null)
        {
            textObj = new GameObject("AmountText").transform;
            textObj.SetParent(transform);
            textObj.localPosition = new Vector3(0, 0.3f, 0);
            textObj.localScale = Vector3.one;
        }
        
        _amountText = textObj.GetComponent<TextMeshPro>();
        if (_amountText == null)
        {
            _amountText = textObj.gameObject.AddComponent<TextMeshPro>();
            _amountText.alignment = TextAlignmentOptions.Center;
            _amountText.fontSize = 8;
            _amountText.color = Color.white;
            _amountText.sortingOrder = 10;
        }
    }
    
    private void UpdateVisuals()
    {
        if (data == null) return;
        
        if (_spriteRenderer != null && data.sprite != null)
        {
            _spriteRenderer.sprite = data.sprite;
            _spriteRenderer.color = data.resourceColor;
        }
        
        if (_amountText != null)
        {
            _amountText.text = amount > 1 ? amount.ToString() : "";
        }
    }
    
    public bool CanStack(ResourceData otherData)
    {
        return data == otherData && amount < data.maxStack;
    }
    
    public int AddToStack(int addAmount)
    {
        if (data == null) return addAmount;
        
        var availableSpace = data.maxStack - amount;
        var amountToAdd = Mathf.Min(addAmount, availableSpace);
        
        amount += amountToAdd;
        UpdateVisuals();
        
        return addAmount - amountToAdd;
    }
    
    public int RemoveFromStack(int removeAmount)
    {
        var amountToRemove = Mathf.Min(removeAmount, amount);
        amount -= amountToRemove;
        
        UpdateVisuals();
        
        if (amount <= 0)
        {
            OnResourceDepleted?.Invoke(this);
        }
        
        return amountToRemove;
    }
    
    public void ResetForPool()
    {
        data = null;
        amount = 1;
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        
        if (_amountText != null)
        {
            _amountText.text = "";
        }

        OnResourceDepleted = null;
    }
}