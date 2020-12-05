// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Exceptions;
using Microsoft.Build.Execution;
using Microsoft.DotNet.Cli.Sln.Internal.Lz;
using Microsoft.DotNet.Tools.ProjectExtensions.Lz;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// Modified for LazyStack
namespace Microsoft.DotNet.Tools.Common.Lz
{


    public static class SlnFileExtensions
    {

        public static bool ProjectExists(this SlnFile slnFile, string projectName)
        {
            foreach (var project in slnFile.Projects)
                if (project.Name.Equals(projectName))
                    return true;
            return false;
        }

        public static SlnProject GetProjectByName(this SlnFile slnFile, string projectName)
        {
            foreach (var project in slnFile.Projects)
                if (project.Name.Equals(projectName))
                    return project;
            return null;
        }

        public static void AddProject(this SlnFile slnFile, string fullProjectPath, string relativeProjectPath, string solutionFolder)
        {
            if (string.IsNullOrEmpty(fullProjectPath))
                throw new ArgumentException();

            var rootElement = ProjectRootElement.Open(relativeProjectPath);

            var projectInstance = new ProjectInstance(rootElement);
                

            var slnProject = new SlnProject
            {
                Id = projectInstance.GetProjectId(),
                TypeGuid = rootElement.GetProjectTypeGuid() ?? projectInstance.GetDefaultProjectTypeGuid(),
                Name = Path.GetFileNameWithoutExtension(relativeProjectPath),
                FilePath = relativeProjectPath
            };

            if (string.IsNullOrEmpty(slnProject.TypeGuid))
                throw new Exception("Invalid TypeGuid");

            // NOTE: The order you create the sections determines the order they are written to the sln
            // file. In the case of an empty sln file, in order to make sure the solution configurations
            // section comes first we need to add it first. This doesn't affect correctness but does
            // stop VS from re-ordering things later on. Since we are keeping the SlnFile class low-level
            // it shouldn't care about the VS implementation details. That's why we handle this here.
            slnFile.AddDefaultBuildConfigurations();

            slnFile.MapSolutionConfigurationsToProject(
                projectInstance,
                slnFile.ProjectConfigurationsSection.GetOrCreatePropertySet(slnProject.Id));

            if (!string.IsNullOrEmpty(solutionFolder))
                slnFile.AddSolutionFolderProject(slnProject, solutionFolder);

            slnFile.Projects.Add(slnProject);

        }

        private static void AddDefaultBuildConfigurations(this SlnFile slnFile)
        {
            var configurationsSection = slnFile.SolutionConfigurationsSection;

            if (!configurationsSection.IsEmpty)
            {
                return;
            }

            var defaultConfigurations = new List<string>()
            {
                "Debug|Any CPU",
                "Debug|x64",
                "Debug|x86",
                "Release|Any CPU",
                "Release|x64",
                "Release|x86",
            };

            foreach (var config in defaultConfigurations)
            {
                configurationsSection[config] = config;
            }
        }

        private static void MapSolutionConfigurationsToProject(
            this SlnFile slnFile,
            ProjectInstance projectInstance,
            SlnPropertySet solutionProjectConfigs)
        {
            var (projectConfigurations, defaultProjectConfiguration) = GetKeysDictionary(projectInstance.GetConfigurations());
            var (projectPlatforms, defaultProjectPlatform) = GetKeysDictionary(projectInstance.GetPlatforms());

            foreach (var solutionConfigKey in slnFile.SolutionConfigurationsSection.Keys)
            {
                var projectConfigKey = MapSolutionConfigKeyToProjectConfigKey(
                    solutionConfigKey,
                    projectConfigurations,
                    defaultProjectConfiguration,
                    projectPlatforms,
                    defaultProjectPlatform);
                if (projectConfigKey == null)
                {
                    continue;
                }

                var activeConfigKey = $"{solutionConfigKey}.ActiveCfg";
                if (!solutionProjectConfigs.ContainsKey(activeConfigKey))
                {
                    solutionProjectConfigs[activeConfigKey] = projectConfigKey;
                }

                var buildKey = $"{solutionConfigKey}.Build.0";
                if (!solutionProjectConfigs.ContainsKey(buildKey))
                {
                    solutionProjectConfigs[buildKey] = projectConfigKey;
                }
            }
        }

        private static (Dictionary<string, string> Keys, string DefaultKey) GetKeysDictionary(IEnumerable<string> keys)
        {
            // A dictionary mapping key -> key is used instead of a HashSet so the original case of the key can be retrieved from the set
            var dictionary = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);

            foreach (var key in keys)
            {
                dictionary[key] = key;
            }

            return (dictionary, keys.FirstOrDefault());
        }

