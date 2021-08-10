using GoussanMedia.Domain.Models;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Azure.Management.Media.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoussanMedia.DataAccess
{
    public interface IAzMediaService
    {
        Task<Asset> CreateAsset(IBrowserFile fileToUpload, Videos videos, long maxFileSize = 52428800);
    }
}
