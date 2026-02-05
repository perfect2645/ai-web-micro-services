using repository.doraemon.Entities;

namespace service.domain.Models
{
    public static class DoraemonItemMapper
    {
        extension(DoraemonItemCreateDto doraemonItemCreateDto)
        {
            public DoraemonItem ToDto()
            {
                return new DoraemonItem(doraemonItemCreateDto.UserId,
                                        doraemonItemCreateDto.InputImageId,
                                        doraemonItemCreateDto.InputImageUrl,
                                        doraemonItemCreateDto.PropmtText);
            }
        }
    }
}
