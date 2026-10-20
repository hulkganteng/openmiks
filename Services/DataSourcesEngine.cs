using System.Data;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;

namespace Openmiks.Services
{
    public interface IDataSource : IDisposable
    {
        string Name { get; }
        string SourcePathOrUrl { get; }
        int PollIntervalMs { get; set; }
        bool IsRunning { get; }
        IReadOnlyList<string> GetTableNames();
        DataTable? GetTable(string tableName);
        void Start();
        void Stop();
        event EventHandler<string>? TableChanged;
    }

    public abstract class DataSourceBase : IDataSource
    {
        public string Name { get; protected set; }
        public string SourcePathOrUrl { get; protected set; }
        public int PollIntervalMs { get; set; } = 5000;
        public bool IsRunning { get; private set; }

        protected readonly Dictionary<string, DataTable> _tables = new(StringComparer.OrdinalIgnoreCase);
        private Timer? _timer;
        private int _busy;

        public event EventHandler<string>? TableChanged;

        protected DataSourceBase(string name, string sourcePathOrUrl, int pollIntervalMs = 5000)
        {
            Name = name;
            SourcePathOrUrl = sourcePathOrUrl;
            PollIntervalMs = Math.Max(500, pollIntervalMs);
        }

        public IReadOnlyList<string> GetTableNames()
        {
            lock (_tables)
            {
                return _tables.Keys.ToList();
            }
        }

        public DataTable? GetTable(string tableName)
        {
            lock (_tables)
            {
                return _tables.TryGetValue(tableName, out var dt) ? dt.Copy() : null;
            }
        }

        public void Start()
        {
            if (IsRunning) return;
            IsRunning = true;
            Refresh();
            _timer = new Timer(_ => Refresh(), null, PollIntervalMs, PollIntervalMs);
        }

        public void Stop()
        {
            IsRunning = false;
            _timer?.Dispose();
            _timer = null;
        }

        public void Refresh()
        {
            if (Interlocked.Exchange(ref _busy, 1) != 0) return;
            try
            {
                UpdateTables();
            }
            finally
            {
                Interlocked.Exchange(ref _busy, 0);
            }
        }

        protected abstract void UpdateTables();

        protected void SetTable(string tableName, DataTable table)
        {
            lock (_tables)
            {
                _tables[tableName] = table;
            }
            TableChanged?.Invoke(this, tableName);
        }

        public virtual void Dispose()
        {
            Stop();
            lock (_tables)
            {
                _tables.Clear();
            }
        }
    }

    public class CsvDataSource : DataSourceBase
    {
        public bool FirstRowHasHeaders { get; set; } = true;

        public CsvDataSource(string name, string filePathOrUrl, int pollIntervalMs = 5000)
            : base(name, filePathOrUrl, pollIntervalMs)
        {
        }

        protected override void UpdateTables()
        {
            string content;
            if (SourcePathOrUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                SourcePathOrUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
                content = client.GetStringAsync(SourcePathOrUrl).GetAwaiter().GetResult();
            }
            else
            {
                if (!File.Exists(SourcePathOrUrl)) return;
                content = File.ReadAllText(SourcePathOrUrl);
            }

            var dt = ParseCsv(content, FirstRowHasHeaders);
            SetTable("Table1", dt);
        }

        private static DataTable ParseCsv(string text, bool headers)
        {
            var dt = new DataTable();
            using var reader = new StringReader(text);
            string? line = reader.ReadLine();
            if (line == null) return dt;

            var firstRow = SplitCsvLine(line);
            if (headers)
            {
                for (int i = 0; i < firstRow.Count; i++)
                {
                    string colName = string.IsNullOrWhiteSpace(firstRow[i]) ? $"Column{i + 1}" : firstRow[i].Trim();
                    dt.Columns.Add(colName);
                }
            }
            else
            {
                for (int i = 0; i < firstRow.Count; i++)
                    dt.Columns.Add($"Column{i + 1}");

                dt.Rows.Add(firstRow.Cast<object>().ToArray());
            }

            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var row = SplitCsvLine(line);
                while (row.Count < dt.Columns.Count) row.Add(string.Empty);
                dt.Rows.Add(row.Take(dt.Columns.Count).Cast<object>().ToArray());
            }

            return dt;
        }

