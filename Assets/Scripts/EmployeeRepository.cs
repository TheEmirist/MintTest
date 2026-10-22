using System;
using System.Collections.Generic;
using UnityEngine;

public interface IEmployeeLookup
{
    bool TryGet(int id, out EmployeeRecord record);
}

public interface IEmployeeCatalog
{
    EmployeeRecord[] All { get; }
}

public sealed class JsonEmployeeRepository : IEmployeeLookup, IEmployeeCatalog
{
    public static readonly JsonEmployeeRepository Empty = new JsonEmployeeRepository(Array.Empty<EmployeeRecord>());

    readonly EmployeeRecord[] all;
    readonly Dictionary<int, EmployeeRecord> byId;

    JsonEmployeeRepository(EmployeeRecord[] records)
    {
        all = records ?? Array.Empty<EmployeeRecord>();
        byId = new Dictionary<int, EmployeeRecord>(all.Length);
        for (int i = 0; i < all.Length; i++)
        {
            EmployeeRecord record = all[i];
            if (record != null)
                byId[record.id] = record;
        }
    }

    public EmployeeRecord[] All => all;

    public bool TryGet(int id, out EmployeeRecord record)
    {
        return byId.TryGetValue(id, out record);
    }

    public static bool TryLoad(string resourceName, out JsonEmployeeRepository repository)
    {
        repository = Empty;

        TextAsset asset = Resources.Load<TextAsset>(resourceName);
        if (asset == null)
            return false;

        EmployeeFile file = JsonUtility.FromJson<EmployeeFile>(asset.text);
        if (file == null || file.data == null)
            return false;

        repository = new JsonEmployeeRepository(file.data);
        return true;
    }

    [Serializable]
    public class EmployeeFile
    {
        public EmployeeRecord[] data;
    }
}

[Serializable]
public class EmployeeRecord
{
    public int id;
    public string first_name;
    public string last_name;
    public string email;
    public string gender;
    public string ip_address;
}
