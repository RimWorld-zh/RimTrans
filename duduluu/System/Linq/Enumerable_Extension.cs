using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace duduluu.System.Linq {
    public static partial class Enumerable_Extension {
        public static IEnumerable<TSource> ForEach<TSource>(
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

        public static IEnumerable<TSource> ForEach<TSource>(
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
