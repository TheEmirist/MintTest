using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace MintTest.Employees
{
    public sealed class EmployeeDataStore
    {
        const string FileName = "test_task_mock_data.json";
        const string FallbackResource = "test_task_mock_data";
        const string DownloadUrl =
            "https://drive.google.com/uc?export=download&id=1YvE6Y5-vxVWXrrYyb83ssqMfVUUkndLw";

        public JsonEmployeeRepository Repository { get; private set; } = JsonEmployeeRepository.Empty;

        public IEnumerator Ensure()
        {
            if (TryLoadLocal())
                yield break;

            yield return Download();

            if (TryLoadLocal())
                yield break;

            if (JsonEmployeeRepository.TryLoad(FallbackResource, out JsonEmployeeRepository fallback))
            {
                Debug.LogWarning("EmployeeDataStore: скачивание не удалось, взят файл из Resources.");
                Repository = fallback;
                yield break;
            }

            Debug.LogError("EmployeeDataStore: нет локального файла и не удалось скачать данные.");
            Repository = JsonEmployeeRepository.Empty;
        }

        bool TryLoadLocal()
        {
            string path = LocalPath();
            if (!File.Exists(path))
                return false;

            string text;
            try
            {
                text = File.ReadAllText(path, Encoding.UTF8);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("EmployeeDataStore: не прочитал " + path + " (" + exception.Message + ").");
                return false;
            }

            if (!JsonEmployeeRepository.TryParse(text, out JsonEmployeeRepository repository))
            {
                Debug.LogWarning("EmployeeDataStore: локальный файл битый, будет скачан заново.");
                try
                {
                    File.Delete(path);
                }
                catch (Exception exception)
                {
                    Debug.LogWarning("EmployeeDataStore: не удалил битый файл (" + exception.Message + ").");
                }

                return false;
            }

            Repository = repository;
            return true;
        }

        IEnumerator Download()
        {
            using (UnityWebRequest request = UnityWebRequest.Get(DownloadUrl))
            {
                request.timeout = 60;
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning("EmployeeDataStore: не скачал данные (" + request.error + ").");
                    yield break;
                }

                byte[] data = request.downloadHandler.data;
                if (data == null || data.Length == 0)
                {
                    Debug.LogWarning("EmployeeDataStore: пустой ответ от Google Drive.");
                    yield break;
                }

                string text = Encoding.UTF8.GetString(data);
                if (!JsonEmployeeRepository.TryParse(text, out _))
                {
                    Debug.LogWarning("EmployeeDataStore: ответ не похож на JSON с сотрудниками.");
                    yield break;
                }

                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(LocalPath()));
                    File.WriteAllBytes(LocalPath(), data);
                }
                catch (Exception exception)
                {
                    Debug.LogWarning("EmployeeDataStore: не сохранил файл (" + exception.Message + ").");
                }
            }
        }

        static string LocalPath()
        {
            return Path.Combine(Application.persistentDataPath, FileName);
        }
    }
}
