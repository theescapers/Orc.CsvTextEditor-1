﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StringExtensions.cs" company="WildGums">
//   Copyright (c) 2008 - 2017 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.CsvTextEditor
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Catel;

    public static class StringExtensions
    {
        public static string RemoveCommaSeparatedText(this string text, int positionStart, int length, string newLine)
        {
            Argument.IsNotNull(nameof(text), text);

            var endPosition = positionStart + length;

            var replacementText = string.Empty;

            for (var i = positionStart; i < endPosition; i++)
            {
                var c = text[i];
                if (c == Symbols.Comma)
                {
                    replacementText += Symbols.Comma;
                    continue;
                }

                if (IsLookupMatch(text, i, newLine))
                {
                    replacementText += newLine;
                    continue;
                }
            }

            text = text.Remove(positionStart, length)
                .Insert(positionStart, replacementText);

            return text;
        }

        public static string GetWordFromOffset(this string text, int offset)
        {
            Argument.IsNotNull(() => text);

            var textLength = text.Length;
            if (offset < 0 || offset >= textLength)
            {
                return string.Empty;
            }

            var i = offset;
            while (i >= 0 && char.IsLetterOrDigit(text[i]))
            {
                i--;
            }

            var startOffset = i + 1;

            i = offset;
            while (i < textLength && char.IsLetterOrDigit(text[i]))
            {
                i++;
            }

            var endOffset = i;

            var length = endOffset - startOffset;
            return length > 0 ? text.Substring(startOffset, length) : string.Empty;
        }

        public static string InsertCommaSeparatedColumn(this string text, int column, int linesCount, int columnsCount, string newLine)
        {
            Argument.IsNotNull(nameof(text), text);

            if (column == columnsCount)
            {
                return text.Replace(newLine, Symbols.Comma + newLine) + Symbols.Comma;
            }

            if (column == 0)
            {
                return text.Insert(0, Symbols.Comma.ToString())
                    .Replace(newLine, newLine + Symbols.Comma);
            }

            var newCount = text.Length + linesCount;
            var textArray = new char[newCount];
            var indexer = 0;
            var withinQuotes = false;

            var commaCounter = 1;
            foreach (var c in text)
            {

                if (c == Symbols.Quote)
                {
                    withinQuotes = !withinQuotes;
                }

                if (c == Symbols.Comma && !withinQuotes)
                {
                    if (commaCounter == column)
                    {
                        textArray[indexer] = Symbols.Comma;
                        indexer++;
                    }

                    commaCounter++;

                    if (commaCounter == columnsCount)
                    {
                        commaCounter = 1;
                    }
                }

                textArray[indexer] = c;
                indexer++;
            }

            return new string(textArray).TrimEnd(newLine);
        }

        public static string RemoveEmptyLines(this string text)
        {
            Argument.IsNotNull(() => text);
            var newLineSymbol = text.GetNewLineSymbol();
            var lines = text.Split(new string[] { newLineSymbol }, StringSplitOptions.None).ToList();
            lines.RemoveAll(x => x == String.Empty);
            return String.Join(newLineSymbol,lines);
        }

        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
                
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        public static string[] GetLines(this string text, out string newLineSymbol)
        {
            newLineSymbol = text.GetNewLineSymbol();

            return text.Split(new[] { newLineSymbol }, StringSplitOptions.None);
        }

        public static string InsertLineWithTextTransfer(this string text, int insertLineIndex, int offsetInLine, int columnCount, string lineEnding)
        {
            Argument.IsNotNull(nameof(text), text);

            var lineEndingLength = lineEnding.Length;

            if (insertLineIndex == 0)
            {
                return InsertLine(text, insertLineIndex, columnCount, lineEnding);
            }

            var previousLineOffset = insertLineIndex == 1 ? 0 : text.IndexOfSpecificOccurance(lineEnding, insertLineIndex - 1) + lineEndingLength;
            var leftLineChunk = text.Substring(previousLineOffset, offsetInLine);
            var splitColumnIndex = leftLineChunk.Count(x => x.Equals(Symbols.Comma));

            var insetionText = $"{new string(Symbols.Comma, columnCount - splitColumnIndex - 1)}{lineEnding}{new string(Symbols.Comma, splitColumnIndex)}";

            var insertPosition = previousLineOffset + offsetInLine;
            return text.Insert(insertPosition, insetionText).TrimEnd(lineEnding);
        }

        public static string DuplicateTextInLine(this string text, int startOffset, int endOffset, string newLine)
        {
            Argument.IsNotNull(nameof(text), text);

            var lineToDuplicate = text.Substring(startOffset, endOffset - startOffset);
            if (!lineToDuplicate.EndsWith(newLine))
            {
                lineToDuplicate = newLine + lineToDuplicate;
            }

            return text.Insert(endOffset, lineToDuplicate).TrimEnd(newLine);
        }

        public static string RemoveText(this string text, int startOffset, int endOffset, string newLine)
        {
            Argument.IsNotNull(nameof(text), text);

            return text.Remove(startOffset, endOffset - startOffset).TrimEnd(newLine);
        }

        public static string RemoveCommaSeparatedColumn(this string text, int column, int linesCount, int columnsCount, string newLine)
        {
            Argument.IsNotNull(nameof(text), text);

            if (columnsCount == 0 || linesCount == 0)
            {
                return string.Empty;
            }

            if (columnsCount == 1)
            {
                return string.Empty;
            }

            var newLineLength = newLine.Length;

            var newCount = text.Length;
            var textArray = new char[newCount];
            var indexer = 0; 

            var separatorCounter = 0;
            var isLastColumn = columnsCount - 1 == column;

            var withinQuotes = false;

            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];
                var isSeparator = false;

                if (c == Symbols.Quote)
                {
                    withinQuotes = !withinQuotes;
                }

                if (c == Symbols.Comma && !withinQuotes)
                {
                    isSeparator = true;

                    if (separatorCounter == column)
                    {
                        separatorCounter++;
                        continue;
                    }

                    separatorCounter++;

                    if (isLastColumn && separatorCounter == column)
                    {
                        continue;
                    }
                }
                else
                {
                    if (IsLookupMatch(text, i, newLine))
                    {
                        separatorCounter = 0;

                        i += newLineLength - 1;
                        indexer = WriteStringToCharArray(textArray, newLine, indexer);

                        continue;
                    }
                }

                if (!isSeparator && separatorCounter == column)
                {
                    continue;
                }

                textArray[indexer] = c;
                indexer++;
            }

            return new string(textArray, 0, indexer).TrimEnd(newLine);
        }

        public static string GetNewLineSymbol(this string text)
        {
            Argument.IsNotNull(nameof(text), text);

            if (text.Contains("\r\n"))
            {
                return "\r\n";
            }

            if (text.Contains("\n"))
            {
                return "\n";
            }

            if (text.Contains("\r"))
            {
                return "\r";
            }

            return Environment.NewLine;
        }

        public static bool IsEmptyCommaSeparatedLine(this string textLine)
        {
            Argument.IsNotNull(() => textLine);

            return textLine.All(x => x == Symbols.Comma || x == Symbols.Space);
        }

        public static string TrimCommaSeparatedValues(this string textLine)
        {
            Argument.IsNotNull(() => textLine);

            var trimmedValues = textLine.Split(new []{Symbols.Comma}, StringSplitOptions.None).Select(x => x.Trim());

            return string.Join($"{Symbols.Comma}", trimmedValues);
        }

        public static string TrimEnd(this string text, string trimString)
        {
            Argument.IsNotNull(nameof(text), text);

            var result = text;
            while (result.EndsWith(trimString))
            {
                result = result.Substring(0, result.Length - trimString.Length);
            }

            return result;
        }

        private static string InsertLine(this string text, int insertLineIndex, int columnsCount, string newLine)
        {
            Argument.IsNotNull(nameof(text), text);

            var newLineLenght = newLine.Length;

            var insertLineText = $"{new string(Symbols.Comma, columnsCount - 1)}{newLine}";
            var insertionPosition = insertLineIndex != 0 ? text.IndexOfSpecificOccurance(newLine, insertLineIndex) + newLineLenght : 0;

            return text.Insert(insertionPosition, insertLineText).TrimEnd(newLine);
        }

        private static bool IsLookupMatch(string text, int startIndex, string lookup)
        {
            var lookupLength = lookup.Length;
            if (text.Length - startIndex < lookupLength)
            {
                return false;
            }

            var lookupNewLine = text.Substring(startIndex, lookupLength);
            return string.Equals(lookupNewLine, lookup);
        }

        private static int IndexOfSpecificOccurance(this string source, string value, int occuranceNumber)
        {
            var index = -1;
            for (var i = 0; i < occuranceNumber; i++)
            {
                index = source.IndexOf(value, index + 1, StringComparison.Ordinal);

                if (index == -1)
                {
                    break;
                }
            }

            return index;
        }

        private static int WriteStringToCharArray(char[] array, string text, int startPosition)
        {
            var indexer = startPosition;

            foreach (var newLineChar in text)
            {
                array[indexer] = newLineChar;
                indexer++;
            }

            return indexer;
        }
    }
}