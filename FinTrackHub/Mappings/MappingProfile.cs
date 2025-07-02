using AutoMapper;
using FinTrackHub.Entities;
using FinTrackHub.Models.DTOs;

namespace FinTrackHub.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            //CreateMap<Account, AccountDto>()
            //    .ForMember(dest => dest.AccountGroupName,
            //        opt => opt.MapFrom(src => src.AccountGroup != null ? src.AccountGroup.AccountGroupName : string.Empty))
            //    .ForMember(dest => dest.AccountGroupTypeName,
            //        opt => opt.MapFrom(src => src.AccountGroup != null && src.AccountGroup.AccountGroupType != null
            //                                 ? src.AccountGroup.AccountGroupType.AccountGroupTypeName : string.Empty))
            //    .ForMember(dest => dest.CreatedBy,
            //        opt => opt.MapFrom(src => src.CreatedByUser != null ? src.CreatedByUser.FullName : string.Empty))
            //    .ForMember(dest => dest.UpdatedBy,
            //        opt => opt.MapFrom(src => src.UpdatedByUser != null ? src.UpdatedByUser.FullName : string.Empty));

            CreateMap<Account, AccountDto>()
            .ForMember(dest => dest.AccountGroupName,
                opt => opt.MapFrom(src => src.AccountGroup != null ? src.AccountGroup.AccountGroupName : string.Empty))
            .ForMember(dest => dest.AccountGroupTypeName,
                opt => opt.MapFrom(src => src.AccountGroup != null && src.AccountGroup.AccountGroupType != null
                                         ? src.AccountGroup.AccountGroupType.AccountGroupTypeName : string.Empty))
            .ForMember(dest => dest.CreatedBy,
                opt => opt.MapFrom(src => src.CreatedByUser != null ? src.CreatedByUser.FullName : string.Empty))
            .ForMember(dest => dest.UpdatedBy,
                opt => opt.MapFrom(src => src.UpdatedByUser != null ? src.UpdatedByUser.FullName : string.Empty))
            .ForMember(dest => dest.AccountgroupTypeId,
                    opt => opt.MapFrom(src => src.AccountGroup != null
                        ? src.AccountGroup.AccountgroupTypeId
                        : (long?)null))

            .ReverseMap(); // <-- Add this to allow AccountDto → Account mapping



            CreateMap<Transaction, TransactionDto>()
             .ForMember(dest => dest.EncodedID, opt => opt.MapFrom(src => "100 000 " + src.TransactionId))
             .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.IncomeExpenseCategory.CategoryName))
             .ForMember(dest => dest.AccountName, opt => opt.MapFrom(src => src.Account.AccountName))
             .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedByUser.UserName))
             .ForMember(dest => dest.UpdatedBy, opt => opt.MapFrom(src => src.UpdatedByUser.UserName))
             .ReverseMap() // DTO → Entity
             .ForMember(dest => dest.TransactionId, opt => opt.Ignore()) // Optional: EF generates it
             .ForMember(dest => dest.IncomeExpenseCategory, opt => opt.Ignore())
             .ForMember(dest => dest.Account, opt => opt.Ignore())
             .ForMember(dest => dest.CreatedByUser, opt => opt.Ignore())
             .ForMember(dest => dest.UpdatedByUser, opt => opt.Ignore())
             .ForMember(dest => dest.CreatedById, opt => opt.Ignore()) // Set manually in controller
             .ForMember(dest => dest.UpdatedById, opt => opt.Ignore()) // Set manually in controller
             .ForMember(dest => dest.CreatedDate, opt => opt.Ignore()) // Optional: set in controller or DB
             .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore()) // Optional: set in controller
             .ForMember(dest => dest.IsActive, opt => opt.Ignore()); // Optional: default in entity



        }

    }

}
