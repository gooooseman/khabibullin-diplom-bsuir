using FinancialAssistant.UI.ViewModels;
using ZXing.Net.Maui;

namespace FinancialAssistant.UI.Views;

public partial class ScanReceiptPage : ContentPage
{
    public ScanReceiptViewModel ViewModel => (ScanReceiptViewModel)BindingContext;

    public ScanReceiptPage(ScanReceiptViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        barcodeReader.BarcodesDetected += OnBarcodesDetected;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        barcodeReader.BarcodesDetected -= OnBarcodesDetected;
    }

    private void OnBarcodesDetected(object? sender, BarcodeDetectionEventArgs e)
    {
        var value = e.Results.FirstOrDefault()?.Value ?? string.Empty;
        if (!string.IsNullOrEmpty(value))
            MainThread.BeginInvokeOnMainThread(() => ViewModel.QrResult = value);
    }
}
