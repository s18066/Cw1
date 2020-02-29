using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cw1
{
    class Program
    {

        static async Task Main(string[] args)
        {
            Guard.HasArgumentOfIndex(args, 1);
            var isUrlValid = TryCreateValidUrl(args[0], out var url);

            if(!isUrlValid)
            {
                throw new ArgumentException();
            }

            var emailCrawler = new EmailCrawler(url);

            if(! await emailCrawler.SiteHasEmailsAsync())
            {
                throw new Exception("Site has no emails");
            }

            var emails = await emailCrawler.GetEmailsAsync();

            Console.WriteLine(emails.Distinct());
            
        }

        private static bool TryCreateValidUrl(string urlString, out Uri result) => 
            Uri.TryCreate(urlString, UriKind.Absolute, out result) 
            && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }

    public class EmailCrawler
    {
        private static readonly Regex emailRegex = new Regex(@"^(?("")("".+?(?<!\\)""@) | (([0 - 9a - z]((\.(? !\.)) |[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$");

        private readonly SiteContent _siteContent;

        public EmailCrawler(Uri checkedSite)
        {
            _siteContent = new SiteContent(checkedSite);
        }

        public async Task<IEnumerable<string>> GetEmailsAsync() => GetAllEmails(await _siteContent.GetSiteContentAsync());

        public async Task<bool> SiteHasEmailsAsync()
        {
            var siteContent = await _siteContent.GetSiteContentAsync();
            return GetAllEmails(siteContent).Any();
        }

        private static IEnumerable<string> GetAllEmails(string stringToSearch) => emailRegex.Matches(stringToSearch).Select(match => match.Value);
    }

    public class SiteContent
    {
        private readonly Uri _uri;
        private HttpResponseMessage _responseMessage;

        public SiteContent(Uri siteUri)
        {
            _uri = siteUri;
        }

        public async Task<string> GetSiteContentAsync()
        {
            var resonse = await GetResponseMessageAsync();
            return await resonse.Content.ReadAsStringAsync();
        }

        public async Task<HttpResponseMessage> GetResponseMessageAsync()
        {
            if (_responseMessage != null)
            {
                return _responseMessage;
            }

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(_uri);

                if(!response.IsSuccessStatusCode)
                {
                    throw new Exception("Blad pobierania strony");
                }

                _responseMessage = response;
            }

            return _responseMessage;
        }
    }

    static class Guard
    {
        public static void HasArgumentOfIndex<T>(IEnumerable<T> collectionToCheck, int indexOfArgument)
        {
            if (collectionToCheck.Count() < indexOfArgument - 1)
            {
                throw new ArgumentNullException();
            }
        }
    }
}
