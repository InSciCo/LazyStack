﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Linq;
using Microsoft.DotNet.Cli.Sln.Internal.Lz;

// Modified for LazyStack
namespace Microsoft.DotNet.Tools.Common.Lz
{
    public static class SlnFileFactory
    {
        public static SlnFile CreateFromFileOrDirectory(string fileOrDirectory)
        {
            if (File.Exists(fileOrDirectory))
            {
                return FromFile(fileOrDirectory);
            }
            else
            {
                return FromDirectory(fileOrDirectory);
            }
        }

        private static SlnFile FromFile(string solutionPath)
        {
            SlnFile slnFile = null;
            try
            {
                slnFile = SlnFile.Read(solutionPath);
            }
            catch (InvalidSolutionFormatException e)
            {
                throw new Exception(
                    "InvalidSolutionFormatString " + e.Message);
            }
            return slnFile;
        }

        private static SlnFile FromDirectory(string solutionDirectory)
        {
            DirectoryInfo dir;
            try
            {
                dir = new DirectoryInfo(solutionDirectory);
                if (!dir.Exists)
                {
                    throw new Exception(
                        "CouldNotFindSolutionOrDirectory " + 
                        solutionDirectory);
                }
            }
            catch (ArgumentException)
            {
                throw new Exception(
                    "CouldNotFindSolutionOrDirectory " +
                    solutionDirectory);
            }

            FileInfo[] files = dir.GetFiles("*.sln");
            if (files.Length == 0)
            {
                throw new Exception(
                    "CouldNotFindSolutionIn " + 
                    solutionDirectory);
            }

            if (files.Length > 1)
            {
                throw new Exception("MoreThanOneSolutionInDirectory " + 
                    solutionDirectory);
            }

            FileInfo solutionFile = files.Single();
            if (!solutionFile.Exists)
            {
                throw new Exception("CouldNotFindSolutionIn " + 
                    solutionDirectory);
            }

            return FromFile(solutionFile.FullName);
        }
    }
}
