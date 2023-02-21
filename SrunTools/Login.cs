using System.Text.RegularExpressions;
using System.Text.Json;
using Storge;
using Interact;
using ExceptionHandler;

namespace SrunTools
{
    public class Login
    {
        public Login(IDataManager dataManager)
        {   
            (this.username, this.password) = dataManager.GetUser();
            if(string.IsNullOrEmpty(this.username) || string.IsNullOrEmpty(this.password))
                InteractiveManager.ExceptionPrinter(new AccountNullError());
        }
        private string getIPAPI = "srun_portal_pc?ac_id=1&theme=buaa";
        private string getChallengeApi = "cgi-bin/get_challenge";
        private string srunPortalApi = "cgi-bin/srun_portal";
        private string getInfoApi="cgi-bin/rad_user_info?callback=jQuery112403076366133070929_1630949443031&_=1630949443033";
        // private string user_agent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.26 Safari/537.36";

        private string? username;
        private string? password;
        private string? responseText;
        private string? ip;
        private string? token;
        private string? info;
        private string? hmd5;
        private string? chksum;
        private const string n = "200";
        private const string type = "1";
        private const string ac_id = "1";
        private const string enc = "srun_bx1";

        private Base64Encoder base64Encoder = new Base64Encoder();
        private MD5Encoder md5Encoder = new MD5Encoder();
        private SHA1Encoder sha1Encoder = new SHA1Encoder();
        private XencodeEncoder XEncoder = new XencodeEncoder();

        private static HttpClient client = new()
        {
            BaseAddress = new Uri("https://gw.buaa.edu.cn")
        };
        private void InitializeHTTPClient()
        {
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.26 Safari/537.36");
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
            client.DefaultRequestHeaders.Add("Connection", "keep-alive");
        }
        private async Task GetIP()
        {
            InteractiveManager.ColorfulProgress("1","5","获取IP地址");
            var response = await client.GetAsync(getIPAPI);
            responseText = await response.Content.ReadAsStringAsync();
            ip = Regex.Match(responseText, "id=\"user_ip\" value=\"(.*?)\"").Groups[1].Value;
            InteractiveManager.ColorfulProgress(ip);
        }

        private async Task GetToken()
        {
            InteractiveManager.ColorfulProgress("2","5","获取Token");
            var getChallengeParams = new string[]
            {
                "callback=jQuery112404953340710317169_" + (new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds()).ToString(),
                "username=" + username,
                "ip=" + ip,
                "_" + (new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds()).ToString()
            };
            var getChallengeResponse = await client.GetAsync(getChallengeApi + "?" + string.Join("&", getChallengeParams));
            var responseText = await getChallengeResponse.Content.ReadAsStringAsync();
            token = Regex.Match(responseText, "\"challenge\":\"(.*?)\"").Groups[1].Value;
            InteractiveManager.ColorfulProgress(token);
        }

        private string GetChksum()
        {
            var chkstr = token + username;
            chkstr += token + hmd5;
            chkstr += token + ac_id;
            chkstr += token + ip;
            chkstr += token + n;
            chkstr += token + type;
            chkstr += token + info;
            return chkstr;
        }

        private string GetInfo()
        {
            var info_temp = new
            {
                username = username,
                password = password,
                ip = ip,
                acid = ac_id,
                enc_ver = enc
            };

            var json = JsonSerializer.Serialize(info_temp);
            json = json.Replace("'", "\"").Replace(" ", "");

            return json;
        }
        private void Encode()
        {
            InteractiveManager.ColorfulProgress("3","5","进行XEncode加密");
            string msg = GetInfo();
            info = "{SRBX1}" + base64Encoder.GetBase64(XEncoder.GetXEncode(msg, token));
            hmd5 = md5Encoder.GetMD5(password, token);
            chksum = sha1Encoder.GetSHA1(GetChksum());
            InteractiveManager.ColorfulProgress("OK");
        }
        private async Task FinalRequest()
        {
            InteractiveManager.ColorfulProgress("4", "5", "提交验证信息");
            var srun_portal_params = new string[]
            {
                "callback=jQuery11240645308969735664_"+(new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds()).ToString(),
                "action=login",
                "username="+username,
                "password={MD5}"+Uri.EscapeDataString(hmd5),
                "ac_id="+ac_id,
                "ip="+ip,
                "chksum="+Uri.EscapeDataString(chksum),
                "info="+Uri.EscapeDataString(info),
                "n="+n,
                "type="+type,
                "os="+Uri.EscapeDataString("Windows 10"),
                "name=Windows",
                "double_stack=0",
                "_="+(new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds()).ToString()
            };
            var getPortalResponse = await client.GetAsync(srunPortalApi + "?" + string.Join("&", srun_portal_params));
            var responseText = await getPortalResponse.Content.ReadAsStringAsync();
            CheckSuccess(responseText);
            // lock (Console.Out)
            // {
            //     Console.WriteLine(responseText);
            //     Console.WriteLine();
            // }
            InteractiveManager.ColorfulProgress("OK");
        }

        private async Task GetInfoPage()
        {
            InteractiveManager.ColorfulProgress("5", "5", "获取登录信息");
            var getPortalResponse = await client.GetAsync(getInfoApi);
            var responseText = await getPortalResponse.Content.ReadAsStringAsync();
            if (CheckSuccess(responseText))
            {
                InteractiveManager.ColorfulProgress("OK");
                InteractiveManager.Success();
            }
        }

        private bool CheckSuccess(string response)
        {
            Regex resRegex = new Regex("\"res\":\"(\\w+)\"");
            Match resMatch = resRegex.Match(response);
            Regex errormsgRegex = new Regex("\"error_msg\":\"(.+?)\"");
            Match errormsgMatch = errormsgRegex.Match(response);
            if (resMatch.Success && errormsgMatch.Success)
            {
                string res = resMatch.Groups[1].Value;
                if (res != "ok"){
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error\u001b[0m");
                    InteractiveManager.ExceptionPrinter(new LoginFailError(errormsgMatch.Groups[1].Value));}
            }
            return true;
        }

        public async Task ExecuteLogin()
        {
            try
            {
                InitializeHTTPClient();
                await GetIP();
                await GetToken();
                Encode();
                await FinalRequest();
                await GetInfoPage();
            }
            catch (System.Exception e)
            {
                InteractiveManager.ExceptionPrinter(e);
            }
        }
    }
}