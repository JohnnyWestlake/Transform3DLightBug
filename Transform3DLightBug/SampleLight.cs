using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Transform3DLightBug
{
    public class SampleLight : XamlLight
    {
        protected override void OnConnected(UIElement newElement)
        {
            Compositor compositor = Window.Current.Compositor;

            // Create AmbientLight and set its properties
            var light = compositor.CreatePointLight();
            light.Color = Colors.White;
            light.Intensity = 1.5f;
            light.Offset = new System.Numerics.Vector3(75);

            // Associate CompositionLight with XamlLight
            CompositionLight = light;

            // Add UIElement to the Light's Targets
            SampleLight.AddTargetElement(GetId(), newElement);
        }

        protected override void OnDisconnected(UIElement oldElement)
        {
            // Dispose Light when it is removed from the tree
            SampleLight.RemoveTargetElement(GetId(), oldElement);
            CompositionLight.Dispose();
        }

        protected override string GetId() => typeof(SampleLight).FullName;
    }
}
