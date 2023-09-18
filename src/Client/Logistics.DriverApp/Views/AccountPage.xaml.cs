using Syncfusion.Maui.DataForm;

namespace Logistics.DriverApp.Views;

public partial class AccountPage
{
	public AccountPage()
	{
		InitializeComponent();
	}

    private void SfDataForm_GenerateDataFormItem(object sender, GenerateDataFormItemEventArgs e)
    {
        if (e.DataFormItem.FieldName == "PhoneNumber" && 
            e.DataFormItem is DataFormMaskedTextItem phoneNumberMasked)
        {
            phoneNumberMasked.Keyboard = Keyboard.Numeric;
            phoneNumberMasked.MaskType = MaskedEditorMaskType.Simple;
            phoneNumberMasked.Mask = "+1 (000) 000 0000";
        }
    }
}
