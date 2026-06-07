using System;

namespace PuntoVenta.Blazor.Services;

public class UserModeService
{
    public event Action<string>? OnShortcutPressed;

    public void TriggerShortcut(string key)
    {
        OnShortcutPressed?.Invoke(key);
    }
}
