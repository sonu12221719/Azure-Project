using System;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace DemoFunctionApp;

public class TimerTrigger
{
    private readonly ILogger _logger;

    public TimerTrigger(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<TimerTrigger>();
    }

    [Function("TimerTrigger")]
    public void Run([TimerTrigger(" 0 0 */1 * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation("C# Timer trigger function executed at: {executionTime}", DateTime.Now);

        List<Courses> _lst = new List<Courses>();
        string connectionstring = "";
        string statement = "SELECT CourseID,CourseName,Rating from Courses";
        SqlConnection _connection = new SqlConnection(connectionstring);
        _connection.Open();
        SqlCommand sqlCommand = new SqlCommand(statement, _connection);
        using (SqlDataReader reader = sqlCommand.ExecuteReader())
        {
            while (reader.Read())
            {
                Courses _course = new Courses();
                {
                    _course.CourseID = reader.GetString(0);
                    _course.CourseName = reader.GetString(1);
                    _course.Rating = reader.GetDouble(2);
                }
                _lst.Add(_course);
            }

        }
        _connection.Close();

        var json = JsonSerializer.Serialize(_lst);
        _logger.LogInformation("Course List: {json}", json);

        if (myTimer.ScheduleStatus is not null)
        {
            _logger.LogInformation("Next timer schedule at: {nextSchedule}", myTimer.ScheduleStatus.Next);
        }
    }
}