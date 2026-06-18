using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lightweight singleton inventory. Items are identified by a string key set in the Inspector
/// on each <see cref="InventoryItem"/>. No persistence yet — resets on scene load.
/// </summary>
[DefaultExecutionOrder(-50)]
public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }

    /// <summary>Fired with the item key when an item is added.</summary>
    public event Action<string> OnItemAdded;

    /// <summary>Fired with the item key when an item is removed.</summary>
    public event Action<string> OnItemRemoved;

    private readonly HashSet<string> _items = new();

    private void Awake()
    {
        Instance = this;
    }

    public void Add(string key)
    {
        if (string.IsNullOrEmpty(key)) return;
        _items.Add(key);
        OnItemAdded?.Invoke(key);
    }

    public bool Has(string key) =>
        !string.IsNullOrEmpty(key) && _items.Contains(key);

    public void Remove(string key)
    {
        if (!_items.Remove(key)) return;
        OnItemRemoved?.Invoke(key);
    }
}
