﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnumerableCheckExtensions.cs" company="">
//   Copyright 2013 Thomas PIERRAIN
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//       http://www.apache.org/licenses/LICENSE-2.0
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NFluent
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
#if !DOTNET_30 && !DOTNET_20
    using System.Linq;
#endif
    using Extensibility;

    using Extensions;

    /// <summary>
    /// Provides check methods to be executed on an <see cref="IEnumerable"/> value.
    /// </summary>
    public static class EnumerableCheckExtensions
    {
        /// <summary>
        /// Checks that the enumerable contains all the given expected values, in any order.
        /// </summary>
        /// <typeparam name="T">Type of the elements contained in the enumerable.</typeparam>
        /// <param name="check">The fluent check to be extended.</param>
        /// <param name="expectedValues">The expected values to be found.</param>
        /// <returns>
        /// A check link.
        /// </returns>
        /// <exception cref="FluentCheckException">The enumerable does not contain all the expected values.</exception>
        public static IExtendableCheckLink<IEnumerable, IEnumerable> Contains<T>(
            this ICheck<IEnumerable> check,
            params T[] expectedValues)
        {
            var properExpectedValues = ExtractEnumerableValueFromPossibleOneValueArray(expectedValues);
            return check.Contains(properExpectedValues);
        }

        /// <summary>
        /// Checks that the enumerable contains all the values present in another enumerable, in any order.
        /// </summary>
        /// <param name="check">The fluent check to be extended.</param>
        /// <param name="otherEnumerable">The enumerable containing the expected values to be found.</param>
        /// <returns>
        /// A check link.
        /// </returns>
        /// <exception cref="FluentCheckException">The enumerable does not contain all the expected values present in the other enumerable.</exception>
        public static IExtendableCheckLink<IEnumerable, IEnumerable> Contains(
            this ICheck<IEnumerable> check,
            IEnumerable otherEnumerable)
        {
            var checker = ExtensibilityHelper.ExtractChecker(check);

            checker.ExecuteCheck(
                () =>
                    {
                        if (otherEnumerable == null)
                        {
                            return;
                        }

                        if (checker.Value == null)
                        {
                            var message = checker
                                .BuildMessage("The {0} is null and thus, does not contain the given expected value(s).")
                                .ExpectedValues(otherEnumerable).ToString();
                            throw new FluentCheckException(message);
                        }

                        var notFoundValues = ExtractNotFoundValues(checker.Value, otherEnumerable);

                        if (notFoundValues.Count > 0)
                        {
                            var message = checker
                                .BuildMessage(
                                    string.Format(
                                        "The {{0}} does not contain the expected value(s):" + Environment.NewLine
                                        + "\t[{0}]",
                                        notFoundValues.ToEnumeratedString().DoubleCurlyBraces()))
                                .ExpectedValues(otherEnumerable).ToString();
                            throw new FluentCheckException(message);
                        }
                    },
                checker.BuildMessage("The {0} contains all the given values whereas it must not.").ExpectedValues(otherEnumerable).ToString());

            return ExtensibilityHelper.BuildExtendableCheckLink(check, otherEnumerable);
        }

        /// <summary>
        /// Checks that the enumerable contains only the given values and nothing else, in any order.
        /// Note: this check succeeded with empty value.
        /// </summary>
        /// <typeparam name="T">Type of the expected values to be found.</typeparam>
        /// <param name="check">The fluent check to be extended.</param>
        /// <param name="expectedValues">The expected values to be found.</param>
        /// <returns>
        /// A check link.
        /// </returns>
        /// <exception cref="FluentCheckException">The enumerable does not contain only the expected values provided.</exception>
        public static ICheckLink<ICheck<IEnumerable>> IsOnlyMadeOf<T>(
            this ICheck<IEnumerable> check,
            params T[] expectedValues)
        {
            var properExpectedValues = ExtractEnumerableValueFromPossibleOneValueArray(expectedValues);

            return check.IsOnlyMadeOf(properExpectedValues);
        }

        /// <summary>
        /// Checks that the enumerable contains only the values present in another enumerable, and nothing else, in any order.
        /// Note: this check succeeded with empty value.
        /// </summary>
        /// <param name="check">The fluent check to be extended.</param>
        /// <param name="expectedValues">The expected values to be found.</param>
        /// <returns>
        /// A check link.
        /// </returns>
        /// <exception cref="FluentCheckException">The enumerable does not contain only the expected values present in the other enumerable.</exception>
        public static ICheckLink<ICheck<IEnumerable>> IsOnlyMadeOf(
            this ICheck<IEnumerable> check,
            IEnumerable expectedValues)
        {
            var checker = ExtensibilityHelper.ExtractChecker(check);

            return checker.ExecuteCheck(
                () =>
                    {
                        if (Equals(checker.Value, expectedValues))
                        {
                            return;
                        }

                        if (checker.Value == null && expectedValues != null)
                        {
                            var message = checker
                                .BuildMessage("The {0} is null and thus, does not contain exactly the given value(s).")
                                .ExpectedValues(expectedValues).ToString();
                            throw new FluentCheckException(message);
                        }

                        var unexpectedValuesFound = ExtractUnexpectedValues(checker.Value, expectedValues);

                        if (unexpectedValuesFound.Count > 0)
                        {
                            var message = checker
                                .BuildMessage(
                                    string.Format(
                                        "The {{0}} does not contain only the given value(s)." + Environment.NewLine
                                        + "It contains also other values:" + Environment.NewLine + "\t[{0}]",
                                        unexpectedValuesFound.ToEnumeratedString().DoubleCurlyBraces()))
                                .ExpectedValues(expectedValues).ToString();
                            throw new FluentCheckException(message);
                        }
                    },
                checker.BuildMessage("The {0} contains only the given values whereas it must not.").ExpectedValues(expectedValues).ToString());
        }

        /// <summary>
        /// Checks that the enumerable contains only the given expected values and nothing else, in order.
        /// This check should only be used with IEnumerable that have a consistent iteration order
        /// (i.e. don't use it with Hashtable, prefer <see cref="IsOnlyMadeOf{T}" /> in that case).
        /// </summary>
        /// <typeparam name="T">Type of the elements to be found.</typeparam>
        /// <param name="check">The fluent check to be extended.</param>
        /// <param name="expectedValues">The expected values to be found.</param>
        /// <returns>
        /// A check link.
        /// </returns>
        /// <exception cref="FluentCheckException">The enumerable does not contains only the exact given values and nothing else, in order.</exception>
        public static ICheckLink<ICheck<IEnumerable>> ContainsExactly<T>(
            this ICheck<IEnumerable> check,
            params T[] expectedValues)
        {
            var properExpectedValues = ExtractEnumerableValueFromPossibleOneValueArray(expectedValues);

            return check.ContainsExactly(properExpectedValues);
        }

        /// <summary>
        /// Checks that the enumerable contains only the values of another enumerable and nothing else, in order.
        /// This check should only be used with IEnumerable that have a consistent iteration order
        /// (i.e. don't use it with Hashtable, prefer <see cref="IsOnlyMadeOf{T}" /> in that case).
        /// </summary>
        /// <param name="check">The fluent check to be extended.</param>
        /// <param name="otherEnumerable">The other enumerable containing the exact expected values to be found.</param>
        /// <returns>
        /// A check link.
        /// </returns>
        /// <exception cref="FluentCheckException">The enumerable does not contains only the exact given values and nothing else, in order.</exception>
        public static ICheckLink<ICheck<IEnumerable>> ContainsExactly(
            this ICheck<IEnumerable> check,
            IEnumerable otherEnumerable)
        {
            var checker = ExtensibilityHelper.ExtractChecker(check);

            return checker.ExecuteCheck(
                () =>
                    {
                        if (Equals(checker.Value, otherEnumerable))
                        {
                            return;
                        }

                        if (checker.Value == null && otherEnumerable != null)
                        {
                            var message = checker
                                .BuildMessage("The {0} is null and thus does not contain exactly the {1}.")
                                .ExpectedValues(otherEnumerable).ToString();
                            throw new FluentCheckException(message);
                        }

                        if (otherEnumerable == null)
                        {
                            throw new FluentCheckException(BuildNotExactlyExceptionMessage(checker, null, 0));
                        }

                        var index = 0;
                        var enumerable = otherEnumerable as IList<object> ?? otherEnumerable.Cast<object>().ToList();
                        var first = checker.Value.GetEnumerator();
                        using (var second = enumerable.GetEnumerator())
                        {
                            while (first.MoveNext())
                            {
                                if (!second.MoveNext() || !Equals(first.Current, second.Current))
                                {
                                    throw new FluentCheckException(
                                        BuildNotExactlyExceptionMessage(checker, enumerable, index));
                                }

                                index++;
                            }

                            if (second.MoveNext())
                            {
                                throw new FluentCheckException(
                                    BuildNotExactlyExceptionMessage(checker, enumerable, index));
                            }
                        }
                    },
                BuildExceptionMessageForContainsExactly(checker));
        }

        /// <summary>
        /// Checks that the enumerable has the proper number of elements.
        /// </summary>
        /// <param name="check">The fluent check to be extended.</param>
        /// <param name="expectedSize">The expected size to be found.</param>
        /// <returns>
        /// A check link.
        /// </returns>
        /// <exception cref="FluentCheckException">The enumerable has not the expected number of elements.</exception>
        public static ICheckLink<ICheck<IEnumerable>> HasSize(this ICheck<IEnumerable> check, long expectedSize)
        {
            var checker = ExtensibilityHelper.ExtractChecker(check);

            return checker.ExecuteCheck(
                () => HasSizeImpl(checker, expectedSize),
                BuildHasSizeExceptionMessage(checker));
        }

        private static void HasSizeImpl(IChecker<IEnumerable, ICheck<IEnumerable>> checker, long expectedSize)
        {
            var checkedEnumerable = checker.Value;
            long itemsCount = checkedEnumerable.Cast<object>().LongCount();

            if (expectedSize != itemsCount)
            {
                var foundElementsNumberDescription = BuildElementNumberLiteral(itemsCount);

                var errorMessage = checker
                    .BuildMessage(
                        $"The {{0}} has {foundElementsNumberDescription.DoubleCurlyBraces()} instead of {expectedSize}.").On(checkedEnumerable).ToString();
                throw new FluentCheckException(errorMessage);
            }
        }

        private static string BuildHasSizeExceptionMessage(IChecker<IEnumerable, ICheck<IEnumerable>> checker)
        {
            var checkedEnumerable = checker.Value;
            long itemsCount = checkedEnumerable.Cast<object>().LongCount();
            var foundElementsNumberDescription = BuildElementNumberLiteral(itemsCount);

            return checker
                .BuildMessage(
                    string.Format(
                        "The {{0}} has {0} which is unexpected.",
                        foundElementsNumberDescription.DoubleCurlyBraces())).On(checkedEnumerable).ToString();
        }

        private static string BuildElementNumberLiteral(long itemsCount)
        {
            string foundElementsNumberDescription = itemsCount.ToString();
            if (itemsCount > 1)
            {
                foundElementsNumberDescription += " elements";
            }
            else
            {
                foundElementsNumberDescription += " element";
            }

            return foundElementsNumberDescription;
        }

        /// <summary>
        /// Checks that the enumerable is empty.
        /// </summary>
        /// <param name="check">The fluent check to be extended.</param>
        /// <returns>
        /// A check link.
        /// </returns>
        /// <exception cref="FluentCheckException">The enumerable is not empty.</exception>
        public static ICheckLink<ICheck<IEnumerable>> IsEmpty(this ICheck<IEnumerable> check)
        {
            var checker = ExtensibilityHelper.ExtractChecker(check);

            return checker.ExecuteCheck(
                () =>
                    {
                        if (checker.Value.Cast<object>().Any())
                        {
                            var errorMessage = checker.BuildMessage("The {0} is not empty.").ToString();
                            throw new FluentCheckException(errorMessage);
                        }
                    },
                checker.BuildShortMessage("The checked enumerable is empty, which is unexpected.").ToString());
        }

        /// <summary>
        /// Checks that the enumerable is null or empty.
        /// </summary>
        /// <param name="check">The fluent check to be extended.</param>
        /// <returns>
        /// A check link.
        /// </returns>
        /// <exception cref="FluentCheckException">The enumerable is not empty.</exception>
        public static ICheckLink<ICheck<IEnumerable>> IsNullOrEmpty(this ICheck<IEnumerable> check)
        {
            var checker = ExtensibilityHelper.ExtractChecker(check);

            string message = null;

            if (checker.Value != null && checker.Value.Count() > 0)
            {
                if (!checker.Negated)
                {
                    message = checker.BuildMessage("The {0} contains elements, whereas it must be null or empty.")
                        .For(typeof(IEnumerable)).ToString();
                }
            }
            else if (checker.Negated)
            {
                if (checker.Value == null)
                {
                    message = checker.BuildShortMessage("The {0} is null, where as it must contain at least one element.")
                        .For(typeof(IEnumerable)).ToString();
                }
                else
                {
                    message = checker.BuildShortMessage("The {0} is empty, where as it must contain at least one element.")
                        .For(typeof(IEnumerable)).ToString();
                }
            }

            if (!string.IsNullOrEmpty(message))
            {
                throw new FluentCheckException(message);
            }

            return checker.BuildChainingObject();
        }

        /// <summary>
        /// Check a specific item in the <see cref="IEnumerable"/>.
        /// </summary>
        /// <typeparam name="T">
        /// Enumerated type.
        /// </typeparam>
        /// <param name="check">
        /// The checker.
        /// </param>
        /// <param name="index">
        /// Index to check.
        /// </param>
        /// <returns>
        /// An extensible checker.
        /// </returns>
        public static ICheckLinkWhich<ICheck<IEnumerable<T>>, ICheck<T>> HasElementAt<T>(this ICheck<IEnumerable<T>> check, int index)
        {
            var checker = ExtensibilityHelper.ExtractChecker(check);
            ICheckLinkWhich<ICheck<IEnumerable<T>>, ICheck<T>> result = null;
            checker.ExecuteCheck(
                () =>
                    {
                        if (checker.Value == null)
                        {
                            throw new FluentCheckException(checker.BuildShortMessage(
                                string.Format("The {{0}} is null, whereas it must have an element with number {0}.", index)).ToString());
                        }

                        if (!TryGetElementByNumber(checker.Value, index, out T item))
                        {
                            throw new FluentCheckException(checker.BuildMessage(
                                $"The {{0}} does not have an element at index {index}.").ToString());
                        }
                        // ReSharper disable once SuspiciousTypeConversion.Global
                        var itemCheck = Check.That(item);
                            var subChecker = ExtensibilityHelper.ExtractChecker(itemCheck);
                            subChecker.SetSutLabel($"element #{index}");
                            result = checker.BuildLinkWhich(itemCheck);
                    },
                checker.BuildMessage("The {{0}} does have an element at index {0} whereas it should not.").ToString());
            return result;
        }

        /// <summary>
        /// Checks that the given enumerable does contain at least one item matching a predicate.
        /// </summary>
        /// <typeparam name="T">Type of items.</typeparam>
        /// <param name="check">Check item.</param>
        /// <param name="predicate">Predicate to evaluate.</param>
        /// <returns>A linkable check.</returns>
        public static ICheckLinkWhich<ICheck<IEnumerable<T>>, ICheck<T>> HasElementThatMatches<T>(
            this ICheck<IEnumerable<T>> check,
            Func<T, bool> predicate)
        {
            var checker = ExtensibilityHelper.ExtractChecker(check);
            return checker.ExecuteCheckAndProvideSubItem(
                () =>
                    {
                        using (var scan = checker.Value.GetEnumerator())
                        {
                            int? index = null;
                            for (var i = 0; scan.MoveNext(); i++)
                            {
                                if (predicate(scan.Current))
                                {
                                    index = i;
                                    break;
                                }
                            }

                            if (!index.HasValue)
                            {
                                var message =
                                    checker.BuildMessage(
                                        "The {0} does not contain any element that matches the given predicate.");
                                throw new FluentCheckException(message.ToString());
                            }

                            var itemCheck = Check.That(scan.Current);
                            var subChecker = ExtensibilityHelper.ExtractChecker(itemCheck);
                            subChecker.SetSutLabel($"element #{index}");
                            return itemCheck;
                        }
                    },
            checker.BuildMessage("The {0} contains element(s) that matches the given predicate, whereas it must not.").ToString());
        }

        /// <summary>
        /// Checks that the given enumerable does contain all items matching a predicate.
        /// </summary>
        /// <typeparam name="T">Type of items.</typeparam>
        /// <param name="check">Check item.</param>
        /// <param name="predicate">Predicate to evaluate.</param>
        /// <returns>A linkable check.</returns>
        public static ICheckLinkWhich<ICheck<IEnumerable<T>>, ICheck<T>> ContainsOnlyElementsThatMatch<T>(
            this ICheck<IEnumerable<T>> check,
            Func<T, bool> predicate)
        {
            var checker = ExtensibilityHelper.ExtractChecker(check);
            return checker.ExecuteCheckAndProvideSubItem(
                () =>
                {
                    using (var scan = checker.Value.GetEnumerator())
                    {
                     
                        while (scan.MoveNext())
                        {
                            if (!predicate(scan.Current))
                            {
                                var message =
                                    checker.BuildMessage(
                                        "The {0} does contain an element that does not match the given predicate.");
                                throw new FluentCheckException(message.ToString());
                            }
                        }

                        var itemCheck = Check.That(scan.Current);
                        var subChecker = ExtensibilityHelper.ExtractChecker(itemCheck);
                        subChecker.SetSutLabel($"all elements");
                        return itemCheck;
                    }
                },
                checker.BuildMessage("The {0} contains only element(s) that match the given predicate, whereas it must not.").ToString());
        }

        /// <summary> 
        /// Checks that the enumerable has a first element, and returns a check on that element. 
        /// </summary> 
        /// <typeparam name="T">The element type of the collection.</typeparam> 
        /// <param name="check">The fluent check to be extended.</param> 
        /// <returns>A check on the first element.</returns> 
        public static ICheckLinkWhich<ICheck<IEnumerable<T>>, ICheck<T>> HasFirstElement<T>(this ICheck<IEnumerable<T>> check)
        {
            var checker = ExtensibilityHelper.ExtractChecker(check);

            return checker.ExecuteCheckAndProvideSubItem(
                () =>
                    {
                        if (checker.Value == null)
                        {
                            var errorMessage = checker.BuildShortMessage("The {0} is null, whereas it must have a first element.");
                            throw new FluentCheckException(errorMessage.ToString());
                        }

                        if (!TryGetElementByNumber(checker.Value, 0, out T first))
                        {
                            var errorMessage = checker.BuildShortMessage("The {0} is empty, whereas it must have a first element.");
                            throw new FluentCheckException(errorMessage.ToString());
                        }

                        var subChecker = Check.That(first);
                        ExtensibilityHelper.ExtractChecker(subChecker).SetSutLabel("First item");
                        return subChecker;
                    },
                checker.BuildMessage("The {0} has a first element, whereas it should not.").ToString());
        }

        /// <summary>
        /// Checks that the enumerable has a last element, and returns a check on that element.
        /// </summary>
        /// <typeparam name="T">The element type of the collection.</typeparam>
        /// <param name="check">The fluent check to be extended.</param>
        /// <returns>A check on the last element.</returns>
        public static ICheckLinkWhich<ICheck<IEnumerable<T>>, ICheck<T>> HasLastElement<T>(this ICheck<IEnumerable<T>> check)
        {
            var checker = ExtensibilityHelper.ExtractChecker(check);
            return checker.ExecuteCheckAndProvideSubItem(
                () =>
                    {
                        if (checker.Value == null)
                        {
                            var nullErrorMessage = checker.BuildShortMessage("The {0} is null, whereas it must have a last element.");
                            throw new FluentCheckException(nullErrorMessage.ToString());
                        }

                        T last;
                        if (!TryGetLastElement(checker.Value, out last))
                        {
                            var emptyErrorMessage = checker.BuildShortMessage("The {0} is empty, whereas it must have a last element.");
                            throw new FluentCheckException(emptyErrorMessage.ToString());
                        }

                        var subChecker = Check.That(last);
                        ExtensibilityHelper.ExtractChecker(subChecker).SetSutLabel("Last item");
                        return subChecker;
                    },
                checker.BuildMessage("The {0} has a last element, whereas it must be empty.").ToString());
         }

        /// <summary>
        /// Checks that the enumerable has a single element, and returns a check on that element.
        /// </summary>
        /// <typeparam name="T">The element type of the collection.</typeparam>
        /// <param name="check">The fluent check to be extended.</param>
        /// <returns>A check on the single element.</returns>
        public static ICheckLinkWhich<ICheck<IEnumerable<T>>, ICheck<T>> HasOneElementOnly<T>(this ICheck<IEnumerable<T>> check)
        {
            var checker = ExtensibilityHelper.ExtractChecker(check);
            return checker.ExecuteCheckAndProvideSubItem(
                () =>
                    {
                        if (checker.Value == null)
                        {
                            var errorMessage = checker.BuildShortMessage("The {0} is null, whereas it must have one element.");
                            throw new FluentCheckException(errorMessage.ToString());
                        }

                        using (var enumerator = checker.Value.GetEnumerator())
                        {
                            if (!enumerator.MoveNext())
                            {
                                var errorMessage = checker.BuildMessage("The {0} is empty, whereas it must have one element.");
                                throw new FluentCheckException(errorMessage.ToString());
                            }

                            var first = enumerator.Current;

                            if (enumerator.MoveNext())
                            {
                                var errorMessage = checker.BuildMessage("The {0} contains more than one element, whereas it must have one element only.");
                                throw new FluentCheckException(errorMessage.ToString());
                            }

                            var subChecker = Check.That(first);
                            ExtensibilityHelper.ExtractChecker(subChecker).SetSutLabel("single element");
                            return subChecker;
                        }
                    },
                checker.BuildMessage("The {0} has exactly one element, whereas it should not.").ToString());
        }

#region private or internal methods

        /// <summary>
        /// Returns all expected values that aren't present in the enumerable.
        /// </summary>
        /// <param name="enumerable">The enumerable to inspect.</param>
        /// <param name="expectedValues">The expected values to search within the enumerable.</param>
        /// <returns>
        /// A list containing all the expected values that aren't present in the enumerable.
        /// </returns>
        private static IList<object> ExtractNotFoundValues(IEnumerable enumerable, IEnumerable expectedValues)
        {
            // Prepares the list to return
            var notFoundValues = expectedValues.Cast<object>().ToList();

            foreach (var element in enumerable)
            {
                foreach (var expectedValue in expectedValues)
                {
                    // ReSharper disable once RedundantNameQualifier
                    if (!object.Equals(element, expectedValue))
                    {
                        continue;
                    }

                    // ReSharper disable once RedundantNameQualifier
                    notFoundValues.RemoveAll(one => object.Equals(one, expectedValue));
                    break;
                }
            }

            return notFoundValues;
        }

        /// <summary>
        /// Returns all the values of the enumerable that don't belong to the expected ones.
        /// </summary>
        /// <param name="enumerable">The enumerable to inspect.</param>
        /// <param name="expectedValues">The allowed values to be part of the enumerable.</param>
        /// <returns>
        /// A list with all the values found in the enumerable that don't belong to the expected ones.
        /// </returns>
        private static IList ExtractUnexpectedValues(IEnumerable enumerable, IEnumerable expectedValues)
        {
            var unexpectedValuesFound = new List<object>();
            var values = expectedValues as object[] ?? expectedValues.Cast<object>();
            foreach (var element in enumerable)
            {
                var isExpectedValue = values.Contains(element);

                if (!isExpectedValue)
                {
                    unexpectedValuesFound.Add(element);
                }
            }

            return unexpectedValuesFound;
        }

        private static bool IsAOneValueArrayWithOneCollectionInside<T>(T[] expectedValues)
        {
            // For every collections like ArrayList, List<T>, IEnumerable<T>, StringCollection, etc.
#if !(PORTABLE) && !(NETSTANDARD1_3)
            return expectedValues != null && (expectedValues.LongLength == 1) && IsAnEnumerableButNotAnEnumerableOfChars(expectedValues[0]);
#else
            return expectedValues != null && (expectedValues.Length == 1)
                   && IsAnEnumerableButNotAnEnumerableOfChars(expectedValues[0]);
#endif
        }

        private static bool IsAnEnumerableButNotAnEnumerableOfChars<T>(T element)
        {
            return (element is IEnumerable) && !(element is IEnumerable<char>);
        }

        private static string BuildExceptionMessageForContainsExactly(
            IChecker<IEnumerable, ICheck<IEnumerable>> checker)
        {
            var checkedValue = checker.Value;
            return checker.BuildMessage("The {0} contains exactly the given values whereas it must not.")
                .On(checkedValue).WithEnumerableCount(checkedValue.Count()).ToString();
        }

        private static string BuildNotExactlyExceptionMessage(
            IChecker<IEnumerable, ICheck<IEnumerable>> checker,
            IEnumerable<object> enumerable,
            int index)
        {
            var checkedValue = checker.Value;
            var sutCount = checkedValue.Count();
            var expectedCount = enumerable.Count();
            FluentMessage message;
            if (enumerable != null)
            {
                if (sutCount < expectedCount && index == sutCount)
                {
                    message = checker.BuildMessage(
                        string.Format(
                            "The {{0}} does not contain exactly the expected value(s). Elements are missing starting at index #{0}.",
                            index));
                }
                else if (sutCount > expectedCount && index == expectedCount)
                {
                    message = checker.BuildMessage(
                        string.Format(
                            "The {{0}} does not contain exactly the expected value(s). There are extra elements starting at index #{0}.",
                            index));
                }
                else
                {
                    message = checker.BuildMessage(
                        string.Format(
                            "The {{0}} does not contain exactly the expected value(s). First difference is at index #{0}.",
                            index));
                }
            }
            else
            {
                message = checker.BuildMessage("The {0} does not contain exactly the expected value(s).");
            }

            message.For(typeof(IEnumerable)).On(checkedValue, index).WithEnumerableCount(sutCount).And
                .ExpectedValues(enumerable, index).WithEnumerableCount(expectedCount);

            return message.ToString();
        }

        private static IEnumerable ExtractEnumerableValueFromPossibleOneValueArray<T>(T[] expectedValues)
        {
            IEnumerable properExpectedValues;
            if (IsAOneValueArrayWithOneCollectionInside(expectedValues))
            {
                properExpectedValues = expectedValues[0] as IEnumerable;
            }
            else
            {
                properExpectedValues = expectedValues;
            }

            return properExpectedValues;
        }

        private static bool TryGetLastElement<T>(IEnumerable<T> collection, out T last)
        {
            var list = collection as IList<T>;
            if (list != null)
            {
                if (list.Count != 0)
                {
                    last = list[list.Count - 1];
                    return true;
                }
            }
            else
            {
                using (var enumerator = collection.GetEnumerator())
                {
                    if (enumerator.MoveNext())
                    {
                        last = enumerator.Current;
                        while (enumerator.MoveNext())
                        {
                            last = enumerator.Current;
                        }

                        return true;
                    }
                }
            }

            last = default(T);
            return false;
        }

        private static bool TryGetElementByNumber<T>(IEnumerable<T> collection, int number, out T element)
        {
            if (number < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(number), "The specified number is less than zero, whereas it must be a 0-based index.");
            }

            var list = collection as IList<T>;
            if (list != null)
            {
                if (list.Count > number)
                {
                    element = list[number];
                    return true;
                }
            }
            else
            {
                var currentNumber = 0;
                foreach (var currentElement in collection)
                {
                    if (currentNumber == number)
                    {
                        element = currentElement;
                        return true;
                    }

                    currentNumber++;
                }
            }

            element = default(T);
            return false;
        }

#endregion
    }
}