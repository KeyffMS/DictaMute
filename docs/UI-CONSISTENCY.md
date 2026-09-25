# DictaMute i SightAdapt — wspólny język interfejsu

## Punkt odniesienia

Porównanie dotyczy aplikacji desktopowej **SightAdapt**, nie jej strony internetowej. Źródłem jest `KeyffMS/SightAdapt`, gałąź `main`, commit `4a8849ee51096515c22e0934fa1a699ef69b2dc3`:

- `src/SightAdapt/AppTheme.cs` — 22 kolory semantyczne, Segoe UI, wiersze 42 i nagłówki kolumn 44.
- `src/SightAdapt/ConfigurationForm.cs` — nagłówek 104 z paskiem akcentu 5, karta automatyki, listy aplikacji, pasek akcji i karta informacji o projekcie.
- `src/SightAdapt/ModernButton.cs` — role Primary/Secondary/Danger/Ghost, promień 9 i minimalna wysokość 40.
- `src/SightAdapt/RoundedPanel.cs` — promień kart 12.
- `src/SightAdapt/ToggleSwitch.cs` — przełącznik 50 × 28.
- `docs/BRAND.md` — identyfikacja wydawcy `KeyffMS / aiteracja.pl`.

Porównanie wykonano na podstawie kodu interfejsów. Podglądy generowane przez testy są renderami WPF DictaMute z danymi demonstracyjnymi, nie zrzutami działającego mikrofonu ani SightAdapt.

## Różnice i dostosowanie

| Obszar | DictaMute przed zmianą | DictaMute po dostosowaniu |
|---|---|---|
| Tło okna | `#101827` | `#14171F`, identyczne z SightAdapt |
| Powierzchnie | `#182235` | `#1D222D`, podniesione `#242A37` |
| Akcent | Niebieskie przyciski `#29486B`, osobny zielony zapis | Wspólny akcent `#708BFF` |
| Typografia | Segoe UI 14 DIP, duży tytuł produktu | Segoe UI 12,667 DIP ≈ 9,5 pt; hierarchia nagłówków SightAdapt |
| Nagłówek | Nazwa aplikacji i checkbox w jednym wierszu | Nagłówek zadania, opis i pionowy pasek akcentu |
| Automatyka | Zwykły checkbox | Osobna karta, przełącznik 50 × 28 i tekstowy znacznik stanu |
| Karty / przyciski | Promienie 8 / 5 | Promienie 12 / 9, pola 7 |
| Akcje | Zbliżony wygląd wszystkich działań | Wyraźne role: główna, pomocnicza, usuwanie i neutralne zamknięcie |
| Tabele | Wiersze 34, inne kolory nagłówków | Wiersze 42, nagłówki 44, wspólne zaznaczenia i pasy wierszy |
| Listy rozwijane | Jasne pola i systemowe rozwijane listy | Ciemne pola i listy również podczas edycji trybu w tabeli |
| Suwaki / przewijanie | Kontrolki z domyślnym wyglądem | Szablony dopasowane do palety, zachowane komendy klawiatury |
| Zasobnik | Systemowe menu | Ciemne menu odczytujące te same kolory co WPF |
| Wydawca | Brak informacji w głównym oknie | Karta DictaMute, wydawca, licencja i repozytorium |

## Granice wspólnej tożsamości

DictaMute zachowuje własną nazwę, funkcję i grupy X/Y. Nie przejmuje logo ani oznaczenia znaku towarowego SightAdapt. Nie dodano nowej nazwy pakietu, wspólnego instalatora, zależności między aplikacjami ani integracji ustawień. Spójność dotyczy wyglądu i wzorców obsługi.

SightAdapt korzysta z WinForms, DictaMute z WPF. Nie zmieniono frameworka DictaMute: wzorce zostały odwzorowane w zasobach WPF. Nie zmieniono algorytmów audio, przechowywanych profili, skrótów ani znaczenia istniejących kontrolek.

## Utrzymanie motywu

`src/DictaMute/Themes/Suite.xaml` zawiera paletę i szablony kontrolek. Nazwy kolorów odpowiadają `AppTheme` SightAdapt. Menu WinForms pobiera kolory przez `SuiteTrayTheme` z tych samych zasobów, zamiast utrzymywać drugą paletę. Aktualizacja SightAdapt nie aktualizuje automatycznie DictaMute — zmiany należy porównać i przenieść świadomie.

Nie jest to mechaniczne kopiowanie każdego szczegółu: tekst głównego przycisku jest ciemny na jasnym akcencie, aby zachować lepszy kontrast małej etykiety. Przełączniki i statusy zawierają informacje dostępne tekstowo; kontrolki mają etykiety automatyzacji i wyróżnienie fokusu. Natywna ramka okna zachowuje zmianę rozmiaru i obsługę systemową; ustawiany jest tylko ciemny pasek tytułu.

## Weryfikacja

Na Windows, z katalogu repozytorium:

```powershell
dotnet build DictaMute.sln -c Release
dotnet run --project tests/DictaMute.Tests -c Release
dotnet run --project tests/DictaMute.UiTests -c Release -- .
```

Test UI ładuje rzeczywisty XAML widoku i skompilowany motyw, ale usuwa obsługę zdarzeń oraz nie tworzy silnika audio. Sprawdza kontrakt nazw istniejących kontrolek, zgodność palety, wymiary, stany pustych list, rozwijane listy, zapis edytowanego trybu w tabeli, komendy suwaków i przewijanie w małym oknie. Kompilacja aplikacji osobno sprawdza powiązania z rzeczywistymi metodami obsługi zdarzeń.

Przebieg CI publikuje osobny artefakt `DictaMute-ui-preview` z renderami pustego i skonfigurowanego okna, listy rozwijanej, ustawień i małego okna. Dane w podglądach są fikcyjne. Test nie potwierdza obsługi rzeczywistego audio, działania czytników ekranu, wyglądu natywnej ramki ani menu zasobnika. Przed wydaniem potrzebna jest ręczna kontrola tych elementów oraz skalowania 125–200% na rzeczywistym monitorze.
