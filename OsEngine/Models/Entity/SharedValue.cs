namespace OsEngine.Models.Entity;

public class SharedValue<T>(T value)
{
    public T Value = value;

    public static implicit operator T(SharedValue<T> sv) => sv.Value;
    public static implicit operator SharedValue<T>(T v) => new(v);
}

