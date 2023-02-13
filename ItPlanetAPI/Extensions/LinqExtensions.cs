namespace ItPlanetAPI.Extensions;

public static class LinqExtensions
{
    public static (T? Previous, T Current, T? Next)? FindWithNextAndPrevious<T>
        (this IEnumerable<T> source, Predicate<T> filteringFunction)
    {
        using var iterator = source.GetEnumerator();
        if (!iterator.MoveNext()) return null;

        var previous = iterator.Current;
        if (filteringFunction(previous))
        {
            var current = previous;
            var next = iterator.MoveNext() ? iterator.Current : default;
            return (default, current, next);
        }

        while (iterator.MoveNext())
        {
            var current = iterator.Current;
            if (filteringFunction(current))
            {
                var next = iterator.MoveNext() ? iterator.Current : default;
                return (previous, current, next);
            }

            previous = current;
        }

        return null;
    }

    public static IEnumerable<T> NotNull<T>(this IEnumerable<T?> enumerable) where T : struct
    {
        return enumerable.Where(e => e != null).Select(e => e!.Value);
    }

    public static IEnumerable<T> NotNull<T>(this IEnumerable<T?> enumerable) where T : class
    {
        return enumerable.Where(e => e != null).Select(e => e!);
    }

    public static bool IsEmpty<T>(this IEnumerable<T?> enumerable)
    {
        return !enumerable.Any();
    }
}