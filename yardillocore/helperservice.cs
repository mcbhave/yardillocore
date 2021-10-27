using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using yardillocore.Models;
using yardillocore.Services;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Linq;
using Newtonsoft.Json;
using System.Security.Cryptography;
using MongoDB.Bson.Path;

namespace yardillocore
{

    public static class helperservice
    {
        public static IMongoDatabase Gettenant(string tenantid, MongoClient Client, IMongoDatabase MBADDatabase, IDatabaseSettings settings)
        {
            IMongoCollection<Tenant> _tenant = MBADDatabase.GetCollection<Tenant>("Tenants");
            try
            {
                Tenant oten = _tenant.Find<Tenant>(book => book.Tenantname.ToUpper() == tenantid.ToUpper()).FirstOrDefault();
                if (oten == null)
                {
                    oten = new Tenant();
                    oten._owner = tenantid;
                    oten.Tenantname = tenantid;
                    oten.Tenantdesc = "";
                    oten.Createdate = DateTime.UtcNow.ToString();
                    //register new tenant
                    _tenant.InsertOne(oten);

                }
                if (oten.Dbconnection != null && oten.Dbconnection != "")
                {
                    if (oten.Dbconnection.Contains("@VAULT|"))
                    {
                        try
                        {

                            string pattern = @"@VAULT|w*@";
                            // Create a Regex  
                            Regex rg = new Regex(pattern);

                            // Get all matches  
                            MatchCollection matchedAuthors = rg.Matches(oten.Dbconnection);
                            // Print all matched authors  
                            for (int count = 0; count < matchedAuthors.Count; count++)
                            {
                                oten.Dbconnection = oten.Dbconnection.Replace(matchedAuthors[count].Value, GetVaultSecret(matchedAuthors[count].Value, tenantid, Client, MBADDatabase, settings));
                            }

                            Client = new MongoClient(oten.Dbconnection);

                        }
                        catch (Exception ex)
                        {
                            SetMBADMessage(settings, MBADDatabase, ICallerType.TENANT, oten._id, tenantid, "TENANT", "Failed to get connection string", "Tenant login", tenantid, ex);
                        };

                    }
                    else
                    {
                        try
                        {
                            Client = new MongoClient(oten.Dbconnection);
                        }
                        catch (Exception ex)
                        {
                            SetMBADMessage(settings, MBADDatabase, ICallerType.TENANT, oten._id, tenantid, "TENANT", "Failed to connect DB", "Tenant login", tenantid, ex);
                        };
                    }

                }
                IMongoDatabase TenantDatabase = Client.GetDatabase(oten.Tenantname); ;

                SetMBADMessage(settings, MBADDatabase, ICallerType.TENANT, oten._id, tenantid, "TENANT", "Success", "Tenant login", tenantid, null);

                return TenantDatabase;
            }
            catch { throw; };
        }
        //public static void LogWixMessages(string method, string mess)
        //{
        //    MongoClient _client;
        //    IMongoDatabase MBADDatabase;
        //    IMongoCollection<Message> _messagemaster;
        //    Message omess = new Message();
        //    _client = new MongoClient("mongodb://yardilloadmin:1pkGpqdqHV42AvOD@cluster0-shard-00-00.tj6lt.mongodb.net:27017,cluster0-shard-00-01.tj6lt.mongodb.net:27017,cluster0-shard-00-02.tj6lt.mongodb.net:27017/yardillo_dev?ssl=true&replicaSet=atlas-d5jcxa-shard-0&authSource=admin&retryWrites=true&w=majority");
        //    MBADDatabase = _client.GetDatabase("YARDILLO");
        //    _messagemaster = MBADDatabase.GetCollection<Message>("WIXlogins");

        //    omess.Callertype = method;
        //    omess.Messagecode = "wix";

