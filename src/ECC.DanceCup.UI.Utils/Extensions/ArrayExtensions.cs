namespace ECC.DanceCup.UI.Utils.Extensions;

public static class ArrayExtensions
{
    public static T Random<T>(this T[] source)
    {
        var randomIndex = System.Random.Shared.Next(source.Length);
        return source[randomIndex];
    }
}