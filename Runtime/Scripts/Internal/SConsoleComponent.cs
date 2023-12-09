// 
// Copyright 2015 https://github.com/hope1026

using UnityEngine;
using System;

internal class SConsoleComponent : MonoBehaviour
{
    internal Action DelegateOnDestroy { get; set; }
    internal Action DelegateOnUpdate { get; set; }

    void Update()
    {
        DelegateOnUpdate?.Invoke();
    }

    void OnDestroy()
    {
        DelegateOnDestroy?.Invoke();
        GameObject.Destroy(base.gameObject);
    }
}