using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinancialAssistant.Core.Interfaces;
using FinancialAssistant.Core.Models;
using Microsoft.Maui.Controls;

namespace FinancialAssistant.UI.ViewModels;

public partial class ScanReceiptViewModel : BaseViewModel
{
    private readonly IQrScannerService _qrScannerService;
    private readonly IReceiptParser _receiptParser;
    private readonly IFinancialService _financialService;
    private readonly IAuthService _authService;
    private readonly IRepository<Category> _categoryRepo;

    [ObservableProperty]
    public partial string ScanStatus { get; set; } = "Готово к сканированию";

    [ObservableProperty]
    public partial bool IsScanning { get; set; }

    [ObservableProperty]
    public partial ReceiptData? ScannedReceipt { get; set; }

    [ObservableProperty]
    public partial bool IsConfirmVisible { get; set; }

    [ObservableProperty]
    public partial Category? SelectedCategory { get; set; }

    [ObservableProperty]
    public partial string ErrorMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsQrScanMode { get; set; } = true;

    [ObservableProperty]
    public partial string QrResult { get; set; } = string.Empty;

    public ObservableCollection<Category> Categories { get; } = new();

    public ScanReceiptViewModel(
        IQrScannerService qrScannerService,
        IReceiptParser receiptParser,
        IFinancialService financialService,
        IAuthService authService,
        IRepository<Category> categoryRepo)
    {
        _qrScannerService = qrScannerService;
        _receiptParser = receiptParser;
        _financialService = financialService;
        _authService = authService;
        _categoryRepo = categoryRepo;
        Title = "Сканирование чека";
        LoadCategories();
    }

    private async void LoadCategories()
    {
        var all = await _categoryRepo.GetAllAsync();
        Categories.Clear();
        foreach (var c in all.OrderBy(x => x.Name))
            if (c.Type == TransactionType.Expense)
                Categories.Add(c);
    }

    partial void OnQrResultChanged(string value)
    {
        if (string.IsNullOrEmpty(value)) return;

        var (data, error) = _receiptParser.Parse(value);
        if (error != null)
        {
            ErrorMessage = error;
            return;
        }

        ScannedReceipt = data;
        IsQrScanMode = false;
        IsConfirmVisible = true;
        IsScanning = false;
        ScanStatus = "Чек распознан";
    }

    [RelayCommand]
    public async Task ScanAsync()
    {
        var (_, error) = await _qrScannerService.ScanAsync();
        if (error != null)
        {
            ErrorMessage = error;
            return;
        }

        IsScanning = true;
        ScanStatus = "Сканирование...";
        IsQrScanMode = true;
    }

    [RelayCommand]
    public async Task ConfirmAsync()
    {
        if (ScannedReceipt == null) return;

        var user = await _authService.GetCurrentUserAsync();
        if (user == null) return;

        var expense = new Expense
        {
            Amount = ScannedReceipt.TotalAmount,
            Date = ScannedReceipt.PurchaseDate,
            CategoryId = SelectedCategory?.Id ?? 0,
            UserId = user.Id,
            Comment = "Чек: " + ScannedReceipt.ReceiptId,
            ReceiptId = ScannedReceipt.ReceiptId
        };

        await _financialService.AddExpenseAsync(expense);
        await Shell.Current.DisplayAlertAsync("Успех", "Расход добавлен", "OK");

        ScannedReceipt = null;
        IsConfirmVisible = false;
        IsQrScanMode = true;
        QrResult = string.Empty;
        ErrorMessage = string.Empty;
        ScanStatus = "Готово к сканированию";
    }

    [RelayCommand]
    public Task CancelAsync()
    {
        ScannedReceipt = null;
        IsConfirmVisible = false;
        IsQrScanMode = true;
        QrResult = string.Empty;
        ErrorMessage = string.Empty;
        ScanStatus = "Готово к сканированию";
        IsScanning = false;
        return Task.CompletedTask;
    }
}
