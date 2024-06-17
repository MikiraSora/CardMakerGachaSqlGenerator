using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CardMakerGachaSqlGenerator.UI
{
    public class DropFileBehavior : Behavior<FrameworkElement>
    {
        public ICommand DragEndCommand
        {
            get
            {
                return (ICommand)this.GetValue(DragEndCommandProperty);
            }
            set
            {
                this.SetValue(DragEndCommandProperty, value);
            }
        }

        public static readonly DependencyProperty DragEndCommandProperty = DependencyProperty.Register(
            "DragEndCommand",
            typeof(ICommand),
            typeof(DropFileBehavior),
            new PropertyMetadata(null));

        protected override void OnAttached()
        {
            AssociatedObject.AllowDrop = true;
            AssociatedObject.Drop += DropHandler;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Drop -= DropHandler;
        }

        private void DropHandler(object sender, DragEventArgs e)
        {
            DragEndCommand?.Execute(e);
        }
    }
}
