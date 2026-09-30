using System;
using System.Collections.Generic;
using System.Linq;
using DennokoWorks.Tool.PSDExporter.App.Validation.Rules;
using DennokoWorks.Tool.PSDExporter.Model;

namespace DennokoWorks.Tool.PSDExporter.App.Validation
{
    public sealed class ProjectValidator
    {
        private readonly IReadOnlyList<IProjectValidationRule> _rules;

        public ProjectValidator(IEnumerable<IProjectValidationRule> rules)
        {
            _rules = (rules ?? throw new ArgumentNullException(nameof(rules))).ToList();
        }

        public static ProjectValidator CreateDefault()
        {
            return new ProjectValidator(new IProjectValidationRule[]
            {
                new NoEnabledSlotRule(),
                new MissingMaskTextureRule(),
                new EmptyLayerNameRule(),
                new OutputFolderRule(),
                new DuplicateOutputNameRule(),
                new ResolutionMismatchRule(),
            });
        }

        public List<ValidationMessage> Validate(ExportProject project)
        {
            if (project == null) throw new ArgumentNullException(nameof(project));

            return _rules
                .SelectMany(rule => rule.Validate(project))
                .OrderByDescending(m => m.Severity)
                .ToList();
        }

        public static bool HasErrors(IEnumerable<ValidationMessage> messages)
            => messages.Any(m => m.Severity == ValidationSeverity.Error);
    }
}
