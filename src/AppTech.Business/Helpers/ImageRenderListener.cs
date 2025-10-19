using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace AppTech.Business.Helpers
{
    public class ImageRenderListener : IEventListener
    {
        private List<byte[]> images = new List<byte[]>();

        public void EventOccurred(IEventData data, EventType type)
        {
            if (type == EventType.RENDER_IMAGE)
            {
                var renderInfo = (ImageRenderInfo)data;
                var image = renderInfo.GetImage();
                var imageBytes = image.GetImageBytes(true);
                images.Add(imageBytes);
            }
        }

        public ICollection<EventType> GetSupportedEvents()
        {
            return null;
        }

        public List<byte[]> GetImages()
        {
            return images;
        }
    }
}
