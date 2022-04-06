﻿using System.Diagnostics.CodeAnalysis;
using Codelyzer.Analysis;
using Codelyzer.Analysis.Build;
using CTA.Rules.Models;
using CTA.Rules.Update;

namespace CTA.Rules.PortCore
{
    public class PortCoreProjectRewriterFactory : IProjectRewriterFactory
    {
        public ProjectRewriter GetInstance(AnalyzerResult analyzerResult, ProjectConfiguration projectConfiguration)
        {
            var projectType = projectConfiguration.ProjectType;
            var projectRewriter = projectType switch
            {
                ProjectType.WCFCodeBasedService => new WCFProjectRewriter(analyzerResult, projectConfiguration),
                ProjectType.WCFConfigBasedService => new WCFProjectRewriter(analyzerResult, projectConfiguration),
                _ => new ProjectRewriter(analyzerResult, projectConfiguration)
            };
            return projectRewriter;
        }

        [ExcludeFromCodeCoverage]
        public ProjectRewriter GetInstance(IDEProjectResult ideProjectResult, ProjectConfiguration projectConfiguration)
        {
            var projectType = projectConfiguration.ProjectType;
            var projectRewriter = projectType switch
            {
                ProjectType.WCFCodeBasedService => new WCFProjectRewriter(ideProjectResult, projectConfiguration),
                ProjectType.WCFConfigBasedService => new WCFProjectRewriter(ideProjectResult, projectConfiguration),
                _ => new ProjectRewriter(ideProjectResult, projectConfiguration)
            };
            return projectRewriter;
        }
    }
}
