1. Cel i Główna Zasada Działania

Aplikacja działa cicho w tle (w zasobniku systemowym / System Tray) i w czasie rzeczywistym monitoruje sesje audio w systemie Windows 10. Jej zadaniem jest automatyczne obniżanie głośności, wyciszanie (Mute) lub wysyłanie sygnału PAUZA do wskazanych aplikacji (Grupa Y – Aplikacje Docelowe), ilekroć jakakolwiek aplikacja ze wskazanej listy (Grupa X – Aplikacje Źródłowe) zacznie korzystać z mikrofonu i przekroczy ustalony próg głośności.

Aplikacja rozwiązuje problem zakłóceń podczas dyktowania tekstu, rozmów Voice-over-IP czy nagrywania komend głosowych bez konieczności ręcznego pauzowania muzyki.
2. Kluczowe Funkcje i Zależności Logiczne
A. Dynamiczne Grupowanie Aplikacji (X → Y)

    Grupa X (Źródła / Aplikacje Nasłuchujące): Lista procesów, których aktywność na mikrofonie wyzwala akcję (np. Narzędzie dyktowania Windows, Discord, OBS, przeglądarka z otwartym Meetem).

    Grupa Y (Cele / Aplikacje Wyciszane): Lista procesów, które mają zostać wyciszone lub spauzowane (np. Tidal, Spotify, YouTube w przeglądarce).

    Tryb Globalny („Wszystko poza X”): Opcja wyciszania wszystkich pozostałych aplikacji emitujących dźwięk w systemie, z wyjątkiem wybranych aplikacji źródłowych.

B. Przechwytywanie Aplikacji Skrótem Klawiszowym (Szybkie Konfigurowanie)

Zamiast ręcznie szukać plików .exe na dysku, użytkownik może łatwo dodawać aplikacje do reguł za pomocą globalnego skrótu:

    Użytkownik klika w oknie aplikacji, którą chce dodać (np. w oknie Tidala lub aplikacji dyktującej).

    Wciska zdefiniowany skrót klawiszowy (np. Ctrl + Alt + X dla dodania do Źródeł lub Ctrl + Alt + Y dla dodania do Celów).

    Aplikacja wykrywa aktywny proces na pierwszym planie, pobiera jego nazwę oraz ikonę i automatycznie dodaje go do odpowiedniej listy w konfiguracji.

C. Inteligentna Detekcja Dźwięku i Histereza (Hold Time)

    Detekcja progu (Noise Gate): Aplikacja reaguje dopiero wtedy, gdy głośność z mikrofonu przekroczy ustawiony próg (np. >3%), co zapobiega wyciszaniu muzyki przez szumy tła lub stukanie w klawiaturę.

    Czas podtrzymania (Hold Time / Release Delay): Po zaprzestaniu mówienia aplikacja odczekuje skonfigurowany czas (np. 1.5 sekundy) zanim przywróci dźwięk. Zapobiega to gwałtownemu "skakaniu" i rwanemu włączaniu/wyłączaniu muzyki pomiędzy pauzami w zdaniach.

D. Dwa Tryby Reakcji na Aktywność

Dla aplikacji z Grupy Y użytkownik może wybrać preferowane zachowanie:

    Wyciszenie / Obniżenie głośności (Audio Ducking): Płynne wyciszenie suwaka głośności aplikacji w mikserze systemowym Windows do 0% (lub ustalonego poziomu, np. 20%).

    Pauza multimedialna (Media Pause): Wysłanie komendy Media_Play_Pause bezpośrednio do procesu (lub globalnie), co zatrzymuje odtwarzanie utworu w aplikacjach takich jak Tidal czy Spotify, zamiast tylko wyciszać dźwięk.

3. Interfejs Użytkownika i Doświadczenie (UX/UI)
Interfejs Główny (Dark Mode)

    Estetyka: Nowoczesny, ciemny motyw (Dark Mode) dopasowany do stylistyki Windows 10/11 (np. Fluent Design / WPF z w ciemnej palecie barw).

    Sekcja Źródeł (Mikrofon / Aplikacje X): Lista aktywnych aplikacji z ikonami i wskaźnikiem na żywo (pasek głośności mikrofonu), pokazującym aktualną aktywność.

    Sekcja Celów (Odtwarzacze / Aplikacje Y): Lista wyciszanych aplikacji z przełącznikami trybu (Wycisz vs Pauza).

    Panel Sterowania Czułością: Suwaki do regulacji progu czułości mikrofonu (Threshold) oraz czasu opóźnienia powrotu dźwięku (Release Time).

Praca w Tle (System Tray)

    Po zamknięciu głównego okna aplikacja minimalizuje się do zasobnika systemowego (obok zegarka).

    Menu kontekstowe pod prawym przyciskiem myszy:

        Szybkie włączenie / wyłączenie automatyki (Enable/Disable).

        Wybór profilu (np. "Dyktowanie", "Spotkania/Meetings").

        Otwórz konfigurację / Wyjście.

    Ikona w trayu: Zmienia stan wizualny (np. podświetla się), gdy automatyczne wyciszenie jest w danej chwili aktywne.

4. Architektura Techniczna i Wymagania

    System operacyjny: Windows 10 (oraz Windows 11).

    Stos technologiczny: C# / .NET 8 (lub .NET Framework 4.8 dla maksymalnej zgodności bez instalacji dodatkowych bibliotek uruchomieniowych).

    Komunikacja z Audio Windows: Wykorzystanie interfejsów Windows Audio Session API (WASAPI) do nasłuchu liczników AudioMeterInformation oraz zarządzania AudioSessionManager dla poszczególnych procesów.

    Licencja: MIT License – w pełni otwarty kod źródłowy, zezwalający na swobodne użycie, modyfikację i dystrybucję.

    Wydajność: Niskie zużycie zasobów (cel: <30 MB RAM, <0.5% CPU w stanie czuwania).

5. Przebieg Scenariusza Użycia (Example Workflow)

    Użytkownik uruchamia program, włącza Tidala i zaczyna słuchać muzyki.

    Użytkownik klika okno Tidala i wciska Ctrl + Alt + Y – Tidal zostaje dodany do listy wyciszanych.

    Użytkownik wciska skrót dyktowania w Windows (Win + H).

    Aplikacja wykrywa, że mikrofon zaczął rejestrować głos.

    Pasek głośności w Tidalu natychmiast spada do 0% (lub wyzwalana jest pauza).

    Użytkownik kończy dyktować i milknie.

    Po odczekaniu 1.5 sekundy ciszy na mikrofonie, aplikacja płynnie przywraca dźwięk w Tidalu.
