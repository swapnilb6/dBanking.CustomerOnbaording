using AutoMapper;
using dBanking.Core.DTOS;
using dBanking.Core.Entities;

namespace dBanking.Core.Mappers
{

    public sealed class CustomerMappingProfile : Profile
    {
        public CustomerMappingProfile()
        {
            // Enum mappings
            CreateMap<CustomerStatus, CustomerStatusDto>()
                .ConvertUsing((Func<CustomerStatus, CustomerStatusDto>)(status => status switch
                {
                    CustomerStatus.PENDING_KYC => CustomerStatusDto.PENDING_KYC,
                    CustomerStatus.VERIFIED => CustomerStatusDto.VERIFIED,
                    CustomerStatus.CLOSED => CustomerStatusDto.CLOSED,
                    _ => CustomerStatusDto.PENDING_KYC
                }));

            CreateMap<KycStatus, KycStatusDto>()
                .ConvertUsing((Func<KycStatus, KycStatusDto>)(status => status switch
                {
                    KycStatus.PENDING => KycStatusDto.PENDING,
                    KycStatus.VERIFIED => KycStatusDto.VERIFIED,
                    KycStatus.FAILED => KycStatusDto.FAILED,
                    _ => KycStatusDto.PENDING
                }));

            // KYC summary mapping
            CreateMap<KycCase, KycCaseSummaryDto>()
                .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status))
                .ForMember(d => d.CreatedAt, opt => opt.MapFrom(s => s.CreatedAt))
                .ForMember(d => d.CheckedAt, opt => opt.MapFrom(s => s.CheckedAt))
                .ForMember(d => d.ProviderRef, opt => opt.MapFrom(s => s.ProviderRef));

            // Customer → CustomerResponseDto
            CreateMap<Customer, CustomerResponseDto>()
                .ForMember(d => d.Dob, opt => opt.MapFrom(s => DateOnly.FromDateTime(s.Dob)))
                .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status))          // uses enum converter above
                .ForMember(d => d.KycCases, opt => opt.MapFrom(s => s.KycCases));       // uses KycCase → KycCaseSummaryDto
        }
    }

}
