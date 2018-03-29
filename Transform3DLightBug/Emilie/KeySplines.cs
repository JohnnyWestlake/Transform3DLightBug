//*********************************************************
// 
// PROJECT EMILIE
//
// Copyright (c) Johnny Westlake. All rights reserved.
//
// This code is licensed under the MIT License (MIT).
// THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, 
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, 
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH 
// THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
//*********************************************************

using Windows.UI.Xaml.Media.Animation;

namespace Emilie.UWP.Media
{
    public static class KeySplines
    {
        /// <summary>
        /// Returns a KeySpline for use as an easing function
        /// to replicate the easing of the EntranceThemeTransition
        /// </summary>
        /// <returns></returns>
        public static KeySpline EntranceTheme => Animation.CreateKeySpline(0.1, 0.9, 0.2, 1);

        public static KeySpline PerspectiveZoomOpacity => Animation.CreateKeySpline(0.2, 0.6, 0.3, 0.9);

        /// <summary>
        /// A more precise alternative to EntranceTheme         /// Returns a KeySpline for use as an easing function

        /// </summary>
        public static KeySpline Popup => Animation.CreateKeySpline(0.100000001490116, 0.899999976158142, 0.200000002980232, 1);
      
    }
}
