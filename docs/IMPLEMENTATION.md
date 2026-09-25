# DictaMute — pierwsza implementacja

Kod znajduje się na gałęzi `first-attempt`. Nazwa jest zapisana z łącznikiem, ponieważ Git nie dopuszcza spacji w nazwach gałęzi. Główny README pozostaje opisem założeń projektu; ten dokument opisuje działanie i ograniczenia pierwszej implementacji.

## Uruchomienie ze źródeł

Wymagania: Windows 10 od wersji 1809 lub Windows 11 oraz .NET 8 SDK. Do samego uruchomienia wersji zależnej od frameworka potrzebny jest .NET 8 Desktop Runtime. Zwykłe konto użytkownika wystarcza; aplikacja nie żąda uprawnień administratora.

```powershell
git clone --branch first-attempt https://github.com/KeyffMS/DictaMute.git
cd DictaMute
dotnet restore DictaMute.sln
dotnet build DictaMute.sln -c Release
dotnet run --project src/DictaMute/DictaMute.csproj -c Release
```

Projekt można też otworzyć w Visual Studio z obsługą .NET 8 i składnikiem „Programowanie aplikacji klasycznych dla platformy .NET”.

## Wersja przenośna

```powershell
dotnet publish src/DictaMute/DictaMute.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false -o artifacts/DictaMute-win-x64
```

Uruchom `artifacts/DictaMute-win-x64/DictaMute.exe`. Przenieś cały folder, nie tylko plik EXE. Publikacja self-contained zawiera środowisko uruchomieniowe. Workflow `.github/workflows/build.yml` kompiluje aplikację, wykonuje testy i przygotowuje taki folder jako artefakt `DictaMute-win-x64`.

## Pierwsza konfiguracja

1. Uruchom odtwarzacz i rozpocznij odtwarzanie. Wybierz go w sekcji Y i kliknij „Dodaj cel”. Alternatywnie aktywuj okno odtwarzacza i naciśnij `Ctrl+Alt+Y`.
2. Uruchom rozmowę albo dyktowanie. W sekcji X wybierz rzeczywistą aplikację używającą mikrofonu i kliknij „Dodaj źródło”. `Ctrl+Alt+X` dodaje proces aktywnego okna — niekoniecznie proces odpowiedzialny za mikrofon.
3. Wybierz dla celu `Duck`, `Mute` albo `Pause`. Ustaw próg, czas podtrzymania i poziom ściszenia. Kliknij „Zapisz i zastosuj”.
4. Mów do mikrofonu. Automatyka uruchomi się dopiero po przekroczeniu progu przez sygnał urządzenia używanego przez aktywne źródło.
5. `Ctrl+Alt+M` włącza i wyłącza automatykę. Jej wyłączenie przywraca stan bez oczekiwania na Hold Time.

Skróty są konfigurowalne w rozwijanej sekcji. Konflikt z innym programem jest zgłaszany. Można również dodać program przez wybór pliku `.exe`.

Zamknięcie albo zminimalizowanie okna pozostawia aplikację w zasobniku systemowym. Dwukrotne kliknięcie ikony otwiera konfigurację. Menu ikony umożliwia zmianę profilu, włączenie/wyłączenie automatyki oraz zakończenie programu. Zakończenie programu najpierw przywraca zmienione ustawienia audio.

## Co implementuje ta wersja

| Obszar | Implementacja |
|---|---|
| Interfejs | WPF, ciemny motyw, listy X/Y, ikony procesów, miernik sygnału, suwaki |
| Wykrywanie | Aktywne sesje wejściowe WASAPI + miernik poziomu odpowiadającego urządzenia |
| Bramka | Próg amplitudy i odnawiany czas podtrzymania; pomiar czasu przez Stopwatch |
| Cele | Reguły dla procesów oraz „Wszystko poza X”; źródła mają pierwszeństwo wykluczenia |
| Duck | Obniżenie poziomu do ustawionego limitu, płynne przejście i powrót do poprzedniego poziomu |
| Mute | Zmiana bitu wyciszenia sesji, z zapamiętaniem poprzedniego stanu |
| Pause | Adresowane polecenia GSMTC Pause/Play, mapowanie identyfikatora sesji, ściszenie zastępcze |
| Konfiguracja | Profile, skróty i ustawienia zapisane atomowo w JSON |
| Praca w tle | Ikona w trayu, menu, pojedyncza instancja, obsługa kończenia sesji Windows |
| Testy | Niezależny od Windows zestaw testów bramki, reguł, przywracania głośności i zapisu konfiguracji |

## Ważne ograniczenia

### Mikrofon i rozpoznawanie aplikacji

To bramka poziomu sygnału, nie detektor mowy. Nie rozróżnia głosu, stukania w klawiaturę i muzyki z otoczenia. Wartość procentowa oznacza znormalizowaną amplitudę, nie decybele.

Windows udostępnia tu poziom urządzenia wejściowego. Wersja ta łączy go z informacją o aktywnej sesji mikrofonowej procesu; **nie wyodrębnia osobnego sygnału każdego procesu**. Dwie aplikacje korzystające z tego samego mikrofonu mogą otrzymywać ten sam odczyt miernika. Przy wielu urządzeniach próg jest sprawdzany na urządzeniu, z którego korzysta źródło.