        private static List<string> SplitCsvLine(string line)
        {
            var res = new List<string>();
            var matches = Regex.Matches(line, "(?<=^|,)(\"(?:[^\"]|\"\")*\"|[^,]*)");
            foreach (Match m in matches)
            {
                if (m.Index == line.Length && m.Length == 0) continue;
                string val = m.Value.Trim();
                if (val.StartsWith('"') && val.EndsWith('"') && val.Length >= 2)
                    val = val.Substring(1, val.Length - 2).Replace("\"\"", "\"");
                res.Add(val);
            }
            return res;
        }
    }

    public class JsonDataSource : DataSourceBase
    {
        public JsonDataSource(string name, string filePathOrUrl, int pollIntervalMs = 5000)
            : base(name, filePathOrUrl, pollIntervalMs)
        {
        }

        protected override void UpdateTables()
        {
            string content;
            if (SourcePathOrUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                SourcePathOrUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
                content = client.GetStringAsync(SourcePathOrUrl).GetAwaiter().GetResult();
            }
            else
            {
                if (!File.Exists(SourcePathOrUrl)) return;
                content = File.ReadAllText(SourcePathOrUrl);
            }

            var dt = ParseJson(content);
            SetTable("Table1", dt);
        }

        private static DataTable ParseJson(string json)
        {
            var dt = new DataTable();
            using var doc = JsonDocument.Parse(json);
            JsonElement root = doc.RootElement;

            JsonElement items = root.ValueKind == JsonValueKind.Array ? root :
                (root.TryGetProperty("data", out var d) && d.ValueKind == JsonValueKind.Array ? d :
                 (root.TryGetProperty("items", out var it) && it.ValueKind == JsonValueKind.Array ? it : root));

            if (items.ValueKind == JsonValueKind.Array)
            {
                var colSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var el in items.EnumerateArray())
                {
                    if (el.ValueKind != JsonValueKind.Object) continue;
                    foreach (var prop in el.EnumerateObject())
                    {
                        if (colSet.Add(prop.Name))
                            dt.Columns.Add(prop.Name);
                    }
                }

                foreach (var el in items.EnumerateArray())
                {
                    if (el.ValueKind != JsonValueKind.Object) continue;
                    var row = dt.NewRow();
                    foreach (var prop in el.EnumerateObject())
                    {
                        row[prop.Name] = prop.Value.ToString();
                    }
                    dt.Rows.Add(row);
                }
            }
            else if (items.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in items.EnumerateObject())
                    dt.Columns.Add(prop.Name);

                var row = dt.NewRow();
                foreach (var prop in items.EnumerateObject())
                    row[prop.Name] = prop.Value.ToString();
                dt.Rows.Add(row);
            }

            return dt;
        }
    }

    public class RssDataSource : DataSourceBase
    {
        public RssDataSource(string name, string url, int pollIntervalMs = 15000)
            : base(name, url, pollIntervalMs)
        {
        }

        protected override void UpdateTables()
        {
            string content;
            if (SourcePathOrUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                SourcePathOrUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
                content = client.GetStringAsync(SourcePathOrUrl).GetAwaiter().GetResult();
            }
            else
            {
                if (!File.Exists(SourcePathOrUrl)) return;
                content = File.ReadAllText(SourcePathOrUrl);
            }

            var dt = new DataTable();
            dt.Columns.Add("Title");
            dt.Columns.Add("Description");
            dt.Columns.Add("Link");
            dt.Columns.Add("PubDate");

            var doc = XDocument.Parse(content);
            foreach (var item in doc.Descendants("item"))
            {
                string title = item.Element("title")?.Value ?? string.Empty;
                string desc = item.Element("description")?.Value ?? string.Empty;
                string link = item.Element("link")?.Value ?? string.Empty;
                string pubDate = item.Element("pubDate")?.Value ?? string.Empty;

                dt.Rows.Add(title, desc, link, pubDate);
            }

            SetTable("Feed", dt);
        }
    }

    public class DataSourceManager : IDisposable
    {
        private readonly Dictionary<string, IDataSource> _sources = new(StringComparer.OrdinalIgnoreCase);

        public IReadOnlyCollection<IDataSource> Sources
        {
            get
            {
                lock (_sources)
                {
                    return _sources.Values.ToList();
                }
            }
        }

        public void AddSource(IDataSource source)
        {
            lock (_sources)
            {
                _sources[source.Name] = source;
            }
            source.Start();
        }

        public bool RemoveSource(string name)
        {
            IDataSource? src;
            lock (_sources)
            {
                if (!_sources.TryGetValue(name, out src)) return false;
                _sources.Remove(name);
            }
            src.Dispose();
            return true;
        }

        public IDataSource? GetSource(string name)
        {
            lock (_sources)
            {
                return _sources.TryGetValue(name, out var src) ? src : null;
            }
        }

        public string GetValue(string sourceName, string table, string column, int rowIndex = 0)
        {
            var src = GetSource(sourceName);
            if (src == null) return string.Empty;
            var dt = src.GetTable(table);
            if (dt == null || rowIndex < 0 || rowIndex >= dt.Rows.Count) return string.Empty;
            if (!dt.Columns.Contains(column)) return string.Empty;
            return dt.Rows[rowIndex][column]?.ToString() ?? string.Empty;
        }

        public void Dispose()
        {
            lock (_sources)
            {
                foreach (var s in _sources.Values) s.Dispose();
                _sources.Clear();
            }
        }
    }
}
