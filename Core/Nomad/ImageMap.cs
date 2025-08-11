using System.Drawing;

namespace FC2Editor.Core.Nomad
{
    internal class ImageMap
    {
        public Image Image { get; }
        public float Minimum { get; }
        public float Maximum { get; }

        public ImageMap(Image img, float min, float max)
        {
            Image = img;
            Minimum = min;
            Maximum = max;
        }

        public void Dispose()
        {
            Image.Dispose();
        }
    }
}