        private static string GetMatchingProjectKey(IDictionary<string, string> projectKeys, string solutionKey)
        {
            if (projectKeys.TryGetValue(solutionKey, out string projectKey))
                return projectKey;

            var keyWithoutWhitespace = String.Concat(solutionKey.Where(c => !Char.IsWhiteSpace(c)));
            if (projectKeys.TryGetValue(keyWithoutWhitespace, out projectKey))
                return projectKey;

            return null;
        }

        private static string MapSolutionConfigKeyToProjectConfigKey(
            string solutionConfigKey,
            Dictionary<string, string> projectConfigurations,
            string defaultProjectConfiguration,
            Dictionary<string, string> projectPlatforms,
            string defaultProjectPlatform)
        {
            var pair = solutionConfigKey.Split(new char[] {'|'}, 2);
            if (pair.Length != 2)
            {
                return null;
            }

            var projectConfiguration = GetMatchingProjectKey(projectConfigurations, pair[0]) ?? defaultProjectConfiguration;
            if (projectConfiguration == null)
            {
                return null;
            }

            var projectPlatform = GetMatchingProjectKey(projectPlatforms, pair[1]) ?? defaultProjectPlatform;
            if (projectPlatform == null)
            {
                return null;
            }

            // VS stores "Any CPU" platform in the solution regardless of how it is named at the project level
            return $"{projectConfiguration}|{(projectPlatform == "AnyCPU" ? "Any CPU" : projectPlatform)}";
        }

        private static void AddSolutionFolderProject(this SlnFile slnFile, SlnProject slnProject, string solutionFolder)
        {
            var nestedProjectsSection = slnFile.Sections.GetOrCreateSection(
                "NestedProjects",
                SlnSectionType.PreProcess);

            var pathToGuidMap = slnFile.GetSolutionFolderPaths(nestedProjectsSection.Properties);

            string parentDirGuid = null;
            var solutionFolderHierarchy = string.Empty;
            solutionFolderHierarchy = Path.Combine(solutionFolderHierarchy, solutionFolder);
            if (pathToGuidMap.ContainsKey(solutionFolderHierarchy))
            {
                parentDirGuid = pathToGuidMap[solutionFolderHierarchy];
            }
            else
            {
                var solutionFolderProject = new SlnProject
                {
                    Id = Guid.NewGuid().ToString("B").ToUpper(),
                    TypeGuid = ProjectTypeGuids.SolutionFolderGuid,
                    Name = solutionFolder,
                    FilePath = solutionFolder
                };

                slnFile.Projects.Add(solutionFolderProject);

                if (parentDirGuid != null)
                {
                    nestedProjectsSection.Properties[solutionFolderProject.Id] = parentDirGuid;
                }
                parentDirGuid = solutionFolderProject.Id;
            }

            nestedProjectsSection.Properties[slnProject.Id] = parentDirGuid;
        }

        public static void AddSolutionFolder(this SlnFile slnFile, string folderName)
        {
            var solutionFolderProject = new SlnProject
            {
                Id = Guid.NewGuid().ToString("B").ToUpper(),
                TypeGuid = ProjectTypeGuids.SolutionFolderGuid,
                Name = folderName,
                FilePath = folderName
            };
            
            slnFile.Projects.Add(solutionFolderProject);

        }
        private static IDictionary<string, string> GetSolutionFolderPaths(
            this SlnFile slnFile,
            SlnPropertySet nestedProjects)
        {
            var solutionFolderPaths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var solutionFolderProjects = slnFile.Projects.GetProjectsByType(ProjectTypeGuids.SolutionFolderGuid);
            foreach (var slnProject in solutionFolderProjects)
            {
                var path = slnProject.FilePath;
                var id = slnProject.Id;
                while (nestedProjects.ContainsKey(id))
                {
                    id = nestedProjects[id];
                    var parentSlnProject = solutionFolderProjects.Where(p => p.Id == id).Single();
                    path = Path.Combine(parentSlnProject.FilePath, path);
                }

                solutionFolderPaths[path] = slnProject.Id;
            }

            return solutionFolderPaths;
        }

        public static bool RemoveProject(this SlnFile slnFile, string projectPath)
        {
            if (string.IsNullOrEmpty(projectPath))
            {
                throw new ArgumentException();
            }

            var projectPathNormalized = PathUtility.GetPathWithDirectorySeparator(projectPath);

            var projectsToRemove = slnFile.Projects.Where((p) =>
                    string.Equals(p.FilePath, projectPathNormalized, StringComparison.OrdinalIgnoreCase)).ToList();

            bool projectRemoved = false;
            if (projectsToRemove.Count == 0)
            {
                throw new Exception("ProjectNotFoundInTheSolution " + 
                    projectPath);
            }
            else
            {
                foreach (var slnProject in projectsToRemove)
                {
                    var buildConfigsToRemove = slnFile.ProjectConfigurationsSection.GetPropertySet(slnProject.Id);
                    if (buildConfigsToRemove != null)
                    {
                        slnFile.ProjectConfigurationsSection.Remove(buildConfigsToRemove);
                    }

                    var nestedProjectsSection = slnFile.Sections.GetSection(
                        "NestedProjects",
                        SlnSectionType.PreProcess);
                    if (nestedProjectsSection != null && nestedProjectsSection.Properties.ContainsKey(slnProject.Id))
                    {
                        nestedProjectsSection.Properties.Remove(slnProject.Id);
                    }

                    slnFile.Projects.Remove(slnProject);
                }

                foreach (var project in slnFile.Projects)
                {
                    var dependencies = project.Dependencies;
                    if (dependencies == null)
                    {
                        continue;
                    }

                    dependencies.SkipIfEmpty = true;

                    foreach (var removed in projectsToRemove)
                    {
                        dependencies.Properties.Remove(removed.Id);
                    }
                }

                projectRemoved = true;
            }

            return projectRemoved;
        }

