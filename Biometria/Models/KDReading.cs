using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biometria.Models
{
    public class KDReading
    {
        public string Name { get; set; }
        public int[] LetterMeasurements { get; set; } = new int[26];  
        public override string ToString()
        {
            return string.Join(" ", LetterMeasurements);
        }
    }
}
