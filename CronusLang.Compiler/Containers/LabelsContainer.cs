using CronusLang.Compiler.Definitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CronusLang.Compiler.Containers
{
    public class LabelsContainer
    {
        protected int _labelIdCounter = 1;

        protected Dictionary<int, LabelDefinition> Labels = new Dictionary<int, LabelDefinition>();

        public LabelPlaceholder Create(InstructionsDefinition body, string? tag = null)
        {
            var labelId = _labelIdCounter++;
            
            if (tag != null)
            {
                Labels[labelId] = new LabelDefinition { LabelId = labelId, Tag = tag };
            }

            return new LabelPlaceholder(labelId);
        }

        public void Assign(InstructionsDefinition body, LabelPlaceholder label)
        {
            body.Labels[label.LabelId] = body.Instructions.Count();
        }
    }

    public class LabelDefinition
    {
        public int LabelId { get; set; }

        public string? Tag { get; set; }
    }
}
