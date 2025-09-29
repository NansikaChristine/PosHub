using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PosHubApi.Dtos
{
    public class SelectionDto
    {
        public string PosReference { get; set; }
        public int MinPermitted { get; set; }
        public int MaxPermitted { get; set; }
        public List<ModifierDto> Modifiers { get; set; } = new();
        public List<SelectionDto> Selections { get; set; } = new(); 
    }
}