using System.Windows.Input;

namespace WindowsCalculatorMaui.ViewModels;

public class ScientificViewModel : StandardViewModel
{
    // ── State ──────────────────────────────────────────────────────
    private bool _isDeg       = true;
    private bool _isSecond    = false;
    private bool _trigVisible = false;
    private bool _funcVisible = false;
    private readonly Stack<(double Acc, string? Op)> _parenStack = new();

    // ── Properties ─────────────────────────────────────────────────
    public bool IsDeg
    {
        get => _isDeg;
        set { if (SetProperty(ref _isDeg, value)) OnPropertyChanged(nameof(AngleModeText)); }
    }

    public bool IsSecond
    {
        get => _isSecond;
        set
        {
            if (SetProperty(ref _isSecond, value))
            {
                OnPropertyChanged(nameof(Btn2ndBg));
                // Row-1 buttons
                OnPropertyChanged(nameof(BtnX2Text));   OnPropertyChanged(nameof(BtnX2Param));
                OnPropertyChanged(nameof(BtnSqrtText)); OnPropertyChanged(nameof(BtnSqrtParam));
                // Row-3 buttons
                OnPropertyChanged(nameof(BtnPowText));  OnPropertyChanged(nameof(BtnPowParam));
                // Row-4 buttons
                OnPropertyChanged(nameof(BtnPow10Text)); OnPropertyChanged(nameof(BtnPow10Param));
                // Row-5 buttons
                OnPropertyChanged(nameof(BtnLogText));  OnPropertyChanged(nameof(BtnLogParam));
                // Row-6 buttons
                OnPropertyChanged(nameof(BtnLnText));   OnPropertyChanged(nameof(BtnLnParam));
                // Trig panel
                OnPropertyChanged(nameof(TrigSinText)); OnPropertyChanged(nameof(TrigSinParam));
                OnPropertyChanged(nameof(TrigCosText)); OnPropertyChanged(nameof(TrigCosParam));
                OnPropertyChanged(nameof(TrigTanText)); OnPropertyChanged(nameof(TrigTanParam));
            }
        }
    }

    public bool TrigVisible
    {
        get => _trigVisible;
        set => SetProperty(ref _trigVisible, value);
    }

    public bool FuncVisible
    {
        get => _funcVisible;
        set => SetProperty(ref _funcVisible, value);
    }

    public string AngleModeText => IsDeg ? "DEG" : "RAD";
    public Color  Btn2ndBg      => IsSecond
        ? Color.FromArgb("#0067C0")
        : Colors.Transparent;

    // ── 2nd-mode button pairs ──────────────────────────────────────
    public string BtnX2Text    => IsSecond ? "x³"   : "x²";
    public string BtnX2Param   => IsSecond ? "cube"  : "sqr";
    public string BtnSqrtText  => IsSecond ? "³√x"  : "²√x";
    public string BtnSqrtParam => IsSecond ? "cbrt"  : "sqrt";
    public string BtnPowText   => IsSecond ? "ʸ√x"  : "xʸ";
    public string BtnPowParam  => IsSecond ? "yroot" : "pow";
    public string BtnPow10Text  => IsSecond ? "2ˣ"  : "10ˣ";
    public string BtnPow10Param => IsSecond ? "pow2" : "pow10";
    public string BtnLogText   => IsSecond ? "logᵧ" : "log";
    public string BtnLogParam  => IsSecond ? "logy"  : "log";
    public string BtnLnText    => IsSecond ? "eˣ"   : "ln";
    public string BtnLnParam   => IsSecond ? "powe"  : "ln";

    // Trig panel
    public string TrigSinText  => IsSecond ? "sin⁻¹" : "sin";
    public string TrigSinParam => IsSecond ? "asin"   : "sin";
    public string TrigCosText  => IsSecond ? "cos⁻¹" : "cos";
    public string TrigCosParam => IsSecond ? "acos"   : "cos";
    public string TrigTanText  => IsSecond ? "tan⁻¹" : "tan";
    public string TrigTanParam => IsSecond ? "atan"   : "tan";