        public static void RemoveEmptyConfigurationSections(this SlnFile slnFile)
        {
            if (slnFile.Projects.Count == 0)
            {
                var solutionConfigs = slnFile.Sections.GetSection("SolutionConfigurationPlatforms");
                if (solutionConfigs != null)
                {
                    slnFile.Sections.Remove(solutionConfigs);
                }

                var projectConfigs = slnFile.Sections.GetSection("ProjectConfigurationPlatforms");
                if (projectConfigs != null)
                {
                    slnFile.Sections.Remove(projectConfigs);
                }
            }
        }

        public static void RemoveEmptySolutionFolders(this SlnFile slnFile)
        {
            var solutionFolderProjects = slnFile.Projects
                .GetProjectsByType(ProjectTypeGuids.SolutionFolderGuid)
                .ToList();

            if (solutionFolderProjects.Any())
            {
                var nestedProjectsSection = slnFile.Sections.GetSection(
                    "NestedProjects",
                    SlnSectionType.PreProcess);

                if (nestedProjectsSection == null)
                {
                    foreach (var solutionFolderProject in solutionFolderProjects)
                    {
                        if (solutionFolderProject.Sections.Count() == 0)
                        {
                            slnFile.Projects.Remove(solutionFolderProject);
                        }
                    }
                }
                else
                {
                    var solutionFoldersInUse = slnFile.GetSolutionFoldersThatContainProjectsInItsHierarchy(
                        nestedProjectsSection.Properties);

                    solutionFoldersInUse.UnionWith(slnFile.GetSolutionFoldersThatContainSolutionItemsInItsHierarchy(
                        nestedProjectsSection.Properties));

                    foreach (var solutionFolderProject in solutionFolderProjects)
                    {
                        if (!solutionFoldersInUse.Contains(solutionFolderProject.Id))
                        {
                            nestedProjectsSection.Properties.Remove(solutionFolderProject.Id);
                            if (solutionFolderProject.Sections.Count() == 0)
                            {
                                slnFile.Projects.Remove(solutionFolderProject);
                            }
                        }
                    }

                    if (nestedProjectsSection.IsEmpty)
                    {
                        slnFile.Sections.Remove(nestedProjectsSection);
                    }
                }
            }
        }

        private static HashSet<string> GetSolutionFoldersThatContainProjectsInItsHierarchy(
            this SlnFile slnFile,
            SlnPropertySet nestedProjects)
        {
            var solutionFoldersInUse = new HashSet<string>();

            var nonSolutionFolderProjects = slnFile.Projects.GetProjectsNotOfType(
                ProjectTypeGuids.SolutionFolderGuid);

            foreach (var nonSolutionFolderProject in nonSolutionFolderProjects)
            {
                var id = nonSolutionFolderProject.Id;
                while (nestedProjects.ContainsKey(id))
                {
                    id = nestedProjects[id];
                    solutionFoldersInUse.Add(id);
                }
            }

            return solutionFoldersInUse;
        }

        private static HashSet<string> GetSolutionFoldersThatContainSolutionItemsInItsHierarchy(
            this SlnFile slnFile,
            SlnPropertySet nestedProjects)
        {
            var solutionFoldersInUse = new HashSet<string>();

            var solutionItemsFolderProjects = slnFile.Projects
                    .GetProjectsByType(ProjectTypeGuids.SolutionFolderGuid)
                    .Where(ContainsSolutionItems);

            foreach (var solutionItemsFolderProject in solutionItemsFolderProjects)
            {
                var id = solutionItemsFolderProject.Id;
                solutionFoldersInUse.Add(id);

                while (nestedProjects.ContainsKey(id))
                {
                    id = nestedProjects[id];
                    solutionFoldersInUse.Add(id);
                }
            }

            return solutionFoldersInUse;
        }

        private static bool ContainsSolutionItems(SlnProject project)
        {
            return project.Sections
                .GetSection("SolutionItems", SlnSectionType.PreProcess) != null;
        }
    }
}
