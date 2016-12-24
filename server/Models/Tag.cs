using System;
using System.Collections.Generic;

namespace hutel.Models
{
    public class Tag
    {
        public string Id { get; set; }

        public IDictionary<string, Field> Fields { get; set; }

        public class Field
        {
            public string Name { get; set; }

            public Type Type { get; set; }

        }
    }
}