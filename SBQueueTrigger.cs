using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Threading.Tasks;

namespace DemoFunctionApp;

public class SBQueueTrigger
{
    private readonly ILogger<SBQueueTrigger> _logger;

    public SBQueueTrigger(ILogger<SBQueueTrigger> logger)
    {
        _logger = logger;
    }

    [Function(nameof(SBQueueTrigger))]
    public async Task Run(
        [ServiceBusTrigger("saurabhqueue", Connection = "saurabhSAS")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        var data = JsonConvert.DeserializeObject<Courses>(message.Body.ToString());
        string connectionstring = "";
        string statement = "INSERT INTO Courses(CourseID,CourseName,Rating) VALUES (@param1,@param2,@param3)";
        SqlConnection _connection = new SqlConnection(connectionstring);
        _connection.Open();

        using (SqlCommand cmd = new SqlCommand(statement, _connection))
        {
            cmd.Parameters.Add("@param1", SqlDbType.VarChar).Value = data.CourseID.ToString();
            cmd.Parameters.Add("@param2", SqlDbType.VarChar).Value = data.CourseName.ToString();
            cmd.Parameters.Add("@param3", SqlDbType.Decimal).Value = data.Rating.ToString();
            cmd.CommandType = CommandType.Text;
            cmd.ExecuteNonQuery();
        }

        _connection.Close();
        _logger.LogInformation("Message ID: {id}", message.MessageId);
        _logger.LogInformation("Message Body: {body}", message.Body);
        _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);

        // Complete the message
        await messageActions.CompleteMessageAsync(message);
    }
}