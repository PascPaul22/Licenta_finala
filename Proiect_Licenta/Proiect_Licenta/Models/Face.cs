using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Image;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proiect_Licenta.Models
{
    public class Face
    {
        [ImageType(ImageSetings.imageHeight, ImageSetings.imageWidth)]
        public Bitmap Image { get; set; }
    }
}
