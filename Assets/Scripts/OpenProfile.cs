using System;

public static class OpenProfile
{
    public static event Action Raised;

    public static void Raise()
    {
        Raised?.Invoke();
    }
}
