﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTA.Rules.Config;
using CTA.Rules.Models;
using CTA.WebForms.Extensions;
using CTA.WebForms.FileInformationModel;
using CTA.WebForms.Helpers;
using CTA.WebForms.Metrics;
using CTA.WebForms.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CTA.WebForms.ClassConverters
{
    public class PageCodeBehindClassConverter : ClassConverter
    {
        private const string ActionName = "PageCodeBehindClassConverter";
        private WebFormMetricContext _metricsContext;

        private IDictionary<BlazorComponentLifecycleEvent, IEnumerable<StatementSyntax>> _newLifecycleLines;

        public PageCodeBehindClassConverter(
            string relativePath,
            string sourceProjectPath,
            SemanticModel sourceFileSemanticModel,
            TypeDeclarationSyntax originalDeclarationSyntax,
            INamedTypeSymbol originalClassSymbol,
            TaskManagerService taskManager,
            WebFormMetricContext metricsContext)
            : base(relativePath, sourceProjectPath, sourceFileSemanticModel, originalDeclarationSyntax, originalClassSymbol, taskManager)
        {
            _newLifecycleLines = new Dictionary<BlazorComponentLifecycleEvent, IEnumerable<StatementSyntax>>();
            _metricsContext = metricsContext;
        }

        public override Task<IEnumerable<FileInformation>> MigrateClassAsync()
        {
            LogStart();

            _metricsContext.CollectActionMetrics(WebFormsActionType.ClassConversion, ActionName);
            // NOTE: Removed temporarily until usings can be better determined, at the moment, too
            // many are being removed
            // var requiredNamespaceNames = _sourceFileSemanticModel
            //     .GetNamespacesReferencedByType(_originalDeclarationSyntax)
            //     .Select(namespaceSymbol => namespaceSymbol.ToDisplayString())
            //     // This is so we can use ComponentBase base class
            //     .Append(Constants.BlazorComponentsNamespace);

            var requiredNamespaceNames = _sourceFileSemanticModel.GetOriginalUsingNamespaces().Append(Constants.BlazorComponentsNamespace);
            requiredNamespaceNames = CodeSyntaxHelper.RemoveFrameworkUsings(requiredNamespaceNames);
            var allMethods = _originalDeclarationSyntax.DescendantNodes().OfType<MethodDeclarationSyntax>();
            var currentClassDeclaration = ((ClassDeclarationSyntax)_originalDeclarationSyntax)
                // Need to track methods so modifications can be made one after another
                .TrackNodes(allMethods)
                // Remove outdated base type references
                // TODO: Scan and remove specific base types in the future
                .ClearBaseTypes()
                // ComponentBase base class is required to use lifecycle events
                .AddBaseType(Constants.ComponentBaseClass);

            var orderedMethods = allMethods
                .Select(method => (method, LifecycleManagerService.CheckMethodPageLifecycleHook(method)))
                // Filter out non-lifecycle methods
                .Where(methodTuple => methodTuple.Item2 != null)
                // Order matters within new events so we order before processing
                .OrderBy(methodTuple =>
                {
                    return (int)methodTuple.Item2;
                });

            // Remove old lifecycle methods, sort, and record their content
            foreach (var methodTuple in orderedMethods)
            {
                try
                {
                    // This records the statements in the proper collection
                    ProcessLifecycleEventMethod(methodTuple.Item1, (WebFormsPageLifecycleEvent)methodTuple.Item2);
                }
                catch (Exception e)
                {
                    LogHelper.LogError(e, $"{Rules.Config.Constants.WebFormsErrorTag}Failed to process WebForms lifecycle event method {methodTuple.Item1.Identifier} " +
                        $"from {OriginalClassName} class at {_fullPath}");
                }

                // Refresh node before removing
                var currentMethodNode = currentClassDeclaration.GetCurrentNode(methodTuple.Item1);
                currentClassDeclaration = currentClassDeclaration.RemoveNode(currentMethodNode, SyntaxRemoveOptions.AddElasticMarker);
            }

            // Construct new lifecycle methods and add them to the class
            foreach (var newLifecycleEventKvp in _newLifecycleLines)
            {
                var newLifecycleEvent = newLifecycleEventKvp.Key;
                var newLifecycleEventStatements = newLifecycleEventKvp.Value;

                try
                {
                    var newMethodDeclaration = ComponentSyntaxHelper.ConstructComponentLifecycleMethod(newLifecycleEvent, newLifecycleEventStatements);
                    currentClassDeclaration = currentClassDeclaration.AddMembers(newMethodDeclaration);
                }
                catch (Exception e)
                {
                    LogHelper.LogError(e, $"{Rules.Config.Constants.WebFormsErrorTag}Failed to construct new lifecycle event method for {newLifecycleEvent} Blazor event " +
                        $"using {OriginalClassName} class at {_fullPath}");
                }
            }

            // If we need to make use of the dispose method, add the IDisposable
            // interface to the class, usings are fine as is because this come from
            // the System namespace
            if (_newLifecycleLines.ContainsKey(BlazorComponentLifecycleEvent.Dispose))
            {
                currentClassDeclaration = currentClassDeclaration.AddBaseType(Constants.DisposableInterface);
            }

            var namespaceNode = CodeSyntaxHelper.BuildNamespace(_originalClassSymbol.ContainingNamespace?.ToDisplayString(), currentClassDeclaration);
            var fileText = CodeSyntaxHelper.GetFileSyntaxAsString(namespaceNode, CodeSyntaxHelper.BuildUsingStatements(requiredNamespaceNames));

            DoCleanUp();
            LogEnd();

            var result = new[] { new FileInformation(GetNewRelativePath(), Encoding.UTF8.GetBytes(fileText)) };

            return Task.FromResult((IEnumerable<FileInformation>)result);
        }

        private string GetNewRelativePath()
        {
            // TODO: Potentially remove certain folders from beginning of relative path
            var newRelativePath = FilePathHelper.AlterFileName(_relativePath,
                oldExtension: Constants.PageCodeBehindExtension,
                newExtension: Constants.RazorCodeBehindFileExtension);

            return FilePathHelper.RemoveDuplicateDirectories(Path.Combine(Constants.RazorPageDirectoryName, newRelativePath));
        }

        private void ProcessLifecycleEventMethod(MethodDeclarationSyntax methodDeclaration, WebFormsPageLifecycleEvent lifecycleEvent)
        {
            var statements = (IEnumerable<StatementSyntax>)methodDeclaration.Body.Statements;

            // Dont do anything if the method is empty, no reason to move over nothing
            if (statements.Any())
            {
                statements = statements.AddComment(string.Format(Constants.NewEventRepresentationCommentTemplate, lifecycleEvent.ToString()));

                var blazorLifecycleEvent = LifecycleManagerService.GetEquivalentComponentLifecycleEvent(lifecycleEvent);

                if (_newLifecycleLines.ContainsKey(blazorLifecycleEvent))
                {
                    // Add spacing between last added method
                    statements = statements.Prepend(CodeSyntaxHelper.GetBlankLine());
                    _newLifecycleLines[blazorLifecycleEvent] = _newLifecycleLines[blazorLifecycleEvent].Concat(statements);
                }
                else
                {
                    _newLifecycleLines.Add(blazorLifecycleEvent, statements);
                }
            }
        }
    }
}