        //    omess.Callerrequest = mess;
        //    _messagemaster.InsertOneAsync(omess);
        //}
        private static string GetVaultSecret(string stringtoreplace, string tenantid, MongoClient Client, IMongoDatabase MBADDatabase, IDatabaseSettings settings)
        {
            try
            {
                VaultResponse ovr = null;
                IMongoCollection<Vault> _Vaultcollection = MBADDatabase.GetCollection<Vault>(settings.Vaultcollection);
                Vault ov = _Vaultcollection.Find<Vault>(book => book.Macroname.ToLower() == stringtoreplace.ToLower() && book.Tenantid == tenantid).FirstOrDefault();
                if (ov != null)
                {
                    ovr = new VaultResponse();
                    ovr._id = ov._id;
                    ovr.Name = ov.Name;
                    ovr.Macroname = ov.Macroname;
                    helperservice.VaultCrypt ovrcr = new helperservice.VaultCrypt(helperservice.Gheparavli(_Vaultcollection));
                    ovr.Encryptwithkey = ovrcr.Decrypt(ov.Encryptwithkey);
                    ovr.Safekeeptext = ovrcr.Decrypt(ov.Safekeeptext);
                    return ovr.Safekeeptext;
                }
                else
                {
                    return stringtoreplace;
                }
            }
            catch
            {
                throw;
            }
        }
        public static string Gheparavli(IMongoCollection<Vault> _Vaultcollection)
        {
            var ovault = new helperservice.VaultCrypt("M610ffa52610B35af2b32e13d5D");
            Vault ombad = _Vaultcollection.Find(c => c.Name == "MBADKEY").FirstOrDefault();
            if (ombad == null)
            {
                ombad = new Vault() { Name = "MBADKEY", Encryptwithkey = ovault.Encrypt(Guid.NewGuid().ToString()) };
                //SAVE
                _Vaultcollection.InsertOne(ombad);
            }
            ombad = _Vaultcollection.Find(c => c.Name == "MBADKEY").FirstOrDefault();
            if (ombad == null)
            { throw new Exception("something is totally wrong with secured keys, call administrator"); }
            var encrptkey = ovault.Decrypt(ombad.Encryptwithkey);

            return encrptkey;
        }

        public static Message SetMBADMessage(IDatabaseSettings settings, IMongoDatabase MBADDatabase, string callrtype, string caseid, string srequest, string srequesttype, string sMessageCode, string sMessagedesc, string userid, Exception ex)
        {

            var _MessageType = string.Empty;
            var _MessageCode = string.Empty;
            var _MessageDesc = string.Empty;
            if (ex != null)
            {
                _MessageType = "ERROR";
                _MessageCode = ex.Message;
                _MessageDesc = ex.ToString();
            }
            else
            {
                _MessageType = "INFO";
                _MessageCode = sMessageCode;
                _MessageDesc = sMessagedesc;
            }
            Message oms = new Message
            {
                Tenantid = userid,
                Callerid = caseid,
                Callertype = callrtype,
                Messagecode = _MessageCode,
                Messageype = _MessageType,
                MessageDesc = _MessageDesc,
                Callerrequest = srequest,
                Callerrequesttype = srequesttype,
                Userid = userid,
                Messagedate = DateTime.UtcNow.ToString()
            };

            MessageService omesssrv = new MessageService(settings, MBADDatabase, MBADDatabase);
            oms = omesssrv.Create(oms);

            return oms;

        }

        public static List<SetCasetypefield> ExecuteAdapter(Adapter oa, Adapterresponsemap iact, StringBuilder slog)
        {

            try
            {

                //Adapter oa = new Adapter();
                if (oa.Url == null || oa.Url == "") { slog.Append("url not set "); return null; }
                if (oa.Method == null || oa.Method == "") { slog.Append("method not set"); return null; }
                HttpClient _client = new HttpClient();
                foreach (string s in oa.Headers)
                {
                    var shead = s.Split(":");
                    _client.DefaultRequestHeaders.Add(shead[0], shead[1]);
                    //_client.DefaultRequestHeaders.Accept.Add(
                    //new MediaTypeWithQualityHeaderValue("application/json"));
                }

                string sReturnresp = string.Empty;
                slog.Append("Method:" + oa.Method);


                sReturnresp = GetRESTResponse(oa.Method.ToUpper(), oa.Url, oa.Body, _client, slog);

                if (sReturnresp == "") { slog.Append("No response found"); return null; };

                slog.Append("Setting Fields:");
                BsonDocument omd = MongoDB.Bson.BsonDocument.Parse(sReturnresp);
                List<SetCasetypefield> ocasedb = new List<SetCasetypefield>();
                foreach (SetCasetypefield f in iact.Fields)
                {
                    slog.Append("Field:" + f.Fieldid);
                    //"$.activities[?(@.activityid == 'VALIDATE')]"
                    try
                    {
                        var svalue = omd.SelectToken(f.Value, true).AsString;
                        slog.Append(".Value=" + svalue);
                        SetCasetypefield ocasesetfld;
                        //add field
                        ocasesetfld = new SetCasetypefield();
                        ocasesetfld.Fieldid = f.Fieldid;
                        ocasesetfld.Value = svalue;
                        ocasedb.Add(ocasesetfld);
                    }
                    catch (Exception e)
                    {
                        slog.Append("Exception:" + e.ToString());
                    }
                }
                return ocasedb;
            }
            catch (Exception ex)
            {
                //log
                throw ex;
            }
        }
        //public static BsonValue GetPath(this BsonValue bson, string path)
        //{
        //    if (bson.BsonType != BsonType.Document)
        //    {
        //        return bson;
        //    }
        //    if (bson.BsonType != BsonType.Array)
        //    {
        //        return bson;
        //    }

