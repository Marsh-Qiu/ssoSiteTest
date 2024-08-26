using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text;
using System.Web.UI;

namespace ssoSiteTest
{
    public class SsoLoginParam
    {
        [Description("使用者學號/帳號")] public string UserId { get; set; }

        [Description("使用色色代碼('學生或是職員等...的代碼')")]
        public string Role { get; set; }

        [Description("SSO 系統產生 Token 後，讓原民系統驗證的有效期限(unixtimestap)")]
        public long ExpireIn { get; set; }

        [Description("經過 sha256 加密後的 token")] public string Token { get; set; }
    }

    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e) { }

        protected void OnServerClick(object sender, EventArgs e)
        {
            var userId = "113314011"; //
            var role = "S"; //

            const string baseUrl = "https://isrcsys.ntunhs.edu.tw";
            const string path = "SsoCallback.aspx";
            const string salt = "9rju8duu@Velecity"; //加鹽(鄭老師可自訂一個參數值，並且告知我們，或可用電話討論)
            var token = GetActiveSessionToken(baseUrl, path, salt, userId, role);

            Response.Redirect($"{baseUrl}/ActiveSessionLogin.aspx?token={token}");
        }

        /// <summary>
        /// 獲取handshake後的 token
        /// </summary>
        /// <param name="baseUrl">三金系統的網址</param>
        /// <param name="path">三金系統handshake的路徑</param>
        /// <param name="salt">加鹽值</param>
        /// <param name="userId">登入user的id</param>
        /// <param name="role">登入user的角色代碼</param>
        /// <returns>handshake後的 token</returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="Exception"></exception>
        private static string GetActiveSessionToken(string baseUrl, string path, string salt, string userId, string role)
        {
            var ssoUrl = $"{baseUrl}/{path}";
            var request = (HttpWebRequest)WebRequest.Create(ssoUrl);

            var ssoLoginParam = GenerateLoginParam(salt, userId, role);

            request.Headers.Add("UserId", ssoLoginParam.UserId);
            request.Headers.Add("Role", ssoLoginParam.Role);
            request.Headers.Add("ExpireIn", ssoLoginParam.ExpireIn.ToString());
            request.Headers.Add("Token", ssoLoginParam.Token);
            request.Method = "GET";
            request.Accept = "application/json";

            using (var httpWebResponse = (HttpWebResponse)request.GetResponse())
            {
                using (var reader = new StreamReader(httpWebResponse.GetResponseStream() ?? throw new InvalidOperationException()))
                {
                    var jObject = JObject.Parse(reader.ReadToEnd());

                    var activeSessionTokenResult = new
                    {
                        isSucess = (jObject["isSuccess"] ?? throw new InvalidOperationException()).Value<bool>(),
                        data = jObject["Data"]?.Value<string>(),
                        message = jObject["Message"]?.Value<string>()
                    };

                    if (activeSessionTokenResult.isSucess)
                        return activeSessionTokenResult.data;

                    var errorMessage = jObject["Message"]?.Value<string>();
                    throw new Exception(errorMessage);
                }
            }
        }

        /// <summary>
        /// 產生 loginParam，附帶在 Header 中
        /// </summary>
        /// <param name="salt"></param>
        /// <param name="userId"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        private static SsoLoginParam GenerateLoginParam(string salt, string userId, string role)
        {
            const int timeLimit = 3; //預設是三分鐘內要驗證完畢，鄭老師可依據校內考量修改並告知我們

            var expireInUnixTime = (long)DateTime.Now.AddMinutes(timeLimit).Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

            var loginParam = new SsoLoginParam
            {
                UserId = userId,
                Role = role, //範例:學生角色代碼，可依據校內規則修改並告知我們
                ExpireIn = expireInUnixTime, //後續會傳入sha256一起加密，若竄改也無效
                Token = ComputeSha256(salt, userId, expireInUnixTime)
            };

            return loginParam;
        }

        /// <summary>
        /// Sha256 加密
        /// </summary>
        /// <param name="salt"></param>
        /// <param name="userId"></param>
        /// <param name="unixTimeStamp"></param>
        /// <returns></returns>
        private static string ComputeSha256(string salt, string userId, long unixTimeStamp)
        {
            var slatedStr = salt + userId + unixTimeStamp;

            var sha256 = System.Security.Cryptography.SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(slatedStr));
            return BitConverter.ToString(hash).Replace("-", string.Empty);
        }
    }
}
