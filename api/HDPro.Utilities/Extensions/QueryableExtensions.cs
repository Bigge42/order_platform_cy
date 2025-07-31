using System;
using System.Linq;
using System.Linq.Expressions;

namespace HDPro.Utilities.Extensions
{
    public static class QueryableExtensions
    {
         /// <summary>
        /// Filters a sequence of values based on a predicate on when the <paramref name="condition"/> is <c>true</c>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">An <see cref="IQueryable{T}"/> to filter.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="condition">A <see cref="Boolean"/> to determine whether the <paramref name="predicate"/> can be applied.</param>
        /// <returns>An <see cref="IQueryable{T}"/> that contains elements from the input sequence that satisfy the predicate condition.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source"/> or <paramref name="predicate"/> is <see langword="null"/>.
        /// </exception>
        public static IQueryable<TSource> WhereIf<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, bool condition)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            return condition ? source.Where(predicate) : source;
        }

        /// <summary>
        /// Filters a sequence of values based on a predicate on when the <paramref name="condition"/> is <c>true</c>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">An <see cref="IQueryable{T}"/> to filter.</param>
        /// <param name="predicate">
        /// A function to test each source element for a condition; 
        /// the second parameter of the function represents the index of the source element.
        /// </param>
        /// <param name="condition">A <see cref="Boolean"/> to determine whether the <paramref name="predicate"/> can be applied.</param>
        /// <returns>An <see cref="IQueryable{T}"/> that contains elements from the input sequence that satisfy the predicate condition.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source"/> or <paramref name="predicate"/> is <see langword="null"/>.
        /// </exception>
        public static IQueryable<TSource> WhereIf<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, int, bool>> predicate, bool condition)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            return condition ? source.Where(predicate) : source;
        }

        /// <summary>
        /// Paginate the elements in the source.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">An <see cref="IQueryable{TSource}"/> to paginate.</param>
        /// <param name="pageIndex">The zero-based index at which page the paginate starts.</param>
        /// <param name="pageSize">The number of elements in the page.</param>
        /// <returns>An <see cref="IQueryable{T}"/> that contains elements from the input sequence that paginated by using the <paramref name="pageIndex"/> and <paramref name="pageSize"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="pageIndex"/> or <paramref name="pageSize"/> is negative number.
        /// </exception>
        public static IQueryable<TSource> Paginate<TSource>(this IQueryable<TSource> source, int pageIndex, int pageSize)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (pageIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageIndex), "Non negative number is required.");
            }
            if (pageSize < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Non negative number is required.");
            }
            var query = source;
            if (pageIndex > 0 && pageSize != int.MaxValue && pageSize > 0)
            {
                query = query.Skip(pageIndex * pageSize);
            }
            if (pageSize != int.MaxValue)
            {
                query = query.Take(pageSize);
            }
            return query;
        }
   }
}
