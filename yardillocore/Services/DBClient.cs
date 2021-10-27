using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using yardillocore.Models;

namespace yardillocore.DB
{
    public static class DBClientCon
    {
        public static DBClient GetDbClient(IDatabaseSettings settings)
        {
            DBClient dbclient = new DBClient();
            dbclient.Settings = settings;
            dbclient.Client = new MongoClient(settings.ConnectionString);
            dbclient.MasterDatabase = dbclient.Client.GetDatabase(settings.DatabaseName);
            return dbclient;
        }
       
    }
    public class DBClient
    {
        public IDatabaseSettings Settings { get; set; }
        public MongoClient Client { get; set; }
        public IMongoDatabase MasterDatabase { get; set; }
       
    }
}