        //    var doc = bson.AsBsonDocument;

        //    //var tokens = path.Split(".".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //    var tokens = path.Split(new[] { '.' }, 2, StringSplitOptions.RemoveEmptyEntries);
        //    if (tokens.Length == 0)
        //    {
        //        return doc;
        //    }

        //    if (!doc.Contains(tokens[0]))
        //    {
        //        return BsonNull.Value;
        //    }

        //    if (tokens.Length > 1)
        //    {
        //        return GetPath(doc[tokens[0]], tokens[1]);
        //    }

        //    return doc[tokens[0]];
        //}
        //private static string CallGET(string uri, HttpClient client)
        //{
        //    var webRequest = new HttpRequestMessage(HttpMethod.Get, uri)
        //    {
        //        Content = new StringContent("", Encoding.UTF8, "")  

        //    };
        //    using (HttpResponseMessage response = client.SendAsync(webRequest).GetAwaiter().GetResult())
        //    {
        //        response.EnsureSuccessStatusCode();
        //        string responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        //        //return JsonConvert.DeserializeObject<T>(responseBody);
        //        return responseBody;
        //    }



        //}
        public static string GetRESTResponse(string smethod, string uri, string content, HttpClient client, StringBuilder slog)
        {
            try
            {
                using (client)
                {
                    StringContent serialized;
                    switch (smethod.ToUpper())
                    {
                        case "GET":
                            using (HttpResponseMessage response = client.GetAsync(uri).GetAwaiter().GetResult())
                            {
                                response.EnsureSuccessStatusCode();
                                slog.Append("Response: ");
                                string responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                                slog.Append(responseBody);
                                return responseBody;
                            }

                        case "POST":
                            serialized = new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json");

                            using (HttpResponseMessage response = client.PostAsync(uri, serialized).GetAwaiter().GetResult())
                            {
                                response.EnsureSuccessStatusCode();
                                slog.Append("Response: ");
                                string responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                                slog.Append(responseBody);
                                return responseBody;
                            }
                        case "PUT":
                            serialized = new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json");

                            using (HttpResponseMessage response = client.PostAsync(uri, serialized).GetAwaiter().GetResult())
                            {
                                response.EnsureSuccessStatusCode();
                                slog.Append("Response: ");
                                string responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                                slog.Append(responseBody);
                                return responseBody;
                            }
                        case "DELETE":
                            using (HttpResponseMessage response = client.DeleteAsync(uri).GetAwaiter().GetResult())
                            {
                                response.EnsureSuccessStatusCode();
                                slog.Append("Response: ");
                                string responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                                slog.Append(responseBody);
                                return responseBody;
                            }
                        case "PATCH":
                            serialized = new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json");

                            using (HttpResponseMessage response = client.PostAsync(uri, serialized).GetAwaiter().GetResult())
                            {
                                response.EnsureSuccessStatusCode();
                                slog.Append("REST call successful");
                                string responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                                slog.Append(responseBody);
                                return responseBody;
                            }
                    }
                    return "";
                }


            }
            catch (Exception e)
            {
                slog.Append("REST Exception : " + e.ToString());
                return "";
            }
        }
        public static string PostRequest(string uri, string content, HttpClient client)
        {
            try
            {
                using (client)
                {
                    client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                    var serialized = new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json");

                    using (HttpResponseMessage response = client.PostAsync(uri, serialized).GetAwaiter().GetResult())
                    {
                        response.EnsureSuccessStatusCode();
                        string responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                        return responseBody;
                    }
                }
            }
            catch
            {
                throw;
            }
        }
        public static string PutRequest(string uri, string content, HttpClient client)
        {
            try
            {
                using (client)
                {
                    client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                    var serialized = new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json");

                    using (HttpResponseMessage response = client.PutAsync(uri, serialized).GetAwaiter().GetResult())
                    {
                        response.EnsureSuccessStatusCode();
                        string responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                        return responseBody;
                    }
                }
            }
            catch
            {
                throw;
            }
        }
        public static string DeleteRequest(string uri, string content, HttpClient client)
        {
            try
            {
                using (client)
                {
                    client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                    var serialized = new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json");

                    using (HttpResponseMessage response = client.DeleteAsync(uri).GetAwaiter().GetResult())
                    {
                        response.EnsureSuccessStatusCode();
                        string responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                        return responseBody;
                    }
                }
            }
            catch
            {
                throw;
            }
        }


