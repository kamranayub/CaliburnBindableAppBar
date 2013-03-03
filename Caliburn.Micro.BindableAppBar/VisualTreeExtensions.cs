using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Caliburn.Micro.BindableAppBar {
    /// <summary>
    /// Provides useful extensions for working with the visual tree.
    /// </summary>
    /// <remarks>
    /// Since many of these extension methods are declared on types like
    /// DependencyObject high up in the class hierarchy, we've placed them in
    /// the Primitives namespace which is less likely to be imported for normal
    /// scenarios.
    /// </remarks>
    /// <QualityBand>Experimental</QualityBand>
    public static class VisualTreeExtensions {
        /// <summary>
        /// Get the visual tree ancestors of an element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The visual tree ancestors of the element.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="element"/> is null.
        /// </exception>
        public static IEnumerable<DependencyObject> GetVisualAncestors(this DependencyObject element) {
            if (element == null) {
                throw new ArgumentNullException("element");
            }

            return GetVisualAncestorsAndSelfIterator(element).Skip(1);
        }

        /// <summary>
        /// Get the visual tree ancestors of an element and the element itself.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>
        /// The visual tree ancestors of an element and the element itself.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="element"/> is null.
        /// </exception>
        public static IEnumerable<DependencyObject> GetVisualAncestorsAndSelf(this DependencyObject element) {
            if (element == null) {
                throw new ArgumentNullException("element");
            }

            return GetVisualAncestorsAndSelfIterator(element);
        }

        /// <summary>
        /// Get the visual tree ancestors of an element and the element itself.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>
        /// The visual tree ancestors of an element and the element itself.
        /// </returns>
        private static IEnumerable<DependencyObject> GetVisualAncestorsAndSelfIterator(DependencyObject element) {
            Debug.Assert(element != null, "element should not be null!");

            for (DependencyObject obj = element;
                 obj != null;
                 obj = VisualTreeHelper.GetParent(obj)) {
                yield return obj;
            }
        }

        public static T FindChildOfType<T>(this DependencyObject root) where T : class {
            var queue = new Queue<DependencyObject>();
            queue.Enqueue(root);

            while (queue.Count > 0) {
                DependencyObject current = queue.Dequeue();
                for (int i = VisualTreeHelper.GetChildrenCount(current) - 1; 0 <= i; i--) {
                    var child = VisualTreeHelper.GetChild(current, i);
                    var typedChild = child as T;
                    if (typedChild != null) {
                        return typedChild;
                    }
                    queue.Enqueue(child);
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the visual children of type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetVisualChildren<T>(this DependencyObject target)
            where T : DependencyObject {
            return GetVisualChildren(target).Where(child => child is T).Cast<T>();
        }

        /// <summary>
        /// Gets the visual children of type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetVisualChildren<T>(this DependencyObject target, bool strict)
            where T : DependencyObject {
            return GetVisualChildren(target, strict).Where(child => child is T).Cast<T>();
        }

        /// <summary>
        /// Gets the visual children.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="strict">Prevents the search from navigating the logical tree; eg. ContentControl.Content</param>
        /// <returns></returns>
        public static IEnumerable<DependencyObject> GetVisualChildren(this DependencyObject target, bool strict) {
            int count = VisualTreeHelper.GetChildrenCount(target);
            if (count == 0) {
                if (!strict && target is ContentControl) {
                    var child = ((ContentControl)target).Content as DependencyObject;
                    if (child != null) {
                        yield return child;
                    } else {
                        yield break;
                    }
                }
            } else {
                for (int i = 0; i < count; i++) {
                    yield return VisualTreeHelper.GetChild(target, i);
                }
            }
            yield break;
        }

        /// <summary>
        /// Get the visual tree children of an element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The visual tree children of an element.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="element"/> is null.
        /// </exception>
        public static IEnumerable<DependencyObject> GetVisualChildren(this DependencyObject element) {
            if (element == null) {
                throw new ArgumentNullException("element");
            }

            return GetVisualChildrenAndSelfIterator(element).Skip(1);
        }

        /// <summary>
        /// Get the visual tree children of an element and the element itself.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>
        /// The visual tree children of an element and the element itself.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="element"/> is null.
        /// </exception>
        public static IEnumerable<DependencyObject> GetVisualChildrenAndSelf(this DependencyObject element) {
            if (element == null) {
                throw new ArgumentNullException("element");
            }

            return GetVisualChildrenAndSelfIterator(element);
        }

        /// <summary>
        /// Get the visual tree children of an element and the element itself.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>
        /// The visual tree children of an element and the element itself.
        /// </returns>
        private static IEnumerable<DependencyObject> GetVisualChildrenAndSelfIterator(this DependencyObject element) {
            Debug.Assert(element != null, "element should not be null!");

            yield return element;

            int count = VisualTreeHelper.GetChildrenCount(element);
            for (int i = 0; i < count; i++) {
                yield return VisualTreeHelper.GetChild(element, i);
            }
        }

        /// <summary>
        /// A helper method used to get visual decnedants using a breadth-first strategy.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="strict">Prevents the search from navigating the logical tree; eg. ContentControl.Content</param>
        /// <param name="queue"></param>
        /// <returns></returns>
        private static IEnumerable<DependencyObject> GetVisualDecendants(DependencyObject target, bool strict, Queue<DependencyObject> queue) {
            foreach (var child in GetVisualChildren(target, strict)) {
                queue.Enqueue(child);
            }

            if (queue.Count == 0) {
                yield break;
            } else {
                DependencyObject node = queue.Dequeue();
                yield return node;

                foreach (var decendant in GetVisualDecendants(node, strict, queue)) {
                    yield return decendant;
                }
            }
        }

        /// <summary>
        /// A helper method used to get visual decnedants using a depth-first strategy.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="strict">Prevents the search from navigating the logical tree; eg. ContentControl.Content</param>
        /// <param name="stack"></param>
        /// <returns></returns>
        private static IEnumerable<DependencyObject> GetVisualDecendants(DependencyObject target, bool strict, Stack<DependencyObject> stack) {
            foreach (var child in GetVisualChildren(target, strict)) {
                stack.Push(child);
            }

            if (stack.Count == 0) {
                yield break;
            } else {
                DependencyObject node = stack.Pop();
                yield return node;

                foreach (var decendant in GetVisualDecendants(node, strict, stack)) {
                    yield return decendant;
                }
            }
        }

        /// <summary>
        /// Get the visual tree descendants of an element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The visual tree descendants of an element.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="element"/> is null.
        /// </exception>
        public static IEnumerable<DependencyObject> GetVisualDescendants(this DependencyObject element) {
            if (element == null) {
                throw new ArgumentNullException("element");
            }

            return GetVisualDescendantsAndSelfIterator(element).Skip(1);
        }

        /// <summary>
        /// Get the visual tree descendants of an element and the element
        /// itself.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>
        /// The visual tree descendants of an element and the element itself.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="element"/> is null.
        /// </exception>
        public static IEnumerable<DependencyObject> GetVisualDescendantsAndSelf(this DependencyObject element) {
            if (element == null) {
                throw new ArgumentNullException("element");
            }

            return GetVisualDescendantsAndSelfIterator(element);
        }

        /// <summary>
        /// Get the visual tree descendants of an element and the element
        /// itself.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>
        /// The visual tree descendants of an element and the element itself.
        /// </returns>
        private static IEnumerable<DependencyObject> GetVisualDescendantsAndSelfIterator(DependencyObject element) {
            Debug.Assert(element != null, "element should not be null!");

            Queue<DependencyObject> remaining = new Queue<DependencyObject>();
            remaining.Enqueue(element);

            while (remaining.Count > 0) {
                DependencyObject obj = remaining.Dequeue();
                yield return obj;

                foreach (DependencyObject child in obj.GetVisualChildren()) {
                    remaining.Enqueue(child);
                }
            }
        }
    }
}