    // ── Commands ───────────────────────────────────────────────────
    public ICommand ToggleDegCommand  { get; }
    public ICommand ToggleFeCommand   { get; }
    public ICommand ToggleSecondCommand { get; }
    public ICommand ToggleTrigCommand { get; }
    public ICommand ToggleFuncCommand { get; }
    public ICommand ConstantCommand   { get; }
    public ICommand OpenParenCommand  { get; }
    public ICommand CloseParenCommand { get; }

    public ScientificViewModel() : base()
    {
        ToggleDegCommand    = new Command(() => IsDeg    = !IsDeg);
        ToggleFeCommand     = new Command(OnToggleFe);
        ToggleSecondCommand = new Command(() => IsSecond = !IsSecond);
        ToggleTrigCommand   = new Command(OnToggleTrig);
        ToggleFuncCommand   = new Command(OnToggleFunc);
        ConstantCommand     = new Command<string>(OnConstant);
        OpenParenCommand    = new Command(OnOpenParen);
        CloseParenCommand   = new Command(OnCloseParen);
    }

    private void OnToggleFe()
    {
        if (double.TryParse(DisplayText, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out double v))
        {
            DisplayText = DisplayText.Contains('E') || DisplayText.Contains('e')
                ? Format(v)
                : v.ToString("E6", System.Globalization.CultureInfo.InvariantCulture);
        }
    }

    private void OnToggleTrig()
    {
        TrigVisible = !TrigVisible;
        if (TrigVisible) FuncVisible = false;
    }

    private void OnToggleFunc()
    {
        FuncVisible = !FuncVisible;
        if (FuncVisible) TrigVisible = false;
    }

    private void OnConstant(string? c)
    {
        if (string.IsNullOrEmpty(c) || _hasError) return;
        DisplayText = c == "pi" ? Format(Math.PI) : Format(Math.E);
        _isNewEntry = true;
    }

    private void OnOpenParen()
    {
        if (_hasError) return;
        _parenStack.Push((_accumulator, _pendingOp));
        _accumulator = 0; _pendingOp = null; _isNewEntry = true;
        ExpressionText += " ( ";
    }

    private void OnCloseParen()
    {
        if (_hasError || _parenStack.Count == 0) return;
        double cur = ParseDisplay();
        if (_pendingOp != null)
        {
            if (!Compute(_accumulator, cur, _pendingOp, out double r, out string? err))
            { ShowError(err!); return; }
            cur = r;
        }
        (_accumulator, _pendingOp) = _parenStack.Pop();
        DisplayText = Format(cur);
        ExpressionText += " ) ";
        _isNewEntry = true;
    }

    protected override void OnClear()
    {
        base.OnClear();
        _parenStack.Clear();
        IsSecond    = false;
        TrigVisible = false;
        FuncVisible = false;
    }

