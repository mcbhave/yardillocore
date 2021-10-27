using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using yardillocore.Models;

namespace yardillocore.Services
{
    public class MessageService
    {
        private readonly IMongoCollection<Message> _message;
        private readonly IMongoCollection<Message> _messagemaster;
        public IMongoDatabase _database;

        public MessageService(IDatabaseSettings settings, IMongoDatabase TenantDatabase, IMongoDatabase MBADDatabase)
        {
            if (TenantDatabase == null) { throw new Exception("Unable to connect to the tenant database."); }
            var client = new MongoClient(settings.ConnectionString);
            _database = TenantDatabase;
            try
            {
                _message = TenantDatabase.GetCollection<Message>(settings.MessagesCollectionName);
                _messagemaster = MBADDatabase.GetCollection<Message>("Logs");
            }
            catch
            {
                throw new Exception("Unable to connect to the database.");
            }

        }
        public Message Create(Message omess)
        {
            try
            {
                if (omess.Messageype == "ERROR" || omess.Messageype == "Status417ExpectationFailed")
                {
                    _messagemaster.InsertOneAsync(omess);
                }

                //omess.MessageDesc = "";
                _message.InsertOneAsync(omess);
                return omess;
            }
            catch
            {
                throw;
            }

        }
    }
}
