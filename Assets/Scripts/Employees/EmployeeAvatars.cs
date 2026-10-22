using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace MintTest.Employees
{
    public interface IEmployeeAvatars
    {
        event Action Changed;

        Sprite SpriteFor(int employeeId);
    }

    public sealed class EmployeeAvatars : IEmployeeAvatars
    {
        public const int Count = 5;

        const string FolderName = "employee-avatars";
        const string PrimaryUrl = "https://picsum.photos/200";

        readonly Sprite[] sprites = new Sprite[Count];
        readonly Dictionary<int, int> assigned = new Dictionary<int, int>();

        static Sprite placeholder;
        byte[] fetched;

        public event Action Changed;

        public static Sprite Placeholder => placeholder;

        public static void SetPlaceholder(Sprite sprite)
        {
            placeholder = sprite;
        }

        public Sprite SpriteFor(int employeeId)
        {
            if (!assigned.TryGetValue(employeeId, out int index))
                return null;

            if (index < 0 || index >= sprites.Length)
                return null;

            return sprites[index];
        }

        public void LoadSaved()
        {
            for (int i = 0; i < Count; i++)
            {
                string path = ImagePath(i);
                if (!File.Exists(path))
                    continue;

                byte[] data;
                try
                {
                    data = File.ReadAllBytes(path);
                }
                catch (Exception exception)
                {
                    Debug.LogWarning("EmployeeAvatars: не прочитал " + path + " (" + exception.Message + ").");
                    continue;
                }

                if (!TryCreateSprite(i, data))
                {
                    try
                    {
                        File.Delete(path);
                    }
                    catch (Exception exception)
                    {
                        Debug.LogWarning("EmployeeAvatars: не удалил битый файл (" + exception.Message + ").");
                    }
                }
            }

            LoadMap();
        }

        public IEnumerator Ensure(EmployeeRecord[] records)
        {
            int gained = 0;
            for (int i = 0; i < Count; i++)
            {
                if (sprites[i] != null)
                    continue;

                byte[] candidate = null;
                string token = i + "-" + DateTime.UtcNow.Ticks;
                string[] urls =
                {
                    PrimaryUrl,
                    PrimaryUrl + "?t=" + token,
                    "https://i.pravatar.cc/200?u=" + token
                };

                for (int attempt = 0; attempt < urls.Length; attempt++)
                {
                    yield return Fetch(urls[attempt]);
                    if (fetched == null)
                        continue;

                    candidate = fetched;
                    if (!Duplicates(candidate))
                        break;
                }

                if (candidate != null && Adopt(i, candidate))
                    gained++;
                else
                    Debug.LogWarning("EmployeeAvatars: картинка " + i + " не загрузилась, остаётся placeholder.");
            }

            if (!HasSprite())
                Debug.LogWarning("EmployeeAvatars: картинки не загрузились, на карточках остаётся placeholder.");

            bool mapped = EnsureMap(records);
            if (gained > 0 || mapped)
                Changed?.Invoke();
        }

        IEnumerator Fetch(string url)
        {
            fetched = null;
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.timeout = 20;
                request.SetRequestHeader("Accept", "image/jpeg,image/png,image/*");
                request.SetRequestHeader("Cache-Control", "no-cache");
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning("EmployeeAvatars: не скачал " + url + " (" + request.error + ").");
                    yield break;
                }

                byte[] data = request.downloadHandler.data;
                if (!IsImage(data))
                {
                    Debug.LogWarning("EmployeeAvatars: ответ не картинка " + url + ".");
                    yield break;
                }

                fetched = new byte[data.Length];
                Array.Copy(data, fetched, data.Length);
            }
        }

        bool Adopt(int index, byte[] data)
        {
            if (!WriteFile(ImagePath(index), data))
                return false;

            if (TryCreateSprite(index, data))
                return true;

            try
            {
                File.Delete(ImagePath(index));
            }
            catch (Exception exception)
            {
                Debug.LogWarning("EmployeeAvatars: не удалил файл после ошибки (" + exception.Message + ").");
            }

            return false;
        }

        bool TryCreateSprite(int index, byte[] data)
        {
            if (!IsImage(data))
                return false;

            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!texture.LoadImage(data))
            {
                UnityEngine.Object.Destroy(texture);
                return false;
            }

            ReplaceSprite(index, texture);
            return true;
        }

        void ReplaceSprite(int index, Texture2D texture)
        {
            if (sprites[index] != null)
            {
                Texture2D previous = sprites[index].texture;
                UnityEngine.Object.Destroy(sprites[index]);
                if (previous != null)
                    UnityEngine.Object.Destroy(previous);
            }

            sprites[index] = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100f);
        }

        bool EnsureMap(EmployeeRecord[] records)
        {
            if (records == null || records.Length == 0 || !HasSprite())
                return false;

            int[] pool = Available();
            if (pool.Length == 0)
                return false;

            bool dirty = false;
            int previous = -1;
            for (int i = 0; i < records.Length; i++)
            {
                EmployeeRecord record = records[i];
                if (record == null)
                    continue;

                if (assigned.TryGetValue(record.id, out int current))
                {
                    previous = current;
                    continue;
                }

                int next = NextPinned(records, i + 1);
                assigned[record.id] = Pick(pool, previous, next);
                previous = assigned[record.id];
                dirty = true;
            }

            if (!dirty)
                return false;

            SaveMap();
            return true;
        }

        int NextPinned(EmployeeRecord[] records, int start)
        {
            for (int i = start; i < records.Length; i++)
            {
                EmployeeRecord record = records[i];
                if (record == null)
                    continue;

                if (assigned.TryGetValue(record.id, out int index))
                    return index;
            }

            return -1;
        }

        static int Pick(int[] pool, int previous, int next)
        {
            if (pool.Length == 1)
                return pool[0];

            for (int attempt = 0; attempt < 12; attempt++)
            {
                int pick = pool[UnityEngine.Random.Range(0, pool.Length)];
                if (pick != previous && pick != next)
                    return pick;
            }

            for (int i = 0; i < pool.Length; i++)
            {
                if (pool[i] != previous && pool[i] != next)
                    return pool[i];
            }

            return pool[0];
        }

        int[] Available()
        {
            int count = 0;
            for (int i = 0; i < Count; i++)
            {
                if (sprites[i] != null)
                    count++;
            }

            int[] pool = new int[count];
            int cursor = 0;
            for (int i = 0; i < Count; i++)
            {
                if (sprites[i] == null)
                    continue;

                pool[cursor] = i;
                cursor++;
            }

            return pool;
        }

        bool HasSprite()
        {
            for (int i = 0; i < Count; i++)
            {
                if (sprites[i] != null)
                    return true;
            }

            return false;
        }

        bool Duplicates(byte[] data)
        {
            for (int i = 0; i < Count; i++)
            {
                string path = ImagePath(i);
                if (!File.Exists(path))
                    continue;

                byte[] saved;
                try
                {
                    saved = File.ReadAllBytes(path);
                }
                catch (Exception)
                {
                    continue;
                }

                if (Same(saved, data))
                    return true;
            }

            return false;
        }

        void LoadMap()
        {
            string path = MapPath();
            if (!File.Exists(path))
                return;

            string[] lines;
            try
            {
                lines = File.ReadAllLines(path);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("EmployeeAvatars: не прочитал назначения (" + exception.Message + ").");
                return;
            }

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                int split = line.IndexOf(':');
                if (split <= 0)
                    continue;

                if (!int.TryParse(line.Substring(0, split), out int id))
                    continue;

                if (!int.TryParse(line.Substring(split + 1), out int index))
                    continue;

                if (index < 0 || index >= Count)
                    continue;

                assigned[id] = index;
            }
        }

        void SaveMap()
        {
            int[] ids = new int[assigned.Count];
            assigned.Keys.CopyTo(ids, 0);

            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            for (int i = 0; i < ids.Length; i++)
            {
                builder.Append(ids[i]);
                builder.Append(':');
                builder.Append(assigned[ids[i]]);
                builder.Append('\n');
            }

            WriteFile(MapPath(), System.Text.Encoding.UTF8.GetBytes(builder.ToString()));
        }

        static bool WriteFile(string path, byte[] data)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllBytes(path, data);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning("EmployeeAvatars: не сохранил " + path + " (" + exception.Message + ").");
                return false;
            }
        }

        static bool IsImage(byte[] data)
        {
            if (data == null || data.Length < 8)
                return false;

            if (data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF)
                return true;

            return data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47;
        }

        static bool Same(byte[] left, byte[] right)
        {
            if (left == null || right == null || left.Length != right.Length)
                return false;

            for (int i = 0; i < left.Length; i++)
            {
                if (left[i] != right[i])
                    return false;
            }

            return true;
        }

        static string Folder()
        {
            return Path.Combine(Application.persistentDataPath, FolderName);
        }

        static string ImagePath(int index)
        {
            return Path.Combine(Folder(), index.ToString() + ".img");
        }

        static string MapPath()
        {
            return Path.Combine(Folder(), "assignments.txt");
        }
    }
}
