// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;

namespace Microsoft.AspNetCore.Razor.Evolution.Legacy
{
    /// <summary>
    /// A location in a Razor file.
    /// </summary>
    internal struct SourceLocation : IEquatable<SourceLocation>
    {
        /// <summary>
        /// An undefined <see cref="SourceLocation"/>.
        /// </summary>
        public static readonly SourceLocation Undefined =
            new SourceLocation(absoluteIndex: -1, lineIndex: -1, characterIndex: -1);

        /// <summary>
        /// A <see cref="SourceLocation"/> with <see cref="AbsoluteIndex"/>, <see cref="LineIndex"/>, and
        /// <see cref="CharacterIndex"/> initialized to 0.
        /// </summary>
        public static readonly SourceLocation Zero =
            new SourceLocation(absoluteIndex: 0, lineIndex: 0, characterIndex: 0);

        /// <summary>
        /// Initializes a new instance of <see cref="SourceLocation"/>.
        /// </summary>
        /// <param name="absoluteIndex">The absolute index.</param>
        /// <param name="lineIndex">The line index.</param>
        /// <param name="characterIndex">The character index.</param>
        public SourceLocation(int absoluteIndex, int lineIndex, int characterIndex)
            : this(filePath: null, absoluteIndex: absoluteIndex, lineIndex: lineIndex, characterIndex: characterIndex)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="SourceLocation"/>.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="absoluteIndex">The absolute index.</param>
        /// <param name="lineIndex">The line index.</param>
        /// <param name="characterIndex">The character index.</param>
        public SourceLocation(string filePath, int absoluteIndex, int lineIndex, int characterIndex)
        {
            FilePath = filePath;
            AbsoluteIndex = absoluteIndex;
            LineIndex = lineIndex;
            CharacterIndex = characterIndex;
        }

        /// <summary>
        /// Path of the file.
        /// </summary>
        /// <remarks>When <c>null</c>, the parser assumes the location is in the file currently being processed.
        /// </remarks>
        public string FilePath { get; set; }

        /// <remarks>Set property is only accessible for deserialization purposes.</remarks>
        public int AbsoluteIndex { get; set; }

        /// <summary>
        /// Gets the 1-based index of the line referred to by this Source Location.
        /// </summary>
        /// <remarks>Set property is only accessible for deserialization purposes.</remarks>
        public int LineIndex { get; set; }

        /// <remarks>Set property is only accessible for deserialization purposes.</remarks>
        public int CharacterIndex { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                "({0}:{1},{2})",
                AbsoluteIndex,
                LineIndex,
                CharacterIndex);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is SourceLocation &&
                Equals((SourceLocation)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return 0;
        }

        /// <inheritdoc />
        public bool Equals(SourceLocation other)
        {
            // LineIndex and CharacterIndex can be calculated from AbsoluteIndex and the document content.
            return string.Equals(FilePath, other.FilePath, StringComparison.Ordinal) &&
                AbsoluteIndex == other.AbsoluteIndex;
        }
    }
}