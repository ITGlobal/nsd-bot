using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NSD.Bot2.Models
{
    public class KB
    {
        [Key]
        public Guid KBId { get; set; }
        public string KBName { get; set; }

        public KB(Guid id, string name)
        {
            KBId = id;
            KBName = name;
        }

        public KB() { }
    }
}
