using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeChan
{
    public class ReplaceDefinition
    {
        public bool Apply { get; set; }
        public string MatchingPattern { get; set; }
        public string ReplaceText { get; set; }

        public ReplaceDefinition Clone()
        {
            return (ReplaceDefinition)MemberwiseClone();
        }
    }
}
