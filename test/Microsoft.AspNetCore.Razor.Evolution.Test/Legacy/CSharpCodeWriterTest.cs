// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.AspNetCore.Razor.Evolution.Legacy
{
    public class CSharpCodeWriterTest
    {
        [Theory]
        [InlineData("")]
        [InlineData("source-location-file-path")]
        public void WriteLineNumberDirective_UsesSourceSpanFilePath(string filePath)
        {
            // Arrange
            var writer = new CSharpCodeWriter();
            var expected = $"#line 5 \"{filePath}\"" + writer.NewLine;
            var sourceLocation = new SourceLocation(filePath, 10, 4, 3);
            var mappingLocation = new SourceSpan(sourceLocation, 9);

            // Act
            writer.WriteLineNumberDirective(mappingLocation);
            var code = writer.GenerateCode();

            // Assert
            Assert.Equal(expected, code);
        }
    }
}
