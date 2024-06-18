using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proiect_Licenta.Models
{
    internal class FacePrediction
    {
        [ColumnName("model_outputs0")]
        public float[] Face { get; set; }
    }
}
