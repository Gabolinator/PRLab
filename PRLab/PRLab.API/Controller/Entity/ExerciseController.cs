using Microsoft.AspNetCore.Mvc;
using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Repositories.Entity;
using PRLab.Domain.Utilities.Interface;

namespace PRLab.API.Controller.Entity;

[ApiController]
[Route("exercises")]
public class ExerciseController : ControllerBase
{
    private readonly IExerciseRepository repo;
    private readonly IAppLogger logger;
    private readonly IUserService userService;
    
    public ExerciseController(
        IExerciseRepository repo,
        IUserService userService,
        IAppLogger logger)
    {
        this.repo = repo;
        this.logger = logger;
        this.userService = userService;
    }
}