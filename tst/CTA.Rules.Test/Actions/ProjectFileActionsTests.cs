﻿using CTA.Rules.Actions;
using CTA.Rules.Config;
using CTA.Rules.Models;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace CTA.Rules.Test.Actions
{
    public class ProjectFileActionsTests
    {
        private string _projectDir;
        private string _projectFile;
        private ProjectFileActions _projectFileActions;

        [SetUp]
        public void SetUp()
        {
            _projectDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _projectFile = Path.Combine(_projectDir, "sample.csproj");

            _projectFileActions = new ProjectFileActions();
        }

        [Test]
        public void ProjectFileCreationNoVersion()
        {
            var result = CreateNewFile(ProjectType.Mvc, new List<string>(), new Dictionary<string, string>(), new List<string>());
            StringAssert.Contains(Constants.DefaultCoreVersion, result);
        }

        [Test]
        public void ProjectFileCreationMvcOneVersion()
        {
            var result = CreateNewFile(ProjectType.Mvc, new List<string>() { SupportedFrameworks.Net5 }, new Dictionary<string, string>(), new List<string>());
            StringAssert.Contains(SupportedFrameworks.Net5, result);
        }

        [Test]
        public void ProjectFileCreationMvcTwoVersions()
        {
            var result = CreateNewFile(ProjectType.Mvc, new List<string>() { SupportedFrameworks.Netcore31, SupportedFrameworks.Net5 }, new Dictionary<string, string>(), new List<string>());
            StringAssert.Contains(SupportedFrameworks.Netcore31, result);
        }

        [Test]
        public void ProjectFileCreationWebClassLibrary()
        {
            var result = CreateNewFile(ProjectType.WebClassLibrary, new List<string>() { SupportedFrameworks.Net5 }, new Dictionary<string, string>(), new List<string>());
            StringAssert.Contains(SupportedFrameworks.Net5, result);
            StringAssert.Contains("Microsoft.AspNetCore.App", result);
        }

        private string CreateNewFile(ProjectType projectType, List<string> targetVersions, Dictionary<string, string> packageReferences, List<string> projectReferences)
        {
            ResetProjectFile();
            var migrationProjectFileAction = _projectFileActions.GetMigrateProjectFileAction("");
            var metaRefs = new List<string>
            {
                @"C:\\RandomFile.dll"
            };
            var result = migrationProjectFileAction(_projectFile, projectType, targetVersions, packageReferences, projectReferences, metaRefs);
            return string.Concat(result, File.ReadAllText(_projectFile));
        }

        private void ResetProjectFile(string newContent = "")
        {
            if (!string.IsNullOrEmpty(newContent))
            {
                File.WriteAllText(_projectFile, newContent);
            }
            else
            {
                File.WriteAllText(_projectFile,
                                @"<?xml version=""1.0"" encoding=""utf-8""?>
<Project ToolsVersion=""Current"" DefaultTargets=""Build"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
</Project>");
            }
        }
    }
}