﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;

namespace Microsoft.Deployment.DotNet.Dependencies
{
    /// <summary>
    /// Represents the version range of a dependency.
    /// </summary>
    public class VersionRange
    {
        private const char MinExclusiveChar = '(';
        private const char MaxExclusiveChar = ')';
        private const char MinInclusiveChar = '[';
        private const char MaxInclusiveChar = ']';

        /// <summary>
        /// Gets the minimum version of the dependency.
        /// </summary>
        public Version? Minimum { get; private set; }

        /// <summary>
        /// Gets a value indicating whether <see cref="Minimum"/> is inclusive within the range.
        /// </summary>
        public bool IsMinimumInclusive { get; private set; }

        /// <summary>
        /// Gets the maximum version of the dependency.
        /// </summary>
        public Version? Maximum { get; private set; }

        /// <summary>
        /// Gets a value indicating whether <see cref="Maximum"/> is inclusive within the range.
        /// </summary>
        public bool IsMaximumInclusive { get; private set; }

        internal static VersionRange Parse(string versionRange)
        {
            if (versionRange == string.Empty)
            {
                throw new ArgumentException("Value cannot be empty.", nameof(versionRange));
            }

            bool isMinimumInclusive = versionRange[0] == MinInclusiveChar || versionRange[0] != MinExclusiveChar;
            bool isMaximumInclusive = versionRange.Last() == MaxInclusiveChar;

            // Get the version numbers from within the range notation
            string[] minMax = versionRange
                .TrimStart(MinExclusiveChar, MinInclusiveChar)
                .TrimEnd(MaxExclusiveChar, MaxInclusiveChar)
                .Split(',');

            Version? minimum = null;
            Version? maximum = null;

            if (minMax.Length > 1)
            {
                if (minMax.Length > 2)
                {
                    throw new ArgumentException(
                        $"Version range '{versionRange}' is invalid. A maximum of 2 version numbers are allowed in the range.",
                        nameof(versionRange));
                }

                if (minMax[0].Length > 0)
                {
                    minimum = Version.Parse(minMax[0]);
                }

                if (minMax[1].Length > 0)
                {
                    maximum = Version.Parse(minMax[1]);
                }
            }
            else
            {
                minimum = Version.Parse(minMax[0]);
                if (minMax.Length == 1 && isMaximumInclusive)
                {
                    maximum = minimum;
                }
            }

            if (minimum is not null && maximum is not null && minimum > maximum)
            {
                throw new ArgumentException(
                    $"Version range '{versionRange}' is invalid. The minimum version must be less than the maximum version.",
                    nameof(versionRange));
            }

            if (!isMinimumInclusive && maximum is null && minMax.Length == 1)
            {
                // Example: (1.0)
                throw new ArgumentException(
                    $"Version range '{versionRange}' is invalid. The exclusive notation used for both minimum and maximum causes no possible version matches.",
                    nameof(versionRange));
            }

            return new VersionRange
            {
                Minimum = minimum,
                IsMinimumInclusive = isMinimumInclusive,
                Maximum = maximum,
                IsMaximumInclusive = isMaximumInclusive
            };
        }
    }
}
