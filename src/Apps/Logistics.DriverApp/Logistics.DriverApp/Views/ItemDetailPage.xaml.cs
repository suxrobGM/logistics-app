using Logistics.DriverApp.ViewModels;
using System.ComponentModel;
using Xamarin.Forms;

namespace Logistics.DriverApp.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}