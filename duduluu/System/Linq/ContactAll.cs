using System;
using System.Collections.Generic;
using System.Linq;

namespace duduluu.System.Linq {
    public static partial class Enumerable_Extension {
        /// <summary>
        /// Contact all <see cref="IEnumerable{T}"/> into one.
        /// 将所有 <see cref="IEnumerable{T}"/> 连接为一个。
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the element of each <see cref="IEnumerable{T}"/>.
        /// 每个 <see cref="IEnumerable{T}"/> 的元素的类型。
        /// </typeparam>
        /// <param name="source">
        /// A sequence of <see cref="IEnumerable{T}"/> to contact.
        /// 需连接的一系列 <see cref="IEnumerable{T}"/>。
        /// </param>
        /// <returns></returns>
        public static IEnumerable<TSource> ContactAll<TSource>(
            this IEnumerable<IEnumerable<TSource>> source) {

            if (source == null) {
                throw Errors.ArgumentNull(nameof(source));
            }

            return from se in source
                   from el in se
                   select el;
        }
    }
}