        //public static async Task<string> GetRequest<T>(string uri, HttpClient client)
        //{
        //    try
        //    {
        //        using (client)
        //        {
        //            using (HttpResponseMessage response = await client.GetAsync(uri))
        //            {
        //                response.EnsureSuccessStatusCode();
        //                string responseBody = await response.Content.ReadAsStringAsync();
        //                //return JsonConvert.DeserializeObject<T>(responseBody);
        //                return responseBody;
        //            }
        //        }
        //    }
        //    catch
        //    {
        //        throw;
        //    }
        //}
        //public static async Task<string> PostRequest<T>(string uri, string content, HttpClient client)
        //{
        //    try
        //    {
        //        using (client)
        //        {
        //            client.DefaultRequestHeaders.Accept.Add(
        //            new MediaTypeWithQualityHeaderValue("application/json"));

        //            var serialized = new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json");

        //            using (HttpResponseMessage response = await client.PostAsync(uri, serialized))
        //            {
        //                response.EnsureSuccessStatusCode();
        //                string responseBody = await response.Content.ReadAsStringAsync();

        //                return  responseBody ;
        //            }
        //        }
        //    }
        //    catch  
        //    {
        //        throw;
        //    }
        //}
        public static string GetFieldValueByFieldID(Case fcase, string Fieldid)
        {
            if (fcase != null)
            {
                Casefield ocf;
                if (fcase.Fields != null)
                {
                    if ((ocf = fcase.Fields.Where(f => f.Fieldid.ToLower() == Fieldid.ToLower()).FirstOrDefault()) != null)
                    {
                        return ocf.Value;
                    }
                }
            }
            return "";
        }
        public static bool GetCompareResults(Case ocase, Models.Action iAct, IMongoCollection<ActionAuthLogs> Logcollection)
        {
            if (iAct == null) { return true; }
            if (iAct.Actionauth == null) { return WriteCompareLog(Logcollection, iAct, "Action Auth configuration is null", true); }

            if (iAct.Actionauth.Fieldid != null || iAct.Actionauth.Fieldid == "")
            {
                iAct.Actionauth.ValueX = helperservice.GetFieldValueByFieldID(ocase, iAct.Actionauth.Fieldid);
            }

            var sactionconfig = Newtonsoft.Json.JsonConvert.SerializeObject(iAct.Actionauth);
            var defaultret = iAct.Actionauth.Defaultreturn;
            var bretiftrue = iAct.Actionauth.Returniftrue;
            var bretiffalse = iAct.Actionauth.Returniffalse;
            var FieldValue = iAct.Actionauth.ValueX;
            if (iAct.Actionauth == null) { return true; }
            if (iAct.Actionauth.Operator == null || iAct.Actionauth.Operator == "") { iAct.Actionauth.Operator = "="; }
            if (iAct.Actionauth.Type == null) { return true; }
            switch (iAct.Actionauth.Type.ToUpper())
            {
                case "STRING":
                    switch (iAct.Actionauth.Operator.ToUpper())
                    {
                        case "=":
                            if (FieldValue.ToLower() == iAct.Actionauth.ValueY.ToLower())
                            { return WriteCompareLog(Logcollection, iAct, sactionconfig, bretiftrue); }
                            else { return WriteCompareLog(Logcollection, iAct, sactionconfig, bretiffalse); }
                        case "CONTAINS":
                            if (FieldValue.ToLower().Contains(iAct.Actionauth.ValueY.ToLower()))
                            { return bretiftrue; }
                            else { return WriteCompareLog(Logcollection, iAct, sactionconfig, bretiffalse); }
                        case "STARTSWITH":
                            if (FieldValue.ToLower().StartsWith(iAct.Actionauth.ValueY.ToLower()))
                            { return WriteCompareLog(Logcollection, iAct, sactionconfig, bretiftrue); }
                            else { return WriteCompareLog(Logcollection, iAct, sactionconfig, bretiffalse); }
                        case "ENDSWITH":
                            if (FieldValue.ToLower().EndsWith(iAct.Actionauth.ValueY.ToLower()))
                            { return WriteCompareLog(Logcollection, iAct, sactionconfig, bretiftrue); }
                            else { return WriteCompareLog(Logcollection, iAct, sactionconfig, bretiffalse); }
                        default:
                            return defaultret;
                    }
                default:
                    return defaultret;
            }
        }
        public static string RandomString(int size, bool lowerCase = false)
        {
            var builder = new StringBuilder(size);

            // Unicode/ASCII Letters are divided into two blocks
            // (Letters 65–90 / 97–122):
            // The first group containing the uppercase letters and
            // the second group containing the lowercase.  

            // char is a single Unicode character  
            char offset = lowerCase ? 'a' : 'A';
            const int lettersOffset = 26; // A...Z or a..z: length=26  

            for (var i = 0; i < size; i++)
            {
                var @char = (char)_random.Next(offset, offset + lettersOffset);
                builder.Append(@char);
            }

            return lowerCase ? builder.ToString().ToLower() : builder.ToString();
        }
        private static Random _random = new Random();

