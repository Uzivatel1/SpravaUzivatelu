using Microsoft.EntityFrameworkCore;

namespace SpravaUzivatelu.Data
{
    public class PaginatedList<T> : List<T>
    {
        // Index aktuální stránky (začíná od 1)
        public int PageIndex { get; private set; }

        // Celkový počet stránek
        public int TotalPages { get; private set; }

        // Konstruktor inicializuje stránkovaný seznam
        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize); // Výpočet celkového počtu stránek

            // Přidání položek do seznamu
            this.AddRange(items);
        }

        // Indikuje, zda existuje předchozí stránka
        public bool HasPreviousPage => PageIndex > 1;

        // Indikuje, zda existuje další stránka
        public bool HasNextPage => PageIndex < TotalPages;

        // Vytvoří stránkovaný seznam asynchronně z IQueryable zdroje
        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
        {
            // Získá celkový počet záznamů ve zdroji
            var count = await source.CountAsync();

            // Načte pouze položky pro aktuální stránku
            var items = await source
                .Skip((pageIndex - 1) * pageSize) // Přeskočí položky před aktuální stránkou
                .Take(pageSize) // Načte pouze požadovaný počet položek
                .ToListAsync(); // Převod na seznam

            // Vytvoří a vrátí instanci PaginatedList
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }
    }
}
