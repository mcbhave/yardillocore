using System;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using yardillocore.Models;
using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace yardillocore
{
    public static class Authorization
    {
        private static readonly string _realm = string.Empty;
        public static void OnBasicAPIAuth(AuthorizationFilterContext context, IConfigurationBuilder builder)
        {
            bool allpass = true;
            IConfigurationRoot configuration = builder.Build();
            //string dbconn = configuration.GetSection("DatabaseSettings").GetSection("ConnectionString").Value;
            Message omess = new Message();
            string sSecretKey = configuration.GetSection("AppConfig").GetSection("SecretKey").Value;
            string sSecretKeyValue = configuration.GetSection("AppConfig").GetSection("SecretKeyValue").Value;

            if (sSecretKey == null || sSecretKey == "") { allpass = false; omess.Messageype = "Unauthorized"; omess.Messagecode = "Implementation is missing secret keys."; ReturnUnauthorizedResult(context, omess); return ; }

            try
            {

                string srapidsecretkey = context.HttpContext.Request.Headers["X-RapidAPI-Proxy-Secret"];
                string rapiduserid = context.HttpContext.Request.Headers["X-RapidAPI-User"];
                var ssubs = context.HttpContext.Request.Headers["X-RapidAPI-Subscription"];
                string sauthsrc = context.HttpContext.Request.Headers["Y-Auth-Src"];
                string srapidapikey = context.HttpContext.Request.Headers["x-rapidapi-key"];
                string syauthuseridopt = context.HttpContext.Request.Headers["Y-Auth-userid"];

                omess.Callertype = "Headers";
                omess.Messagecode = "yardillo";
                string sheaders = Newtonsoft.Json.JsonConvert.SerializeObject(context.HttpContext.Request.Headers);
                omess.Headerrequest = sheaders;
                omess.Userid = rapiduserid;
                omess.YAuthSource = sauthsrc;

                string sActualKeyvalue=context.HttpContext.Request.Headers[sSecretKey];
                if (sActualKeyvalue != sSecretKeyValue)
                {
                    allpass = false; omess.Messageype = "Unauthorized"; omess.Messagecode = "00001";
                }
                //if (srapidsecretkey != "1f863a60-f3b6-11eb-bc3e-c3f329db9ee7" && srapidsecretkey != "6acc1280-fde1-11eb-b480-3f057f12dc26" && srapidsecretkey != "ade9f2f0-fe3e-11eb-8e8b-29cf15887162" && srapidsecretkey != "d602ee50-2f9d-11ec-9121-f55b1f38643f")
                //{ allpass = false; omess.Messageype = "Unauthorized"; omess.Messagecode = "00001"; }


                if (allpass) { return  ; }

                if (allpass)
                {

                    string authHeader = context.HttpContext.Request.Headers["Authorization"];
                    if (authHeader != null)
                    {
                        var authHeaderValue = AuthenticationHeaderValue.Parse(authHeader);
                        if (authHeaderValue.Scheme.Equals(AuthenticationSchemes.Basic.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            var credentials = Encoding.UTF8
                                                .GetString(Convert.FromBase64String(authHeaderValue.Parameter ?? string.Empty))
                                                .Split(':', 2);
                            if (credentials.Length == 2)
                            {
                                //if (IsAuthorized(context, credentials[0], credentials[1]))
                                //{
                                //    return;
                                context.HttpContext.Session.SetString("mbadtanent", credentials[0]);

                                return  ;
                                //}
                            }
                        }
                    }
                }


                 ReturnUnauthorizedResult(context,omess );
                return ;
            }
            catch (FormatException e)
            {
                omess.MessageDesc = "Unabel to validate user" + e.ToString();
                // _messagemaster.InsertOneAsync(omess);
                return ;
            }
            finally
            {

            }
         
        }

        public static void OnRapidHubAuth(AuthorizationFilterContext context, IConfigurationBuilder builder)
        {
            bool allpass = true;
            Message omess = new Message();
            IConfigurationRoot configuration = builder.Build();
            string dbconn = configuration.GetSection("DatabaseSettings").GetSection("ConnectionString").Value;
            if(dbconn==null || dbconn == "") { allpass = false; omess.Messageype = "Unauthorized"; omess.Messagecode = "00000"; omess.Callertype = "DB"; }

            string sDatabaseName = configuration.GetSection("DatabaseSettings").GetSection("DatabaseName").Value;
            if (sDatabaseName == null || sDatabaseName == "") { allpass = false; omess.Messageype = "Unauthorized"; omess.Messagecode = "00001"; omess.Callertype = "DB"; }

            string sSecretKey = configuration.GetSection("AppConfig").GetSection("SecretKey").Value;
            string sSecretKeyValue = configuration.GetSection("AppConfig").GetSection("SecretKeyValue").Value;

            string sActualKeyvalue = context.HttpContext.Request.Headers[sSecretKey];
            if (sActualKeyvalue != sSecretKeyValue)
            {
                allpass = false; omess.Messageype = "Unauthorized"; omess.Messagecode = "00002"; omess.Callertype = "Headers";
            }


          
            string sYauthSourceKey = configuration.GetSection("AppConfig").GetSection("YAuthSourceKey").Value;
            string ssYauthSourceValue = configuration.GetSection("AppConfig").GetSection("YAuthSourceValue").Value;
            string sYauthSource = context.HttpContext.Request.Headers[sYauthSourceKey];
            if (sYauthSource != ssYauthSourceValue)
            {
                allpass = false; omess.Messageype = "Unauthorized"; omess.Messagecode = "00003"; omess.Callertype = "Headers"; 
            }


            MongoClient _client;
            IMongoDatabase MBADDatabase = null;
            IMongoCollection<Message> _messagemaster = null;


            string xrapidhost = "";
            //_client = new MongoClient("mongodb://yardilloadmin:1pkGpqdqHV42AvOD@cluster0-shard-00-00.tj6lt.mongodb.net:27017,cluster0-shard-00-01.tj6lt.mongodb.net:27017,cluster0-shard-00-02.tj6lt.mongodb.net:27017/yardillo_dev?ssl=true&replicaSet=atlas-d5jcxa-shard-0&authSource=admin&retryWrites=true&w=majority");
            try
            {
                _client = new MongoClient(dbconn);

                MBADDatabase = _client.GetDatabase(sDatabaseName);
                _messagemaster = MBADDatabase.GetCollection<Message>("Logins");
            }
            catch (Exception e)
            {
                allpass = false; omess.Messageype = e.Message; omess.Messagecode = "00005"; omess.Callertype = "DB";
            }
          
            try
            {


                //string srapidsecretkey = context.HttpContext.Request.Headers["X-RapidAPI-Proxy-Secret"];
                string rapiduserid = context.HttpContext.Request.Headers["X-RapidAPI-User"];
                var ssubs = context.HttpContext.Request.Headers["X-RapidAPI-Subscription"];
                //string sauthsrc = context.HttpContext.Request.Headers["Y-Auth-Src"];
                string srapidapikey = context.HttpContext.Request.Headers["x-rapidapi-key"];
                string syauthuseridopt = context.HttpContext.Request.Headers["Y-Auth-userid"];

                if (syauthuseridopt != null)
                {
                    //this is a optional user id, currently used for speech to text
                    helperservice.VaultCrypt ovrcr = new helperservice.VaultCrypt("Y-Auth-userid");
                    context.HttpContext.Session.SetString("yathuid", ovrcr.Encrypt(syauthuseridopt));
                }


                string sheaders = Newtonsoft.Json.JsonConvert.SerializeObject(context.HttpContext.Request.Headers);
                omess.Headerrequest = sheaders;
                omess.Userid = rapiduserid;
                omess.YAuthSource = sYauthSource;

                //6acc1280-fde1-11eb-b480-3f057f12dc26 - Yardillo
                //ade9f2f0-fe3e-11eb-8e8b-29cf15887162 - MongoDBWix
                //d602ee50-2f9d-11ec-9121-f55b1f38643f - is speech

                //if (srapidsecretkey != "1f863a60-f3b6-11eb-bc3e-c3f329db9ee7" && srapidsecretkey != "6acc1280-fde1-11eb-b480-3f057f12dc26" && srapidapikey != "ade9f2f0-fe3e-11eb-8e8b-29cf15887162" && srapidsecretkey != "d602ee50-2f9d-11ec-9121-f55b1f38643f")
                //{ allpass = false; omess.Messageype = "Unauthorized"; omess.Messagecode = "00001"; }

                //if (allpass)
                //{
                //    if (sauthsrc != "yardillo" && sauthsrc != "WixAdapter" && sauthsrc != "YardilloSpeech")
                //    { allpass = false; omess.Messageype = "Unauthorized"; omess.Messagecode = "00002"; }

                //}

                //if (allpass)
                //{
                //    xrapidhost = context.HttpContext.Request.Headers["x-rapidapi-host"];
                //    omess.Callerid = xrapidhost;
                //    if (xrapidhost != "mbad.p.rapidapi.com" && xrapidhost != "yardillo.p.rapidapi.com" && xrapidhost != "mongodb-wix.p.rapidapi.com" && xrapidhost != "speech-to-text-feedback.p.rapidapi.com")
                //    { allpass = false; omess.Messageype = "Unauthorized"; omess.Messagecode = "00003"; }

                //}

                IMongoCollection<TenantUser> _tenantusercoll = null;
                if (allpass)
                {
                    _tenantusercoll = MBADDatabase.GetCollection<TenantUser>("TenantUsers");
                }

                if (allpass)
                {

                    if (rapiduserid == null || rapiduserid == "")
                    {
                        allpass = false; omess.Messageype = "Unauthorized"; omess.Messagecode = "00004";
                    }
                    else
                    {
                        context.HttpContext.Session.SetString("mbaduserid", rapiduserid);
                    }

                    //try to find users tenants

                    if (allpass && _tenantusercoll != null)
                    {
                        string ytenantname = context.HttpContext.Request.Headers["y-auth-tenantname"];
                        List<TenantUser> ou;
                        //get the user for the tenant created for a source
                        if ((ou = _tenantusercoll.Find<TenantUser>(book => book.Userid.ToUpper() == rapiduserid.ToUpper() && book.YAuthSource == sYauthSource).ToList()) != null)
                        {
                            if (ou.Count > 1)
                            {
                                //multiple present look for y-auth-tenantname
                                //continue
                                if (ytenantname != null && ytenantname != "")
                                {
                                    TenantUser o = ou.Where(t => t.Tenantname.ToUpper() == ytenantname.ToUpper()).FirstOrDefault();
                                    if (o != null)
                                    {
                                        context.HttpContext.Session.SetString("mbadtanent", o.Tenantname);
                                        omess.Tenantid = o.Tenantname;
                                        return;
                                    }
                                    else
                                    {
                                        allpass = false;
                                        omess.Messageype = "Unauthorized, You do not have access to this Tenant";
                                        omess.Messagecode = "00005";



                                    }
                                }
                                else
                                {
                                    //pick the top one
                                    context.HttpContext.Session.SetString("mbadtanent", ou[0].Tenantname);
                                    omess.Tenantid = ou[0].Tenantname;
                                    return;
                                }

                            }
                            if (ou.Count == 1)
                            {
                                //only one present then default to that
                                context.HttpContext.Session.SetString("mbadtanent", ou[0].Tenantname);
                                omess.Tenantid = ou[0].Tenantname;
                                return;
                            }
                            else
                            {
                                //continue
                            }
                        }

                    }

                    //so far goood, check if tenant name is passed
                    if (allpass)
                    {
                        string ytenantname = context.HttpContext.Request.Headers["y-auth-tenantname"];
                        //get the tenant from user name
                        if (ytenantname == null || ytenantname == "")
                        {
                            //generate one for this user
                            IMongoCollection<Tenant> _tenantcoll;
                            _tenantcoll = MBADDatabase.GetCollection<Tenant>("Tenants");
                            Tenant oten = new Tenant();
                            oten.Tenantname = "TENANT" + "_" + helperservice.RandomString(7, false); ;
                            oten.Tenantdesc = rapiduserid;
                            oten.Createdate = DateTime.UtcNow.ToString();
                            oten.Createuser = rapiduserid;
                            oten.YAuthSource = sYauthSource;
                            oten.Rapidsubscription = ssubs.ToString();
                            oten.Rapidhost = xrapidhost;
                            //set the rapid key for the first user who creates the tenant. Use this key for all calls
                            oten.Rapidapikey = srapidapikey;
                            string snewtenname = oten.Tenantname.ToUpper();
                            //tenant names are uniqe in entire yardillo
                            if (_tenantcoll.Find<Tenant>(book => book.Tenantname.ToUpper() == snewtenname.ToUpper()).FirstOrDefault() != null)
                            {
                                snewtenname = "TENANT" + "_" + helperservice.RandomString(7, false);
                                while (_tenantcoll.Find<Tenant>(book => book.Tenantname.ToUpper() == snewtenname.ToUpper()).FirstOrDefault() != null)
                                {
                                    //name must be unique assing a random string
                                    snewtenname = "TENANT" + "_" + helperservice.RandomString(7, false);
                                }
                                oten.Tenantname = snewtenname;
                            };

                            _tenantcoll.InsertOne(oten);
                            if (oten._id != "")
                            {
                                context.HttpContext.Session.SetString("mbadtanent", oten.Tenantname.ToUpper());
                                //and register as tenantuser
                                TenantUser ousr = new TenantUser();
                                ousr.Userid = rapiduserid;
                                ousr.Tenantname = oten.Tenantname;
                                ousr.Createdate = DateTime.UtcNow.ToString();
                                ousr.Createuserid = rapiduserid;
                                ousr.YAuthSource = sYauthSource;
                                ousr.RapidAPIkey = srapidapikey;
                                _tenantusercoll.InsertOne(ousr);
                                if (ousr._id != "")
                                {
                                    context.HttpContext.Session.SetString("mbaduserid", ousr.Userid);
                                    return;
                                }
                            }
                            else
                            {
                                omess.MessageDesc = "Unabel to create tenant for userid = " + rapiduserid;
                                _messagemaster.InsertOneAsync(omess);
                                allpass = false; omess.Messageype = "Unauthorized"; omess.Messagecode = "00006";
                            }
                        }
                        else
                        {
                            allpass = false; omess.Messageype = "Unauthorized"; omess.Messagecode = "00007";
                        }
                    }
                }

                if (allpass) { return; }

                if (allpass)
                {

                    string authHeader = context.HttpContext.Request.Headers["Authorization"];
                    if (authHeader != null)
                    {
                        var authHeaderValue = AuthenticationHeaderValue.Parse(authHeader);
                        if (authHeaderValue.Scheme.Equals(AuthenticationSchemes.Basic.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            var credentials = Encoding.UTF8
                                                .GetString(Convert.FromBase64String(authHeaderValue.Parameter ?? string.Empty))
                                                .Split(':', 2);
                            if (credentials.Length == 2)
                            {
                                //if (IsAuthorized(context, credentials[0], credentials[1]))
                                //{
                                //    return;
                                context.HttpContext.Session.SetString("mbadtanent", credentials[0]);

                                return;
                                //}
                            }
                        }
                    }
                }
                _messagemaster.InsertOneAsync(omess);
                ReturnUnauthorizedResult(context, omess);
            }
            catch (FormatException e)
            {
                omess.MessageDesc = "Unabel to validate user" + e.ToString();
                _messagemaster.InsertOneAsync(omess);
                ReturnUnauthorizedResult(context, omess);
            }
            finally
            {
                // _messagemaster.InsertOneAsync(omess);
            }
        }
        
        
        private static void ReturnUnauthorizedResult(AuthorizationFilterContext context)
        {
            // Return 401 and a basic authentication challenge (causes browser to show login dialog)
            context.HttpContext.Response.Headers["WWW-Authenticate"] = $"Basic realm=\"{_realm}\"";
            context.Result = new UnauthorizedResult();
        }
        private static void ReturnUnauthorizedResult(AuthorizationFilterContext context, Message message)
        {
            // Return 401 and a basic authentication challenge (causes browser to show login dialog)
            context.HttpContext.Response.Headers["WWW-Authenticate"] = $"Basic realm=\"{_realm}\"";
            context.HttpContext.Response.Headers["Content-type"] = "application/json";
            Errorresp errmess = new Errorresp();
            errmess.type = message.Callertype;
            errmess.title = message.Messageype;
            errmess.status = message.Messagecode;
            errmess.traceId = message._id;

            var bytes = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(errmess));

            context.HttpContext.Response.Body.WriteAsync(bytes, 0, bytes.Length);
            context.Result = new UnauthorizedResult();

        }
    }
    public  class Errorresp
    {

        public string type { get; set; }
        public string title { get; set; }
        public string status { get; set; }
        public string traceId { get; set; }

    }
}
