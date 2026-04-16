using AngularEnterpriseAPI.DTOs.User;
using AngularEnterpriseAPI.DTOs.Activity;
using AngularEnterpriseAPI.DTOs.PasswordReset;
using System;
using AngularEnterpriseAPI.Models.Entities;
using AutoMapper;

namespace AngularEnterpriseAPI.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // User mappings
            CreateMap<User, UserResponseDto>();
            CreateMap<CreateUserDto, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.RefreshTokens, opt => opt.Ignore());

            CreateMap<UpdateUserDto, User>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Activity mappings
            CreateMap<UserActivity, ActivityDto>()
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.ActivityType))
                .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => src.Timestamp))
                .ForMember(dest => dest.Icon, opt => opt.MapFrom(src => GetIconForActivityType(src.ActivityType)));

            // Password Reset mappings
            CreateMap<PasswordResetToken, PasswordResetTokenDto>();

            // Role mappings
            CreateMap<AngularEnterpriseAPI.Models.Entities.Role, AngularEnterpriseAPI.DTOs.Role.RoleResponseDto>();

            // Permission mappings
            CreateMap<AngularEnterpriseAPI.Models.Entities.Permission, AngularEnterpriseAPI.DTOs.Permission.PermissionResponseDto>();
        }

        private string GetIconForActivityType(string activityType)
        {
            return activityType.ToLower() switch
            {
                "login" => "login",
                "logout" => "logout",
                "create" => "person_add",
                "update" => "edit",
                "delete" => "delete",
                "profileupdate" => "account_circle",
                _ => "info"
            };
        }
    }
}