    // ── Unary override ─────────────────────────────────────────────
    protected override void OnUnary(string? func)
    {
        if (string.IsNullOrEmpty(func) || _hasError) return;
        IsSecond = false;

        double val = ParseDisplay();
        double result;
        string snippet;

        switch (func)
        {
            // ── Trig ──
            case "sin":
                result  = Math.Sin(IsDeg ? Rad(val) : val);
                if (IsDeg && Math.Abs(val % 180) < 1e-10) result = 0;
                snippet = $"sin({Format(val)})"; break;
            case "cos":
                result  = Math.Cos(IsDeg ? Rad(val) : val);
                if (IsDeg && Math.Abs((val - 90) % 180) < 1e-10) result = 0;
                snippet = $"cos({Format(val)})"; break;
            case "tan":
                if (IsDeg && Math.Abs((val - 90) % 180) < 1e-10) { ShowError("Invalid input"); return; }
                result = Math.Tan(IsDeg ? Rad(val) : val);
                if (!double.IsFinite(result)) { ShowError("Invalid input"); return; }
                snippet = $"tan({Format(val)})"; break;
            case "asin":
                if (val < -1 || val > 1) { ShowError("Invalid input"); return; }
                result = IsDeg ? Deg(Math.Asin(val)) : Math.Asin(val);
                snippet = $"sin⁻¹({Format(val)})"; break;
            case "acos":
                if (val < -1 || val > 1) { ShowError("Invalid input"); return; }
                result = IsDeg ? Deg(Math.Acos(val)) : Math.Acos(val);
                snippet = $"cos⁻¹({Format(val)})"; break;
            case "atan":
                result = IsDeg ? Deg(Math.Atan(val)) : Math.Atan(val);
                snippet = $"tan⁻¹({Format(val)})"; break;

            // ── Log ──
            case "log":
                if (val <= 0) { ShowError("Invalid input"); return; }
                result = Math.Log10(val); snippet = $"log({Format(val)})"; break;
            case "logy":
                if (val <= 0 || _accumulator <= 0 || _accumulator == 1) { ShowError("Invalid input"); return; }
                result = Math.Log(val) / Math.Log(_accumulator);
                snippet = $"logᵧ({Format(val)})"; break;
            case "ln":
                if (val <= 0) { ShowError("Invalid input"); return; }
                result = Math.Log(val); snippet = $"ln({Format(val)})"; break;

            // ── Powers ──
            case "sqr":    result = val * val;       snippet = $"sqr({Format(val)})"; break;
            case "cube":   result = val * val * val;  snippet = $"cube({Format(val)})"; break;
            case "pow10":  result = Math.Pow(10, val); snippet = $"10^({Format(val)})"; break;
            case "pow2":   result = Math.Pow(2, val);  snippet = $"2^({Format(val)})"; break;
            case "powe":   result = Math.Exp(val);     snippet = $"e^({Format(val)})"; break;

            // ── Roots ──
            case "sqrt":
                if (val < 0) { ShowError("Invalid input"); return; }
                result = Math.Sqrt(val); snippet = $"√({Format(val)})"; break;
            case "cbrt":
                result = Math.Cbrt(val); snippet = $"³√({Format(val)})"; break;

            // ── Other ──
            case "fact":
                if (val < 0 || Math.Floor(val) != val || val > 170) { ShowError("Invalid input"); return; }
                result = Factorial((int)val); snippet = $"{(int)val}!"; break;
            case "abs":
                result = Math.Abs(val); snippet = $"|{Format(val)}|"; break;
            case "reciprocal":
                if (val == 0) { ShowError("Cannot divide by zero"); return; }
                result = 1.0 / val; snippet = $"1/({Format(val)})"; break;

            default: base.OnUnary(func); return;
        }

        if (!double.IsFinite(result)) { ShowError("Invalid input"); return; }

        DisplayText = Format(result);
        ExpressionText = _pendingOp != null
            ? $"{Format(_accumulator)} {Symbol(_pendingOp)} {snippet}"
            : snippet;
        _isNewEntry = true;
    }

    // ── Operation override (yroot) ─────────────────────────────────
    protected override void OnOperation(string? op)
    {
        if (string.IsNullOrEmpty(op) || _hasError) return;
        IsSecond = false;

        if (op == "yroot")
        {
            double cur = ParseDisplay();
            if (_pendingOp != null && !_isNewEntry)
            {
                if (!Compute(_accumulator, cur, _pendingOp, out double r, out string? err))
                { ShowError(err!); return; }
                _accumulator = r;
                DisplayText  = Format(r);
            }
            else { _accumulator = cur; }
            _pendingOp  = "yroot"; _isNewEntry = true; _lastOperand = null;
            ExpressionText = $"{Format(_accumulator)} ʸ√";
            return;
        }
        base.OnOperation(op);
    }

    // ── Helpers ────────────────────────────────────────────────────
    private static double Rad(double d) => d * Math.PI / 180.0;
    private static double Deg(double r) => r * 180.0 / Math.PI;

    private static double Factorial(int n)
    {
        double r = 1;
        for (int i = 2; i <= n; i++) r *= i;
        return r;
    }
}

