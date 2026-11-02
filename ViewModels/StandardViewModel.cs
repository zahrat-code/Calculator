using System.Globalization;
using System.Windows.Input;

namespace WindowsCalculatorMaui.ViewModels;

public class StandardViewModel : BaseViewModel
{
    // ── State ──────────────────────────────────────────────────────
    protected string  _displayText    = "0";
    protected string  _expressionText = string.Empty;
    protected double  _accumulator    = 0;
    protected string? _pendingOp      = null;
    protected bool    _isNewEntry     = true;
    protected bool    _hasError       = false;
    protected double? _lastOperand    = null;

    private double _memoryValue = 0;
    private bool   _hasMemory   = false;

    // ── Bindable Properties ────────────────────────────────────────
    public string DisplayText
    {
        get => _displayText;
        set => SetProperty(ref _displayText, value);
    }

    public string ExpressionText
    {
        get => _expressionText;
        set => SetProperty(ref _expressionText, value);
    }

    public bool HasMemory
    {
        get => _hasMemory;
        set
        {
            if (SetProperty(ref _hasMemory, value))
                RefreshMemoryCanExecute();
        }
    }

    // ── Commands ───────────────────────────────────────────────────
    public ICommand InputDigitCommand   { get; }
    public ICommand InputDecimalCommand { get; }
    public ICommand OperationCommand    { get; }
    public ICommand EqualsCommand       { get; }
    public ICommand ClearCommand        { get; }
    public ICommand ClearEntryCommand   { get; }
    public ICommand BackspaceCommand    { get; }
    public ICommand NegateCommand       { get; }
    public ICommand UnaryCommand        { get; }

    public ICommand MemoryClearCommand    { get; }
    public ICommand MemoryRecallCommand   { get; }
    public ICommand MemoryStoreCommand    { get; }
    public ICommand MemoryAddCommand      { get; }
    public ICommand MemorySubtractCommand { get; }
    public ICommand MemoryViewCommand     { get; }

    public StandardViewModel()
    {
        InputDigitCommand   = new Command<string>(OnInputDigit);
        InputDecimalCommand = new Command(OnInputDecimal);
        OperationCommand    = new Command<string>(OnOperation);
        EqualsCommand       = new Command(OnEquals);
        ClearCommand        = new Command(OnClear);
        ClearEntryCommand   = new Command(OnClearEntry);
        BackspaceCommand    = new Command(OnBackspace);
        NegateCommand       = new Command(OnNegate);
        UnaryCommand        = new Command<string>(OnUnary);

        MemoryClearCommand    = new Command(OnMemoryClear,    () => HasMemory);
        MemoryRecallCommand   = new Command(OnMemoryRecall,   () => HasMemory);
        MemoryStoreCommand    = new Command(OnMemoryStore);
        MemoryAddCommand      = new Command(OnMemoryAdd);
        MemorySubtractCommand = new Command(OnMemorySubtract);
        MemoryViewCommand     = new Command(OnMemoryView,     () => HasMemory);
    }

    // ── Input ──────────────────────────────────────────────────────
    protected virtual void OnInputDigit(string? digit)
    {
        if (string.IsNullOrEmpty(digit)) return;
        if (_hasError) OnClear();

        if (_isNewEntry)
        {
            DisplayText  = digit;
            _isNewEntry  = false;
        }
        else
        {
            if (DisplayText == "0")
                DisplayText = digit;
            else if (DisplayText.Length < 16)
                DisplayText += digit;
        }
    }

    protected virtual void OnInputDecimal()
    {
        if (_hasError) OnClear();

        if (_isNewEntry)
        {
            DisplayText = "0.";
            _isNewEntry = false;
            return;
        }

        if (!DisplayText.Contains('.'))
            DisplayText += ".";
    }

    // ── Operations ─────────────────────────────────────────────────
    protected virtual void OnOperation(string? op)
    {
        if (string.IsNullOrEmpty(op) || _hasError) return;

        double cur = ParseDisplay();

        if (_pendingOp != null && !_isNewEntry)
        {
            if (!Compute(_accumulator, cur, _pendingOp, out double r, out string? err))
            { ShowError(err!); return; }
            _accumulator = r;
            DisplayText  = Format(r);
        }
        else
        {
            _accumulator = cur;
        }

        _pendingOp    = op;
        _isNewEntry   = true;
        _lastOperand  = null;
        ExpressionText = $"{Format(_accumulator)} {Symbol(op)}";
    }

    protected virtual void OnEquals()
    {
        if (_hasError) return;

        double cur = ParseDisplay();

        if (_pendingOp != null)
        {
            double op2 = _lastOperand ?? cur;
            _lastOperand = op2;

            if (!Compute(_accumulator, op2, _pendingOp, out double r, out string? err))
            { ShowError(err!); return; }

            ExpressionText = $"{Format(_accumulator)} {Symbol(_pendingOp)} {Format(op2)} =";
            _accumulator   = r;
            DisplayText    = Format(r);
            _isNewEntry    = true;
        }
        else
        {
            ExpressionText = $"{Format(cur)} =";
            _isNewEntry    = true;
        }
    }

