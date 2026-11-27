using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Entities
{
    public class ActuatorView
    {
        public string Name { get; set; } = "";
        public string Topic { get; set; } = "";
        public string Kind { get; set; } = "";
        public int X { get; set; }
        public int Y { get; set; }
    }
}
