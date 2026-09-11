# Aktualizacja mobilna 9 września

## Testowanie

Zmiany źródłowe, bez publikacji PWA. Kompilacja Assembly-CSharp przeszła. Testy dotykowe na iPhonie i testy NUnit w Unity pozostają do wykonania.

1. Profil: biały kursor na końcu nicku, znikająca podpowiedź, zapis poprawnej nazwy na żywo, zamknięcie edycji po klawiaturze lub kliknięciu tła. Swipe w prawo przechodzi do następnej kategorii, w lewo do poprzedniej. Po LUDZIE są RAMKI i REWERSY.
2. Spiny: migracja do pełnego zapasu 3; odzyskiwanie jednego co 4 godziny, także offline, maksymalnie 3. Wydanie kolejnego nie resetuje rozpoczętego odliczania. Szansa na kosmetyk 10%, drugi 5%, trzeci i kolejne 1,67%. Wśród dostępnych kosmetyków 80% avatar, 20% ramka (najnowsza dyspozycja z 10 września zastępuje wcześniejsze rewersy). Po wyczerpaniu jednej kolekcji wygrywa druga; po obu koło zatrzymuje się na diamentach, nigdy nie pokazuje waluty pod znakiem zapytania. Pusty obiekt zapisanej nagrody jest odrzucany. Przycisk zawsze „Zakręć spinem”; bez procentów, tylko licznik i komunikat odnowienia. Nagroda dopisywana po prezentacji, przerwane poprawne losowanie można wznowić bez kolejnej opłaty.
3. Ramki: 23 pliki z Download/ramki przypisane do poziomów nazwą pliku; poziomy od 10 do 400, stałe identyfikatory level:N. Odblokowanie nie zakłada ramki automatycznie. Awans daje złoto oraz diamenty równe osiągniętemu poziomowi; komunikat czeka do otwarcia menu/profilu.
4. Boty: host klika wiersz bota, aby przełączyć Początkujący/Zaawansowany. Zaawansowany ma avatar man10 (hantel), używa 96 losowych symulacji nieznanych kart, tylko własnej ręki i publicznej liczby kart. Początkujący sprawdza z szansą 8% przy częściowym wsparciu i 16% bez wsparcia, nie sprawdza udowodnionego układu. Blef większy pozostaje 20%. Wygrana w meczu z zaawansowanym botem daje 15% więcej złota za mecz (bez mnożenia nagrody za poziom).
5. HotSeat: karty podsumowania skalują się od 2 do 6 graczy; większe symbole narożne, dotknięcie odkrytej karty daje podgląd i pauzę. Odkrywanie domyślnie co 1,14 s, pauza i przerwanie animacji. Ustawienia w prawym górnym rogu: tempo oraz wyjście do menu z potwierdzeniem.
6. Multiplayer/PWA: bez wymuszonego 16:9 na telefonach; nadal respektuje bezpieczny obszar systemu. Skrajny gracz nie jest już wypychany o 150 jednostek poza stół. Pusty obraz ładowania nie renderuje szarej planszy podczas pobierania.

## Kody lokalne

10 kliknięć logo w menu ujawnia WPROWADŹ KOD na dole sklepu.

* KYRE: jednorazowo na lokalny profil 500 złota i 25 diamentów.
* T9K4X: menu testowe w ustawieniach. Zmiana walut, poziomów, statystyk, odblokowanie kosmetyków i uzupełnienie spinów. Dodawanie poziomów w narzędziu to bezpośrednia zmiana EXP, bez fikcyjnych meczów i nagród.

Te kody i zapisy przeglądarkowe nie są zabezpieczeniem przed oszustwami. Nie używać jako autoryzacji płatnych nagród.

## Grafiki avatarów

56 plików Download/avatary ma identyczne hashe z Resources/ShopAvatars. Inny nowy zestaw wymaga wskazania folderu. Nie nadpisano tych grafik.

## Dalsze poprawki źródeł, 10 września

* Multiplayer: wspólny skalowany obszar stołu, miejsc i kart poza panelem układów; nowe tło i stół. Karty lokalne obok siebie po prawej, zakryte przeciwników w wachlarzu. Pierścienie tury i gotowości większe od ramki, czerwony puls i szybsza muzyka podczas przekroczenia czasu własnej tury.
* Ustawienia: suwak przycisków 100–160%. Lupa otwiera wyszukiwanie kombinacji, np. QQ99 niezależnie od kolejności par. Szerszy log dopasowany wysokością do tekstu.
* HotSeat: status nad listą graczy, większa przewijana lista, wynik bez zwycięzcy, zielone/czerwone obramowania dowodów. Dialog ramki ciemnozielony z większym odstępem nagłówków. EXP krótszy, okno awansu uruchamiane także przy już otwartym menu.
* Powrót do meczu podłączony w menu. PWA ponawia pomiar po obrocie; nie dodaje animacji CSS. Animacji systemowej iOS nie da się uznać za wyłączoną na podstawie tej poprawki. Potrzebny test na urządzeniu.
* Zmiany nie zostały opublikowane. Kompilacja C# nie zastępuje testu multiplayer na dwóch telefonach ani testu PWA/iOS. Dodano testy regresji pustej nagrody i jednokrotnego odbioru ramki; wymagają uruchomienia w Unity.
