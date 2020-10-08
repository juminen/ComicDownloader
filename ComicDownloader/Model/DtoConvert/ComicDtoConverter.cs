using ComicDownloader.Repo;
using System.Collections.Generic;

namespace ComicDownloader.Model.DtoConvert
{
    static class ComicDtoConverter
    {
        public static Comic ConvertDtoToItem(ComicDto dto, bool convertChildren)
        {
            Comic item = new Comic(dto.UniqueIdentifier)
            {
                Name = dto.Name,
                StartUrl = dto.StartUrl,
                LastDownloadDate = dto.LastDownloadDate,
                SavingLocation = dto.SavingLocation
            };
            if (convertChildren)
            {
                item.Photos.AddRange(ComicPhotoDtoConverter.ConvertDtosToItems(dto.Photos, item));
            }
            return item;
        }

        public static List<Comic> ConvertDtosToItems(IEnumerable<ComicDto> dtos, bool convertChildren)
        {
            List<Comic> items = new List<Comic>();
            foreach (ComicDto item in dtos)
            {
                items.Add(ConvertDtoToItem(item, convertChildren));
            }
            return items;
        }

        public static ComicDto ConvertItemToDto(Comic item)
        {
            ComicDto dto = new ComicDto
            {
                UniqueIdentifier = item.Identifier.GetIdObject(),
                Name = item.Name,
                StartUrl = item.StartUrl,
                LastDownloadDate = item.LastDownloadDate,
                SavingLocation = item.SavingLocation
            };
            return dto;
        }

        public static List<ComicDto> ConvertItemsToDtos(IEnumerable<Comic> items)
        {
            List<ComicDto> dtos = new List<ComicDto>();
            foreach (Comic item in items)
            {
                dtos.Add(ConvertItemToDto(item));
            }
            return dtos;
        }
    }
}
