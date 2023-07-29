using CronusLang.Compiler.Definitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CronusLang.Compiler.Containers
{
    public class SourceMapsContainer
    {
        public HashSet<InstructionsDefinition> Bodies { get; set; } = new HashSet<InstructionsDefinition>();

        public void Register(InstructionsDefinition body, int instructionsStart, int instructionsEnd, int sourceStart, int sourceEnd)
        {
            Register(body, new SourceMapRange(instructionsStart, instructionsEnd), new SourceMapRange(sourceStart, sourceEnd));
        }

        public void Register(InstructionsDefinition body, int instructionsStart, int instructionsEnd, SourceMapRange source)
        {
            Register(body, new SourceMapRange(instructionsStart, instructionsEnd), source);
        }

        public void Register(InstructionsDefinition body, SourceMapRange instructions, SourceMapRange source)
        {
            Register(body, new SourceMapDefinition
            {
                Instructions = instructions,
                Source = source,
            });
        }

        public void Register(InstructionsDefinition body, SourceMapDefinition sourceMap)
        {
            if (!Bodies.Contains(body))
            {
                Bodies.Add(body);
            }

            body.SourceMaps.Add(sourceMap);
        }
    }

    public class SourceMapDefinition
    {
        public SourceMapRange Instructions { get; set; }

        public SourceMapRange Source { get; set; }
    }

    public struct SourceMapRange {
        public int Start;

        public int End;

        public SourceMapRange(int start, int end)
        {
            Start = start;
            End = end;
        }
    }
}
