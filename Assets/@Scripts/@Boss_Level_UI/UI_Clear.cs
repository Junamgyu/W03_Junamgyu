using System;
using UnityEngine;
using UnityEngine.UI;

public class UI_Clear : MonoBehaviour
{
    [SerializeField] private Button _mainMenuButton;

    public event Action OnMainMenuRequested;

    private void Awake()
    {
        _mainMenuButton?.onClick.AddListener(() => OnMainMenuRequested?.Invoke());
    }

    private void OnDestroy()
    {
        _mainMenuButton?.onClick.RemoveAllListeners();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
