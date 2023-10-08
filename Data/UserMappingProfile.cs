using AutoMapper;
using System;
using UserServiceAPI.DTOs;
using UserServiceAPI.Models;

namespace UserServiceAPI.Data
{
  public class UserMappingProfile : Profile
  {    
    public UserMappingProfile()
    {
      CreateMap<UserCreateDto, User>();
      CreateMap<UserEditDto, User>(); 
    }    
  }
}
