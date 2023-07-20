using CronusLang.Parser.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CronusLang.Compiler
{
    public class DiagnosticMessage
    {
        public DiagnosticLevel Level { get; set; }

        public string Message { get; set; }

        public LocationSpan? Location { get; set; }

        public DiagnosticMessage(DiagnosticLevel level, string message, LocationSpan? location = null)
        {
            Level = level;
            Message = message;
            Location = location;
        }
    }

    public enum DiagnosticLevel
    {
        Error,
        Warning,
        Information,
    }
}
