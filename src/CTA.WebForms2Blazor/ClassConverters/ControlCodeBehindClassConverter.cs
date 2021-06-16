﻿using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace CTA.WebForms2Blazor.ClassConverters
{
    public class ControlCodeBehindClassConverter : ClassConverter
    {
        public ControlCodeBehindClassConverter(
            string relativePath,
            SemanticModel sourceFileSemanticModel,
            INamedTypeSymbol originalClassSymbol)
            : base(relativePath, sourceFileSemanticModel, originalClassSymbol)
        {

        }

        public override async Task MigrateClassAsync()
        {
            throw new NotImplementedException();
        }
    }
}
