# DictaMute

**DictaMute** to darmowe, open-source'owe narzędzie dla Windows 10/11, które automatycznie ścisza, wycisza lub pauzuje wybrane aplikacje, gdy aktywność skonfigurowanej sesji mikrofonu przekroczy ustawiony próg sygnału.

Projekt powstał z myślą o dyktowaniu tekstu, rozmowach VoIP, spotkaniach online, nagrywaniu i innych sytuacjach, w których odtwarzany dźwięk przeszkadza podczas korzystania z mikrofonu.

> DictaMute nie rozpoznaje mowy ani nie klasyfikuje dźwięku jako ludzkiego głosu. Reguła opiera się na aktywności sesji mikrofonowej oraz poziomie sygnału.

**Publisher:** KeyffMS / aiteracja.pl  
**Repository:** https://github.com/KeyffMS/DictaMute  
**Canonical product URL:** https://aiteracja.pl/DictaMute/ *(kanoniczny publiczny adres produktu)*

Aktualnie projekt jest rozwijany przed pierwszym publicznym wydaniem przez GitHub Releases.

---

## Jak działa DictaMute?

Aplikacja działa w tle, w zasobniku systemowym Windows, i monitoruje aktywność audio w czasie rzeczywistym.

Konfiguracja opiera się na dwóch grupach aplikacji:

| Grupa | Rola | Przykłady |
|---|---|---|
| **X — Źródła** | Aplikacje, których aktywność mikrofonowa uruchamia akcję | Windows Dictation, Discord, OBS, przeglądarka z Google Meet |
| **Y — Cele** | Aplikacje, które mają zostać ściszone, wyciszone lub zatrzymane | Tidal, Spotify, przeglądarka z YouTube |

Schemat działania:

```text
Aktywna sesja mikrofonu z grupy X
              ↓
Poziom sygnału przekracza próg
              ↓
DictaMute reaguje
              ↓
Aplikacje z grupy Y zostają
ściszone / wyciszone / spauzowane
              ↓
Poziom sygnału spada poniżej progu
              ↓
Mija skonfigurowany Hold Time
              ↓
Dźwięk zostaje przywrócony
```

---

## Najważniejsze funkcje

### Dynamiczne grupowanie aplikacji — X → Y

#### Grupa X — Źródła

Lista procesów, których aktywność na mikrofonie może uruchomić regułę.

Przykłady:

- Windows Dictation,
- Discord,
- OBS,
- Google Meet,
- inne aplikacje korzystające z mikrofonu.

#### Grupa Y — Cele

Lista procesów, które mają zostać wyciszone, ściszone lub zatrzymane.

Przykłady:

- Tidal,
- Spotify,
- YouTube,
- odtwarzacze multimedialne,
- przeglądarki.

### Tryb globalny

Opcjonalny tryb **„Wszystko poza X”** pozwala reagować na wszystkie pozostałe aplikacje emitujące dźwięk w systemie, bez konieczności ręcznego dodawania każdej z nich do grupy Y.

---

## Szybkie dodawanie aplikacji

Nie trzeba ręcznie wyszukiwać plików `.exe`.

Aplikację można dodać do odpowiedniej grupy za pomocą globalnego skrótu klawiszowego.

Przykładowy przebieg:

1. Aktywuj okno programu, który chcesz dodać.
2. Naciśnij odpowiedni skrót.
3. DictaMute wykryje proces znajdujący się na pierwszym planie.
4. Nazwa procesu oraz jego ikona zostaną automatycznie dodane do konfiguracji.

Przykładowe skróty:

```text
Ctrl + Alt + X  → dodaj aplikację do Źródeł
Ctrl + Alt + Y  → dodaj aplikację do Celów
```

---

## Detekcja aktywności mikrofonu

### Noise Gate / Threshold

DictaMute reaguje dopiero po przekroczeniu określonego poziomu sygnału wejściowego.

Przykład:

```text
Threshold: 3%
```

Próg pozwala ograniczyć reakcje na bardzo cichy sygnał, ale nie jest klasyfikatorem mowy: szum, klawiatura lub inne dźwięki przechwytywane przez mikrofon również mogą przekroczyć próg.

### Hold Time / Release Delay

Po zakończeniu mówienia aplikacja nie przywraca dźwięku natychmiast.

Można ustawić czas podtrzymania, np.:

```text
Hold Time: 1.5 s
```

Zapobiega to ciągłemu wyciszaniu i przywracaniu muzyki podczas krótkich przerw między słowami lub zdaniami.

---

## Tryby reakcji

Dla aplikacji docelowych można zastosować różne sposoby reakcji.

### Audio Ducking

Zmniejszenie głośności konkretnej aplikacji w mikserze Windows.

Przykłady:

```text
100% → 20%
100% → 0%
```

Po zakończeniu aktywności mikrofonowej poprzedni poziom głośności jest przywracany.

### Media Pause

Wysłanie komendy:

```text
Media_Play_Pause
```

Pozwala zatrzymać odtwarzanie zamiast jedynie je wyciszać.

Tryb ten może być używany m.in. z:

- Tidal,
- Spotify,
- odtwarzaczami multimedialnymi,
- kompatybilnymi aplikacjami przeglądarkowymi.

---

## Interfejs

Interfejs wykorzystuje ciemny motyw współdzielący język wizualny z innymi aplikacjami KeyffMS / aiteracja.pl.

