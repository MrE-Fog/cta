﻿using System.Text;
using CTA.WebForms.FileInformationModel;

namespace CTA.WebForms.Services
{
    public class ProgramCsService
    {
        private const string FileName = "Program.cs";
        private const string ProgramCsTemplate =
@"using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace {0}
{{
    public class Program
    {{
        public static void Main(string[] args)
        {{
            CreateHostBuilder(args).Build().Run();
        }}

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {{
                    webBuilder.UseStartup<Startup>();
                }});
    }}
}}";

        public string ProgramCsNamespace { get; set; } = "Replace_this_with_root_namespace";

        public FileInformation ConstructProgramCsFile()
        {
            // TODO: Allow wider configuration of webBuilder beyond basic UseStartup()
            // TODO: Construct using Roslyn rather than string templates
            var fileContents = string.Format(ProgramCsTemplate, ProgramCsNamespace);
            var fileBytes = Encoding.UTF8.GetBytes(fileContents);

            return new FileInformation(FileName, fileBytes);
        }
    }
}