        // Generates a random number within a range.      
        public static int RandomNumber(int min, int max)
        {
            return _random.Next(min, max);
        }
        public static bool WriteCompareLog(IMongoCollection<ActionAuthLogs> _logs, Models.Action iAct, string Logdesc, bool Returnbool)
        {


            ActionAuthLogs olog = new ActionAuthLogs() { Activityid = iAct.Activityid, Caseid = iAct.Caseid, Logdesc = Logdesc, Actionid = iAct.Actionid, Actionauthresult = Returnbool, Actionseq = iAct.Actionseq, Activityseq = iAct.Activityseq };


            _logs.InsertOneAsync(olog);


            return Returnbool;
        }
        public class VaultCrypt
        {
            private string mysecurityKey = "M610ffa52610B35af2b32e13d5D";
            public VaultCrypt(string sp)
            {
                mysecurityKey = sp;
            }

            public string Encrypt(string TextToEncrypt)
            {
                byte[] MyEncryptedArray = UTF8Encoding.UTF8
                   .GetBytes(TextToEncrypt);

                MD5CryptoServiceProvider MyMD5CryptoService = new
                   MD5CryptoServiceProvider();

                byte[] MysecurityKeyArray = MyMD5CryptoService.ComputeHash
                   (UTF8Encoding.UTF8.GetBytes(mysecurityKey));

                MyMD5CryptoService.Clear();

                var MyTripleDESCryptoService = new
                   TripleDESCryptoServiceProvider();

                MyTripleDESCryptoService.Key = MysecurityKeyArray;

                MyTripleDESCryptoService.Mode = CipherMode.ECB;

                MyTripleDESCryptoService.Padding = PaddingMode.PKCS7;

                var MyCrytpoTransform = MyTripleDESCryptoService
                   .CreateEncryptor();

                byte[] MyresultArray = MyCrytpoTransform
                   .TransformFinalBlock(MyEncryptedArray, 0,
                   MyEncryptedArray.Length);

                MyTripleDESCryptoService.Clear();

                return Convert.ToBase64String(MyresultArray, 0,
                   MyresultArray.Length);
            }

