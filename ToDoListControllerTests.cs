using System.Text;
using System.Linq;
using System;
using Moq;
using ToDoList.Controllers;
using ToDoList.Services;
using ToDoList.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using ToDoList.Repositories;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Amqp.Transaction;

namespace ToDoListTest;

public class ToDoListControllerTests
{
    private readonly ToDoListController _toDoListController;
    private readonly ILogger<ToDoListController> _logger;
    Mock<IToDoListRepository> toDoListRepositoryMock;
    Mock<ControllerBase> controllerBase;

    public ToDoListControllerTests()
    {
        toDoListRepositoryMock = new Mock<IToDoListRepository>();
        controllerBase = new Mock<ControllerBase>();
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<ToDoListService>();
        var toDoListServiceMock = new Mock<ToDoListService>(logger,toDoListRepositoryMock.Object);       
        var controllerLogger = loggerFactory.CreateLogger<ToDoListController>();
        _toDoListController = new ToDoListController(controllerLogger, toDoListServiceMock.Object);
    }
    [Theory]
    [InlineData(1)]
    public void GetUserTasksTest(int id)
    {
        var task = new List<ToDo>
        {
            new ToDo
            {
                Id = 1,
                Status = "PENDING",
                Approval = "APPROVED",
                Task = "task",
                UserId = 1
            }
        };
        toDoListRepositoryMock.Setup(x => x.GetUserTasks(id)).Returns(task);
        var user = new ClaimsPrincipal(
            new ClaimsIdentity(
                new Claim[]
                {
                    new Claim(ClaimTypes.Name,"1")
                }));
        _toDoListController.ControllerContext = new ControllerContext();
        _toDoListController.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
        var actionResult = _toDoListController.GetUserTasks();
        Assert.IsType<OkObjectResult>(actionResult);
        Assert.NotNull(actionResult);
        var okObjectResult = actionResult as OkObjectResult;
        var actualTasks = okObjectResult!.Value as List<TasksModel>;
        Assert.NotNull(actualTasks);

        var expectedTasks = new List<TasksModel>()
        {
            new TasksModel()
            { Id = 1, Task ="task", Status = "PENDING", Approval = "APPROVED", UserId = 1 }
        };

        Assert.Equal(expectedTasks, actualTasks);
    }
    [Theory]
    [InlineData(1)]
    public void GetUserCompletedTasksTest(int id)
    {
        var task = new List<ToDo>
        {
            new ToDo
            {
                Id = 1,
                Status = "COMPLETED",
                Approval = "APPROVED",
                Task = "task",
                UserId = 1
            }
        };
        toDoListRepositoryMock.Setup(x => x.GetUserCompletedTasks(id)).Returns(task);
        var user = new ClaimsPrincipal(
            new ClaimsIdentity(
                new Claim[]
                {
                    new Claim(ClaimTypes.Name,"1")
                }));
        _toDoListController.ControllerContext = new ControllerContext();
        _toDoListController.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
        var actionResult = _toDoListController.GetUserCompletedTasks();
        Assert.IsType<OkObjectResult>(actionResult);
        Assert.NotNull(actionResult);
        var okObjectResult = actionResult as OkObjectResult;
        var actualResult = okObjectResult!.Value as List<TasksModel>;
        Assert.NotNull(actualResult);

        var expectedTasks = new List<TasksModel>()
        {
            new TasksModel()
            { Id = 1, Task ="task", Status = "COMPLETED", Approval = "APPROVED", UserId = 1 }
        };

        Assert.Equal(expectedTasks, actualResult);
    }
    [Theory]
    [InlineData(1)]
    public void GetTaskByIdTest(int id)
    {
        var task = new ToDo
        {
            Id = 1,
            Status = "PENDING",
            Approval = "APPROVED",
            Task = "task",
            UserId = 1
        };
        toDoListRepositoryMock.Setup(x => x.GetTaskById(id)).Returns(task);
        var actionResult = _toDoListController.GetTaskById(id);
        Assert.IsType<OkObjectResult>(actionResult);
        Assert.NotNull(actionResult);
        var okObjectResult = actionResult as OkObjectResult;
        var actualResult = okObjectResult!.Value as TasksModel;
        Assert.NotNull(actualResult);

        var expectedTasks = new TasksModel()
        {
            Id = 1,
            Task = "task",
            Status = "PENDING",
            Approval = "APPROVED",
            UserId = 1
        };

        Assert.Equal(expectedTasks, actualResult);
    }
    [Theory]
    [InlineData(1)]
    public void CreateTaskTest(int id)
    {
        var task = new ToDoList.Model.Task
        {
                task = "task",
        };
        var mockUserData = new User
        {
            Id = 1,
            Username = "Virat_Kohli",
            Email = "virat@gmail.com",
            Password = "kohli",
            Role = "USER"
        };
        var toDo = new ToDo
        {
            Id = 1,
            Status = "PENDING",
            Approval = "PENDING",
            Task = "task",
            UserId = 1
        };
        toDoListRepositoryMock.Setup(x => x.GetUser(id)).Returns(mockUserData);
        toDoListRepositoryMock.Setup(x => x.CreateTask(toDo));
        var user = new ClaimsPrincipal(
            new ClaimsIdentity(
                new Claim[]
                {
                    new Claim(ClaimTypes.Name,"1")
                }));
        _toDoListController.ControllerContext = new ControllerContext();
        _toDoListController.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
        var actionResult = _toDoListController.CreateTask(task);
        Assert.IsType<OkObjectResult>(actionResult);
        Assert.NotNull(actionResult);
        var okObjectResult = actionResult as OkObjectResult;
        var actualResult = okObjectResult!.Value as String;
        Assert.NotNull(actualResult);

        var expectedTasks = "Created task succesfully";
        Assert.Equal(expectedTasks, actualResult);
    }
    [Theory]
    [InlineData(1)]
    public void UpdateStatusTests(int id)
    {
        var task = new ToDoList.Model.Task
        {
            task = "task",
        };
        var mockUserData = new User
        {
            Id = 1,
            Username = "Virat_Kohli",
            Email = "virat@gmail.com",
            Password = "kohli",
            Role = "USER"
        };
        var toDo = new ToDo
        {
            Id = 1,
            Status = "PENDING",
            Approval = "APPROVED",
            Task = "task",
            UserId = 1
        };
        toDoListRepositoryMock.Setup(x => x.GetUser(id)).Returns(mockUserData);
        toDoListRepositoryMock.Setup(x => x.GetTaskById(id)).Returns(toDo);
        toDoListRepositoryMock.Setup(x => x.UpdateTask(toDo));
        var user = new ClaimsPrincipal(
            new ClaimsIdentity(
                new Claim[]
                {
                    new Claim(ClaimTypes.Name,"1")
                }));
        _toDoListController.ControllerContext = new ControllerContext();
        _toDoListController.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
        var actionResult = _toDoListController.UpdateStatus(id);
        Assert.IsType<OkObjectResult>(actionResult);
        Assert.NotNull(actionResult);
        var okObjectResult = actionResult as OkObjectResult;
        var actualResult = okObjectResult!.Value as String;
        Assert.NotNull(actualResult);

        var expectedTasks = "Updated task successfully";
        Assert.Equal(expectedTasks, actualResult);
    }
    [Theory]
    [InlineData(1)]
    public void DeleteTaskTests(int id)
    {
        var task = new ToDoList.Model.Task
        {
            task = "task",
        };
        var mockUserData = new User
        {
            Id = 1,
            Username = "Virat_Kohli",
            Email = "virat@gmail.com",
            Password = "kohli",
            Role = "USER"
        };
        var toDo = new ToDo
        {
            Id = 1,
            Status = "COMPLETED",
            Approval = "APPROVED",
            Task = "task",
            UserId = 1
        };
        toDoListRepositoryMock.Setup(x => x.GetUser(id)).Returns(mockUserData);
        toDoListRepositoryMock.Setup(x => x.GetTaskById(id)).Returns(toDo);
        toDoListRepositoryMock.Setup(x => x.DeleteTask(toDo));
        var user = new ClaimsPrincipal(
            new ClaimsIdentity(
                new Claim[]
                {
                    new Claim(ClaimTypes.Name,"1")
                }));
        _toDoListController.ControllerContext = new ControllerContext();
        _toDoListController.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
        var actionResult = _toDoListController.DeleteTask(id);
        Assert.IsType<OkObjectResult>(actionResult);
        Assert.NotNull(actionResult);
        var okObjectResult = actionResult as OkObjectResult;
        var actualResult = okObjectResult!.Value as String;
        Assert.NotNull(actualResult);

        var expectedTasks = "Deleted task Successfully";
        Assert.Equal(expectedTasks, actualResult);
    }
}
