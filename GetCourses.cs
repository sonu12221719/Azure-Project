using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace DemoFunctionApp;

public class GetCourses
{
    private readonly ILogger<GetCourses> _logger;

    public GetCourses(ILogger<GetCourses> logger)
    {
        _logger = logger;
    }

    [Function("GetCourses")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
    {
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
        return new OkObjectResult(_lst);
        //_logger.LogInformation("C# HTTP trigger function processed a request.");
        //return new OkObjectResult("Welcome to Azure Functions!");
    }
}