    // ── Unary ──────────────────────────────────────────────────────
    protected virtual void OnUnary(string? func)
    {
        if (string.IsNullOrEmpty(func) || _hasError) return;

        double val = ParseDisplay();
        double result;
        string snippet;

        switch (func)
        {
            case "sqrt":
                if (val < 0) { ShowError("Invalid input"); return; }
                result  = Math.Sqrt(val);
                snippet = $"√({Format(val)})";
                break;
            case "sqr":
                result  = val * val;
                snippet = $"sqr({Format(val)})";
                break;
            case "reciprocal":
                if (val == 0) { ShowError("Cannot divide by zero"); return; }
                result  = 1.0 / val;
                snippet = $"1/({Format(val)})";
                break;
            case "percent":
                result  = _pendingOp != null ? _accumulator * (val / 100.0) : val / 100.0;
                snippet = Format(result);
                break;
            default:
                return;
        }

        DisplayText = Format(result);
        ExpressionText = _pendingOp != null
            ? $"{Format(_accumulator)} {Symbol(_pendingOp)} {snippet}"
            : snippet;
        _isNewEntry = true;
    }

    // ── Clear / Edit ───────────────────────────────────────────────
    protected virtual void OnClear()
    {
        DisplayText    = "0";
        ExpressionText = string.Empty;
        _accumulator   = 0;
        _pendingOp     = null;
        _lastOperand   = null;
        _isNewEntry    = true;
        _hasError      = false;
    }

    protected virtual void OnClearEntry()
    {
        if (_hasError) { OnClear(); return; }
        DisplayText = "0";
        _isNewEntry = true;
    }

    protected virtual void OnBackspace()
    {
        if (_hasError) { OnClear(); return; }
        if (_isNewEntry) return;

        if (DisplayText.Length <= 1 || (DisplayText.Length == 2 && DisplayText[0] == '-'))
        {
            DisplayText = "0";
            _isNewEntry = true;
        }
        else
        {
            DisplayText = DisplayText[..^1];
        }
    }

    protected virtual void OnNegate()
    {
        if (_hasError || DisplayText == "0") return;
        DisplayText = DisplayText.StartsWith('-')
            ? DisplayText[1..]
            : "-" + DisplayText;
    }

    // ── Memory ─────────────────────────────────────────────────────
    private void OnMemoryClear()    { _memoryValue = 0; HasMemory = false; }
    private void OnMemoryStore()    { _memoryValue = ParseDisplay(); HasMemory = true; }
    private void OnMemoryAdd()      { _memoryValue += ParseDisplay(); HasMemory = true; }
    private void OnMemorySubtract() { _memoryValue -= ParseDisplay(); HasMemory = true; }
    private void OnMemoryRecall()   { if (HasMemory) { DisplayText = Format(_memoryValue); _isNewEntry = true; } }
    private void OnMemoryView()     { if (HasMemory) { DisplayText = Format(_memoryValue); _isNewEntry = true; } }

    private void RefreshMemoryCanExecute()
    {
        ((Command)MemoryClearCommand).ChangeCanExecute();
        ((Command)MemoryRecallCommand).ChangeCanExecute();
        ((Command)MemoryViewCommand).ChangeCanExecute();
    }

    // ── Helpers ────────────────────────────────────────────────────
    protected bool Compute(double a, double b, string op, out double result, out string? error)
    {
        result = 0; error = null;
        switch (op)
        {
            case "+": result = a + b; return true;
            case "-": result = a - b; return true;
            case "*": result = a * b; return true;
            case "/":
                if (Math.Abs(b) < 1e-15) { error = "Cannot divide by zero"; return false; }
                result = a / b; return true;
            case "mod":
                if (Math.Abs(b) < 1e-15) { error = "Cannot divide by zero"; return false; }
                result = a % b; return true;
            case "pow":
                result = Math.Pow(a, b);
                if (!double.IsFinite(result)) { error = "Invalid input"; return false; }
                return true;
            case "exp":
                result = a * Math.Pow(10, b);
                if (!double.IsFinite(result)) { error = "Invalid input"; return false; }
                return true;
            case "yroot":
                if (b == 0) { error = "Cannot divide by zero"; return false; }
                result = Math.Pow(a, 1.0 / b);
                if (!double.IsFinite(result)) { error = "Invalid input"; return false; }
                return true;
            default: result = b; return true;
        }
    }

    protected void ShowError(string msg)
    {
        DisplayText = msg;
        _hasError   = true;
        _isNewEntry = true;
    }

    protected double ParseDisplay()
    {
        if (double.TryParse(DisplayText,
            NumberStyles.Float | NumberStyles.AllowLeadingSign,
            CultureInfo.InvariantCulture, out double v)) return v;
        return 0;
    }

    public static string Format(double v)
    {
        if (!double.IsFinite(v))
        {
            if (double.IsNaN(v))              return "Invalid number";
            if (double.IsPositiveInfinity(v)) return "Infinity";
            return "-Infinity";
        }
        if (Math.Abs(v) < 1e-13 && v != 0) v = 0;
        if (Math.Abs(v % 1) <= double.Epsilon * 100)
            return v.ToString("G", CultureInfo.InvariantCulture);
        return v.ToString("G12", CultureInfo.InvariantCulture);
    }

    protected string Symbol(string op) => op switch
    {
        "+" => "+", "-" => "−", "*" => "×", "/" => "÷",
        "mod" => "mod", "pow" => "^", "exp" => "×10^",
        "yroot" => "ʸ√", _ => op
    };
}

