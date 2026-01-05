using AutoMapper;
using dBanking.Core.DTOS;
using dBanking.Core.Entities;
using System.Text.Json;

namespace dBanking.Core.MappingProfiles
{
    public sealed class KycMappingProfile : Profile
    {
        public KycMappingProfile()
        {
            // Create request -> entity
            CreateMap<KycCaseCreateRequestDto, KycCase>()
                .ForMember(dest => dest.KycCaseId, opt => opt.MapFrom(_ => Guid.NewGuid()))
                .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.CustomerId))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => KycStatus.PENDING))
                .ForMember(dest => dest.EvidenceRefsJson, opt => opt.MapFrom(src => src.EvidenceRefs))
                .ForMember(dest => dest.ConsentText, opt => opt.MapFrom(src => src.ConsentText))
                .ForMember(dest => dest.AcceptedAt, opt => opt.MapFrom(src => src.AcceptedAt))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.CheckedAt, opt => opt.Ignore())
                .ForMember(dest => dest.ProviderRef, opt => opt.Ignore())
                .ForMember(dest => dest.Customer, opt => opt.Ignore());

            //// Entity -> Response DTO
            //CreateMap<KycCase, KycCaseResponseDto>()
            //    .ForMember(dest => dest.Status, opt => opt.MapFrom(src => MapStatus(src.Status)))
            //    .ForMember(dest => dest.EvidenceRefs, opt => opt.MapFrom(src => string.IsNullOrWhiteSpace(src.EvidenceRefsJson)
            //            ? Array.Empty<string>() : JsonSerializer.Deserialize<List<string>>(src.EvidenceRefsJson)!));

            // Entity -> Summary DTO
            CreateMap<KycCase, KycCaseSummaryDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => MapStatus(src.Status)));

           
        }

        // Local converter
        static KycStatusDto MapStatus(KycStatus s) => s switch
        {
            KycStatus.PENDING => KycStatusDto.PENDING,
            KycStatus.VERIFIED => KycStatusDto.VERIFIED,
            KycStatus.FAILED => KycStatusDto.FAILED,
            _ => KycStatusDto.PENDING
        };
    }
}
