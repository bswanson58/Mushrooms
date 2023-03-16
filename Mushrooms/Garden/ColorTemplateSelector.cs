using System.Windows;
using System.Windows.Controls;
using Mushrooms.Models;
using ReusableBits.Wpf.Utility;

namespace Mushrooms.Garden {
    internal class ColorTemplateSelector : DataTemplateSelector {
        public  DataTemplate ?  InactiveTemplate { get; set; }
        public  DataTemplate ?  ActiveTemplate { get; set; }

        public override DataTemplate ? SelectTemplate( object item, DependencyObject container ) {
            var userControl = container.FindParent<ItemsControl>();

            if( userControl is { DataContext: GardenSceneViewModel gardenScene }) {
                return gardenScene.IsSceneOn ? 
                    ActiveTemplate : 
                    InactiveTemplate;
            }

            return InactiveTemplate;
        }
    }
}
