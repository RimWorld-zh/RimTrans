// There is List.ForEach method in official library, so I named the method "EachFor".

using System;
using System.Collections.Generic;

namespace duduluu.System.Linq {
    public static partial class Enumerable_Extension {
        /// <summary cref="Enumerable_Extension.EachFor{TSource}(IEnumerable{TSource}, Action{TSource, int})">
        /// Performs the specified action on each element of the <see cref="IEnumerable{T}"/>.
        /// 对 <see cref="IEnumerable{T}"/> 的每个元素应用指定的操作。
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements of source.
        /// Source 的元素的类型。
        /// </typeparam>
        /// <param name="source">
        /// A sequence of object to perform the specified action on.
        /// 应用指定操作的一系列对象。
        /// </param>
        /// <param name="action">
        /// <code>(TSource item) => any</code>
        /// The <see cref="Action{T}"/> delegate to perform on each element of the <see cref="IEnumerable{T}"/>.
        /// 应用到 <see cref="IEnumerable{T}"/> 的每个元素的 <see cref="Action{T}"/> 委托。
        /// </param>
        /// <returns>Source</returns>
        public static IEnumerable<TSource> EachFor<TSource>(
            this IEnumerable<TSource> source, Action<TSource> action) {

            if (source == null) {
                throw Errors.ArgumentNull(nameof(source));
            }

            if (action == null) {
                throw Errors.ArgumentNull(nameof(action));
            }

            foreach (TSource element in source) {
                action(element);
            }

            return source;
        }

        /// <summary>
        /// The edition with the index parameter for <see cref="Enumerable_Extension.EachFor{TSource}(IEnumerable{TSource}, Action{TSource})"/>
        /// </summary>
        /// <param name="action">
        /// <code>(TSource item, int index) => any</code>
        /// The <see cref="Action{T, int}"/> delegate to perform on each element of the <see cref="IEnumerable{T}"/>.
        /// 应用到 <see cref="IEnumerable{T}"/> 的每个元素的 <see cref="Action{T}"/> 委托。
        /// </param>
        public static IEnumerable<TSource> EachFor<TSource>(
            this IEnumerable<TSource> source, Action<TSource, int> action) {

            if (source == null) {
                throw Errors.ArgumentNull(nameof(source));
            }

            if (action == null) {
                throw Errors.ArgumentNull(nameof(action));
            }

            int index = -1;
            foreach (TSource element in source) {
                checked {
                    index++;
                }

                action(element, index);
            }

            return source;
        }
    }
}