### Sekcja Źródeł

Lista aplikacji korzystających z mikrofonu wraz z:

- ikoną procesu,
- nazwą aplikacji,
- wskaźnikiem aktywności,
- podglądem poziomu sygnału audio.

### Sekcja Celów

Lista aplikacji podlegających automatyzacji wraz z możliwością wyboru trybu:

```text
Mute / Ducking / Pause
```

### Sterowanie czułością

Konfiguracja parametrów:

- `Threshold` — próg aktywacji,
- `Release Time` / `Hold Time` — czas oczekiwania przed przywróceniem dźwięku.

---

## System Tray

DictaMute może pracować bez otwartego głównego okna.

Po zminimalizowaniu aplikacja pozostaje dostępna w zasobniku systemowym Windows.

Menu kontekstowe zawiera:

- **Enable / Disable** — szybkie włączenie lub wyłączenie automatyki,
- wybór profilu,
- otwarcie konfiguracji,
- zamknięcie aplikacji.

Przykładowe profile:

```text
Dyktowanie
Spotkania
Nagrywanie
```

Ikona w zasobniku może również wizualnie informować, czy automatyczne wyciszanie jest aktualnie aktywne.

---

## Przykład użycia

Załóżmy, że podczas dyktowania tekstu chcesz automatycznie wyciszać Tidal.

1. Uruchom DictaMute.
2. Uruchom Tidal i rozpocznij odtwarzanie muzyki.
3. Aktywuj okno Tidala.
4. Naciśnij:

```text
Ctrl + Alt + Y
```

5. Tidal zostaje dodany do aplikacji docelowych.
6. Uruchom dyktowanie Windows za pomocą:

```text
Win + H
```

7. Rozpocznij mówienie.
8. DictaMute wykrywa aktywność mikrofonu.
9. Głośność Tidala zostaje zmniejszona do `0%` lub odtwarzanie zostaje zatrzymane.
10. Po zakończeniu mówienia DictaMute odczekuje skonfigurowany `Hold Time`.
11. Odtwarzanie lub poprzednia głośność zostają przywrócone.

---

## Architektura techniczna

| Element | Założenie |
|---|---|
| System operacyjny | Windows 10 / Windows 11 |
| Język | C# |
| Platforma | .NET 8 (WPF) |
| Audio | Windows Audio Session API / WASAPI |
| Zarządzanie sesjami | AudioSessionManager |
| Pomiar poziomu audio | AudioMeterInformation |
| Tryb pracy | Background / System Tray |
| Licencja | MIT |

---

## Windows Audio API

Projekt zakłada wykorzystanie mechanizmów Windows Audio Session API do:

- wykrywania aktywnych sesji audio,
- identyfikowania procesów generujących dźwięk,
- monitorowania poziomu sygnału,
- odczytu `AudioMeterInformation`,
- zmiany poziomu głośności konkretnej sesji,
- wyciszania wybranych aplikacji,
- przywracania wcześniejszego poziomu audio.

---

## Wydajność

DictaMute jest projektowany do stałej pracy w tle przy niewielkim narzucie. Publiczne limity RAM/CPU nie są obecnie deklarowane jako gwarantowane, dopóki nie zostaną zmierzone na zdefiniowanym środowisku testowym.

---

## Przykładowy scenariusz

```text
Tidal gra muzykę
        ↓
Win + H
        ↓
Rozpoczyna się dyktowanie
        ↓
Poziom sygnału mikrofonu przekracza Threshold
        ↓
DictaMute wycisza Tidal
        ↓
Użytkownik mówi
        ↓
Użytkownik kończy mówić
        ↓
Hold Time: 1.5 s
        ↓
DictaMute przywraca dźwięk
```

---

## Zastosowania

DictaMute może być przydatny podczas:

- dyktowania tekstu,
- rozmów przez Discord,
- spotkań Google Meet / Teams,
- nagrywania głosu,
- streamowania,
- używania asystentów głosowych,
- nagrywania komend głosowych,
- pracy z aplikacjami Speech-to-Text.

---

## Prywatność

Aktualna implementacja odczytuje aktywność sesji audio oraz wartość miernika poziomu sygnału. Nie tworzy strumienia nagrywającego mikrofon i nie zapisuje próbek audio. Aplikacja nie zawiera telemetrii, analityki, uploadu logów ani automatycznego sprawdzania aktualizacji przez sieć.

Konfiguracja i log diagnostyczny są przechowywane lokalnie w `%LOCALAPPDATA%\DictaMute\`.

Szczegóły, zakres danych oraz zasady bezpiecznego zgłaszania błędów: [PRIVACY.md](PRIVACY.md).

---

## Bezpieczeństwo

DictaMute działa jako zwykła aplikacja bieżącego użytkownika. Nie instaluje sterownika ani usługi, nie wstrzykuje kodu do innych procesów i nie obchodzi mechanizmów DRM lub uprawnień Windows.

Granice bezpieczeństwa, zachowanie przy awariach i sposób zgłaszania podatności: [SECURITY.md](SECURITY.md).

---

## Licencja

Projekt jest udostępniany na licencji **MIT**.

Możesz go swobodnie:

- używać,
- modyfikować,
- rozwijać,
- rozpowszechniać,

zgodnie z warunkami licencji MIT.
