using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

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

    public static async Task<IEnumerable<TResult>> SelectAsync<TSource, TResult>(
        this IEnumerable<TSource> source, Func<TSource, Task<TResult>> method)
    {
        return await Task.WhenAll(source.Select(async s => await method(s)));
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

    public static async Task<bool> IsEmptyAsync<T>(this IQueryable<T?> enumerable)
    {
        return !await enumerable.AnyAsync();
    }

    public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expression1,
        Expression<Func<T, bool>> expression2)
    {
        var invokedExpression = Expression.Invoke(expression2, expression1.Parameters);

        return Expression.Lambda<Func<T, bool>>(Expression.And(expression1.Body, invokedExpression),
            expression1.Parameters);
    }

    public static bool HasDuplicates<T>(this IEnumerable<T> enumerable)
    {
        return enumerable.GroupBy(typeId => typeId).Any(g => g.Count() > 1);
    }
}