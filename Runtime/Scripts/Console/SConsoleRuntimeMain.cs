// 
// Copyright 2015 https://github.com/hope1026

using System;
using UnityEngine;

namespace SPlugin.RemoteConsole.Runtime
{
    /// <summary>
    /// Runtime Console Manager - Manages the entire runtime console system
    /// </summary>
    public class SConsoleRuntimeMain : MonoBehaviour
    {
        // Static fields
        private static SConsoleRuntimeMain _instance;

        private SConsoleUIController _uiController;
        private SConsoleApp _consoleApp;
        internal SConsoleApp ConsoleApp => _consoleApp;

        // Properties
        public static SConsoleRuntimeMain Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<SConsoleRuntimeMain>();
                    if (_instance == null)
                    {
                        GameObject managerObject = new GameObject("SConsoleRuntimeMain");
                        _instance = managerObject.AddComponent<SConsoleRuntimeMain>();
                        DontDestroyOnLoad(managerObject);
                    }
                }
                return _instance;
            }
        }

        private void Awake()
        {
            _consoleApp = new SConsoleApp();
            ConsolePrefs.ReadPrefs();
        }

        private void OnDestroy()
        {
            ConsolePrefs.WritePrefs();
            _consoleApp = null;

            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void Update()
        {
            if (_consoleApp != null)
            {
                _consoleApp.UpdateCustom();
            }

            if (_uiController != null)
            {
                _uiController.UpdateCustom();
            }
        }

        public void OpenConsole(int sortingOrder_)
        {
            _uiController = GetComponent<SConsoleUIController>();
            if (_uiController == null)
            {
                _uiController = gameObject.AddComponent<SConsoleUIController>();
            }

            _uiController.OnOpenConsole(this, sortingOrder_);
        }

        public void CloseConsole()
        {
            if (_uiController != null)
            {
                _uiController.OnCloseConsole();
                _uiController = null;
            }
        }
    }
}