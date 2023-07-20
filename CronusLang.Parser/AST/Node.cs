using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sawmill;
using Newtonsoft.Json;

namespace CronusLang.Parser.AST
{
    public abstract class Node : IRewritable<Node>
    {
        [JsonIgnore]
        public LocationSpan Location { get; protected set; }

        public Node(LocationSpan locationSpan)
        {
            Location = locationSpan;
        }

        abstract public int CountChildren();

        abstract public void GetChildren(Span<Node> childrenReceiver);

        abstract public Node SetChildren(ReadOnlySpan<Node> newChildren);

        public string ToJSON()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
            });
            //return $"{GetType().Name}({string.Join(", ", this.GetChildren().Select(c => c?.ToString()))})";
        }
    }

    public struct Location
    {
        public int Index;

        public int Line;

        public int Column;

        public Location()
        {
            Index = 0;
            Line = 0;
            Column = 0;
        }

        public Location(int index, int line, int column)
        {
            Index = index;
            Line = line;
            Column = column;
        }

        public Location(Pegasus.Common.Cursor cursor)
        {
            Index = cursor.Location;
            Line = cursor.Line;
            Column = cursor.Column;
        }

        public static Location Empty => new Location
        {
            Index = 0,
            Line = 0,
            Column = 0,
        };

        public static LocationSpan operator +(Location left, Location right) => new LocationSpan
        {
            Start = left,
            End = right,
        };

        public static LocationSpan operator +(Location left, LocationSpan right) => new LocationSpan
        {
            Start = left,
            End = right.End,
        };
    }

    public struct LocationSpan
    {
        public Location Start;

        public Location End;

        public static LocationSpan Empty => new LocationSpan
        {
            Start = Location.Empty,
            End = Location.Empty,
        };

        public static LocationSpan operator +(LocationSpan left, LocationSpan right) => new LocationSpan
        {
            Start = left.Start,
            End = right.End,
        };

        public static LocationSpan operator +(LocationSpan left, Location right) => new LocationSpan
        {
            Start = left.Start,
            End = right,
        };
    }
}
