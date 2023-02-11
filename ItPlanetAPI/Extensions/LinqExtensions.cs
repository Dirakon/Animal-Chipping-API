using LanguageExt;

namespace ItPlanetAPI.Extensions;

public static class LinqExtensions
{
    public static Option<(Option<T>, T, Option<T>)> FindWithNextAndPrevious<T>
        (this IEnumerable<T> source, Predicate<T> filteringFunction)
    {
        using IEnumerator<T> iterator = source.GetEnumerator();
        if (!iterator.MoveNext())
        {
            return Option<(Option<T>, T, Option<T>)>.None;
        }

        T previous = iterator.Current;
        if (filteringFunction(previous))
        {
            T current = previous;
            Option<T> next = iterator.MoveNext() ? iterator.Current : Option<T>.None;
            return (default, current, next);
        }
        while (iterator.MoveNext())
        {
            var current = iterator.Current;
            if (filteringFunction(current))
            {
                Option<T> next = iterator.MoveNext() ? iterator.Current : Option<T>.None;
                return (previous, current, next);
            }

            previous = current;
        }

        return Option<(Option<T>, T, Option<T>)>.None;;
    }

    public static bool IsEmpty<T>(this IEnumerable<T> source) => !source.Any();
}