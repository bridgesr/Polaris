using coordinator.Domain.Tracker;

namespace coordinator.Services.DocumentToggle.Domain
{
    public class Definition
    {
        public DefinitionType Type { get; set; }

        public DefinitionLevel Level { get; set; }

        public string Identifier { get; set; }
    }
}