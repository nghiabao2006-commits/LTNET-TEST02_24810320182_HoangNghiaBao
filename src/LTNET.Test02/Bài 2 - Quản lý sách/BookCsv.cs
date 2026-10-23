using System.Globalization;
using System.Text;

namespace LTNET.Test02.Books;

// RFC 4180 quoting: commas, quotes and line breaks inside a field round-trip correctly.
public static class BookCsv
{
    private static readonly string[] Header = ["BookId", "Title", "Author", "Price", "Quantity", "CoverPath"];
    public static void Save(string path, IEnumerable<BookModel> books)
    {
        string directory = Path.GetDirectoryName(Path.GetFullPath(path))!;
        // Construct before writing, so a validation failure cannot truncate an existing report.
        var text = new StringBuilder().AppendLine(string.Join(',', Header));
        foreach (var book in books)
        {
            Validate(book);
            string cover = string.IsNullOrEmpty(book.CoverPath) ? "" : Path.GetRelativePath(directory, book.CoverPath);
            string[] fields = [book.BookId, book.Title, book.Author, book.Price.ToString(CultureInfo.InvariantCulture), book.Quantity.ToString(CultureInfo.InvariantCulture), cover];
            text.AppendLine(string.Join(',', fields.Select(Quote)));
        }
        File.WriteAllText(path, text.ToString(), new UTF8Encoding(true));
    }

    public static List<BookModel> Load(string path)
    {
        using var reader = new StreamReader(path, Encoding.UTF8, true);
        var rows = ReadRecords(reader).ToList();
        if (rows.Count == 0 || !rows[0].SequenceEqual(Header))
            throw new FormatException("CSV phải có tiêu đề: " + string.Join(',', Header));
        var books = new List<BookModel>();
        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        string directory = Path.GetDirectoryName(Path.GetFullPath(path))!;
        for (int i = 1; i < rows.Count; i++)
        {
            var fields = rows[i];
            if (fields.Length != 6) throw new FormatException($"Bản ghi {i + 1}: cần đúng 6 cột.");
            if (!decimal.TryParse(fields[3], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var price) ||
                !int.TryParse(fields[4], NumberStyles.None, CultureInfo.InvariantCulture, out var quantity))
                throw new FormatException($"Bản ghi {i + 1}: giá hoặc số lượng không hợp lệ.");
            var book = new BookModel
            {
                BookId = fields[0].Trim(), Title = fields[1], Author = fields[2], Price = price, Quantity = quantity,
                CoverPath = fields[5].Length == 0 ? "" : Path.GetFullPath(fields[5], directory)
            };
            Validate(book);
            if (!ids.Add(book.BookId)) throw new FormatException($"Trùng mã sách: {book.BookId}.");
            books.Add(book);
        }
        return books;
    }

    public static void Validate(BookModel book)
    {
        if (string.IsNullOrWhiteSpace(book.BookId) || string.IsNullOrWhiteSpace(book.Title) || string.IsNullOrWhiteSpace(book.Author))
            throw new FormatException("Mã sách, tên sách và tác giả không được để trống.");
        if (book.Price <= 0 || book.Price > 1_000_000_000m || book.Quantity < 0 || book.Quantity > 1_000_000)
            throw new FormatException("Giá phải lớn hơn 0 và không quá 1 tỷ; số lượng từ 0 đến 1 triệu.");
    }

    private static string Quote(string value) => value.IndexOfAny([',', '"', '\r', '\n']) >= 0
        ? "\"" + value.Replace("\"", "\"\"") + "\"" : value;

    private static IEnumerable<string[]> ReadRecords(TextReader reader)
    {
        var fields = new List<string>();
        var field = new StringBuilder();
        bool quoted = false, afterQuote = false, started = false;
        int raw;
        while ((raw = reader.Read()) != -1)
        {
            char c = (char)raw;
            started = true;
            if (quoted)
            {
                if (c != '"') field.Append(c);
                else if (reader.Peek() == '"') { reader.Read(); field.Append('"'); }
                else { quoted = false; afterQuote = true; }
                continue;
            }
            if (c == ',')
            {
                fields.Add(field.ToString()); field.Clear(); afterQuote = false;
            }
            else if (c is '\r' or '\n')
            {
                if (c == '\r' && reader.Peek() == '\n') reader.Read();
                fields.Add(field.ToString());
                yield return fields.ToArray();
                fields.Clear(); field.Clear(); afterQuote = false; started = false;
            }
            else if (c == '"' && field.Length == 0 && !afterQuote) quoted = true;
            else
            {
                if (afterQuote || c == '"') throw new FormatException("Dấu nháy trong CSV không hợp lệ.");
                field.Append(c);
            }
        }
        if (quoted) throw new FormatException("CSV thiếu dấu nháy đóng.");
        if (started) { fields.Add(field.ToString()); yield return fields.ToArray(); }
    }
}
