﻿using System;
using System.Collections.Generic;
using Codelyzer.Analysis;
using CommandLine;

namespace CTA.Rules.Update
{
    public class Options
    {
        [Option('p', "project-path", Required = true, HelpText = "Project file path.")]
        public string ProjectPath { get; set; }

        [Option('s', "solution-path", Required = false, HelpText = "Solution file path.")]
        public string SolutionPath { get; set; }

        [Option('r', "rules-dir", Required = false, HelpText = "Directory containing rules json input file(s)")]
        public string RulesDir { get; set; }

        [Option('a', "assemblies-dir", Required = false, HelpText = "Action Assemblies Dir")]
        public string AssembliesDir { get; set; }

        [Option('m', "mock-run", Required = false, HelpText = "Mock run to generate output only (no changes will be made)")]
        public string IsMockRun { get; set; }

        [Option('c', "syntax-only", Required = false, HelpText = "Creates an analysis without building")]
        public string SyntaxOnlyAnalysis { get; set; }
    }

    public class RulesCli
    {
        public bool Project;
        public string FilePath;
        public string RulesDir;
        public string AssembliesDir;
        public bool IsMockRun;
        public bool IsSyntaxOnlyBuild;
        public AnalyzerConfiguration Configuration;

        public void HandleCommand(String[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithNotParsed(HandleParseError)
                .WithParsed<Options>(o =>
                {
                    if (!string.IsNullOrEmpty(o.ProjectPath))
                    {
                        Project = true;
                        FilePath = o.ProjectPath;
                    }

                    RulesDir = o.RulesDir;
                    AssembliesDir = o.AssembliesDir;

                    if (!string.IsNullOrEmpty(o.IsMockRun) && o.IsMockRun.ToLower() == "true")
                    {
                        IsMockRun = true;
                    }
                    if (!string.IsNullOrEmpty(o.SyntaxOnlyAnalysis) && o.SyntaxOnlyAnalysis.ToLower() == "true")
                    {
                        IsSyntaxOnlyBuild = true;
                    }
                });
        }

        static void HandleParseError(IEnumerable<Error> errs)
        {
            Environment.Exit(-1);
        }
    }


}
