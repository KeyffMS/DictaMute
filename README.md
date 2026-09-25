# DictaMute

**DictaMute** to aplikacja dla Windows 10/11, która automatycznie wycisza, ścisza lub pauzuje wskazane aplikacje, gdy wykryje aktywność głosową w wybranych aplikacjach korzystających z mikrofonu.

Projekt powstał z myślą o dyktowaniu tekstu, rozmowach VoIP, spotkaniach online oraz innych sytuacjach, w których odtwarzana muzyka lub dźwięk przeszkadzają podczas korzystania z mikrofonu.

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
Aplikacja z grupy X wykrywa głos
              ↓
Przekroczony zostaje próg głośności
              ↓
DictaMute reaguje
              ↓
Aplikacje z grupy Y zostają
ściszone / wyciszone / spauzowane
              ↓
Użytkownik przestaje mówić
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

## Detekcja głosu

### Noise Gate / Threshold

DictaMute reaguje dopiero po przekroczeniu określonego poziomu sygnału wejściowego.

Przykład:

```text
Threshold: 3%
```

Dzięki temu przypadkowe szumy, delikatne dźwięki otoczenia lub stukanie w klawiaturę nie muszą powodować wyciszenia muzyki.

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

Planowany interfejs wykorzystuje ciemny motyw dopasowany do Windows 10/11.

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

Menu kontekstowe może zawierać:

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
| Platforma | .NET 8 / .NET Framework 4.8 |
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

## Założenia wydajnościowe

DictaMute ma działać stale w tle przy możliwie niewielkim wykorzystaniu zasobów.

Zakładany cel:

```text
RAM: < 30 MB
CPU idle: < 0.5%
```

---

## Przykładowy scenariusz

```text
Tidal gra muzykę
        ↓
Win + H
        ↓
Rozpoczyna się dyktowanie
        ↓
Mikrofon przekracza Threshold
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

## Licencja

Projekt jest udostępniany na licencji **MIT**.

Możesz go swobodnie:

- używać,
- modyfikować,
- rozwijać,
- rozpowszechniać,

zgodnie z warunkami licencji MIT.
