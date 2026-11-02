# Windows Hesap Makinesi (.NET MAUI)

Windows 10/11 Hesap Makinesi'nin **Standart** ve **Bilimsel** modlarını içeren, **Ayarlar** sayfasından anlık tema değişimi sunan ve **XAML Stilleri** ile geliştirilmiş .NET MAUI uygulaması.

---

## 🚀 Özellikler

### 1. Standart Mod (`StandartPage.xaml`)
- **Dört İşlem**: Toplama (+), Çıkarma (−), Çarpma (×), Bölme (÷).
- **Yardımcı İşlemler**: Yüzde (%), İşaret Değiştirme (+/-), Ondalık Virgül (,), Eşittir (=).
- **Fonksiyonlar**: Karekök (√x), Karesi (x²), Çarpmaya göre tersi (1/x).
- **Temizleme**: Tümünü Temizle (C), Girişi Temizle (CE), Geri Al / Sil (⌫).
- **Çift Satırlı Gösterge**: Üstte işlem geçmişi ifadesi (örn. `12 + 5 =`), altta büyük sonuç göstergesi.

### 2. Bilimsel Mod (`BilimselPage.xaml`)
- **Trigonometri**: `sin`, `cos`, `tan` fonksiyonları.
- **Açı Modu (DEG / RAD)**: Derece ve Radyan birimleri arasında tek tıkla geçiş.
- **Logaritma**: Onluk logaritma (`log`) ve doğal logaritma (`ln`).
- **Üs & Kökler**: `x²`, `x³`, `xʸ`, `10ˣ`, `eˣ`, `2√x`, `cbrt`.
- **Ekstra Fonksiyonlar**: Faktöriyel (`n!`), Mutlak değer (`|x|`), Kalan (`mod`), Bilimsel gösterim (`exp`).
- **Sabitler**: Pi sayısı (`π`), Euler sayısı (`e`).
- **Parantez**: `(` ve `)` ile işlem önceliği ve gruplama.

### 3. Ayarlar Sayfası (`AyarlarPage.xaml`)
- **Tema Seçenekleri**:
  - Açık (Light)
  - Koyu (Dark)
  - Sistem ayarını kullan (Use system setting)
- **Anında Değişim**: `Application.Current.UserAppTheme` ve `AppThemeBinding` kullanılarak uygulama yeniden başlatılmadan tüm sayfalarda tema anında güncellenir.
- **Kalıcılık**: Seçilen tema `Preferences` API'si ile saklanır, uygulama yeniden açıldığında son seçilen tema ile başlar.
- **Hakkında Bölümü**: Uygulama ve sürüm bilgisi.

### 4. Navigasyon (`AppShell.xaml`)
- .NET MAUI Shell Flyout (hamburger menüsü) ile:
  - **Standart**
  - **Bilimsel**
  - **Ayarlar**
  sayfaları arasında kolay ve şık geçiş.

### 5. XAML Stilleri ve Kaynak Sözlükleri
- Sayfalarda **kesinlikle sabit renk, yazı tipi veya yazı boyutu doğrudan yazılmamıştır**.
- **`Colors.xaml`**: Açık ve koyu tema için renk paleti.
- **`Styles.xaml`**:
  - Ortak `BaseButtonStyle`
  - Örtük (Implicit) buton stili: `<Style TargetType="Button" BasedOn="{StaticResource BaseButtonStyle}" />`
  - Açık (Explicit) `x:Key` ve `BasedOn` ile türetilmiş stiller:
    - `NumberButtonStyle` (Rakamlar)
    - `OperatorButtonStyle` (Dört işlem ve temizleme tuşları)
    - `EqualsButtonStyle` (Vurgulu eşittir tuşu)
    - `ScientificFunctionButtonStyle` (Bilimsel fonksiyon tuşları)
    - `AngleModeButtonStyle` (DEG/RAD modu butonu)
  - Dinamik metin ve etiket stilleri (`DisplayPrimaryLabelStyle`, `DisplaySecondaryLabelStyle`, `PageTitleLabelStyle`, vb.).

### 6. Hata Yönetimi
- **Sıfıra Bölme**: "Sıfıra bölünemez" uyarısı.
- **Geçersiz Giriş**: Negatif karekök, tanımsız tanjant veya geçersiz logaritmada "Geçersiz giriş" uyarısı.
- Yeni tuşa basıldığında hata otomatik olarak sıfırlanır.

---

## 🛠️ Proje Klasör Yapısı

```
WindowsCalculatorMaui/
├── Resources/
│   └── Styles/
│       ├── Colors.xaml          # Açık ve Koyu tema renk tanımları
│       └── Styles.xaml          # Örtük ve açık buton stilleri, font boyutları
├── Services/
│   └── ThemeService.cs          # Preferences ve UserAppTheme yönetimi
├── ViewModels/
│   ├── BaseViewModel.cs         # INotifyPropertyChanged temel sınıfı
│   ├── StandardViewModel.cs     # Standart mod hesaplama motoru ve komutları
│   ├── ScientificViewModel.cs   # Bilimsel fonksiyonlar, DEG/RAD ve parantez yönetimi
│   └── SettingsViewModel.cs     # Tema seçimi ve ayarlar
├── Views/
│   ├── StandartPage.xaml(.cs)   # Standart hesap makinesi XAML görünümü
│   ├── BilimselPage.xaml(.cs)   # Bilimsel hesap makinesi XAML görünümü
│   └── AyarlarPage.xaml(.cs)    # Ayarlar sayfası XAML görünümü
├── App.xaml(.cs)                # Tema başlatma ve pencere boyutlandırma
├── AppShell.xaml(.cs)           # Shell Flyout navigasyon yapısı
└── MauiProgram.cs               # MAUI başlangıç yapılandırması
```

---

## 💻 Çalıştırma

Projeyi Windows üzerinde çalıştırmak için:

```powershell
cd C:\Users\Win10\.gemini\antigravity\scratch\WindowsCalculatorMaui
dotnet build -f net9.0-windows10.0.19041.0
dotnet run -f net9.0-windows10.0.19041.0
```