            public string Decrypt(string TextToDecrypt)
            {
                byte[] MyDecryptArray = Convert.FromBase64String
                   (TextToDecrypt);

                MD5CryptoServiceProvider MyMD5CryptoService = new
                   MD5CryptoServiceProvider();

                byte[] MysecurityKeyArray = MyMD5CryptoService.ComputeHash
                   (UTF8Encoding.UTF8.GetBytes(mysecurityKey));

                MyMD5CryptoService.Clear();

                var MyTripleDESCryptoService = new
                   TripleDESCryptoServiceProvider();

                MyTripleDESCryptoService.Key = MysecurityKeyArray;

                MyTripleDESCryptoService.Mode = CipherMode.ECB;

                MyTripleDESCryptoService.Padding = PaddingMode.PKCS7;

                var MyCrytpoTransform = MyTripleDESCryptoService
                   .CreateDecryptor();

                byte[] MyresultArray = MyCrytpoTransform
                   .TransformFinalBlock(MyDecryptArray, 0,
                   MyDecryptArray.Length);

                MyTripleDESCryptoService.Clear();

                return UTF8Encoding.UTF8.GetString(MyresultArray);
            }
        }
    }
    public class MyActivityOrder : IComparer<Activity>
    {
        public int Compare(Activity x, Activity y)
        {
            int compareDate = x.Activityseq.CompareTo(y.Activityseq);
            if (compareDate == 0)
            {
                return x.Activityseq.CompareTo(y.Activityseq);
            }
            return compareDate;
        }


    }
    public class MyCaseFieldOrder : IComparer<Casefield>
    {
        public int Compare(Casefield x, Casefield y)
        {
            int ci = x.Seq.CompareTo(y.Seq);

            return ci;
        }


    }
    public class MyCaseTypeFieldOrder : IComparer<Casetypefield>
    {
        public int Compare(Casetypefield x, Casetypefield y)
        {
            int ci = x.Seq.CompareTo(y.Seq);

            return ci;
        }


    }
    public class MyActionOrder : IComparer<Models.Action>
    {
        public int Compare(Models.Action x, Models.Action y)
        {
            int compareDate = x.Actionseq.CompareTo(y.Actionseq);
            if (compareDate == 0)
            {
                return x.Actionseq.CompareTo(y.Actionseq);
            }
            return compareDate;
        }


    }

    public sealed class JsonNetValueSystem
    {


        public bool HasMember(object value, string member)
        {
            if (value is Newtonsoft.Json.Linq.JObject)
            {
                // return (value as JObject).Properties().Any(property => property.Name == member);

                foreach (Newtonsoft.Json.Linq.JProperty property in (value as Newtonsoft.Json.Linq.JObject).Properties())
                {
                    if (property.Name == member)
                        return true;
                }

                return false;
            }

            if (value is Newtonsoft.Json.Linq.JArray)
            {
                int index = ParseInt(member, -1);
                return index >= 0 && index < (value as Newtonsoft.Json.Linq.JArray).Count;
            }
            return false;
        }


        public object GetMemberValue(object value, string member)
        {
            if (value is Newtonsoft.Json.Linq.JObject)
            {
                var memberValue = (value as Newtonsoft.Json.Linq.JObject)[member];
                return memberValue;
            }
            if (value is Newtonsoft.Json.Linq.JArray)
            {
                int index = ParseInt(member, -1);
                return (value as Newtonsoft.Json.Linq.JArray)[index];
            }
            return null;
        }


        public System.Collections.IEnumerable GetMembers(object value)
        {
            System.Collections.Generic.List<string> ls = new System.Collections.Generic.List<string>();

            var jobject = value as Newtonsoft.Json.Linq.JObject;
            /// return jobject.Properties().Select(property => property.Name);

            foreach (Newtonsoft.Json.Linq.JProperty property in jobject.Properties())
            {
                ls.Add(property.Name);
            }

            return ls;
        }


        public bool IsObject(object value)
        {
            return value is Newtonsoft.Json.Linq.JObject;
        }


        public bool IsArray(object value)
        {
            return value is Newtonsoft.Json.Linq.JArray;
        }


        public bool IsPrimitive(object value)
        {
            if (value == null)
                throw new System.ArgumentNullException("value");

            return value is Newtonsoft.Json.Linq.JObject || value is Newtonsoft.Json.Linq.JArray ? false : true;
        }


        private int ParseInt(string s, int defaultValue)
        {
            int result;
            return int.TryParse(s, out result) ? result : defaultValue;
        }


    }
}