Host dyktowania `Win+H`, procesy UWP, aplikacje wieloprocesowe i chronione procesy mogą nie odpowiadać procesowi aktywnego okna. Użyj listy aktywnych sesji X. Dostępna jest też jawna opcja „Reaguj na dowolną aplikację używającą mikrofonu”, która pomija listę X. Nie otwiera ona własnego strumienia nagrywania i nadal wymaga aktywnej sesji wejściowej.

Sterownik lub tryb exclusive może nie udostępniać użytecznego miernika. Aplikacja nie próbuje obchodzić uprawnień mikrofonu ani zabezpieczeń innych procesów. Systemowe sesje PID 0 oraz procesy, których tożsamości nie można odczytać, są pomijane również w trybie globalnym.

### Przeglądarki

Reguły dotyczą procesów/aplikacji, nie kart. YouTube i Meet w tej samej przeglądarce nie stanowią niezależnych celów. Jeżeli przeglądarka jest źródłem X, jej dźwięk jest wykluczony z wyciszania. Można rozdzielić źródło i odtwarzacz na różne aplikacje.

### Pauza multimedialna

Odtwarzacz musi publikować sesję GSMTC i obsługiwać Pause/Play. W przypadku innego identyfikatora niż nazwa procesu zaznacz cel Y i przypisz właściwe ID z listy sesji multimedialnych. Nie ma dopasowywania po przypadkowym fragmencie nazwy ani globalnego wysyłania klawisza Play/Pause. Brak jednoznacznej sesji, odmowa pauzy albo niedostępny interfejs powodują przejście na Duck oraz komunikat.

DictaMute wznawia wyłącznie sesje, które sam skutecznie zatrzymał. Nie uruchamia odtwarzacza, który był wcześniej spauzowany. Zmiana utworu lub wznowienie odtwarzania przez użytkownika podczas wyciszenia powoduje odstąpienie od automatycznego wznowienia. Windows nie pozwala niezawodnie rozpoznać każdej intencji użytkownika, np. ponownego naciśnięcia Pause na już zatrzymanym utworze. Rzeczywista zgodność zależy od odtwarzacza i wymaga testu na Windows.

### Przywracanie i awarie

Poziom i bit Mute są zapamiętywane oddzielnie dla każdej instancji sesji. Ręczna zmiana w mikserze ma pierwszeństwo: aplikacja nie przywraca wartości, którą w międzyczasie zmienił użytkownik lub inny program. Normalne wyłączenie, zmiana profilu i wyjście przywracają dźwięk.

Nagłe zabicie procesu, utrata zasilania, zniknięcie sesji albo odłączenie urządzenia mogą uniemożliwić przywrócenie. W takiej sytuacji sprawdź mikser Windows lub ręcznie wznów odtwarzacz. Nie ma jeszcze trwałego dziennika odzyskiwania sesji po awarii ani instalatora.

### Wydajność i weryfikacja

Miernik jest sprawdzany co około 75 ms, listy urządzeń i sesji co około 500 ms, a widok odświeżany co około 225 ms. Polecenia multimedialne są asynchroniczne, ale oczekiwanie na odpowiedź odtwarzacza może opóźnić cykl automatyki. Cele z README (`RAM < 30 MB`, `CPU < 0,5%`) pozostają celami — nie wynikami pomiarów. WPF/.NET może przekroczyć zakładany limit pamięci.

Testy jednostkowe nie zastępują sprawdzenia prawdziwego mikrofonu, trayu i odtwarzaczy. Zestaw testów i lista testów ręcznych: [TESTING.md](TESTING.md).

## Pliki użytkownika i prywatność

Konfiguracja: `%LOCALAPPDATA%\DictaMute\settings.json`.
Dziennik diagnostyczny: `%LOCALAPPDATA%\DictaMute\DictaMute.log` (rotacja po około 1 MiB).
Nieprawidłowy JSON jest kopiowany do pliku `.corrupt-… .bak` przed wczytaniem wartości domyślnych. Błąd zapisu nie jest ignorowany.

Aplikacja nie nagrywa próbek dźwięku, nie transkrybuje mowy, nie wysyła telemetrii i nie komunikuje się z serwerem. Zapisuje lokalnie nazwy/ścieżki wybranych aplikacji, ustawienia oraz błędy. Pobieranie pakietów NuGet następuje podczas kompilacji, nie podczas działania programu.

## Układ kodu

- `src/DictaMute.Core`: modele, reguły, bramka, przywracanie głośności, zapis JSON; bez zależności od Windows.
- `src/DictaMute/Services`: sesje WASAPI przez NAudio, GSMTC, globalne skróty, identyfikacja procesów i silnik automatyki.
- `src/DictaMute/MainWindow.xaml`: interfejs WPF; code-behind zarządza edycją, trayem i cyklem życia.
- `tests/DictaMute.Tests`: deterministyczny runner testów zwracający niezerowy kod przy błędzie.

## Dokumentacja użytych API

- [Windows Audio Sessions](https://learn.microsoft.com/en-us/windows/win32/coreaudio/audio-sessions)
- [IAudioMeterInformation](https://learn.microsoft.com/en-us/windows/win32/api/endpointvolume/nn-endpointvolume-iaudiometerinformation)
- [GlobalSystemMediaTransportControlsSession](https://learn.microsoft.com/en-us/uwp/api/windows.media.control.globalsystemmediatransportcontrolssession)
- [WinRT w aplikacjach desktopowych .NET](https://learn.microsoft.com/en-us/windows/apps/desktop/modernize/winrt-apis-desktop-apps)
- [NAudio 2.2.1](https://github.com/naudio/NAudio/tree/v2.2.1)
