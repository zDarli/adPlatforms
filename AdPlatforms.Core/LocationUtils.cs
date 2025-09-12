using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AdPlatforms.Core
{
    public class LocationUtils
    {
        public static string NormalizePath(string raw) // нормализация пути
        {
            // защита от пустой строки
            if (string.IsNullOrEmpty(raw))
                return "/";

            var sb = new StringBuilder(raw.Length);
            bool lastWasSlash = false;

            foreach (char ch in raw)
            {
                if (ch == ' ') continue;           // убрать пробелы 

                if (ch == '/')
                {
                    if (!lastWasSlash)             // последовательность /// в /
                    {
                        sb.Append('/');
                        lastWasSlash = true;
                    }
                }
                else
                {
                    sb.Append(ch);
                    lastWasSlash = false;
                }
            }

            // ведущий слэш
            if (sb.Length == 0 || sb[0] != '/')
                sb.Insert(0, '/');

            // убираем завершающий слэш
            if (sb.Length > 1 && sb[sb.Length - 1] == '/')
                sb.Length -= 1;

            return sb.ToString();
        }

        public static Dictionary<string, HashSet<string>> BuiltIndexFromText(string text) // построение индекса из текста
        {
            Dictionary<string, HashSet<string>> index = new Dictionary<string, HashSet<string>>();

            var lines = text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = lines[i].Trim();
            }

            foreach (var rawline in lines)
            {
                if (rawline.Length == 0) continue;
                if (rawline.Split(':').Length != 2) continue;

                string[] parts = rawline.Split(':');
                string platfrom = parts[0];                    //платформа
                string[] oldPaths = parts[1].Split(',');

                for (int i = 0; i < oldPaths.Length; i++)
                {
                    oldPaths[i] = NormalizePath(oldPaths[i]);
                }

                List<string> paths = new List<string>(oldPaths);//все пути

                foreach (var path in paths)
                {
                    if (!index.TryGetValue(path, out var set))
                    {
                        set = new HashSet<string>();
                        index[path] = set;
                    }
                }
                foreach (var path in oldPaths)
                {
                    index[path].Add(platfrom);
                }
            }
            return index;
        }

        public static string[] GetAllParentPaths(string raw) // получение всех индексов пути
        {
            List<string> paths = new List<string>();
            string path = NormalizePath(raw);
            paths.Add(path);
            string temp = "/";
            for (int i = 1; i < path.Length; i++)
            {
                if (path[i] == '/')
                {
                    paths.Add(temp);
                }
                temp += path[i];
            }
            return paths.ToArray();
        }

        public static string FindPlatforms(string rawPath, Dictionary<string, HashSet<string>> dict)
        {
            
            string[] paths = GetAllParentPaths(rawPath);
            HashSet<string> resultSet = new HashSet<string>();

            foreach (var path in paths)
            {
                if (dict.TryGetValue(path, out var platforms))
                {
                    resultSet.UnionWith(platforms);
                }
            }
            return string.Join(", ",resultSet);
        }

    }

}
