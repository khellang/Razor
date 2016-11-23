using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Microsoft.VisualStudio.RazorExtension.Behaviors
{
    public static class ItemSelectedBehavior
    {
        public static DependencyProperty ItemSelectedProperty =
            DependencyProperty.RegisterAttached(nameof(Selector.SelectedItem),
                typeof(ICommand),
                typeof(ItemSelectedBehavior),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(ItemSelectedChanged)));

        public static ICommand GetItemSelected(DependencyObject target)
        {
            return (ICommand)target.GetValue(ItemSelectedProperty);
        }

        public static void SetItemSelected(DependencyObject target, ICommand value)
        {
            target.SetValue(ItemSelectedProperty, value);
        }

        private static void ItemSelectedChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            var element = target as Selector;
            if (element != null)
            {
                // If we're putting in a new command and there wasn't one already
                // hook the event
                if ((e.NewValue != null) && (e.OldValue == null))
                {
                    element.SelectionChanged += Selector_SelectionChanged;
                }
                // If we're clearing the command and it wasn't already null
                // unhook the event
                else if ((e.NewValue == null) && (e.OldValue != null))
                {
                    element.SelectionChanged -= Selector_SelectionChanged;
                }
            }
        }

        private static void Selector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var element = sender as Selector;
            if (element != null)
            {
                ICommand command = (ICommand)GetItemSelected(element);
                command.Execute(element.SelectedItem);
            }
        }
    }
}
