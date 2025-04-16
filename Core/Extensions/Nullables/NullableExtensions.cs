namespace UtilClasses.Core.Extensions.Nullables;

public static class NullableExtensions
{
    public static bool TryGet<T>(this T? nullable, out T val) where T:struct
    {
        val = default;
        if (null == nullable) return false;
        val = nullable.Value;
        return true;
    }
}