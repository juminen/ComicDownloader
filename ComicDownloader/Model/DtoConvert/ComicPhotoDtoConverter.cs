using ComicDownloader.Repo;
using System.Collections.Generic;

namespace ComicDownloader.Model.DtoConvert
{
    static class ComicPhotoDtoConverter
    {
        public static ComicPhoto ConvertDtoToItem(ComicPhotoDto dto, Comic parent)
        {
            ComicPhoto item = new ComicPhoto(dto.UniqueIdentifier, parent)
            {
                PublishDate = dto.PublishDate,
                DownloadDate = dto.DownloadDate,
                Status = dto.Status,
                RelativeFilePath = dto.RelativeFilePath,
                Url = dto.Url
            };
            return item;
        }

        public static List<ComicPhoto> ConvertDtosToItems(IEnumerable<ComicPhotoDto> dtos, Comic parent)
        {
            List<ComicPhoto> items = new List<ComicPhoto>();
            foreach (ComicPhotoDto item in dtos)
            {
                items.Add(ConvertDtoToItem(item, parent));
            }
            return items;
        }

        public static ComicPhotoDto ConvertItemToDto(ComicPhoto item)
        {
            ComicDto parent = ComicDtoConverter.ConvertItemToDto(item.Parent);
            ComicPhotoDto dto = new ComicPhotoDto
            {
                UniqueIdentifier = item.Identifier.GetGuid(),
                ComicGuid = parent.UniqueIdentifier,
                DownloadDate = item.DownloadDate,
                RelativeFilePath = item.RelativeFilePath,
                PublishDate = item.PublishDate,
                Status = item.Status,
                Url = item.Url
            };
            return dto;
        }

        public static List<ComicPhotoDto> ConvertItemsToDtos(IEnumerable<ComicPhoto> items)
        {
            List<ComicPhotoDto> dtos = new List<ComicPhotoDto>();
            foreach (ComicPhoto item in items)
            {
                dtos.Add(ConvertItemToDto(item));
            }
            return dtos;
        }
    }
}
