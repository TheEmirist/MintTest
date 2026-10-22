using System;
using System.Collections.Generic;
using UnityEngine;

namespace MintTest.Employees
{
    public sealed class FavoriteStore
    {
        const string Key = "employee-favorites";

        readonly List<int> ids = new List<int>();
        readonly HashSet<int> lookup = new HashSet<int>();

        public event Action Changed;

        public static FavoriteStore Load()
        {
            FavoriteStore store = new FavoriteStore();
            string raw = PlayerPrefs.GetString(Key, "");
            if (string.IsNullOrEmpty(raw))
                return store;

            string[] parts = raw.Split(',');
            for (int i = 0; i < parts.Length; i++)
            {
                if (int.TryParse(parts[i], out int id))
                    store.AddSilent(id);
            }

            return store;
        }

        public bool Contains(int id)
        {
            return lookup.Contains(id);
        }

        public int[] Ids()
        {
            return ids.ToArray();
        }

        public void Toggle(int id)
        {
            if (lookup.Contains(id))
            {
                lookup.Remove(id);
                ids.Remove(id);
            }
            else
            {
                lookup.Add(id);
                ids.Add(id);
            }

            Save();
            Changed?.Invoke();
        }

        void AddSilent(int id)
        {
            if (!lookup.Add(id))
                return;

            ids.Add(id);
        }

        void Save()
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            for (int i = 0; i < ids.Count; i++)
            {
                if (i > 0)
                    builder.Append(',');
                builder.Append(ids[i]);
            }

            PlayerPrefs.SetString(Key, builder.ToString());
            PlayerPrefs.Save();
        }
    }
}
