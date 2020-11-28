﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization.ObjectGraphVisitors;

namespace LazyStack
{
    public static class Utilities
    {

        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs, bool overwrite, bool removeTplExtension = false, Dictionary<string,string> replacements = null)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(
                    destDirName, 
                    (removeTplExtension && file.Name.EndsWith(".tpl")) 
                        ? Path.GetFileNameWithoutExtension(file.Name) 
                        : file.Name
                    );
                file.CopyTo(temppath, overwrite);
                if(replacements != null && replacements.Count > 0)
                {
                    var fileText = File.ReadAllText(temppath);
                    foreach (var kvp in replacements)
                        fileText = fileText.Replace(kvp.Key, kvp.Value);
                    File.WriteAllText(temppath, fileText);
                }
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs, overwrite, removeTplExtension);
                }
            }
        }
    }
}
