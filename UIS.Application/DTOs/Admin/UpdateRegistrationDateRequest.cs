using System;

namespace UIS.Application.DTOs.Admin;

public class UpdateRegistrationDateRequest
{
    public DateTime RegistrationStart { get; set; }
    public DateTime RegistrationEnd { get; set; }
    public bool IsActive { get; set; }
}