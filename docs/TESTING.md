# Testowanie DictaMute

## Testy automatyczne

```powershell
dotnet run --project tests/DictaMute.Tests/DictaMute.Tests.csproj -c Release
```

Runner nie wymaga mikrofonu, Windows ani dodatkowego frameworka testowego. Każdy przypadek wypisuje `PASS` albo `FAIL`. Niepowodzenie kończy proces kodem `1`, co zatrzymuje workflow CI. To program testowy uruchamiany przez `dotnet run`, nie projekt obsługiwany przez `dotnet test`.

32 przypadki obejmują: wymóg aktywnego źródła, ścisłe przekroczenie progu, ciszę, NaN/Infinity, podtrzymanie i jego granicę, ponowne wyzwolenie, wyłączenie/reset, tożsamość procesów, reguły globalne i wykluczenia X, ochronę własnego procesu, Duck/Mute, płynne przejście, zachowanie ręcznych zmian i wcześniejszego wyciszenia, walidację profili, atomowy zapis i kopię uszkodzonego JSON.

Workflow `Build and test` dodatkowo przywraca pakiety, kompiluje całe rozwiązanie WPF na Windows i publikuje aplikację self-contained dla `win-x64`.

## Testy ręczne na Windows

Poniższa lista jest protokołem do wykonania, nie deklaracją zakończonych testów sprzętowych. Zapisuj wersję Windows, nazwę mikrofonu, wersję odtwarzacza, rodzaj instalacji (Store/desktop) i wynik.

| Scenariusz | Oczekiwany wynik |
|---|---|
| Puste X i wyłączona opcja dowolnego mikrofonu | Brak wyciszania, nawet gdy inny program nagrywa |
| Źródło X nagrywa, poziom poniżej progu | Brak wyciszania |
| Źródło X przekracza próg; Duck 20% | Cel jest ściszony najwyżej do 20%; cichszy cel nie jest pogłaśniany |
| Pauza w mowie krótsza/dłuższa od Hold Time | Krótka przerwa nie przywraca dźwięku; dłuższa przywraca poprzedni poziom |
| Mute, cel wcześniej wyciszony ręcznie | Cel po zakończeniu mowy pozostaje wyciszony |
| Ręczna zmiana głośności podczas Duck | DictaMute nie nadpisuje ręcznej zmiany |
| Włączenie „Wszystko poza X” | Pozostałe rozpoznane aplikacje są celami, aplikacje X pozostają bez zmian |
| „Dowolny mikrofon”; dotychczasowy cel zaczyna nagrywać | Jego stan jest przywracany i pozostaje wykluczony, dopóki jest aktywnym źródłem |
| Dwa mikrofony, tylko jeden ma źródło X | Sygnał z niezwiązanego mikrofonu nie uruchamia reguły X |
| Nowy odtwarzacz lub nowa sesja podczas mowy | Nowy pasujący cel zostaje objęty automatyką po odświeżeniu listy |
| Pause z odtwarzaczem obsługującym GSMTC | Odtwarzanie zostaje zatrzymane i wznowione po Hold Time |
| Pause; odtwarzacz był wcześniej spauzowany | DictaMute nie uruchamia go po zakończeniu mowy |
| Pause; ręczna zmiana utworu lub wznowienie | DictaMute odstępuje od automatycznego wznowienia starej sesji |
| Pause bez dostępnej sesji lub z niepoprawnym ID | Czytelny komunikat i ściszenie Duck zamiast globalnego Play/Pause |
| Przeglądarka jednocześnie jako źródło i odtwarzacz | Cały proces źródłowy jest wykluczony; karty nie są niezależnymi celami |
| Win+H | Dodaj proces z listy aktywnych sesji X; nie zakładaj, że edytor tekstu jest procesem mikrofonowym |
| Globalne skróty z aktywnym innym oknem | Dodany zostaje proces tego okna; konflikt skrótu jest zgłaszany |
| Zapis, restart, zmiana profilu i profilu z menu trayu | Ustawienia i reguły odpowiadają wybranemu profilowi |
| Zamknięcie/minimalizacja okna, dwuklik ikony | Automatyka nadal działa; dwuklik przywraca okno |
| Wyłączenie automatyki, zmiana profilu i „Zakończ” podczas mowy | Zmienione sesje są przywracane bez oczekiwania na Hold Time |
| Odłączenie/podłączenie urządzenia, uśpienie i wznowienie | Brak awarii interfejsu; urządzenia są ponownie wykrywane; sprawdź stan miksera |
| Wymuszone zabicie procesu | Sprawdź mikser ręcznie; odzyskanie stanu po takim przerwaniu nie jest gwarantowane |
| Uszkodzony JSON, katalog tylko do odczytu | Kopia uszkodzonego pliku albo czytelny błąd zapisu, bez cichej utraty konfiguracji |
| Pamięć i CPU w spoczynku oraz przy mowie | Zmierz w Menedżerze zadań; wartości z README są celami, nie zmierzonym wynikiem |

## Diagnostyka

Plik `%LOCALAPPDATA%\DictaMute\DictaMute.log` zawiera błędy enumeracji, sterowania głośnością, poleceń multimedialnych i zapisu ustawień. Przy zgłoszeniu usuń prywatne ścieżki/nazwy z logu. Nie przesyłaj nagrań rozmów — aplikacja ich nie potrzebuje.
