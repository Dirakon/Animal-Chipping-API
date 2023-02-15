using AutoMapper;
using ItPlanetAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace ItPlanetAPI.Controllers;

[ApiController]
public class BaseEntityController : ControllerBase
{
    protected readonly DatabaseContext _context;
    protected readonly ILogger _logger;
    protected readonly IMapper _mapper;

    public BaseEntityController(ILogger logger, DatabaseContext context, IMapper mapper)
    {
        _logger = logger;
        _context = context;
        _mapper = mapper;
    }
}