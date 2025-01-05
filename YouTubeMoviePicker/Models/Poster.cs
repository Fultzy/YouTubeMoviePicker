using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace YouTubeMoviePicker.Models;
public class Poster
{
    public string YTVideoid { get; set; }
    public string Url { get; set; }
    public BitmapImage Image { get; set; }
}
