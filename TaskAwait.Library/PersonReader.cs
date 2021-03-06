using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TaskAwait.Shared;

namespace TaskAwait.Library
{
    public class PersonReader
    {
        HttpClient client = 
            new HttpClient() { BaseAddress = new Uri("http://localhost:9874") };
        JsonSerializerOptions options = 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        public async Task<List<Person>> GetPeopleAsync(
            CancellationToken cancelToken = new CancellationToken())
        {
            //throw new NotImplementedException("GetAsync is not implemented.");

            cancelToken.ThrowIfCancellationRequested();

            HttpResponseMessage response = 
                await client.GetAsync("people", cancelToken).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var stringResult = 
                    await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return JsonSerializer.Deserialize<List<Person>>(stringResult, options);
            }
            return new List<Person>();
        }

        public async Task<List<int>> GetIdsAsync(
            CancellationToken cancelToken = new CancellationToken())
        {
            HttpResponseMessage response = 
                await client.GetAsync("people/ids", cancelToken).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var stringResult = 
                    await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return JsonSerializer.Deserialize<List<int>>(stringResult);
            }
            return new List<int>();
        }

        public Person GetPerson(int id)
        {
            var client = new WebClient();
            string address = $"http://localhost:9874/people/{id}";
            string reply = client.DownloadString(address);
            return JsonSerializer.Deserialize<Person>(reply, options);
        }

        public async Task<Person> GetPersonAsync(int id,
            CancellationToken cancelToken = new CancellationToken())
        {
            cancelToken.ThrowIfCancellationRequested();

            HttpResponseMessage response = 
                await client.GetAsync($"people/{id}", cancelToken).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var stringResult = 
                    await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return JsonSerializer.Deserialize<Person>(stringResult, options);
            }
            return new Person();
        }

        public async Task<Person> GetPersonAsyncWithFailures(int id,
            CancellationToken cancelToken = new CancellationToken())
        {
            HttpResponseMessage response =
                await client.GetAsync($"people/{id}", cancelToken).ConfigureAwait(false);

            if (id == 2)
            {
                throw new InvalidOperationException("Using id=2 is not supported by this method");
            }

            if (id == 5)
            {
                throw new NotImplementedException("Using id=5 has not been implemented");
            }

            if (response.IsSuccessStatusCode)
            {
                cancelToken.ThrowIfCancellationRequested();
                string stringResult = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                Person result = JsonSerializer.Deserialize<Person>(stringResult, options);
                return result;
            }
            return new Person();
        }

        public async Task<List<Person>> GetPeopleAsync(IProgress<int> progress,
            CancellationToken cancelToken = new CancellationToken())
        {
            List<int> ids = await GetIdsAsync().ConfigureAwait(false);
            var people = new List<Person>();

            for (int i = 0; i < ids.Count; i++)
            {
                cancelToken.ThrowIfCancellationRequested();

                int id = ids[i];
                var person = await Task.Run(() => GetPerson(id)).ConfigureAwait(false);

                int percentComplete = (int)((i + 1) / (float)ids.Count * 100);
                progress.Report(percentComplete);

                people.Add(person);
            }

            return people;
        }
    }
}