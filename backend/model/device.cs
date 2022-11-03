using Microsoft.AspNetCore.Mvc;

namespace backend.model;

[BindProperties]
public class Device
{
    public string? Name { get; set; }
}